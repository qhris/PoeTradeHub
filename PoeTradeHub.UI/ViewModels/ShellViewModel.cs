using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using Caliburn.Micro;
using PoeTradeHub.TradeAPI;
using PoeTradeHub.TradeAPI.Models;
using PoeTradeHub.UI.Models;
using PoeTradeHub.UI.Services;
using Serilog;

namespace PoeTradeHub.UI.ViewModels
{
    public class ShellViewModel : Conductor<object>
    {
        private readonly ILogger _logger;
        private readonly IWindowManager _windowManager;
        private readonly IHotkeyService _hotkeyService;
        private readonly ConfigurationViewModel _configurationViewModel;
        private readonly ITradeAPI _tradeAPI;

        private ObservableCollection<LeagueInfoModel> _leagues;
        private LeagueInfoModel _selectedLeague;

        public ShellViewModel(
            ILogger logger,
            IWindowManager windowManager,
            IHotkeyService hotkeyService,
            ConfigurationViewModel configurationViewModel,
            ITradeAPI tradeAPI)
        {
            _logger = logger;
            _windowManager = windowManager;
            _hotkeyService = hotkeyService;
            _configurationViewModel = configurationViewModel;
            _tradeAPI = tradeAPI;

            Leagues = new ObservableCollection<LeagueInfoModel>();
        }

        public ObservableCollection<LeagueInfoModel> Leagues
        {
            get => _leagues;
            set
            {
                _leagues = value;
                NotifyOfPropertyChange(nameof(Leagues));
            }
        }

        public LeagueInfoModel SelectedLeague
        {
            get => _selectedLeague;
            set
            {
                _selectedLeague = value;
                NotifyOfPropertyChange(nameof(SelectedLeague));
            }
        }

        protected override async void OnInitialize()
        {
            base.OnInitialize();

            IList<LeagueInfo> leagues = await _tradeAPI.QueryLeagues();
            _logger.Information("Available leagues: {@Leagues}",
                leagues.Select(league => league.Text));

            if (leagues.Any())
            {
                Leagues = new ObservableCollection<LeagueInfoModel>(leagues
                    .Select(league => new LeagueInfoModel(league)));
                SelectedLeague = SelectDefaultLeague(Leagues);
            }
        }

        private LeagueInfoModel SelectDefaultLeague(IEnumerable<LeagueInfoModel> leagues)
        {
            IEnumerable<LeagueInfoModel> consideredLeagues = leagues;
            IEnumerable<LeagueInfoModel> tempLeagues = leagues
                    .Where(league => league.Text.Trim() != "Standard")
                    .Where(league => league.Text.Trim() != "Hardcore");

            if (tempLeagues.Any())
            {
                consideredLeagues = tempLeagues;
            }

            LeagueInfoModel selection = consideredLeagues
                .Where(league => !league.Text.Contains("Hardcore"))
                .FirstOrDefault();

            if (selection == null)
            {
                selection = tempLeagues.FirstOrDefault();
            }

            if (selection != null)
            {
                selection.IsSelected = true;
            }

            return selection;
        }

        protected override void OnActivate()
        {
            base.OnActivate();

            _hotkeyService.Enable();
            _hotkeyService.DebugItem += HandleDebugItem;
            _hotkeyService.PriceItem += HandlePriceItem;
        }

        protected override void OnDeactivate(bool close)
        {
            _hotkeyService.DebugItem -= HandleDebugItem;
            _hotkeyService.PriceItem -= HandlePriceItem;
            _hotkeyService.Disable();

            base.OnDeactivate(close);
        }

        private void HandleDebugItem(object sender, ItemEventArgs eventArgs)
        {
            var viewModel = new ItemDebugViewModel();
            viewModel.ClipboardData = Clipboard.GetText();
            viewModel.ItemData = ItemParser.DebugItem(eventArgs.Item);

            _windowManager.ShowWindow(viewModel);
        }

        private async void HandlePriceItem(object sender, ItemEventArgs eventArgs)
        {
            try
            {
                IList<ItemRecord> listings = await _tradeAPI.QueryPrice(_selectedLeague.Id, eventArgs.Item);
                _windowManager.ShowWindow(new ItemListingViewModel(listings));
            }
            catch (NotImplementedException)
            {
                MessageBox.Show("Sorry, pricing for this type of item has not been implemented yet.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void SelectLeague(LeagueInfoModel league)
        {
            foreach (var item in Leagues)
            {
                item.IsSelected = false;
            }

            league.IsSelected = true;
            SelectedLeague = league;
        }

        public void ShowConfiguration()
        {
            if (!_configurationViewModel.IsActive)
            {
                _windowManager.ShowWindow(_configurationViewModel);
            }
        }

        public void ExitApplication()
        {
            Application.Current.Shutdown();
        }
    }
}
