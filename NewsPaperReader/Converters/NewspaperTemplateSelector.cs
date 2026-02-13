using System.Windows;
using System.Windows.Controls;

namespace NewsPaperReader
{
    public class NewspaperTemplateSelector : DataTemplateSelector
    {
        public DataTemplate TextListTemplate { get; set; }
        public DataTemplate ImageListTemplate { get; set; }
        public DataTemplate ImageTileTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            // 暂时默认使用图片列表模板，后续可以根据设置或其他逻辑选择不同模板
            return ImageListTemplate;
        }
    }
}
