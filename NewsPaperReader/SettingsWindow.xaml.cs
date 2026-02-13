using System.IO;
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
    }

    public class SettingsViewModel
    {
        private readonly SettingsWindow _window;
        public AppSettings Settings { get; set; }
        public NewspaperInfo? SelectedNewspaperInfo { get; set; }
        public event Action<AppSettings>? SettingsChanged;

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
            if (SelectedNewspaperInfo != null)
            {
                // 打开编辑报纸对话框
                var dialog = new AddNewspaperDialog(SelectedNewspaperInfo.Name, SelectedNewspaperInfo.Url, SelectedNewspaperInfo.TitleImagePath, SelectedNewspaperInfo.ParsePdf, SelectedNewspaperInfo.ForceWebView);
                if (dialog.ShowDialog() == true)
                {
                    SelectedNewspaperInfo.Name = dialog.NewspaperName;
                    SelectedNewspaperInfo.Url = dialog.NewspaperUrl;
                    SelectedNewspaperInfo.TitleImagePath = dialog.TitleImagePath;
                    SelectedNewspaperInfo.ParsePdf = dialog.ParsePdf;
                    SelectedNewspaperInfo.ForceWebView = dialog.ForceWebView;
                }
            }
        }

        private void DeleteNewspaperInfo(object? parameter)
        {
            if (SelectedNewspaperInfo != null)
            {
                Settings.NewspaperLibrary.Remove(SelectedNewspaperInfo);
            }
        }
    }
}
