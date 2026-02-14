using System;
using System.Globalization;
using System.Windows.Data;

namespace NewsPaperReader
{
    public class EnumToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
                return false;

            string enumValue = value.ToString();
            string targetValue = parameter.ToString();
            return enumValue.Equals(targetValue, StringComparison.OrdinalIgnoreCase);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
                return null;

            bool boolValue = (bool)value;
            if (boolValue)
            {
                return Enum.Parse(targetType, parameter.ToString());
            }
            return null;
        }
    }
}