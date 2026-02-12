using NewsPaperReader.Services;
using System.Configuration;
using System.Data;
using System.Windows;

namespace NewsPaperReader
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // 加载设置
            var settings = SettingsManager.LoadSettings();
        }
    }
}
