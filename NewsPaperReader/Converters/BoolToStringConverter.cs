using System;
using System.Globalization;
using System.Windows.Data;

namespace NewsPaperReader
{
    public class BoolToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isPinned)
            {
                return isPinned ? "取消固定侧边栏" : "固定侧边栏";
            }
            return "固定侧边栏";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
