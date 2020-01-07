using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Caliburn.Micro;
using NHotkey;
using NHotkey.Wpf;
using PoeTradeHub.TradeAPI;
using PoeTradeHub.TradeAPI.Models;
using PoeTradeHub.UI.Utils;
using WindowsInput;
using WindowsInput.Native;

namespace PoeTradeHub.UI.ViewModels
{
    public class ShellViewModel : Conductor<object>
    {
        private readonly IWindowManager _windowManager;
        private readonly ConfigurationViewModel _configurationViewModel;
        private readonly ITradeAPI _tradeAPI;
        private InputSimulator _inputSimulator;

        private const string ItemInfoHotkeyId = "ItemInfo";
        private const string GameWindowTitle = "Path of Exile";

        public ShellViewModel(
            IWindowManager windowManager,
            ConfigurationViewModel configurationViewModel,
            ITradeAPI tradeAPI)
        {
            _windowManager = windowManager;
            _configurationViewModel = configurationViewModel;
            _tradeAPI = tradeAPI;
            _inputSimulator = new InputSimulator();
        }

        protected override void OnActivate()
        {
            base.OnActivate();

            EnableGlobalHotkeys();
        }

        protected override void OnDeactivate(bool close)
        {
            DisableGlobalHotkeys();

            base.OnDeactivate(close);
        }

        private void EnableGlobalHotkeys()
        {
            try
            {
                HotkeyManager.Current.AddOrReplace(ItemInfoHotkeyId, Key.C, ModifierKeys.Control, OnItemInfo);
            }
            catch (HotkeyAlreadyRegisteredException)
            {
                return;
            }
        }

        private void DisableGlobalHotkeys()
        {
            HotkeyManager.Current.Remove(ItemInfoHotkeyId);
        }

        private async void OnItemInfo(object sender, HotkeyEventArgs e)
        {
            // Remove the hotkey before sending the keystroke to the application so we don't infinitely recurse.
            DisableGlobalHotkeys();

            // Global hotkeys eat the keyboard stroke from the application so we need to send it again.
            _inputSimulator.Keyboard.ModifiedKeyStroke(VirtualKeyCode.LCONTROL, VirtualKeyCode.VK_C);

            if (Native.ForegroundWindowTitle == GameWindowTitle)
            {
                try
                {
                    await Task.Delay(50);
                    await OnAcquiredItemInfo();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            EnableGlobalHotkeys();
        }

        private async Task OnAcquiredItemInfo()
        {
            var parser = new ItemParser();
            ItemInformation item;

            try
            {
                item = parser.Parse(Clipboard.GetText());
            }
            catch (InvalidItemException)
            {
                // TODO: Log error and promt user that we failed to parse the item (need to patch asap).
                return;
            }

            await DisplayItemPrice(item);   
        }

        private async Task DisplayItemPrice(ItemInformation item)
        {
            var parser = new ItemParser();

            var viewModel = new ItemDebugViewModel();
            viewModel.ClipboardData = Clipboard.GetText();
            viewModel.ItemData = parser.DebugItem(item);

            _windowManager.ShowWindow(viewModel);
            // MessageBox.Show(parser.DebugItem(item), "Item", MessageBoxButton.OK, MessageBoxImage.Information);

            return;

            if (item.Rarity == ItemRarity.Unique)
            {
                IList<ItemRecord> listings = await QueryUniqueItemListing(item.BaseType, item.Name);
                var builder = new StringBuilder();

                foreach (var listing in listings)
                {
                    if (listing.Listing.Price != null)
                    {
                        builder.AppendLine(
                            $"Item: {listing.Item.Name}, " +
                            $"Price: {listing.Listing.Price.Amount} {listing.Listing.Price.Currency}, " +
                            $"Account: {listing.Listing.Account.Name}, " +
                            $"Character: {listing.Listing.Account.LastCharacterName}, " +
                            $"Stash: {listing.Listing.Stash?.Name} {{{listing.Listing?.Stash.X}, {listing.Listing?.Stash.Y}}}");
                    }
                }

                MessageBox.Show(builder.ToString(), "Price", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private async Task<IList<ItemRecord>> QueryUniqueItemListing(string itemType, string itemName)
        {
            var query = new ItemQuery()
            {
                Name = itemName,
                BaseType = itemType,
            };

            return await _tradeAPI.QueryPrice(query);
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
