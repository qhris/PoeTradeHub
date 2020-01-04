using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
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
using PoeTradeHub.UI.Utils;
using PoeTradeHub.UI.ViewModels;
using WindowsInput;
using WindowsInput.Native;

namespace PoeTradeHub.UI
{
    public class Bootstrapper : BootstrapperBase
    {
        const string ItemInfoHotkeyId = "ItemInfo";
        const string GameWindowTitle = "Path of Exile";

        public Bootstrapper()
        {
            Initialize();

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
                        IEnumerable<FetchResult> listings = await QueryUniqueItemListing(itemType, itemName);
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

        class FetchResultFrame
        {
            public IList<FetchResult> Result { get; set; }
        }

        class FetchResult
        {
            public string Id { get; set; }
            public ItemListing Listing { get; set; }
            public ItemData Item { get; set; }
        }

        class ItemListing
        {
            public string Method { get; set; }
            public DateTime Indexed { get; set; }
            public ItemListingStash Stash { get; set; }
            public string Whisper { get; set; }
            public ItemListingAccount Account { get; set; }
            public ItemListingPrice Price { get; set; }
        }

        class ItemListingStash
        {
            public string Name { get; set; }
            public int X { get; set; }
            public int Y { get; set; }
        }

        class ItemListingAccount
        {
            public string Name { get; set; }
            public string LastCharacterName { get; set; }
            public string Language { get; set; }
        }

        class ItemListingPrice
        {
            public string Type { get; set; }
            public int Amount { get; set; }
            public string Currency { get; set; }
        }

        class ItemData
        {
            public bool Verified { get; set; }
            [JsonProperty("w")]
            public int Width { get; set; }
            [JsonProperty("h")]
            public int Height { get; set; }
            public string Icon { get; set; }
            public string League { get; set; }
            public string Name { get; set; }
            public string TypeLine { get; set; }
            public bool Identified { get; set; }
            [JsonProperty("ilvl")]
            public int ItemLevel { get; set; }
        }

        private async Task<IEnumerable<FetchResult>> QueryUniqueItemListing(string itemType, string itemName)
        {
            const string QueryUrl = "https://www.pathofexile.com/api/trade/search/Metamorph";
            const string FetchUrl = "https://www.pathofexile.com/api/trade/fetch";

            JObject query = JObject.FromObject(new
            {
                query = new
                {
                    status = new
                    {
                        option = "online",
                    },
                    name = itemName,
                    type = itemType,
                },
                sort = new
                {
                    price = "asc",
                },
            });

            // MessageBox.Show(query.ToString(), "Query", MessageBoxButton.OK, MessageBoxImage.Information);

            using (var client = new HttpClient())
            {
                var requestContent = new StringContent(query.ToString(), Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PostAsync(QueryUrl, requestContent);

                string responseData = await response.Content.ReadAsStringAsync();
                JObject responseJson = JObject.Parse(responseData);
                var result = responseJson["result"] as JArray;
                var urlQuery = string.Join(",", result.Select(x => x.Value<string>()).Take(10).ToArray());
                var urlQueryId = responseJson["id"];

                var fetchUrl = $"{FetchUrl}/{urlQuery}?query={urlQueryId}";
                // MessageBox.Show(fetchUrl, "Url", MessageBoxButton.OK, MessageBoxImage.Information);

                HttpResponseMessage fetchResponse = await client.GetAsync(fetchUrl);
                string fetchResponseData = await fetchResponse.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<FetchResultFrame>(fetchResponseData).Result;

                // JObject fetchResponseJson = JObject.Parse(fetchResponseData);
                // MessageBox.Show(fetchResponseData, $"Response: {fetchResponse.StatusCode}", MessageBoxButton.OK, MessageBoxImage.Information);
            }

            return new List<FetchResult>();
        }

        protected override void OnStartup(object sender, StartupEventArgs e)
        {
            DisplayRootViewFor<ShellViewModel>();
        }
    }
}
