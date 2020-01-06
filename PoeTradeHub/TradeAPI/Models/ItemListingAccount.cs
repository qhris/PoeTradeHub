namespace PoeTradeHub.TradeAPI.Models
{
    public class ItemListingAccount
    {
        /// <summary>
        /// Gets the name of the player's account.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets the name of the character last played by the account.
        /// </summary>
        public string LastCharacterName { get; set; }

        /// <summary>
        /// Gets the accounts language locale.
        /// </summary>
        public string Language { get; set; }
    }
}
