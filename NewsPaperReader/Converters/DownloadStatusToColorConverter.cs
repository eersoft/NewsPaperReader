using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace NewsPaperReader
{
    public class DownloadStatusToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string status)
            {
                switch (status)
                {
                    case "已下载":
                        return System.Windows.Media.Brushes.Green;
                    case "下载中":
                        return System.Windows.Media.Brushes.Blue;
                    case "未下载":
                        return System.Windows.Media.Brushes.Gray;
                    default:
                        return System.Windows.Media.Brushes.Gray;
                }
            }
            return System.Windows.Media.Brushes.Gray;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
