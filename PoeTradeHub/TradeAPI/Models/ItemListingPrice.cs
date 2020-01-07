namespace PoeTradeHub.TradeAPI.Models
{
    public class ItemListingPrice
    {
        public string Type { get; set; }

        /// <summary>
        /// Gets the amount of currency the item was listed for.
        /// </summary>
        public float Amount { get; set; }

        /// <summary>
        /// Gets the type of currency the item was listed for.
        /// </summary>
        public string Currency { get; set; }
    }
}
