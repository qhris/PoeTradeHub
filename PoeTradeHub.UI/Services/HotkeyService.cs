using System;
using System.Threading.Tasks;
using System.Windows;
using NHotkey;
using NHotkey.Wpf;
using PoeTradeHub.UI.Models;
using PoeTradeHub.UI.Utility;
using WindowsInput;
using WindowsInput.Native;

namespace PoeTradeHub.UI.Services
{
    public class HotkeyService : IHotkeyService
    {
        private const string GameWindowTitle = "Path of Exile";
        private const string ItemDebugHotkeyId = "PoeTradeHub:ItemDebug";
        private const string ItemPriceHotkeyId = "PoeTradeHub:ItemPrice";

        private readonly IApplicationSettingsService _settingsService;
        private readonly IInputSimulator _inputSimulator;

        public HotkeyService(
            IApplicationSettingsService settingsService,
            IInputSimulator inputSimulator)
        {
            _settingsService = settingsService;
            _inputSimulator = inputSimulator;
        }
        
        public event EventHandler<ItemEventArgs> DebugItem;

        public event EventHandler<ItemEventArgs> PriceItem;

        private void OnDebugItem(ItemInformation item) =>
            DebugItem?.Invoke(this, new ItemEventArgs(item));

        private void OnPriceItem(ItemInformation item) =>
            PriceItem?.Invoke(this, new ItemEventArgs(item));

        public void Enable()
        {
            TryEnableHotkey(ItemPriceHotkeyId,
                _settingsService.HotkeySettings.DebugItem,
                ItemAction(OnDebugItem));
            TryEnableHotkey(ItemDebugHotkeyId,
                _settingsService.HotkeySettings.PriceItem,
                ItemAction(OnPriceItem));
        }

        public void Disable()
        {
            HotkeyManager.Current.Remove(ItemDebugHotkeyId);
            HotkeyManager.Current.Remove(ItemPriceHotkeyId);
        }

        private EventHandler<HotkeyEventArgs> ItemAction(Action<ItemInformation> itemAction)
        {
            if (itemAction == null)
            {
                throw new ArgumentNullException(nameof(itemAction));
            }

            return async (sender, args) =>
            {
                Disable();

                // TODO: The app should listed for window focus events and only enable hotkeys when the game is open.
                if (Native.ForegroundWindowTitle != GameWindowTitle)
                {
                    Enable();
                    return;
                }

                _inputSimulator.Keyboard.ModifiedKeyStroke(
                    VirtualKeyCode.CONTROL,
                    VirtualKeyCode.VK_C);
                await Task.Delay(50);

                try
                {
                    var parser = new ItemParser();
                    var item = parser.Parse(Clipboard.GetText());
                    if (item != null)
                    {
                        itemAction?.Invoke(item);
                    }
                }

                catch (InvalidItemException)
                {
                    // TODO: Log failed parses.
                }

                Enable();
            };
        }

        private bool TryEnableHotkey(string name, HotkeyBinding hotkey, EventHandler<HotkeyEventArgs> eventHandler)
        {
            try
            {
                HotkeyManager.Current.AddOrReplace(name,
                    hotkey.Key,
                    hotkey.Modifiers,
                    eventHandler);

                return true;
            }
            catch (HotkeyAlreadyRegisteredException)
            {
                return false;
            }
        }
    }
}
