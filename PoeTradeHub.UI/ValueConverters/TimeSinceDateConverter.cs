using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;

namespace PoeTradeHub.UI.ValueConverters
{
    public class TimeSinceDateConverter : IValueConverter
    {
        private struct TimeFormat
        {
            public TimeSpan Threshold;
            public TimeSpan Divisor;
            public string Format;
        }

        private static IReadOnlyList<TimeFormat> s_timeFormats = new List<TimeFormat>()
        {
            new TimeFormat { Threshold = TimeSpan.FromDays(730), Divisor = TimeSpan.FromDays(365), Format = "{0} years" },
            new TimeFormat { Threshold = TimeSpan.FromDays(365), Divisor = TimeSpan.FromDays(365), Format = "{0} year" },
            new TimeFormat { Threshold = TimeSpan.FromDays(60), Divisor = TimeSpan.FromDays(30), Format = "{0} months" },
            new TimeFormat { Threshold = TimeSpan.FromDays(30), Divisor = TimeSpan.FromDays(30), Format = "{0} month" },
            new TimeFormat { Threshold = TimeSpan.FromDays(2), Divisor = TimeSpan.FromDays(1), Format = "{0} days" },
            new TimeFormat { Threshold = TimeSpan.FromHours(2), Divisor = TimeSpan.FromHours(1), Format = "{0} hours" },
            new TimeFormat { Threshold = TimeSpan.FromHours(1), Divisor = TimeSpan.FromHours(1), Format = "{0} hour" },
            new TimeFormat { Threshold = TimeSpan.FromMinutes(2), Divisor = TimeSpan.FromMinutes(1), Format = "{0} minutes" },
            new TimeFormat { Threshold = TimeSpan.FromMinutes(1), Divisor = TimeSpan.FromMinutes(1), Format = "{0} minute" },
            new TimeFormat { Threshold = TimeSpan.FromSeconds(2), Divisor = TimeSpan.FromSeconds(1), Format = "{0} seconds" },
            new TimeFormat { Threshold = TimeSpan.FromSeconds(1), Divisor = TimeSpan.FromSeconds(1), Format = "{0} second" },
        };

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DateTime date)
            {
                DateTime currentTime = DateTime.UtcNow;
                return FormatTimeSince(currentTime - date);
            }

            return string.Empty;
        }

        private string FormatTimeSince(TimeSpan elapsed)
        {
            bool isFuture = elapsed.Ticks < 0;
            elapsed = elapsed.Duration();
            int elapsedSeconds = (int)Math.Round(elapsed.TotalSeconds);

            foreach (TimeFormat timeFormat in s_timeFormats)
            {
                if (elapsed >= timeFormat.Threshold)
                {
                    int divisor = (int)Math.Round(timeFormat.Divisor.TotalSeconds);
                    int quotient = elapsedSeconds / divisor;

                    string baseFormat = string.Format(timeFormat.Format, quotient);
                    return isFuture ? $"in {baseFormat}" : $"{baseFormat}";
                }
            }

            return "Just now";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
