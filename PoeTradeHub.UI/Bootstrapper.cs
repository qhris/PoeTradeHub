using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Caliburn.Micro;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NHotkey;
using NHotkey.Wpf;
using PoeTradeHub.TradeAPI;
using PoeTradeHub.TradeAPI.Models;
using PoeTradeHub.TradeAPI.OfficialTrade;
using PoeTradeHub.UI.Utils;
using PoeTradeHub.UI.ViewModels;
using WindowsInput;
using WindowsInput.Native;

namespace PoeTradeHub.UI
{
    public class Bootstrapper : BootstrapperBase
    {
        private ITradeAPI _tradeAPI;

        private const string ItemInfoHotkeyId = "ItemInfo";
        private const string GameWindowTitle = "Path of Exile";

        public Bootstrapper()
        {
            Initialize();

            _tradeAPI = new OfficialTradeAPI("Metamorph");
            HotkeyManager.Current.AddOrReplace(ItemInfoHotkeyId, Key.C, ModifierKeys.Control, OnItemInfo);
            InputSimulator = new InputSimulator();
        }

        InputSimulator InputSimulator { get; }

        private void OnItemInfo(object sender, HotkeyEventArgs e)
        {
            // Remove the hotkey before sending the keystroke to the application so we don't infinitely recurse.
            HotkeyManager.Current.Remove(ItemInfoHotkeyId);

            // Global hotkeys eat the keyboard stroke from the application so we need to send it again.
            InputSimulator.Keyboard.ModifiedKeyStroke(VirtualKeyCode.LCONTROL, VirtualKeyCode.VK_C);

            if (Native.ForegroundWindowTitle == GameWindowTitle)
            {
                // The game needs time to process the item info so wait a bit before checking the clipboard.
                var timer = new DispatcherTimer();
                timer.Tick += new EventHandler(OnAcquiredItemInfo);
                timer.Interval = TimeSpan.FromMilliseconds(50);
                timer.Start();
            }

            HotkeyManager.Current.AddOrReplace(ItemInfoHotkeyId, Key.C, ModifierKeys.Control, OnItemInfo);
        }

        private async void OnAcquiredItemInfo(object sender, EventArgs e)
        {
            (sender as DispatcherTimer)?.Stop();

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

        protected override void OnStartup(object sender, StartupEventArgs e)
        {
            DisplayRootViewFor<ShellViewModel>();
        }
    }
}
