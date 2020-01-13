using System;
using System.Runtime.InteropServices;
using System.Text;

namespace PoeTradeHub.UI.Utility
{
    public static class Native
    {
        public static string ForegroundWindowTitle
        {
            get
            {
                var buffer = new StringBuilder(256);
                var window = GetForegroundWindow();

                if (GetWindowText(window, buffer, buffer.Capacity) > 0)
                {
                    return buffer.ToString();
                }

                return "";
            }
        }

        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        public static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);
    }
}
