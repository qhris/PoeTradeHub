using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
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
            var itemData = Clipboard.GetText().Split(new string[] { "\r\n", "\n" },
                StringSplitOptions.RemoveEmptyEntries);
            if (itemData.Length > 3 && itemData[0].StartsWith("Rarity:"))
            {
                var rarityMatch = Regex.Match(itemData[0], @"^Rarity: (.*)$");
                if (rarityMatch.Success)
                {
                    var namedItems = new HashSet<string>
                    {
                        "Normal",
                        "Magic",
                        "Rare",
                        "Unique",
                    };

                    string itemRarity = rarityMatch.Groups[1].Value;
                    string itemType = rarityMatch.Groups[1].Value;
                    string itemName = itemData[1];


                    if (namedItems.Contains(itemRarity))
                    {
                        itemType = itemData[2];
                        itemName = itemData[1];
                    }

                    if (itemRarity == "Unique")
                    {
                        IList<ItemRecord> listings = await QueryUniqueItemListing(itemType, itemName);
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

                            // builder.AppendLine(listing?.Listing?.Whisper);
                            // builder.AppendLine();
                        }

                        MessageBox.Show(builder.ToString(), "Price", MessageBoxButton.OK, MessageBoxImage.Information);
                    }

                    //var builder = new StringBuilder();
                    //builder.AppendLine($"Type: {itemType}");
                    //builder.AppendLine($"Name: {itemName}");

                    //MessageBox.Show(builder.ToString(), "Item", MessageBoxButton.OK, MessageBoxImage.Information);
                }

                // MessageBox.Show(Clipboard.GetText(), "Item", MessageBoxButton.OK, MessageBoxImage.Information);
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
