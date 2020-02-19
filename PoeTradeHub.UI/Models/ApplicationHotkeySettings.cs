namespace PoeTradeHub.UI.Models
{
    public class ApplicationHotkeySettings
    {
        /// <summary>
        /// Gets or sets the hotkey used for debugging items.
        /// </summary>
        public HotkeyBinding DebugItem { get; set; }

        /// <summary>
        /// Gets or sets the hotkey used for pricing items.
        /// </summary>
        public HotkeyBinding PriceItem { get; set; }

        /// <summary>
        /// Gets or sets the hotkey used for providing item information.
        /// </summary>
        public HotkeyBinding ItemInfo { get; set; }
    }
}
