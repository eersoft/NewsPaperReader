using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Forms;
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
        public RelayCommand ImportNewspaperLibraryCommand { get; }
        public RelayCommand ExportNewspaperLibraryCommand { get; }

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
            ImportNewspaperLibraryCommand = new RelayCommand(ImportNewspaperLibrary);
            ExportNewspaperLibraryCommand = new RelayCommand(ExportNewspaperLibrary);
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
                
                // 创建一个新的集合引用，强制UI更新
                var updatedLibrary = new List<NewspaperInfo>(Settings.NewspaperLibrary);
                Settings.NewspaperLibrary = updatedLibrary;
                
                // 保存设置并触发设置更改事件，确保主窗口实时更新
                SettingsManager.SaveSettings(Settings);
                SettingsChanged?.Invoke(Settings);
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
                    
                    // 保存设置并触发设置更改事件，确保主窗口实时更新
                    SettingsManager.SaveSettings(Settings);
                    SettingsChanged?.Invoke(Settings);
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
                    // 确保触发事件
                    if (SettingsChanged != null)
                    {
                        SettingsChanged(Settings);
                    }
                }
            }
        }
        
        private void ExportNewspaperLibrary(object? parameter)
        {
            // 打开文件夹选择对话框，让用户选择导出位置
            var folderDialog = new System.Windows.Forms.FolderBrowserDialog();
            folderDialog.Description = "选择导出位置";
            folderDialog.ShowNewFolderButton = true;
            
            if (folderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                try
                {
                    // 创建心仪读报报纸库文件夹
                    string exportRoot = Path.Combine(folderDialog.SelectedPath, "心仪读报报纸库");
                    if (!Directory.Exists(exportRoot))
                    {
                        Directory.CreateDirectory(exportRoot);
                    }
                    
                    // 导出报纸库信息到json文件
                    string jsonPath = Path.Combine(exportRoot, "newspaper_library.json");
                    string json = System.Text.Json.JsonSerializer.Serialize(Settings.NewspaperLibrary, new System.Text.Json.JsonSerializerOptions
                    {
                        WriteIndented = true
                    });
                    File.WriteAllText(jsonPath, json);
                    
                    // 复制标题图片文件夹
                    string settingsDirectory = Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                        "esNewsPaperReader"
                    );
                    string imagesFolder = Path.Combine(settingsDirectory, "title_image");
                    string exportImagesFolder = Path.Combine(exportRoot, "title_image");
                    
                    if (Directory.Exists(imagesFolder))
                    {
                        if (Directory.Exists(exportImagesFolder))
                        {
                            // 如果目标文件夹存在，先删除
                            Directory.Delete(exportImagesFolder, true);
                        }
                        // 复制整个图片文件夹
                        DirectoryCopy(imagesFolder, exportImagesFolder, true);
                    }
                    
                    // 提示用户
                    System.Windows.MessageBox.Show(
                        "导出成功！\n\n请保存或分享整个'心仪读报报纸库'文件夹。",
                        "导出成功",
                        System.Windows.MessageBoxButton.OK,
                        System.Windows.MessageBoxImage.Information
                    );
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show(
                        $"导出失败：{ex.Message}",
                        "导出失败",
                        System.Windows.MessageBoxButton.OK,
                        System.Windows.MessageBoxImage.Error
                    );
                }
            }
        }
        
        private void ImportNewspaperLibrary(object? parameter)
        {
            // 打开文件夹选择对话框，让用户选择心仪读报报纸库文件夹
            var folderDialog = new System.Windows.Forms.FolderBrowserDialog();
            folderDialog.Description = "选择心仪读报报纸库文件夹";
            folderDialog.ShowNewFolderButton = false;
            
            if (folderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                try
                {
                    // 检查选择的文件夹是否包含必要的文件
                    string jsonPath = Path.Combine(folderDialog.SelectedPath, "newspaper_library.json");
                    if (!File.Exists(jsonPath))
                    {
                        System.Windows.MessageBox.Show(
                            "所选文件夹不是有效的心仪读报报纸库文件夹，缺少newspaper_library.json文件。",
                            "导入失败",
                            System.Windows.MessageBoxButton.OK,
                            System.Windows.MessageBoxImage.Error
                        );
                        return;
                    }
                    
                    // 读取json文件
                    string json = File.ReadAllText(jsonPath);
                    var importedNewspapers = System.Text.Json.JsonSerializer.Deserialize<List<NewspaperInfo>>(json);
                    
                    if (importedNewspapers == null || importedNewspapers.Count == 0)
                    {
                        System.Windows.MessageBox.Show(
                            "导入的报纸库为空。",
                            "导入失败",
                            System.Windows.MessageBoxButton.OK,
                            System.Windows.MessageBoxImage.Error
                        );
                        return;
                    }
                    
                    // 准备路径信息
                    string importImagesFolder = Path.Combine(folderDialog.SelectedPath, "title_image");
                    string settingsDirectory = Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                        "esNewsPaperReader"
                    );
                    string targetImagesFolder = Path.Combine(settingsDirectory, "title_image");
                    
                    if (!Directory.Exists(targetImagesFolder))
                    {
                        Directory.CreateDirectory(targetImagesFolder);
                    }
                    
                    // 添加报纸信息到当前报纸库
                    foreach (var newspaper in importedNewspapers)
                    {
                        // 检查是否已存在同名报纸
                        var existing = Settings.NewspaperLibrary.FirstOrDefault(n => n.Name == newspaper.Name);
                        if (existing == null)
                        {
                            // 不存在同名报纸，直接添加
                            Settings.NewspaperLibrary.Add(newspaper);
                            
                            // 复制对应的标题图片
                            if (!string.IsNullOrEmpty(newspaper.TitleImagePath) && Directory.Exists(importImagesFolder))
                            {
                                string fileName = Path.GetFileName(newspaper.TitleImagePath);
                                string sourcePath = Path.Combine(importImagesFolder, fileName);
                                string targetPath = Path.Combine(targetImagesFolder, fileName);
                                
                                if (File.Exists(sourcePath))
                                {
                                    // 尝试复制文件，如果文件正在被使用，使用重试机制
                                bool copied = false;
                                int retryCount = 0;
                                int maxRetries = 3;
                                
                                while (!copied && retryCount < maxRetries)
                                {
                                    try
                                    {
                                        // 先尝试删除目标文件（如果存在）
                                        if (File.Exists(targetPath))
                                        {
                                            File.Delete(targetPath);
                                        }
                                        // 然后复制新文件
                                        File.Copy(sourcePath, targetPath, true);
                                        copied = true;
                                    }
                                    catch (IOException)
                                    {
                                        // 文件正在被使用，等待一段时间后重试
                                        retryCount++;
                                        System.Threading.Thread.Sleep(100);
                                    }
                                }
                                
                                if (!copied)
                                {
                                    // 如果重试后仍然失败，尝试使用另一种方法：先复制到临时文件，然后替换
                                    string tempPath = targetPath + ".tmp";
                                    try
                                    {
                                        File.Copy(sourcePath, tempPath, true);
                                        if (File.Exists(targetPath))
                                        {
                                            File.Delete(targetPath);
                                        }
                                        File.Move(tempPath, targetPath);
                                    }
                                    catch (Exception)
                                    {
                                        // 如果仍然失败，忽略此文件，继续处理其他文件
                                        if (File.Exists(tempPath))
                                        {
                                            File.Delete(tempPath);
                                        }
                                    }
                                }
                                }
                            }
                        }
                        else
                        {
                            // 存在同名报纸，询问用户是否替换
                            var result = System.Windows.MessageBox.Show(
                                $"报纸 '{newspaper.Name}' 已存在，是否替换？",
                                "报纸冲突",
                                System.Windows.MessageBoxButton.YesNo,
                                System.Windows.MessageBoxImage.Question
                            );
                            
                            if (result == System.Windows.MessageBoxResult.Yes)
                            {
                                // 用户选择替换，更新信息
                                existing.Url = newspaper.Url;
                                existing.TitleImagePath = newspaper.TitleImagePath;
                                existing.ParsePdf = newspaper.ParsePdf;
                                existing.ForceWebView = newspaper.ForceWebView;
                                
                                // 复制对应的标题图片，不需要提示和询问
                                if (!string.IsNullOrEmpty(newspaper.TitleImagePath) && Directory.Exists(importImagesFolder))
                                {
                                    string fileName = Path.GetFileName(newspaper.TitleImagePath);
                                    string sourcePath = Path.Combine(importImagesFolder, fileName);
                                    string targetPath = Path.Combine(targetImagesFolder, fileName);
                                    
                                    if (File.Exists(sourcePath))
                                    {
                                        // 尝试复制文件，如果文件正在被使用，使用重试机制
                                        bool copied = false;
                                        int retryCount = 0;
                                        int maxRetries = 3;
                                        
                                        while (!copied && retryCount < maxRetries)
                                        {
                                            try
                                            {
                                                // 先尝试删除目标文件（如果存在）
                                                if (File.Exists(targetPath))
                                                {
                                                    File.Delete(targetPath);
                                                }
                                                // 然后复制新文件
                                                File.Copy(sourcePath, targetPath, true);
                                                copied = true;
                                            }
                                            catch (IOException)
                                            {
                                                // 文件正在被使用，等待一段时间后重试
                                                retryCount++;
                                                System.Threading.Thread.Sleep(100);
                                            }
                                        }
                                        
                                        if (!copied)
                                        {
                                            // 如果重试后仍然失败，尝试使用另一种方法：先复制到临时文件，然后替换
                                            string tempPath = targetPath + ".tmp";
                                            try
                                            {
                                                File.Copy(sourcePath, tempPath, true);
                                                if (File.Exists(targetPath))
                                                {
                                                    File.Delete(targetPath);
                                                }
                                                File.Move(tempPath, targetPath);
                                            }
                                            catch (Exception)
                                            {
                                                // 如果仍然失败，忽略此文件，继续处理其他文件
                                                if (File.Exists(tempPath))
                                                {
                                                    File.Delete(tempPath);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            // 用户选择不替换，跳过该报纸，不复制标题图片
                        }
                    }
                    
                    // 创建一个新的集合引用，强制UI更新
                    var updatedLibrary = new List<NewspaperInfo>(Settings.NewspaperLibrary);
                    Settings.NewspaperLibrary = updatedLibrary;
                    
                    // 保存设置并触发设置更改事件
                    SettingsManager.SaveSettings(Settings);
                    SettingsChanged?.Invoke(Settings);
                    
                    // 提示用户
                    System.Windows.MessageBox.Show(
                        "导入成功！\n\n已将报纸库添加到当前设置中。",
                        "导入成功",
                        System.Windows.MessageBoxButton.OK,
                        System.Windows.MessageBoxImage.Information
                    );
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show(
                        $"导入失败：{ex.Message}",
                        "导入失败",
                        System.Windows.MessageBoxButton.OK,
                        System.Windows.MessageBoxImage.Error
                    );
                }
            }
        }
        
        // 辅助方法：复制文件夹
        private void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
        {
            // 获取源目录的信息
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);
            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "源目录不存在或无法访问: "
                    + sourceDirName);
            }
            
            // 获取目标目录的信息
            DirectoryInfo[] dirs = dir.GetDirectories();
            
            // 如果目标目录不存在，则创建
            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }
            
            // 复制所有文件
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string tempPath = Path.Combine(destDirName, file.Name);
                file.CopyTo(tempPath, true);
            }
            
            // 复制所有子目录
            if (copySubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    string tempPath = Path.Combine(destDirName, subdir.Name);
                    DirectoryCopy(subdir.FullName, tempPath, copySubDirs);
                }
            }
        }
        
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
