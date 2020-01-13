using System.Windows.Input;

namespace PoeTradeHub.UI.Models
{
    public class HotkeyBinding
    {
        public HotkeyBinding(Key key, ModifierKeys modifiers)
        {
            Key = key;
            Modifiers = modifiers;
        }

        /// <summary>
        /// Gets the bound hotkey.
        /// </summary>
        public Key Key { get; }

        /// <summary>
        /// Gets the bound hotkey modifiers.
        /// </summary>
        public ModifierKeys Modifiers { get; }
    }
}
