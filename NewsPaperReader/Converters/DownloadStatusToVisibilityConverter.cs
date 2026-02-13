using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace NewsPaperReader
{
    public class DownloadStatusToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string status)
            {
                // 只在未下载状态显示下载按钮
                return status == "未下载" ? Visibility.Visible : Visibility.Collapsed;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
