using NewsPaperReader.Models;
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
            switch (MainWindowViewModel.NewspaperListMode)
            {
                case NewspaperListDisplayMode.TextList:
                    return TextListTemplate;
                case NewspaperListDisplayMode.ImageList:
                    return ImageListTemplate;
                case NewspaperListDisplayMode.ImageTile:
                    return ImageTileTemplate;
                default:
                    return TextListTemplate;
            }
        }
    }
}
