using System.Windows.Input;
using PoeTradeHub.UI.Models;

namespace PoeTradeHub.UI.Services
{
    public class ApplicationSettingsService : IApplicationSettingsService
    {
        public ApplicationSettingsService()
        {
            HotkeySettings = new ApplicationHotkeySettings
            {
                DebugItem = new HotkeyBinding(Key.D, ModifierKeys.Shift | ModifierKeys.Control),
                PriceItem = new HotkeyBinding(Key.D, ModifierKeys.Control),
            };
        }

        public ApplicationHotkeySettings HotkeySettings { get; }
    }
}
