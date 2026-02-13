using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using NewsPaperReader.Models;
using NewsPaperReader.Services;

namespace NewsPaperReader
{
    public partial class SettingsWindow : Window
    {
        public SettingsViewModel ViewModel { get; set; }

        public SettingsWindow()
        {
            InitializeComponent();
            ViewModel = new SettingsViewModel(this);
            DataContext = ViewModel;
        }

        private void ListBox_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (ViewModel.SelectedNewspaperInfo != null)
            {
                ViewModel.EditNewspaperInfoCommand.Execute(null);
            }
        }
    }

    public class SettingsViewModel : INotifyPropertyChanged
    {
        private readonly SettingsWindow _window;
        public AppSettings Settings { get; set; }
        
        private NewspaperInfo? _selectedNewspaperInfo;
        public NewspaperInfo? SelectedNewspaperInfo 
        {
            get => _selectedNewspaperInfo;
            set
            {
                _selectedNewspaperInfo = value;
                OnPropertyChanged();
            }
        }
        public event Action<AppSettings>? SettingsChanged;
        public event PropertyChangedEventHandler? PropertyChanged;

        // 命令
        public RelayCommand OKCommand { get; }
        public RelayCommand CancelCommand { get; }
        public RelayCommand AddNewspaperInfoCommand { get; }
        public RelayCommand EditNewspaperInfoCommand { get; }
        public RelayCommand DeleteNewspaperInfoCommand { get; }

        public SettingsViewModel(SettingsWindow window)
        {
            _window = window;
            // 加载设置
            Settings = SettingsManager.LoadSettings();
            
            // 转换标题图片路径为绝对路径
            string settingsDirectory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "esNewsPaperReader"
            );
            
            foreach (var newspaperInfo in Settings.NewspaperLibrary)
            {
                string titleImagePath = newspaperInfo.TitleImagePath;
                // 如果是相对路径，转换为绝对路径
                if (!string.IsNullOrEmpty(titleImagePath) && !Path.IsPathRooted(titleImagePath))
                {
                    newspaperInfo.TitleImagePath = Path.Combine(settingsDirectory, titleImagePath);
                }
            }

            // 初始化命令
            OKCommand = new RelayCommand(OK);
            CancelCommand = new RelayCommand(Cancel);
            AddNewspaperInfoCommand = new RelayCommand(AddNewspaperInfo);
            EditNewspaperInfoCommand = new RelayCommand(EditNewspaperInfo);
            DeleteNewspaperInfoCommand = new RelayCommand(DeleteNewspaperInfo);
        }

        private void OK(object? parameter)
        {
            // 转换标题图片路径为相对路径
            string settingsDirectory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "esNewsPaperReader"
            );
            
            foreach (var newspaperInfo in Settings.NewspaperLibrary)
            {
                string titleImagePath = newspaperInfo.TitleImagePath;
                // 如果是绝对路径，转换为相对路径
                if (!string.IsNullOrEmpty(titleImagePath) && Path.IsPathRooted(titleImagePath))
                {
                    if (titleImagePath.StartsWith(settingsDirectory))
                    {
                        newspaperInfo.TitleImagePath = titleImagePath.Substring(settingsDirectory.Length + 1);
                    }
                }
            }
            
            // 保存设置
            SettingsManager.SaveSettings(Settings);
            
            // 触发设置更改事件
            SettingsChanged?.Invoke(Settings);
            _window.DialogResult = true;
            _window.Close();
        }

        private void Cancel(object? parameter)
        {
            _window.DialogResult = false;
            _window.Close();
        }

        private void AddNewspaperInfo(object? parameter)
        {
            // 打开添加报纸对话框
            var dialog = new AddNewspaperDialog();
            if (dialog.ShowDialog() == true)
            {
                var newspaperInfo = new NewspaperInfo(dialog.NewspaperName, dialog.NewspaperUrl, dialog.TitleImagePath, dialog.ParsePdf, dialog.ForceWebView);
                Settings.NewspaperLibrary.Add(newspaperInfo);
            }
        }

        private void EditNewspaperInfo(object? parameter)
        {
            var newspaperInfo = parameter as NewspaperInfo ?? SelectedNewspaperInfo;
            if (newspaperInfo != null)
            {
                // 打开编辑报纸对话框
                var dialog = new AddNewspaperDialog(newspaperInfo.Name, newspaperInfo.Url, newspaperInfo.TitleImagePath, newspaperInfo.ParsePdf, newspaperInfo.ForceWebView);
                if (dialog.ShowDialog() == true)
                {
                    newspaperInfo.Name = dialog.NewspaperName;
                    newspaperInfo.Url = dialog.NewspaperUrl;
                    newspaperInfo.TitleImagePath = dialog.TitleImagePath;
                    newspaperInfo.ParsePdf = dialog.ParsePdf;
                    newspaperInfo.ForceWebView = dialog.ForceWebView;
                }
            }
        }

        private void DeleteNewspaperInfo(object? parameter)
        {
            var newspaperInfo = parameter as NewspaperInfo ?? SelectedNewspaperInfo;
            if (newspaperInfo != null)
            {
                // 显示确认对话框
                var result = System.Windows.MessageBox.Show(
                    $"确定要删除报纸 '{newspaperInfo.Name}' 吗？",
                    "确认删除",
                    System.Windows.MessageBoxButton.YesNo,
                    System.Windows.MessageBoxImage.Question
                );
                
                if (result == System.Windows.MessageBoxResult.Yes)
                {
                    // 从集合中移除
                    Settings.NewspaperLibrary.Remove(newspaperInfo);
                    
                    // 创建一个新的集合引用，强制UI更新
                    var updatedLibrary = new List<NewspaperInfo>(Settings.NewspaperLibrary);
                    Settings.NewspaperLibrary = updatedLibrary;
                    
                    // 清除选择
                    SelectedNewspaperInfo = null;
                    
                    // 保存设置并触发设置更改事件，确保主窗口实时更新
                    SettingsManager.SaveSettings(Settings);
                    SettingsChanged?.Invoke(Settings);
                }
            }
        }
        
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
