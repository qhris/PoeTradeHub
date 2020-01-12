namespace PoeTradeHub.TradeAPI.Models
{
    public class ItemListingPrice
    {
        public class ItemListingExchange
        {
            public string Currency { get; set; }
            public float? Amount { get; set; }
        }

        public class ItemListingExchangeItem
        {
            public string Id { get; set; }
            public string Currency { get; set; }
            public float? Amount { get; set; }
            public float? Stock { get; set; }
        }

        public string Type { get; set; }

        /// <summary>
        /// Gets the amount of currency the item was listed for.
        /// </summary>
        public float? Amount { get; set; }

        /// <summary>
        /// Gets the type of currency the item was listed for.
        /// </summary>
        public string Currency { get; set; }

        public ItemListingExchange Exchange { get; set; }

        public ItemListingExchangeItem Item { get; set; }
    }
}
