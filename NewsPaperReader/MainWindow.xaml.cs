using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using Microsoft.Web.WebView2.Wpf;
using NewsPaperReader.Models;

namespace NewsPaperReader
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainWindowViewModel? _viewModel;

        public MainWindow()
        {
            InitializeComponent();
            InitializeViewModel();
            InitializeWebView2();
        }

        private void InitializeViewModel()
        {
            _viewModel = new MainWindowViewModel();
            DataContext = _viewModel;
            // 订阅NavigateToUrl事件
            _viewModel.NavigateToUrl += ViewModel_NavigateToUrl;
        }

        private void ViewModel_NavigateToUrl(string url)
        {
            if (WebView2PdfViewer != null && WebView2PdfViewer.CoreWebView2 != null)
            {
                WebView2PdfViewer.Source = new Uri(url);
            }
        }

        private async void InitializeWebView2()
        {
            // 初始化WebView2
            await WebView2PdfViewer.EnsureCoreWebView2Async();

            // 订阅事件
            if (_viewModel != null)
            {
                _viewModel.PropertyChanged += ViewModel_PropertyChanged;
                _viewModel.ApplySettings += ApplySettings;
                // 加载应用设置
                _viewModel.LoadAppSettings();
            }
        }

        private UIElementDisplayStrategy _currentDisplayStrategy = UIElementDisplayStrategy.AlwaysShow;

        private NewspaperListDisplayMode _currentListMode = NewspaperListDisplayMode.TextList;

        private void ApplySettings(UIElementDisplayStrategy displayStrategy, ContentDisplayMode displayMode, NewspaperListDisplayMode listMode)
        {
            _currentDisplayStrategy = displayStrategy;
            _currentListMode = listMode;
            
            // 应用界面元素显示策略
            if (displayStrategy == UIElementDisplayStrategy.AutoHide)
            {
                // 初始隐藏侧边栏
                HideSidebars();
            }
            else
            {
                // 始终显示
                ShowSidebars();
            }
            
            // 应用内容显示区默认显示模式
            // 这里需要根据displayMode设置PDF的默认显示模式
        }

        private void HideSidebars()
        {
            if (LeftSidebar != null)
            {
                LeftSidebar.Width = 0;
            }
        }

        private void ShowSidebars()
        {
            if (LeftSidebar != null)
            {
                LeftSidebar.Width = 250;
            }
        }

        private void AutoHideTrigger_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (_currentDisplayStrategy == UIElementDisplayStrategy.AutoHide)
            {
                ShowSidebars();
            }
        }

        private void LeftSidebar_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (_currentDisplayStrategy == UIElementDisplayStrategy.AutoHide)
            {
                HideSidebars();
            }
        }

        private void ViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (_viewModel == null || WebView2PdfViewer == null || WebView2PdfViewer.CoreWebView2 == null)
                return;

            if (e.PropertyName == nameof(MainWindowViewModel.SelectedEdition) && _viewModel.SelectedEdition != null)
            {
                // 为SelectedEdition添加属性变化监听
                _viewModel.SelectedEdition.PropertyChanged += Edition_PropertyChanged;
                
                // 检查是否已下载
                UpdatePdfViewer();
            }

            if (e.PropertyName == nameof(MainWindowViewModel.CurrentPage))
            {
                // 尝试使用JavaScript控制PDF页面
                try
                {
                    if (_viewModel.CurrentPage > 0)
                    {
                        // 注意：这需要PDF.js或浏览器内置的PDF查看器支持
                        WebView2PdfViewer.CoreWebView2.ExecuteScriptAsync($"if (typeof PDFViewerApplication !== 'undefined') {{ PDFViewerApplication.page = {_viewModel.CurrentPage - 1}; }}");
                    }
                }
                catch
                {
                    // 忽略错误
                }
            }
        }

        private void Edition_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            // 当Edition的属性变化时，更新PDF查看器
            if (e.PropertyName == nameof(NewsPaperReader.Models.Edition.LocalFilePath) || e.PropertyName == nameof(NewsPaperReader.Models.Edition.IsDownloaded))
            {
                UpdatePdfViewer();
            }
        }

        private void UpdatePdfViewer()
        {
            if (_viewModel == null || _viewModel.SelectedEdition == null || WebView2PdfViewer == null || WebView2PdfViewer.CoreWebView2 == null)
                return;

            // 检查是否已下载
            if (_viewModel.SelectedEdition.IsDownloaded && !string.IsNullOrEmpty(_viewModel.SelectedEdition.LocalFilePath))
            {
                // 使用WebView2显示PDF文件
                string pdfPath = _viewModel.SelectedEdition.LocalFilePath;
                if (!string.IsNullOrEmpty(pdfPath))
                {
                    // 使用file://协议加载本地PDF文件
                    string pdfUrl = "file:///" + pdfPath.Replace('\\', '/');
                    WebView2PdfViewer.Source = new Uri(pdfUrl);
                    
                    // 监听PDF加载完成事件，设置适合宽度显示模式
                    WebView2PdfViewer.CoreWebView2.NavigationCompleted += (sender, args) =>
                    {
                        if (args.IsSuccess)
                        {
                            // 使用JavaScript设置PDF适合宽度显示
                            WebView2PdfViewer.CoreWebView2.ExecuteScriptAsync(
                                "if (typeof PDFViewerApplication !== 'undefined') { " +
                                "PDFViewerApplication.zoom = 'page-width'; " +
                                "}"
                            );
                        }
                    };
                }
            }
            else
            {
                // 如果尚未下载，显示加载状态
                WebView2PdfViewer.Source = new Uri("about:blank");
            }
        }

        protected override void OnClosed(System.EventArgs e)
        {
            base.OnClosed(e);
            // 尝试释放资源
            try
            {
                if (WebView2PdfViewer != null)
                {
                    WebView2PdfViewer.Dispose();
                }
            }
            catch
            {
                // 忽略错误
            }
        }
    }
}