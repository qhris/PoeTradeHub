using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PoeTradeHub.TradeAPI.Models;
using PoeTradeHub.TradeAPI.OfficialTrade.Models;

namespace PoeTradeHub.TradeAPI.OfficialTrade
{
    public class OfficialTradeAPI : ITradeAPI
    {
        private string _leagueName;

        private const string _TradeAPIEndpoint = "https://www.pathofexile.com/api/trade";

        public OfficialTradeAPI(string leagueName)
        {
            // TODO: Aquire league name from a provider, it'll need to be pulled from the trade site or the API.
            _leagueName = leagueName;
        }

        public Uri SearchUri =>
            new Uri($"{_TradeAPIEndpoint}/search/{_leagueName}");

        public async Task<IList<ItemRecord>> QueryPrice(ItemQuery query)
        {
            FetchResponse response = await SearchItem(query);
            return response.Result;
        }

        private async Task<FetchResponse> SearchItem(ItemQuery itemQuery)
        {
            using (var client = new HttpClient())
            {
                ItemCollectionResponse collectionResponse = await QueryItemCollection(client, itemQuery);

                if (!collectionResponse.Result.Any() || string.IsNullOrWhiteSpace(collectionResponse.Id))
                {
                    return FetchResponse.Empty;
                }

                return await FetchItems(client, collectionResponse);
            }
        }

        private async Task<ItemCollectionResponse> QueryItemCollection(HttpClient client, ItemQuery itemQuery)
        {
            string searchQuery = BuildSearchQuery(itemQuery);
            var requestContent = new StringContent(searchQuery.ToString(), Encoding.UTF8, "application/json");
            HttpResponseMessage response = await client.PostAsync(SearchUri, requestContent);

            if (response.StatusCode != HttpStatusCode.OK)
            {
                // TODO: Log error here...
                // TODO: Use exception or null return?
                throw new NotImplementedException();
            }

            string responseText = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<ItemCollectionResponse>(responseText);
        }

        private string BuildSearchQuery(ItemQuery itemQuery)
        {
            JObject query = JObject.FromObject(new
            {
                query = new
                {
                    status = new
                    {
                        option = "online",
                    },
                    name = itemQuery.Name,
                    type = itemQuery.BaseType,
                },
                sort = new
                {
                    price = "asc",
                },
            });

            return query.ToString();
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
            return new Uri($"{_TradeAPIEndpoint}/fetch/{urlData}?query={queryId}");
        }
    }
}
