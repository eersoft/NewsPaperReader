using System.Text;
using System.Windows;
using System.Windows.Controls;
using System;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using Microsoft.Web.WebView2.Wpf;
using NewsPaperReader.Models;
using NewsPaperReader.Services;
using System.Windows.Forms;

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

        private NewspaperListDisplayMode _currentListMode = NewspaperListDisplayMode.TextList;
        
        private async void InitializeWebView2()
        {
            // 初始化WebView2
            await WebView2PdfViewer.EnsureCoreWebView2Async();

            // 禁用WebView2的F12键和调试功能
            WebView2PdfViewer.CoreWebView2.Settings.AreDevToolsEnabled = false;

            // 加载DisableWebView2ContextMenu设置
            var settings = SettingsManager.LoadSettings();
            WebView2PdfViewer.CoreWebView2.Settings.AreDefaultContextMenusEnabled = !settings.DisableWebView2ContextMenu;

            // 订阅WebView2的导航完成事件
            WebView2PdfViewer.NavigationCompleted += WebView2PdfViewer_NavigationCompleted;

            // 订阅WebView2的新窗口请求事件，确保所有链接都在当前窗口中打开
            WebView2PdfViewer.CoreWebView2.NewWindowRequested += WebView2PdfViewer_NewWindowRequested;

            // 加载HTML标题页面
            string titlePagePath = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "3rd_res", "html", "title.html");
            if (System.IO.File.Exists(titlePagePath))
            {
                string titlePageUrl = "file:///" + titlePagePath.Replace('\\', '/');
                WebView2PdfViewer.Source = new Uri(titlePageUrl);
            }

            // 订阅事件
            if (_viewModel != null)
            {
                _viewModel.PropertyChanged += ViewModel_PropertyChanged;
                _viewModel.ApplySettings += ApplySettings;
                // 加载应用设置
                _viewModel.LoadAppSettings();
            }
        }
        

        
        private void ApplySettings(UIElementDisplayStrategy displayStrategy, NewspaperListDisplayMode listMode)
        {
            _currentListMode = listMode;
            
            // 应用DisableWebView2ContextMenu设置
            if (WebView2PdfViewer != null && WebView2PdfViewer.CoreWebView2 != null)
            {
                var settings = SettingsManager.LoadSettings();
                WebView2PdfViewer.CoreWebView2.Settings.AreDefaultContextMenusEnabled = !settings.DisableWebView2ContextMenu;
            }
            
            // 更新报纸列表布局
            UpdateNewspaperListLayout();
            
            // 应用内容显示区默认显示模式
            // 重新加载当前PDF以应用新的显示模式
            UpdatePdfViewer();
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
            
            if (e.PropertyName == nameof(MainWindowViewModel.NewspaperListMode))
            {
                // 更新报纸列表布局
                UpdateNewspaperListLayout();
            }
        }
        
        private void UpdateNewspaperListLayout()
        {
            if (_viewModel == null)
                return;
            
            // 查找报纸列表控件
            var newspaperListBox = FindName("NewspaperListBox") as System.Windows.Controls.ListBox;
            if (newspaperListBox == null)
                return;
            
            // 根据NewspaperListMode的值，动态切换报纸列表的ItemsPanel
            switch (_viewModel.NewspaperListMode)
            {
                case NewspaperListDisplayMode.TextList:
                    // 文本列表模式：使用默认的StackPanel，垂直排列，只显示文本
                    newspaperListBox.ItemsPanel = new System.Windows.Controls.ItemsPanelTemplate(new System.Windows.FrameworkElementFactory(typeof(System.Windows.Controls.StackPanel)));
                    break;
                case NewspaperListDisplayMode.ImageList:
                    // 图片列表模式：使用默认的StackPanel，垂直排列，只显示图片
                    newspaperListBox.ItemsPanel = new System.Windows.Controls.ItemsPanelTemplate(new System.Windows.FrameworkElementFactory(typeof(System.Windows.Controls.StackPanel)));
                    break;
                case NewspaperListDisplayMode.ImageTile:
                    // 图片平铺模式：使用WrapPanel，水平排列，自动换行，只显示图片
                    var wrapPanelFactory = new System.Windows.FrameworkElementFactory(typeof(System.Windows.Controls.WrapPanel));
                    wrapPanelFactory.SetValue(System.Windows.Controls.WrapPanel.OrientationProperty, System.Windows.Controls.Orientation.Horizontal);
                    // 不设置固定的ItemWidth和ItemHeight，让WrapPanel自动计算，实现流式布局
                    newspaperListBox.ItemsPanel = new System.Windows.Controls.ItemsPanelTemplate(wrapPanelFactory);
                    break;
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
                    // 加载应用设置
                    var settings = NewsPaperReader.Services.SettingsManager.LoadSettings();
                    
                    // 使用file://协议加载本地PDF文件
                    string pdfUrl = "file:///" + pdfPath.Replace('\\', '/');
                    
                    // 构建PDF显示参数
                    string toolbarParam = settings.ShowPdfToolbar ? "1" : "0";
                    string pdfUrlWithParams = $"{pdfUrl}#toolbar={toolbarParam}";
                    
                    // 加载带自定义参数的PDF
                    WebView2PdfViewer.Source = new Uri(pdfUrlWithParams);
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
        
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void MaximizeButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.WindowState == WindowState.Maximized)
            {
                this.WindowState = WindowState.Normal;
            }
            else
            {
                this.WindowState = WindowState.Maximized;
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private System.Windows.Threading.DispatcherTimer _sidebarTimer;
        private Border _sidebarBorder;

        private void Sidebar_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            _sidebarBorder = sender as Border;
            if (_sidebarBorder != null)
            {
                // 取消之前的计时器
                _sidebarTimer?.Stop();
                // 显示侧边栏
                _sidebarBorder.Width = 300;
            }
        }

        private void Sidebar_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            _sidebarBorder = sender as Border;
            if (_sidebarBorder != null && _viewModel != null && !_viewModel.IsSidebarPinned)
            {
                // 创建计时器，延迟隐藏侧边栏
                _sidebarTimer = new System.Windows.Threading.DispatcherTimer();
                _sidebarTimer.Interval = TimeSpan.FromSeconds(1);
                _sidebarTimer.Tick += (s, args) =>
                {
                    _sidebarTimer.Stop();
                    // 隐藏侧边栏
                    _sidebarBorder.Width = 50;
                };
                _sidebarTimer.Start();
            }
        }
        
        private void WebView2PdfViewer_NavigationCompleted(object sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationCompletedEventArgs e)
        {
            // 当WebView2导航完成时，更新状态文本
            if (_viewModel != null && WebView2PdfViewer != null && WebView2PdfViewer.Source != null)
            {
                string uriString = WebView2PdfViewer.Source.ToString();
                // 检查是否是HTML标题页面
                if (uriString.Contains("title.html"))
                {
                    _viewModel.StatusText = "就绪";
                }
                // 检查是否是PDF文件
                else if (uriString.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
                {
                    _viewModel.StatusText = "PDF文档加载完成";
                }
                // 检查是否是网页
                else if (uriString.StartsWith("http://") || uriString.StartsWith("https://"))
                {
                    _viewModel.StatusText = "网页加载完成";
                }
                // 其他情况
                else
                {
                    _viewModel.StatusText = "就绪";
                }
            }
        }

        private void WebView2PdfViewer_NewWindowRequested(object sender, Microsoft.Web.WebView2.Core.CoreWebView2NewWindowRequestedEventArgs e)
        {
            // 阻止默认的新窗口行为
            e.Handled = true;

            // 在当前WebView2中打开链接
            if (e.Uri != null && WebView2PdfViewer != null && WebView2PdfViewer.CoreWebView2 != null)
            {
                WebView2PdfViewer.Source = new Uri(e.Uri);
            }
        }


    }
}