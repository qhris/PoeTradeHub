using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PoeTradeHub.TradeAPI.Models;
using PoeTradeHub.TradeAPI.OfficialTrade.Models;
using Serilog;

namespace PoeTradeHub.TradeAPI.OfficialTrade
{
    public class OfficialTradeAPI : ITradeAPI
    {
        private const string TradeApiEndpoint = "https://www.pathofexile.com/api/trade";

        private readonly ILogger _logger;

        public OfficialTradeAPI(ILogger logger)
        {
            _logger = logger;
        }

        public async Task<IList<LeagueInfo>> QueryLeagues()
        {
            using (var client = new HttpClient())
            {
                var uri = new Uri($"{TradeApiEndpoint}/data/leagues");
                _logger.Information($"GET {uri}");
                HttpResponseMessage response = await client.GetAsync(uri);

                if (response.IsSuccessStatusCode)
                {
                    try
                    {
                        string payload = await response.Content.ReadAsStringAsync();
                        var data = JObject.Parse(payload);

                        return data["result"].ToObject<IList<LeagueInfo>>();
                    }
                    catch (JsonReaderException) { }
                }
            }

            return new List<LeagueInfo>();
        }

        public async Task<IList<ItemRecord>> QueryPrice(string leagueName, ItemInformation item)
        {
            FetchResponse response = await SearchItem(leagueName, item);
            return response.Result;
        }

        private async Task<FetchResponse> SearchItem(string leagueName, ItemInformation item)
        {
            using (var client = new HttpClient())
            {
                ItemCollectionResponse collectionResponse = await QueryItemCollection(client, leagueName, item);

                if (!collectionResponse.Result.Any() || string.IsNullOrWhiteSpace(collectionResponse.Id))
                {
                    return FetchResponse.Empty;
                }

                return await FetchItems(client, collectionResponse);
            }
        }

        private async Task<ItemCollectionResponse> QueryItemCollection(HttpClient client, string leagueName, ItemInformation item)
        {
            string searchQuery;
            Uri endpoint;

            if (IsExchangeItem(item))
            {
                searchQuery = BuildExchangeQuery(item);
                endpoint = new Uri($"{TradeApiEndpoint}/exchange/{leagueName}");
            }
            else
            {
                searchQuery = BuildSearchQuery(item);
                endpoint = new Uri($"{TradeApiEndpoint}/search/{leagueName}");
            }

            var requestContent = new StringContent(searchQuery.ToString(), Encoding.UTF8, "application/json");
            HttpResponseMessage response = await client.PostAsync(endpoint, requestContent);

            if (response.StatusCode != HttpStatusCode.OK)
            {
                _logger.Error("Error making request {@Request}, got {@Response", searchQuery, response);

                // TODO: Use exception or null return?
                throw new NotImplementedException();
            }

            string responseText = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<ItemCollectionResponse>(responseText);
        }

        private bool IsExchangeItem(ItemInformation item)
        {
            switch (item.ItemType)
            {
                case ItemType.DivinationCard:
                case ItemType.Currency:
                    return true;

                default:
                    return false;
            }
        }

        private string BuildExchangeQuery(ItemInformation item)
        {
            string sanitizedName = null;

            if (item.ItemType == ItemType.DivinationCard)
            {
                var words = item.Name.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                var name = Regex.Replace(string.Join("-", words), @"[,.:\'\""]+", string.Empty);
                sanitizedName = name.ToLowerInvariant();
            }

            if (sanitizedName == null)
            {
                throw new NotImplementedException("Item type can not be queried.");
            }

            return JObject.FromObject(new
            {
                exchange = new
                {
                    status = new
                    {
                        option = "online",
                    },
                    have = new string[] { },
                    want = new string[] { sanitizedName },
                },
            }).ToString(Formatting.None);
        }

        private string BuildSearchQuery(ItemInformation item)
        {
            if (item.Rarity == ItemRarity.Unique)
            {
                return BuildUniqueItemQuery(item);
            }

            if (item.ItemType == ItemType.Map)
            {
                return BuildMapQuery(item);
            }

            throw new NotImplementedException("Item type can not be queried.");
        }

        private string BuildUniqueItemQuery(ItemInformation item)
        {
            JObject query = JObject.FromObject(new
            {
                query = new
                {
                    status = new
                    {
                        option = "online",
                    },
                    type = item.BaseType,
                },
                sort = new
                {
                    price = "asc",
                },
            });

            if (item.IsIdentified)
            {
                query["query"]["name"] = item.Name;
            }
            else
            {
                query["query"]["filters"] = JObject.FromObject(new
                {
                    type_filters = new
                    {
                        filters = new
                        {
                            rarity = new
                            {
                                option = "unique",
                            },
                        },
                    },
                });
                // query["query"]["filters"]
            }

            return query.ToString(Formatting.None);
        }

        private string BuildMapQuery(ItemInformation item)
        {
            JObject query = JObject.FromObject(new
            {
                query = new
                {
                    status = new
                    {
                        option = "online",
                    },
                    type = new
                    {
                        option = item.BaseType,
                        discriminator = "warfortheatlas",
                    },
                    filters = new
                    {
                        map_filters = new
                        {
                            filters = new
                            {
                                map_tier = new
                                {
                                    min = item.MapTier,
                                    max = item.MapTier,
                                }
                            }
                        },
                        misc_filters = new
                        {
                            filters = new
                            {
                                corrupted = new
                                {
                                    option = item.IsCorrupted.ToString().ToLower(),
                                },
                            },
                        },
                    },
                },
                sort = new
                {
                    price = "asc",
                },
            });

            return query.ToString(Formatting.None);
        }

        private async Task<FetchResponse> FetchItems(HttpClient client, ItemCollectionResponse collectionResponse)
        {
            Uri fetchUri = BuildFetchUri(collectionResponse.Id, collectionResponse.Result);
            HttpResponseMessage fetchResponse = await client.GetAsync(fetchUri);
            string fetchResponseData = await fetchResponse.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<FetchResponse>(fetchResponseData);
        }

        private Uri BuildFetchUri(string queryId, IEnumerable<string> itemIdentifiers, int skip = 0)
        {
            if (string.IsNullOrWhiteSpace(queryId))
            {
                throw new ArgumentException("Must be a valid query ID", nameof(queryId));
            }

            // The maximum amount of items for each fetch is 10.
            var urlData = string.Join(",", itemIdentifiers.Skip(skip).Take(10));
            return new Uri($"{TradeApiEndpoint}/fetch/{urlData}?query={queryId}");
        }
    }
}
