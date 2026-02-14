using System;
using System.Globalization;
using System.Windows.Data;
using NewsPaperReader.Models;

namespace NewsPaperReader.Converters
{
    /// <summary>
    /// 将FontSizeLevel枚举值转换为实际的字体大小
    /// </summary>
    public class FontSizeLevelToFontSizeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is FontSizeLevel fontSizeLevel)
            {
                double baseFontSize = 14; // 基础字体大小
                
                switch (fontSizeLevel)
                {
                    case FontSizeLevel.Normal:
                        baseFontSize = 14;
                        break;
                    case FontSizeLevel.Larger:
                        baseFontSize = 14 * 1.2;
                        break;
                    case FontSizeLevel.Large:
                        baseFontSize = 14 * 1.4;
                        break;
                    case FontSizeLevel.ExtraLarge:
                        baseFontSize = 14 * 1.6;
                        break;
                }
                
                // 处理ConverterParameter，用于设置乘数
                if (parameter != null && double.TryParse(parameter.ToString(), out double multiplier))
                {
                    baseFontSize *= multiplier;
                }
                
                return baseFontSize;
            }
            return 14; // 默认字体大小
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}