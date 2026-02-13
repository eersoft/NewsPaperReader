using System.IO;

namespace NewsPaperReader
{
    public partial class AddNewspaperDialog : System.Windows.Window
    {
        public string NewspaperName { get; set; } = string.Empty;
        public string NewspaperUrl { get; set; } = string.Empty;
        public string TitleImagePath { get; set; } = string.Empty;
        public bool ParsePdf { get; set; } = true;
        public bool ForceWebView { get; set; } = false;
        public bool IsConfirmed { get; private set; }

        public AddNewspaperDialog()
        {
            InitializeComponent();
            DataContext = this;
            ParsePdf = true;
            ForceWebView = false;
        }

        public AddNewspaperDialog(string name, string url, string titleImagePath = "", bool parsePdf = true, bool forceWebView = false)
        {
            InitializeComponent();
            DataContext = this;
            NewspaperName = name;
            NewspaperUrl = url;
            TitleImagePath = titleImagePath;
            ParsePdf = parsePdf;
            ForceWebView = forceWebView;
        }

        private void BrowseButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "图片文件 (*.jpg;*.jpeg;*.png;*.gif)|*.jpg;*.jpeg;*.png;*.gif|所有文件 (*.*)|*.*",
                Title = "选择报纸标题图片"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                // 构建title_image文件夹路径
                string settingsDirectory = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "esNewsPaperReader"
                );
                string titleImageFolder = Path.Combine(settingsDirectory, "title_image");
                
                // 确保文件夹存在
                if (!Directory.Exists(titleImageFolder))
                {
                    Directory.CreateDirectory(titleImageFolder);
                }
                
                // 生成新的文件名，使用报纸名称作为基础
                string fileName = $"{NewspaperName}_{Path.GetFileName(openFileDialog.FileName)}";
                // 移除无效字符
                string invalidChars = new string(Path.GetInvalidFileNameChars());
                foreach (char c in invalidChars)
                {
                    fileName = fileName.Replace(c, '_');
                }
                
                // 构建目标路径
                string destPath = Path.Combine(titleImageFolder, fileName);
                
                // 复制文件
                File.Copy(openFileDialog.FileName, destPath, true);
                
                // 保存相对路径
                TitleImagePath = Path.Combine("title_image", fileName);
            }
        }

        private void OKButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NewspaperName))
            {
                System.Windows.MessageBox.Show("请输入报纸名称", "提示", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(NewspaperUrl))
            {
                System.Windows.MessageBox.Show("请输入报纸URL", "提示", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                return;
            }

            if (!NewspaperUrl.StartsWith("http://") && !NewspaperUrl.StartsWith("https://"))
            {
                NewspaperUrl = "https://" + NewspaperUrl;
            }

            IsConfirmed = true;
            DialogResult = true;
            Close();
        }

        private void ParsePdfRadioButton_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            ParsePdf = true;
            ForceWebView = false;
        }

        private void ParsePdfRadioButton_Unchecked(object sender, System.Windows.RoutedEventArgs e)
        {
            // 当取消选择解析PDF时，如果直接访问网页版也未被选择，则默认选择直接访问网页版
            if (!ForceWebView)
            {
                ForceWebView = true;
            }
        }

        private void ForceWebViewRadioButton_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            ForceWebView = true;
            ParsePdf = false;
        }

        private void ForceWebViewRadioButton_Unchecked(object sender, System.Windows.RoutedEventArgs e)
        {
            // 当取消选择直接访问网页版时，如果解析PDF也未被选择，则默认选择解析PDF
            if (!ParsePdf)
            {
                ParsePdf = true;
            }
        }

        private void CancelButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            IsConfirmed = false;
            DialogResult = false;
            Close();
        }
    }
}