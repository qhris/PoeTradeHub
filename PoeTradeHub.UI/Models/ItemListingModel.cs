using PoeTradeHub.TradeAPI.Models;

namespace PoeTradeHub.UI.Models
{
    public class ItemListingModel
    {
        public ItemData Item { get; set; }
        public ItemListingAccount Account { get; set; }
        public string PriceTag { get; set; }
        public string CurrencyType { get; set; }
        public bool HasPriceTag { get; set; }
    }
}
