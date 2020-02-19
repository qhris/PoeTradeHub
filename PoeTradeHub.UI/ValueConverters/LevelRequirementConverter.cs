using System;
using System.Globalization;
using System.Windows.Data;

namespace PoeTradeHub.UI.ValueConverters
{
    public class LevelRequirementConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int number)
            {
                return Math.Max(1, (int)(number * 0.8f));
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException("Level requirement conversion is one way.");
        }
    }
}
