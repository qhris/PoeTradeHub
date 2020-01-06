using System.Collections.Generic;
using PoeTradeHub.TradeAPI.Models;

namespace PoeTradeHub.TradeAPI.OfficialTrade.Models
{
    internal class FetchResponse
    {
        public IList<ItemRecord> Result { get; set; }

        public static FetchResponse Empty =>
            new FetchResponse { Result = new List<ItemRecord>() };
    }
}
