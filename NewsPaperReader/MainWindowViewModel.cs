using NewsPaperReader.Models;
using NewsPaperReader.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;

namespace NewsPaperReader
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private readonly WebAnalyzer _webAnalyzer;
        private readonly HttpClient _httpClient;
        private string _pathRoot;

        // 命令
        public ICommand AddNewspaperCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand SettingsCommand { get; }
        public ICommand AboutCommand { get; }
        public ICommand OpenAboutCommand { get; }
        public ICommand ToggleSidebarPinCommand { get; }
        public ICommand ToggleSidebarCommand { get; }
        public ICommand DownloadEditionCommand { get; }
        public ICommand DownloadAllEditionsCommand { get; }
        public ICommand EditNewspaperCommand { get; }
        public ICommand DeleteNewspaperCommand { get; }

        // 属性
        private List<Newspaper> _newspapers = new List<Newspaper>();
        public List<Newspaper> Newspapers
        {
            get => _newspapers;
            set
            {
                _newspapers = value;
                OnPropertyChanged();
                // 当Newspapers变化时，也要通知FilteredNewspapers变化
                OnPropertyChanged(nameof(FilteredNewspapers));
            }
        }

        private Newspaper? _selectedNewspaper;
        public Newspaper? SelectedNewspaper
        {
            get => _selectedNewspaper;
            set
            {
                _selectedNewspaper = value;
                OnPropertyChanged();
                if (value != null)
                {
                    // 每次切换报纸都重新加载版面信息
                    value.IsLoaded = false;
                    _ = LoadNewspaperEditionsAsync(value);
                }
            }
        }

        private Edition? _selectedEdition;
        public Edition? SelectedEdition
        {
            get => _selectedEdition;
            set
            {
                _selectedEdition = value;
                OnPropertyChanged();
                if (value != null)
                {
                    _ = LoadPdfFileAsync(value);
                }
            }
        }

        private List<Edition> _editions = new List<Edition>();
        public List<Edition> Editions
        {
            get => _editions;
            set
            {
                _editions = value;
                OnPropertyChanged();
            }
        }

        private int _currentPage;
        public int CurrentPage
        {
            get => _currentPage;
            set
            {
                _currentPage = value;
                OnPropertyChanged();
            }
        }

        private int _totalPages;
        public int TotalPages
        {
            get => _totalPages;
            set
            {
                _totalPages = value;
                OnPropertyChanged();
            }
        }

        private string _statusText = "就绪";
        public string StatusText
        {
            get => _statusText;
            set
            {
                _statusText = value;
                OnPropertyChanged();
                // 控制状态栏的显示和隐藏
                IsStatusBarVisible = !string.IsNullOrEmpty(value) && value != "就绪";
            }
        }

        private bool _isStatusBarVisible = false;
        public bool IsStatusBarVisible
        {
            get => _isStatusBarVisible;
            set
            {
                _isStatusBarVisible = value;
                OnPropertyChanged();
            }
        }

        private bool _isSidebarPinned = true;
        public bool IsSidebarPinned
        {
            get => _isSidebarPinned;
            set
            {
                _isSidebarPinned = value;
                OnPropertyChanged();
            }
        }

        private string _searchText = string.Empty;
        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged();
                // 当搜索文本变化时，更新过滤后的报纸列表
                OnPropertyChanged(nameof(FilteredNewspapers));
            }
        }

        public List<Newspaper> FilteredNewspapers
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_searchText))
                {
                    return Newspapers;
                }
                return Newspapers.Where(n => n.Name.Contains(_searchText, StringComparison.OrdinalIgnoreCase)).ToList();
            }
        }

        private double _sidebarWidth = 300;
        public double SidebarWidth
        {
            get => _sidebarWidth;
            set
            {
                _sidebarWidth = value;
                OnPropertyChanged();
            }
        }
        


        private bool _isSidebarVisible = true;
        public bool IsSidebarVisible
        {
            get => _isSidebarVisible;
            set
            {
                _isSidebarVisible = value;
                OnPropertyChanged();
            }
        }

        private string _currentContent = "about:blank";
        public string CurrentContent
        {
            get => _currentContent;
            set
            {
                _currentContent = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public MainWindowViewModel()
        {
            _webAnalyzer = new WebAnalyzer();
            _httpClient = new HttpClient();
            _pathRoot = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "心仪读报");
            
            // 初始化命令
            AddNewspaperCommand = new RelayCommand(AddNewspaper);
            RefreshCommand = new RelayCommand(Refresh);
            SettingsCommand = new RelayCommand(Settings);
            AboutCommand = new RelayCommand(About);
            OpenAboutCommand = new RelayCommand(About);
            ToggleSidebarPinCommand = new RelayCommand(ToggleSidebarPin);
            ToggleSidebarCommand = new RelayCommand(ToggleSidebar);
            DownloadEditionCommand = new RelayCommand(DownloadEdition);
            DownloadAllEditionsCommand = new RelayCommand(DownloadAllEditions);
            EditNewspaperCommand = new RelayCommand(EditNewspaper);
            DeleteNewspaperCommand = new RelayCommand(DeleteNewspaper);
            
            // 加载应用设置
            LoadAppSettings();
            
            // 从设置中加载报纸列表
            LoadNewspapersFromSettings();
            
            Editions = new List<Edition>();
        }

        private void AddNewspaper(object? parameter)
        {
            var dialog = new AddNewspaperDialog();
            if (dialog.ShowDialog() == true && dialog.IsConfirmed)
            {
                var newspaper = new Newspaper(dialog.NewspaperName, dialog.NewspaperUrl, dialog.TitleImagePath, dialog.ParsePdf, dialog.ForceWebView);
                Newspapers.Add(newspaper);
                
                // 更新设置中的报纸库
                var settings = SettingsManager.LoadSettings();
                settings.NewspaperLibrary.Add(new NewspaperInfo(dialog.NewspaperName, dialog.NewspaperUrl, dialog.TitleImagePath, dialog.ParsePdf, dialog.ForceWebView));
                SettingsManager.SaveSettings(settings);
            }
        }

        private void Refresh(object? parameter)
        {
            if (SelectedNewspaper != null)
            {
                SelectedNewspaper.IsLoaded = false;
                _ = LoadNewspaperEditionsAsync(SelectedNewspaper);
            }
        }

        private void Settings(object? parameter)
        {
            var dialog = new SettingsWindow();
            // 订阅设置更改事件
            dialog.ViewModel.SettingsChanged += (settings) => {
                // 重新加载应用设置
                LoadAppSettings();
                // 重新加载报纸列表
                LoadNewspapersFromSettings();
            };
            dialog.ShowDialog();
            // 无论对话框返回什么结果，都重新加载报纸列表
            LoadNewspapersFromSettings();
        }

        private void About(object? parameter)
        {
            System.Windows.MessageBox.Show("心仪读报 v1.0\n\n一个智能分析和阅读在线报纸的工具", "关于", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
        }

        private void ToggleSidebarPin(object? parameter)
        {
            IsSidebarPinned = !IsSidebarPinned;
            
            // 保存到设置中
            var settings = SettingsManager.LoadSettings();
            settings.IsSidebarPinned = IsSidebarPinned;
            SettingsManager.SaveSettings(settings);
        }

        private void ToggleSidebar(object? parameter)
        {
            IsSidebarVisible = !IsSidebarVisible;
            if (IsSidebarVisible)
            {
                SidebarWidth = 300;
            }
        }

        private void DownloadEdition(object? parameter)
        {
            if (parameter is Edition edition)
            {
                _ = LoadPdfFileAsync(edition);
            }
        }

        private void DownloadAllEditions(object? parameter)
        {
            if (SelectedNewspaper != null && SelectedNewspaper.Editions.Count > 0)
            {
                _ = DownloadAllPdfsAsync(SelectedNewspaper.Editions);
            }
        }

        private void EditNewspaper(object? parameter)
        {
            if (parameter is Newspaper newspaper)
            {
                var dialog = new AddNewspaperDialog(newspaper);
                if (dialog.ShowDialog() == true && dialog.IsConfirmed)
                {
                    // 更新报纸信息
                    newspaper.Name = dialog.NewspaperName;
                    newspaper.Url = dialog.NewspaperUrl;
                    newspaper.TitleImagePath = dialog.TitleImagePath;
                    newspaper.ParsePdf = dialog.ParsePdf;
                    newspaper.ForceWebView = dialog.ForceWebView;

                    // 更新设置中的报纸库
                    var settings = SettingsManager.LoadSettings();
                    var newspaperInfo = settings.NewspaperLibrary.FirstOrDefault(n => n.Name == newspaper.Name);
                    if (newspaperInfo != null)
                    {
                        newspaperInfo.Url = dialog.NewspaperUrl;
                        newspaperInfo.TitleImagePath = dialog.TitleImagePath;
                        newspaperInfo.ParsePdf = dialog.ParsePdf;
                        newspaperInfo.ForceWebView = dialog.ForceWebView;
                        SettingsManager.SaveSettings(settings);
                    }
                }
            }
        }

        private void DeleteNewspaper(object? parameter)
        {
            if (parameter is Newspaper newspaper)
            {
                if (System.Windows.MessageBox.Show($"确定要删除报纸 {newspaper.Name} 吗？", "确认删除", System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Question) == System.Windows.MessageBoxResult.Yes)
                {
                    Newspapers.Remove(newspaper);

                    // 更新设置中的报纸库
                    var settings = SettingsManager.LoadSettings();
                    var newspaperInfo = settings.NewspaperLibrary.FirstOrDefault(n => n.Name == newspaper.Name);
                    if (newspaperInfo != null)
                    {
                        settings.NewspaperLibrary.Remove(newspaperInfo);
                        SettingsManager.SaveSettings(settings);
                    }
                }
            }
        }

        private void LoadNewspapersFromSettings()
        {
            // 重新加载设置
            var settings = SettingsManager.LoadSettings();
            string settingsDirectory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "esNewsPaperReader"
            );
            
            // 创建新的报纸列表
            var newNewspapers = settings.NewspaperLibrary.Select(n => 
            {
                string titleImagePath = n.TitleImagePath;
                // 如果是相对路径，转换为绝对路径
                if (!string.IsNullOrEmpty(titleImagePath) && !Path.IsPathRooted(titleImagePath))
                {
                    titleImagePath = Path.Combine(settingsDirectory, titleImagePath);
                }
                return new Newspaper(n.Name, n.Url, titleImagePath, n.ParsePdf, n.ForceWebView);
            }).ToList();
            
            // 确保更新Newspapers属性，触发PropertyChanged事件
            Newspapers = newNewspapers;
        }

        public event Action<string>? NavigateToUrl;
        public event Action<UIElementDisplayStrategy, NewspaperListDisplayMode>? ApplySettings;
        public event Action<FontSizeLevel>? FontSizeChanged;

        public static NewspaperListDisplayMode NewspaperListMode { get; private set; } = NewspaperListDisplayMode.TextList;
        private FontSizeLevel _currentFontSizeLevel = FontSizeLevel.Normal;
        public FontSizeLevel CurrentFontSizeLevel
        {
            get => _currentFontSizeLevel;
            set
            {
                _currentFontSizeLevel = value;
                OnPropertyChanged();
            }
        }

        public void LoadAppSettings()
        {
            var settings = SettingsManager.LoadSettings();
            NewspaperListMode = settings.NewspaperListDisplayMode;
            IsSidebarPinned = settings.IsSidebarPinned;
            SidebarWidth = settings.LeftSidebarWidth;
            CurrentFontSizeLevel = settings.FontSizeLevel;
            ApplySettings?.Invoke(settings.UIElementDisplayStrategy, settings.NewspaperListDisplayMode);
            FontSizeChanged?.Invoke(settings.FontSizeLevel);
        }

        private async Task LoadNewspaperEditionsAsync(Newspaper newspaper)
        {
            try
            {
                // 显示加载状态
                StatusText = $"正在加载 {newspaper.Name} 的版面信息...";
                
                // 清空之前的版面列表
                Editions = new List<Edition>();

                // 检查是否强制直接访问网页版
                if (newspaper.ForceWebView)
                {
                    StatusText = "正在直接打开网页版...";
                    // 直接用WebView2打开网页
                    NavigateToUrl?.Invoke(newspaper.Url);
                    return;
                }

                // 检查是否尝试解析PDF
                if (newspaper.ParsePdf)
                {
                    // 分析网页
                    var editions = await _webAnalyzer.AnalyzeNewspaperPage(newspaper.Url);
                    
                    if (editions.Count > 0)
                    {
                        newspaper.Editions.Clear();
                        foreach (var edition in editions)
                        {
                            newspaper.Editions.Add(new Edition(edition.Key, edition.Value));
                        }
                        
                        Editions = newspaper.Editions;
                        newspaper.IsLoaded = true;
                        
                        StatusText = $"成功加载 {editions.Count} 个版面，正在异步下载PDF文件...";
                        
                        // 异步下载所有PDF文件
                        _ = DownloadAllPdfsAsync(newspaper.Editions);
                        
                        // 自动载入第一版PDF
                        if (newspaper.Editions.Count > 0)
                        {
                            SelectedEdition = newspaper.Editions[0];
                            // 延迟清空状态文本，确保用户有足够时间看到加载成功的提示
                            System.Threading.Tasks.Task.Delay(500).ContinueWith(t =>
                            {
                                StatusText = string.Empty;
                            });
                        }
                    }
                    else
                    {
                        StatusText = "未找到PDF链接，正在打开网页版...";
                        // 直接用WebView2打开网页
                        NavigateToUrl?.Invoke(newspaper.Url);
                    }
                }
                else
                {
                    StatusText = "已跳过PDF解析，正在打开网页版...";
                    // 直接用WebView2打开网页
                    NavigateToUrl?.Invoke(newspaper.Url);
                }
            }
            catch (Exception ex)
            {
                StatusText = $"加载失败: {ex.Message}，正在打开网页版...";
                // 出错时也直接用WebView2打开网页
                NavigateToUrl?.Invoke(newspaper.Url);
            }
        }

        private async Task DownloadAllPdfsAsync(List<Edition> editions)
        {
            try
            {
                // 创建保存目录
                var dateFolder = DateTime.Now.ToString("yyyy-MM-dd");
                var newspaperFolder = SelectedNewspaper?.Name ?? "未知报纸";
                var savePath = Path.Combine(_pathRoot, dateFolder, newspaperFolder);
                Directory.CreateDirectory(savePath);

                // 并行下载所有PDF文件
                var downloadTasks = editions.Select(async edition =>
                {
                    if (!edition.IsDownloaded || !File.Exists(edition.LocalFilePath))
                    {
                        // 生成文件名
                        var invalidChars = Path.GetInvalidFileNameChars();
                        var fileName = edition.Title;
                        foreach (var c in invalidChars)
                        {
                            fileName = fileName.Replace(c, '_');
                        }
                        fileName += ".pdf";
                        var filePath = Path.Combine(savePath, fileName);

                        // 下载PDF
                        await DownloadPdfAsync(edition.PdfUrl, filePath);

                        // 更新状态
                        edition.LocalFilePath = filePath;
                        edition.IsDownloaded = true;
                    }
                });

                // 等待所有下载任务完成
                await Task.WhenAll(downloadTasks);

                // 更新状态
                StatusText = "就绪";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"下载PDF文件时出错: {ex.Message}");
                StatusText = "部分PDF文件下载失败";
            }
        }

        private async Task LoadPdfFileAsync(Edition edition)
        {
            try
            {
                // 检查是否已下载
                if (!edition.IsDownloaded || !File.Exists(edition.LocalFilePath))
                {
                    // 显示加载状态
                    StatusText = "正在下载PDF文件...";

                    // 创建保存目录
                    var dateFolder = DateTime.Now.ToString("yyyy-MM-dd");
                    var newspaperFolder = SelectedNewspaper?.Name ?? "未知报纸";
                    var savePath = Path.Combine(_pathRoot, dateFolder, newspaperFolder);
                    Directory.CreateDirectory(savePath);

                    // 生成文件名
                    var invalidChars = Path.GetInvalidFileNameChars();
                    var fileName = edition.Title;
                    foreach (var c in invalidChars)
                    {
                        fileName = fileName.Replace(c, '_');
                    }
                    fileName += ".pdf";
                    var filePath = Path.Combine(savePath, fileName);

                    // 下载PDF
                    await DownloadPdfAsync(edition.PdfUrl, filePath);

                    // 更新状态
                    edition.LocalFilePath = filePath;
                    edition.IsDownloaded = true;
                    
                    StatusText = "就绪";
                }

                // WebView2会自动处理PDF文档的加载，这里只需要确保文件已下载
                if (File.Exists(edition.LocalFilePath))
                {
                    StatusText = "PDF文档准备就绪";
                    // 更新当前页和总页数
                    CurrentPage = 1;
                    TotalPages = 1; // WebView2会自动处理页码，这里只是一个占位符
                }
            }
            catch (Exception ex)
            {
                StatusText = $"加载PDF失败: {ex.Message}";
            }
        }

        private async Task DownloadPdfAsync(string pdfUrl, string filePath)
        {
            using (var response = await _httpClient.GetAsync(pdfUrl))
            {
                response.EnsureSuccessStatusCode();
                using (var contentStream = await response.Content.ReadAsStreamAsync())
                {
                    using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.Read))
                    {
                        await contentStream.CopyToAsync(fileStream);
                    }
                }
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    // 简单的命令实现
    public class RelayCommand : ICommand
    {
        private readonly Action<object?> _execute;
        private readonly Func<object?, bool>? _canExecute;

        public event EventHandler? CanExecuteChanged;

        public RelayCommand(Action<object?> execute, Func<object?, bool>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object? parameter)
        {
            return _canExecute == null || _canExecute(parameter);
        }

        public void Execute(object? parameter)
        {
            _execute(parameter);
        }

        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}