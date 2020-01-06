namespace PoeTradeHub.TradeAPI.Models
{
    public class ItemListingStash
    {
        /// <summary>
        /// Gets the name of the stash tab the item was listed in.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets the X position in the stash of the item listed.
        /// </summary>
        public int X { get; set; }

        /// <summary>
        /// Gets the Y position in the stash of the item listed.
        /// </summary>
        public int Y { get; set; }
    }
}
