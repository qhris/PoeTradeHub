using PoeTradeHub.UI.Models;

namespace PoeTradeHub.UI.Services
{
    public interface IApplicationSettingsService
    {
        ApplicationHotkeySettings HotkeySettings { get; }
    }
}
