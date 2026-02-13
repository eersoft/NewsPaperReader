using NewsPaperReader.Models;
using NewsPaperReader.Services;
using System.Configuration;
using System.Data;
using System.Windows;
using System.Windows.Media;

namespace NewsPaperReader
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {
        public static void ApplyTheme(Theme theme)
        {
            // 清除现有的主题资源
            Current.Resources.MergedDictionaries.Clear();
            
            // 创建新的主题资源字典
            ResourceDictionary themeDictionary = new ResourceDictionary();
            
            // 根据主题类型设置对应的颜色
            switch (theme)
            {
                case Theme.Light:
                    themeDictionary.Add("WindowBackground", new SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 255, 255)));
                    themeDictionary.Add("BorderBrush", new SolidColorBrush(System.Windows.Media.Color.FromRgb(224, 224, 224)));
                    themeDictionary.Add("TextBrush", new SolidColorBrush(System.Windows.Media.Color.FromRgb(51, 51, 51)));
                    themeDictionary.Add("AccentBrush", new SolidColorBrush(System.Windows.Media.Color.FromRgb(0, 120, 215)));
                    themeDictionary.Add("HeaderBackground", new SolidColorBrush(System.Windows.Media.Color.FromRgb(240, 240, 240)));
                    themeDictionary.Add("SidebarBackground", new SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 255, 255)));
                    themeDictionary.Add("ButtonBackground", new SolidColorBrush(System.Windows.Media.Color.FromRgb(240, 240, 240)));
                    themeDictionary.Add("ButtonHoverBackground", new SolidColorBrush(System.Windows.Media.Color.FromRgb(224, 224, 224)));
                    themeDictionary.Add("ComboBoxBackground", new SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 255, 255)));
                    themeDictionary.Add("TextBoxBackground", new SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 255, 255)));
                    themeDictionary.Add("GridSplitterBackground", new SolidColorBrush(System.Windows.Media.Color.FromRgb(224, 224, 224)));
                    themeDictionary.Add("ToolBarBackground", new SolidColorBrush(System.Windows.Media.Color.FromRgb(240, 240, 240)));
                    break;
                case Theme.Dark:
                    themeDictionary.Add("WindowBackground", new SolidColorBrush(System.Windows.Media.Color.FromRgb(45, 45, 48)));
                    themeDictionary.Add("BorderBrush", new SolidColorBrush(System.Windows.Media.Color.FromRgb(68, 68, 68)));
                    themeDictionary.Add("TextBrush", new SolidColorBrush(System.Windows.Media.Color.FromRgb(224, 224, 224)));
                    themeDictionary.Add("AccentBrush", new SolidColorBrush(System.Windows.Media.Color.FromRgb(0, 120, 215)));
                    themeDictionary.Add("HeaderBackground", new SolidColorBrush(System.Windows.Media.Color.FromRgb(62, 62, 66)));
                    themeDictionary.Add("SidebarBackground", new SolidColorBrush(System.Windows.Media.Color.FromRgb(37, 37, 38)));
                    themeDictionary.Add("ButtonBackground", new SolidColorBrush(System.Windows.Media.Color.FromRgb(62, 62, 66)));
                    themeDictionary.Add("ButtonHoverBackground", new SolidColorBrush(System.Windows.Media.Color.FromRgb(74, 74, 77)));
                    themeDictionary.Add("ComboBoxBackground", new SolidColorBrush(System.Windows.Media.Color.FromRgb(62, 62, 66)));
                    themeDictionary.Add("TextBoxBackground", new SolidColorBrush(System.Windows.Media.Color.FromRgb(62, 62, 66)));
                    themeDictionary.Add("GridSplitterBackground", new SolidColorBrush(System.Windows.Media.Color.FromRgb(68, 68, 68)));
                    themeDictionary.Add("ToolBarBackground", new SolidColorBrush(System.Windows.Media.Color.FromRgb(62, 62, 66)));
                    break;
                case Theme.Blue:
                    themeDictionary.Add("WindowBackground", new SolidColorBrush(System.Windows.Media.Color.FromRgb(240, 248, 255)));
                    themeDictionary.Add("BorderBrush", new SolidColorBrush(System.Windows.Media.Color.FromRgb(176, 224, 230)));
                    themeDictionary.Add("TextBrush", new SolidColorBrush(System.Windows.Media.Color.FromRgb(0, 0, 128)));
                    themeDictionary.Add("AccentBrush", new SolidColorBrush(System.Windows.Media.Color.FromRgb(30, 144, 255)));
                    themeDictionary.Add("HeaderBackground", new SolidColorBrush(System.Windows.Media.Color.FromRgb(230, 242, 255)));
                    themeDictionary.Add("SidebarBackground", new SolidColorBrush(System.Windows.Media.Color.FromRgb(248, 248, 255)));
                    themeDictionary.Add("ButtonBackground", new SolidColorBrush(System.Windows.Media.Color.FromRgb(230, 242, 255)));
                    themeDictionary.Add("ButtonHoverBackground", new SolidColorBrush(System.Windows.Media.Color.FromRgb(204, 230, 255)));
                    themeDictionary.Add("ComboBoxBackground", new SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 255, 255)));
                    themeDictionary.Add("TextBoxBackground", new SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 255, 255)));
                    themeDictionary.Add("GridSplitterBackground", new SolidColorBrush(System.Windows.Media.Color.FromRgb(176, 224, 230)));
                    themeDictionary.Add("ToolBarBackground", new SolidColorBrush(System.Windows.Media.Color.FromRgb(230, 242, 255)));
                    break;
                case Theme.Green:
                    themeDictionary.Add("WindowBackground", new SolidColorBrush(System.Windows.Media.Color.FromRgb(240, 255, 240)));
                    themeDictionary.Add("BorderBrush", new SolidColorBrush(System.Windows.Media.Color.FromRgb(144, 238, 144)));
                    themeDictionary.Add("TextBrush", new SolidColorBrush(System.Windows.Media.Color.FromRgb(47, 79, 47)));
                    themeDictionary.Add("AccentBrush", new SolidColorBrush(System.Windows.Media.Color.FromRgb(50, 205, 50)));
                    themeDictionary.Add("HeaderBackground", new SolidColorBrush(System.Windows.Media.Color.FromRgb(230, 255, 230)));
                    themeDictionary.Add("SidebarBackground", new SolidColorBrush(System.Windows.Media.Color.FromRgb(248, 255, 248)));
                    themeDictionary.Add("ButtonBackground", new SolidColorBrush(System.Windows.Media.Color.FromRgb(230, 255, 230)));
                    themeDictionary.Add("ButtonHoverBackground", new SolidColorBrush(System.Windows.Media.Color.FromRgb(204, 255, 204)));
                    themeDictionary.Add("ComboBoxBackground", new SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 255, 255)));
                    themeDictionary.Add("TextBoxBackground", new SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 255, 255)));
                    themeDictionary.Add("GridSplitterBackground", new SolidColorBrush(System.Windows.Media.Color.FromRgb(144, 238, 144)));
                    themeDictionary.Add("ToolBarBackground", new SolidColorBrush(System.Windows.Media.Color.FromRgb(230, 255, 230)));
                    break;
                case Theme.ElegantGray:
                    themeDictionary.Add("WindowBackground", new SolidColorBrush(System.Windows.Media.Color.FromRgb(245, 245, 245)));
                    themeDictionary.Add("BorderBrush", new SolidColorBrush(System.Windows.Media.Color.FromRgb(211, 211, 211)));
                    themeDictionary.Add("TextBrush", new SolidColorBrush(System.Windows.Media.Color.FromRgb(74, 74, 74)));
                    themeDictionary.Add("AccentBrush", new SolidColorBrush(System.Windows.Media.Color.FromRgb(105, 105, 105)));
                    themeDictionary.Add("HeaderBackground", new SolidColorBrush(System.Windows.Media.Color.FromRgb(232, 232, 232)));
                    themeDictionary.Add("SidebarBackground", new SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 255, 255)));
                    themeDictionary.Add("ButtonBackground", new SolidColorBrush(System.Windows.Media.Color.FromRgb(232, 232, 232)));
                    themeDictionary.Add("ButtonHoverBackground", new SolidColorBrush(System.Windows.Media.Color.FromRgb(216, 216, 216)));
                    themeDictionary.Add("ComboBoxBackground", new SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 255, 255)));
                    themeDictionary.Add("TextBoxBackground", new SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 255, 255)));
                    themeDictionary.Add("GridSplitterBackground", new SolidColorBrush(System.Windows.Media.Color.FromRgb(211, 211, 211)));
                    themeDictionary.Add("ToolBarBackground", new SolidColorBrush(System.Windows.Media.Color.FromRgb(232, 232, 232)));
                    break;
                case Theme.BookYellow:
                    themeDictionary.Add("WindowBackground", new SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 248, 220)));
                    themeDictionary.Add("BorderBrush", new SolidColorBrush(System.Windows.Media.Color.FromRgb(222, 184, 135)));
                    themeDictionary.Add("TextBrush", new SolidColorBrush(System.Windows.Media.Color.FromRgb(139, 69, 19)));
                    themeDictionary.Add("AccentBrush", new SolidColorBrush(System.Windows.Media.Color.FromRgb(205, 133, 63)));
                    themeDictionary.Add("HeaderBackground", new SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 235, 205)));
                    themeDictionary.Add("SidebarBackground", new SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 255, 255)));
                    themeDictionary.Add("ButtonBackground", new SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 235, 205)));
                    themeDictionary.Add("ButtonHoverBackground", new SolidColorBrush(System.Windows.Media.Color.FromRgb(245, 222, 179)));
                    themeDictionary.Add("ComboBoxBackground", new SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 255, 255)));
                    themeDictionary.Add("TextBoxBackground", new SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 255, 255)));
                    themeDictionary.Add("GridSplitterBackground", new SolidColorBrush(System.Windows.Media.Color.FromRgb(222, 184, 135)));
                    themeDictionary.Add("ToolBarBackground", new SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 235, 205)));
                    break;
            }
            
            // 添加主题资源字典
            Current.Resources.MergedDictionaries.Add(themeDictionary);
        }
        
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // 加载设置
            var settings = SettingsManager.LoadSettings();
            
            // 应用主题
            ApplyTheme(settings.Theme);
        }
    }
}
