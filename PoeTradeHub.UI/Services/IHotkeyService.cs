using System;

namespace PoeTradeHub.UI.Services
{
    /// <summary>
    /// Service for managing global hotkeys.
    /// </summary>
    public interface IHotkeyService
    {
        /// <summary>
        /// Event invoked when an item should be debugged.
        /// </summary>
        event EventHandler<ItemEventArgs> DebugItem;

        /// <summary>
        /// Event invoked when an item should be priced.
        /// </summary>
        event EventHandler<ItemEventArgs> PriceItem;

        /// <summary>
        /// Enable all global hotkeys.
        /// </summary>
        void Enable();

        /// <summary>
        /// Disable all global hotkeys.
        /// </summary>
        void Disable();
    }
}
