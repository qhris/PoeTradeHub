using System;

namespace PoeTradeHub.TradeAPI.Models
{
    public class ItemListing
    {
        public string Method { get; set; }
        public DateTime Indexed { get; set; }
        public ItemListingStash Stash { get; set; }
        public string Whisper { get; set; }
        public ItemListingAccount Account { get; set; }
        public ItemListingPrice Price { get; set; }
    }
}
