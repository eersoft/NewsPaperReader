using System;
using System.Windows;
using PaperReader.ViewModels;
using PaperReader.Models;

namespace PaperReader.Views
{
    public partial class MainWindow : Window
    {
        private MainWindowViewModel _viewModel;

        public MainWindow()
        {
            try
            {
                InitializeComponent();
                _viewModel = DataContext as MainWindowViewModel;
                
                if (_viewModel == null)
                {
                    throw new Exception("Failed to initialize ViewModel");
                }
                
                // 监听SelectedEdition变化
                _viewModel.SelectedEditionChanged += OnSelectedEditionChanged;
                
                // 设置初始状态消息
                _viewModel.StatusMessage = "PaperReader initialized successfully";
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Error initializing MainWindow: " + ex.Message, "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                // 即使初始化失败，也要显示窗口
            }
        }

        // 处理SelectedEdition更改事件
        private void OnSelectedEditionChanged(object sender, EventArgs e)
        {
            try
            {
                var edition = _viewModel.SelectedEdition;
                if (edition != null)
                {
                    _viewModel.StatusMessage = "Selected edition: " + edition.Title;
                }
            }
            catch (Exception ex)
            {
                _viewModel.StatusMessage = "Error selecting edition: " + ex.Message;
            }
        }
    }
}