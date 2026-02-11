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

            // 获取主窗口
            var mainWindow = MainWindow as MainWindow;
            if (mainWindow != null && settings.AutoFullScreenOnStartup)
            {
                // 设置全屏
                mainWindow.WindowStyle = WindowStyle.None;
                mainWindow.WindowState = WindowState.Maximized;
                mainWindow.Topmost = true;
            }
        }
    }
}
