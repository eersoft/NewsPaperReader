namespace NewsPaperReader
{
    public partial class AddNewspaperDialog : System.Windows.Window
    {
        public string NewspaperName { get; set; } = string.Empty;
        public string NewspaperUrl { get; set; } = string.Empty;
        public bool IsConfirmed { get; private set; }

        public AddNewspaperDialog()
        {
            InitializeComponent();
            DataContext = this;
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
            Close();
        }

        private void CancelButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            IsConfirmed = false;
            Close();
        }
    }
}