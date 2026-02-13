using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace NewsPaperReader.Models
{
    public class Newspaper
    {
        /// <summary>
        /// 报纸名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 报纸URL
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// 报纸标题图片路径
        /// </summary>
        public string TitleImagePath { get; set; }

        /// <summary>
        /// 版面列表
        /// </summary>
        public List<Edition> Editions { get; set; }

        /// <summary>
        /// 是否已加载
        /// </summary>
        public bool IsLoaded { get; set; }

        /// <summary>
        /// 是否尝试解析PDF（默认：true）
        /// </summary>
        public bool ParsePdf { get; set; }

        /// <summary>
        /// 是否强制直接访问网页版（默认：false）
        /// </summary>
        public bool ForceWebView { get; set; }

        public Newspaper()
        {
            Name = string.Empty;
            Url = string.Empty;
            TitleImagePath = string.Empty;
            Editions = new List<Edition>();
            IsLoaded = false;
            ParsePdf = true;
            ForceWebView = false;
        }

        public Newspaper(string name, string url, string titleImagePath = "", bool parsePdf = true, bool forceWebView = false)
        {
            Name = name;
            Url = url;
            TitleImagePath = titleImagePath;
            Editions = new List<Edition>();
            IsLoaded = false;
            ParsePdf = parsePdf;
            ForceWebView = forceWebView;
        }
    }

    public class Edition : INotifyPropertyChanged
    {
        /// <summary>
        /// 版面标题
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// PDF链接
        /// </summary>
        public string PdfUrl { get; set; }

        private string _localFilePath;
        /// <summary>
        /// 本地PDF文件路径
        /// </summary>
        public string LocalFilePath
        {
            get => _localFilePath;
            set
            {
                _localFilePath = value;
                OnPropertyChanged();
            }
        }

        private bool _isDownloaded;
        /// <summary>
        /// 是否已下载
        /// </summary>
        public bool IsDownloaded
        {
            get => _isDownloaded;
            set
            {
                _isDownloaded = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(DownloadStatus));
            }
        }

        /// <summary>
        /// 下载状态
        /// </summary>
        public string DownloadStatus
        {
            get
            {
                if (IsDownloaded)
                {
                    return "已下载";
                }
                return "未下载";
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public Edition()
        {
            Title = string.Empty;
            PdfUrl = string.Empty;
            _localFilePath = string.Empty;
            _isDownloaded = false;
        }

        public Edition(string title, string pdfUrl)
        {
            Title = title;
            PdfUrl = pdfUrl;
            _localFilePath = string.Empty;
            _isDownloaded = false;
        }

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}