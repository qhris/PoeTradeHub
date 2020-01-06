namespace PoeTradeHub.TradeAPI.Models
{
    public class ItemRecord
    {
        public string Id { get; set; }
        public ItemListing Listing { get; set; }
        public ItemData Item { get; set; }
    }
}
