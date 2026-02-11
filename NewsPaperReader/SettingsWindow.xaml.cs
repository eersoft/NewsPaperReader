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

            // 初始化命令
            OKCommand = new RelayCommand(OK);
            CancelCommand = new RelayCommand(Cancel);
            AddNewspaperInfoCommand = new RelayCommand(AddNewspaperInfo);
            EditNewspaperInfoCommand = new RelayCommand(EditNewspaperInfo);
            DeleteNewspaperInfoCommand = new RelayCommand(DeleteNewspaperInfo);
        }

        private void OK(object? parameter)
        {
            // 保存设置
            SettingsManager.SaveSettings(Settings);
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
                var newspaperInfo = new NewspaperInfo(dialog.NewspaperName, dialog.NewspaperUrl, dialog.TitleImagePath);
                Settings.NewspaperLibrary.Add(newspaperInfo);
            }
        }

        private void EditNewspaperInfo(object? parameter)
        {
            if (SelectedNewspaperInfo != null)
            {
                // 打开编辑报纸对话框
                var dialog = new AddNewspaperDialog(SelectedNewspaperInfo.Name, SelectedNewspaperInfo.Url, SelectedNewspaperInfo.TitleImagePath);
                if (dialog.ShowDialog() == true)
                {
                    SelectedNewspaperInfo.Name = dialog.NewspaperName;
                    SelectedNewspaperInfo.Url = dialog.NewspaperUrl;
                    SelectedNewspaperInfo.TitleImagePath = dialog.TitleImagePath;
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
