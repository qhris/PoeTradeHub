using System;
using System.Collections.Generic;
using System.Windows;
using Caliburn.Micro;
using PoeTradeHub.TradeAPI;
using PoeTradeHub.TradeAPI.Models;
using PoeTradeHub.UI.Services;

namespace PoeTradeHub.UI.ViewModels
{
    public class ShellViewModel : Conductor<object>
    {
        private readonly IWindowManager _windowManager;
        private readonly IHotkeyService _hotkeyService;
        private readonly ConfigurationViewModel _configurationViewModel;
        private readonly ITradeAPI _tradeAPI;

        public ShellViewModel(
            IWindowManager windowManager,
            IHotkeyService hotkeyService,
            ConfigurationViewModel configurationViewModel,
            ITradeAPI tradeAPI)
        {
            _windowManager = windowManager;
            _hotkeyService = hotkeyService;
            _configurationViewModel = configurationViewModel;
            _tradeAPI = tradeAPI;
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
                IList<ItemRecord> listings = await _tradeAPI.QueryPrice(eventArgs.Item);
                _windowManager.ShowWindow(new ItemListingViewModel(listings));
            }
            catch (NotImplementedException)
            {
                MessageBox.Show("Sorry, pricing for this type of item has not been implemented yet.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
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
