using System;
using System.Threading.Tasks;
using System.Windows;
using NHotkey;
using NHotkey.Wpf;
using PoeTradeHub.UI.Models;
using PoeTradeHub.UI.Utility;
using Serilog;
using WindowsInput;
using WindowsInput.Native;

namespace PoeTradeHub.UI.Services
{
    public class HotkeyService : IHotkeyService
    {
        private const string GameWindowTitle = "Path of Exile";
        private const string ItemDebugHotkeyId = "PoeTradeHub:ItemDebug";
        private const string ItemPriceHotkeyId = "PoeTradeHub:ItemPrice";
        private const string ItemInfoHotkeyId = "PoeTradeHud:ItemInfo";

        private readonly ILogger _logger;
        private readonly IApplicationSettingsService _settingsService;
        private readonly IInputSimulator _inputSimulator;

        public HotkeyService(
            ILogger logger,
            IApplicationSettingsService settingsService,
            IInputSimulator inputSimulator)
        {
            _logger = logger;
            _settingsService = settingsService;
            _inputSimulator = inputSimulator;

            _logger.Information("Initialized hotkey service");
        }

        public event EventHandler<ItemEventArgs> DebugItem;

        public event EventHandler<ItemEventArgs> PriceItem;

        public event EventHandler<ItemEventArgs> ItemInfo;

        private void OnDebugItem(ItemInformation item) =>
            DebugItem?.Invoke(this, new ItemEventArgs(item));

        private void OnPriceItem(ItemInformation item) =>
            PriceItem?.Invoke(this, new ItemEventArgs(item));

        private void OnItemInfo(ItemInformation item) =>
            ItemInfo?.Invoke(this, new ItemEventArgs(item));

        public void Enable()
        {
            TryEnableHotkey(ItemPriceHotkeyId,
                _settingsService.HotkeySettings.DebugItem,
                ItemAction(OnDebugItem));
            TryEnableHotkey(ItemDebugHotkeyId,
                _settingsService.HotkeySettings.PriceItem,
                ItemAction(OnPriceItem));
            TryEnableHotkey(ItemInfoHotkeyId,
                _settingsService.HotkeySettings.ItemInfo,
                ItemAction(OnItemInfo));
        }

        public void Disable()
        {
            HotkeyManager.Current.Remove(ItemDebugHotkeyId);
            HotkeyManager.Current.Remove(ItemPriceHotkeyId);
            HotkeyManager.Current.Remove(ItemInfoHotkeyId);
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

                // Clear the clipboard so we can determine if the game provided any info.
                var clipboardRestore = Clipboard.GetText();
                Clipboard.Clear();

                _inputSimulator.Keyboard.ModifiedKeyStroke(
                    VirtualKeyCode.CONTROL,
                    VirtualKeyCode.VK_C);
                await Task.Delay(50);

                var clipboardText = Clipboard.GetText();
                if (string.IsNullOrWhiteSpace(clipboardText))
                {
                    // Restore the clipboard if nothing was captured.
                    Clipboard.SetText(clipboardRestore);
                }
                else
                {
                    try
                    {
                        var parser = new ItemParser();
                        var item = parser.Parse(clipboardText);
                        if (item != null)
                        {
                            itemAction?.Invoke(item);
                        }
                    }

                    catch (InvalidItemException e)
                    {
                        _logger.Error(e, "Failed to parse item");
                    }
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
