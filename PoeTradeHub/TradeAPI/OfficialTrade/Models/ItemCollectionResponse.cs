using System.Collections.Generic;

namespace PoeTradeHub.TradeAPI.OfficialTrade.Models
{
    internal class ItemCollectionResponse
    {
        public IEnumerable<string> Result { get; set; }
        public string Id { get; set; }
        public int? Total { get; set; }
    }
}
