using System;
using System.Collections.Generic;
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

        private const string ItemDebugHotkeyId = "Item:Debug";
        private const string ItemPriceHotkeyId = "Item:Price";

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
                HotkeyManager.Current.AddOrReplace(ItemPriceHotkeyId, Key.D, ModifierKeys.Control, OnPriceItem);
                HotkeyManager.Current.AddOrReplace(ItemDebugHotkeyId, Key.D, ModifierKeys.Control | ModifierKeys.Shift, OnDebugItem);
            }
            catch (HotkeyAlreadyRegisteredException)
            {
                return;
            }
        }

        private void DisableGlobalHotkeys()
        {
            HotkeyManager.Current.Remove(ItemPriceHotkeyId);
            HotkeyManager.Current.Remove(ItemDebugHotkeyId);
        }

        private async void OnDebugItem(object sender, HotkeyEventArgs e)
        {
            await GameClipboardAction(async (clipboardData) =>
            {
                var parser = new ItemParser();
                ItemInformation item = await Task.Run(() => parser.Parse(clipboardData));

                var viewModel = new ItemDebugViewModel();
                viewModel.ClipboardData = clipboardData;
                viewModel.ItemData = parser.DebugItem(item);

                _windowManager.ShowWindow(viewModel);
            });
        }

        private async void OnPriceItem(object sender, HotkeyEventArgs e)
        {
            await GameClipboardAction(async (clipboardData) =>
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
            });
        }

        private async Task GameClipboardAction(Func<string, Task> clipboardAction)
        {
            if (clipboardAction == null)
            {
                throw new ArgumentNullException(nameof(Clipboard));
            }

            // Remove the hotkey before sending the keystroke to the application so we don't infinitely recurse.
            DisableGlobalHotkeys();

            // Global hotkeys eat the keyboard stroke from the application so we need to send it again.
            _inputSimulator.Keyboard.ModifiedKeyStroke(VirtualKeyCode.LCONTROL, VirtualKeyCode.VK_C);

            if (Native.ForegroundWindowTitle == GameWindowTitle)
            {
                try
                {
                    await Task.Delay(50);
                    await clipboardAction(Clipboard.GetText());
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            EnableGlobalHotkeys();
        }

        private async Task DisplayItemPrice(ItemInformation item)
        {
            try
            {
                IList<ItemRecord> listings = await QueryUniqueItemListing(item);
                _windowManager.ShowWindow(new ItemListingViewModel(listings));
            }
            catch (NotImplementedException)
            {
                MessageBox.Show("Sorry, pricing for this type of item has not been implemented yet.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task<IList<ItemRecord>> QueryUniqueItemListing(ItemInformation item)
        {
            return await _tradeAPI.QueryPrice(item);
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
