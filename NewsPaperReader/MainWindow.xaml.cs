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
            
            // 确保左侧面板显示出来
            ShowSidebars();
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

        private System.Windows.Threading.DispatcherTimer _mousePositionTimer;
        
        private async void InitializeWebView2()
        {
            // 初始化WebView2
            await WebView2PdfViewer.EnsureCoreWebView2Async();

            // 初始化鼠标位置定时器
            _mousePositionTimer = new System.Windows.Threading.DispatcherTimer();
            _mousePositionTimer.Interval = TimeSpan.FromMilliseconds(50);
            _mousePositionTimer.Tick += MousePositionTimer_Tick;
            _mousePositionTimer.Start();

            // 订阅事件
            if (_viewModel != null)
            {
                _viewModel.PropertyChanged += ViewModel_PropertyChanged;
                _viewModel.ApplySettings += ApplySettings;
                // 加载应用设置
                _viewModel.LoadAppSettings();
            }
            
            // 确保左侧面板显示出来
            ShowSidebars();
        }
        
        private void MousePositionTimer_Tick(object sender, EventArgs e)
        {
            if (_currentDisplayStrategy == UIElementDisplayStrategy.AutoHide && this.IsVisible)
            {
                try
                {
                    // 获取鼠标在屏幕上的位置（使用更可靠的方法）
                    var cursorPosition = System.Windows.Forms.Cursor.Position;
                    var screenMousePosition = new System.Windows.Point(cursorPosition.X, cursorPosition.Y);
                    
                    // 获取窗口在屏幕中的位置
                    double windowLeft = this.Left;
                    double windowTop = this.Top;
                    double windowWidth = this.ActualWidth;
                    double windowHeight = this.ActualHeight;
                    
                    // 计算鼠标与窗口左侧边缘的距离
                    double mouseDistanceToLeft = screenMousePosition.X - windowLeft;
                    
                    // 检查鼠标是否在窗口的垂直范围内
                    bool isMouseInWindowVertical = screenMousePosition.Y >= windowTop && screenMousePosition.Y <= windowTop + windowHeight;
                    
                    // 检查鼠标是否在热区内（距离窗口左侧边缘100像素以内）
                    bool isMouseInHotZone = mouseDistanceToLeft >= 0 && mouseDistanceToLeft < 100 && isMouseInWindowVertical;
                    
                    // 计算窗口右下角坐标
                    double windowRight = windowLeft + windowWidth;
                    double windowBottom = windowTop + windowHeight;
                    
                    // 检查鼠标是否在热区内
                    if (isMouseInHotZone)
                    {
                        // 显示左侧面板
                        ShowSidebars();
                    }
                    else if (LeftSidebar != null && LeftSidebar.Visibility == Visibility.Visible)
                    {
                        // 只有当左侧面板可见时，才检查鼠标是否在面板内
                        try
                        {
                            // 检查鼠标是否在左侧面板内
                            var sidebarMousePosition = System.Windows.Input.Mouse.GetPosition(LeftSidebar);
                            
                            bool isMouseInSidebar = sidebarMousePosition.X >= 0 && sidebarMousePosition.X < LeftSidebar.ActualWidth &&
                                                  sidebarMousePosition.Y >= 0 && sidebarMousePosition.Y < LeftSidebar.ActualHeight;
                            
                            if (!isMouseInSidebar)
                            {
                                // 隐藏左侧面板
                                HideSidebars();
                            }
                        }
                        catch
                        {
                            // 忽略可能的异常
                        }
                    }
                }
                catch
                {
                    // 忽略可能的异常
                }
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
                // 设置为悬浮面板模式，不参与布局
                if (LeftSidebar != null)
                {
                    Grid.SetColumn(LeftSidebar, 0);
                    LeftSidebar.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                    LeftSidebar.VerticalAlignment = System.Windows.VerticalAlignment.Top;
                    LeftSidebar.Margin = new Thickness(0, 0, 0, 0);
                }
            }
            else
            {
                // 始终显示
                ShowSidebars();
                // 设置为普通面板模式，参与布局
                if (LeftSidebar != null)
                {
                    Grid.SetColumn(LeftSidebar, 0);
                    LeftSidebar.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
                    LeftSidebar.VerticalAlignment = System.Windows.VerticalAlignment.Stretch;
                    LeftSidebar.Margin = new Thickness(0, 0, 0, 0);
                }
            }
            
            // 更新报纸列表布局
            UpdateNewspaperListLayout();
            
            // 应用内容显示区默认显示模式
            // 这里需要根据displayMode设置PDF的默认显示模式
        }

        private void HideSidebars()
        {
            if (LeftSidebar != null)
            {
                LeftSidebar.Visibility = Visibility.Collapsed;
            }
        }

        private void ShowSidebars()
        {
            if (LeftSidebar != null)
            {
                LeftSidebar.Visibility = Visibility.Visible;
                // 如果是自动隐藏模式，设置为悬浮面板
                if (_currentDisplayStrategy == UIElementDisplayStrategy.AutoHide)
                {
                    LeftSidebar.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                    LeftSidebar.VerticalAlignment = System.Windows.VerticalAlignment.Top;
                    LeftSidebar.Width = 240;
                    LeftSidebar.Height = this.ActualHeight;
                    LeftSidebar.Margin = new Thickness(0, 0, 0, 0);
                    System.Windows.Controls.Panel.SetZIndex(LeftSidebar, 100);
                }
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
                // 直接隐藏面板，不再检查鼠标位置
                // 因为MouseLeave事件只有在鼠标确实离开元素边界时才会触发
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
            
            if (e.PropertyName == nameof(MainWindowViewModel.NewspaperListMode))
            {
                // 更新报纸列表布局
                UpdateNewspaperListLayout();
            }
            
            if (e.PropertyName == nameof(MainWindowViewModel.StatusText))
            {
                // 在窗口标题栏上显示状态和进度
                if (!string.IsNullOrEmpty(_viewModel.StatusText))
                {
                    this.Title = $"在线报纸阅读器 - {_viewModel.StatusText}";
                }
                else
                {
                    this.Title = "在线报纸阅读器";
                }
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
                    
                    // 根据ContentDisplayMode设置项来设置PDF的显示模式
                    string viewMode = "FitW"; // 默认适应宽度
                    switch (settings.ContentDisplayMode)
                    {
                        case NewsPaperReader.Models.ContentDisplayMode.FitWidth:
                            viewMode = "FitW";
                            break;
                        case NewsPaperReader.Models.ContentDisplayMode.FitHeight:
                            viewMode = "FitH";
                            break;
                        case NewsPaperReader.Models.ContentDisplayMode.FitPage:
                            viewMode = "Fit";
                            break;
                    }
                    
                    // 构建PDF显示参数
                    // Edge PDF查看器参数格式：#view=FitW&toolbar=0&pagemode=none
                    string toolbarParam = settings.ShowPdfToolbar ? "1" : "0";
                    string pdfUrlWithParams = $"{pdfUrl}#view={viewMode}&toolbar={toolbarParam}&pagemode=none";
                    
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
            // 确保左侧面板显示出来
            ShowSidebars();
        }
    }
}