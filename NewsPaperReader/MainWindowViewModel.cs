using NewsPaperReader.Models;
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
        public ICommand FullScreenCommand { get; }
        public ICommand AboutCommand { get; }
        public ICommand PreviousPageCommand { get; }
        public ICommand NextPageCommand { get; }
        public ICommand ZoomInCommand { get; }
        public ICommand ZoomOutCommand { get; }

        // 属性
        private List<Newspaper> _newspapers = new List<Newspaper>();
        public List<Newspaper> Newspapers
        {
            get => _newspapers;
            set
            {
                _newspapers = value;
                OnPropertyChanged();
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
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public MainWindowViewModel()
        {
            _webAnalyzer = new WebAnalyzer();
            _httpClient = new HttpClient();
            _pathRoot = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "在线报纸");
            
            // 初始化命令
            AddNewspaperCommand = new RelayCommand(AddNewspaper);
            RefreshCommand = new RelayCommand(Refresh);
            FullScreenCommand = new RelayCommand(FullScreen);
            AboutCommand = new RelayCommand(About);
            PreviousPageCommand = new RelayCommand(PreviousPage);
            NextPageCommand = new RelayCommand(NextPage);
            ZoomInCommand = new RelayCommand(ZoomIn);
            ZoomOutCommand = new RelayCommand(ZoomOut);
            
            // 初始化数据
            Newspapers = new List<Newspaper>
            {
                new Newspaper("人民日报", "https://paper.people.com.cn/rmrb/paperindex.htm"),
                new Newspaper("讽刺与幽默", "https://paper.people.com.cn/fcyym/paperindex.htm"),
                new Newspaper("松江报", "https://www.songjiangbao.com.cn/"),
                new Newspaper("华西都市报", "https://www.wccdaily.com.cn/"),
                new Newspaper("中国教师报", "https://www.chinateacher.com.cn/"),
                new Newspaper("证券日报", "https://www.zqrb.cn/"),
                new Newspaper("学习时报", "https://www.studytimes.cn/"),
                new Newspaper("经济日报", "https://www.ce.cn/"),
                new Newspaper("三峡都市报", "https://www.sxdsb.com.cn/"),
                new Newspaper("重庆日报", "https://www.cqrb.cn/"),
                new Newspaper("中国国防报", "https://www.81.cn/"),
                new Newspaper("科技日报", "https://www.stdaily.com/")
            };
            
            Editions = new List<Edition>();
        }

        private void AddNewspaper(object? parameter)
        {
            var dialog = new AddNewspaperDialog();
            if (dialog.ShowDialog() == true && dialog.IsConfirmed)
            {
                var newspaper = new Newspaper(dialog.NewspaperName, dialog.NewspaperUrl);
                Newspapers.Add(newspaper);
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

        private void FullScreen(object? parameter)
        {
            // 实现全屏功能
        }

        private void About(object? parameter)
        {
            System.Windows.MessageBox.Show("在线报纸阅读器 v1.0\n\n一个智能分析和阅读在线报纸的工具", "关于", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
        }

        private void PreviousPage(object? parameter)
        {
            if (CurrentPage > 1)
            {
                CurrentPage--;
            }
        }

        private void NextPage(object? parameter)
        {
            if (CurrentPage < TotalPages)
            {
                CurrentPage++;
            }
        }

        private void ZoomIn(object? parameter)
        {
            // 实现放大功能
        }

        private void ZoomOut(object? parameter)
        {
            // 实现缩小功能
        }

        private async Task LoadNewspaperEditionsAsync(Newspaper newspaper)
        {
            try
            {
                // 显示加载状态
                StatusText = $"正在加载 {newspaper.Name} 的版面信息...";

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
                    }
                }
                else
                {
                    StatusText = "未找到PDF链接，请检查URL是否正确";
                }
            }
            catch (Exception ex)
            {
                StatusText = $"加载失败: {ex.Message}";
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
                StatusText = $"所有PDF文件下载完成";
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
                    
                    StatusText = "PDF文件下载完成";
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