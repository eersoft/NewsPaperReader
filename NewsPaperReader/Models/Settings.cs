using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace NewsPaperReader.Models
{
    /// <summary>
    /// 界面元素显示策略
    /// </summary>
    public enum UIElementDisplayStrategy
    {
        AlwaysShow,
        AutoHide
    }

    /// <summary>
    /// 内容显示模式
    /// </summary>
    public enum ContentDisplayMode
    {
        FitWidth,
        FitHeight,
        FitPage
    }

    /// <summary>
    /// 报纸列表显示模式
    /// </summary>
    public enum NewspaperListDisplayMode
    {
        TextList,
        ImageList,
        ImageTile
    }

    /// <summary>
    /// 报纸信息
    /// </summary>
    public class NewspaperInfo : INotifyPropertyChanged
    {
        private string _name;
        /// <summary>
        /// 报纸名称
        /// </summary>
        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                OnPropertyChanged();
            }
        }

        private string _url;
        /// <summary>
        /// 报纸URL
        /// </summary>
        public string Url
        {
            get => _url;
            set
            {
                _url = value;
                OnPropertyChanged();
            }
        }

        private string _titleImagePath;
        /// <summary>
        /// 报纸标题图片路径
        /// </summary>
        public string TitleImagePath
        {
            get => _titleImagePath;
            set
            {
                _titleImagePath = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public NewspaperInfo()
        {
            _name = string.Empty;
            _url = string.Empty;
            _titleImagePath = string.Empty;
        }

        public NewspaperInfo(string name, string url, string titleImagePath = "")
        {
            _name = name;
            _url = url;
            _titleImagePath = titleImagePath;
        }

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    /// <summary>
    /// 应用程序设置
    /// </summary>
    public class AppSettings : INotifyPropertyChanged
    {
        // 界面元素显示策略
        private UIElementDisplayStrategy _uiElementDisplayStrategy = UIElementDisplayStrategy.AlwaysShow;
        public UIElementDisplayStrategy UIElementDisplayStrategy
        {
            get => _uiElementDisplayStrategy;
            set
            {
                _uiElementDisplayStrategy = value;
                OnPropertyChanged();
            }
        }

        // 内容显示区默认显示模式
        private ContentDisplayMode _contentDisplayMode = ContentDisplayMode.FitWidth;
        public ContentDisplayMode ContentDisplayMode
        {
            get => _contentDisplayMode;
            set
            {
                _contentDisplayMode = value;
                OnPropertyChanged();
            }
        }

        // 启动后是否自动全屏
        private bool _autoFullScreenOnStartup = false;
        public bool AutoFullScreenOnStartup
        {
            get => _autoFullScreenOnStartup;
            set
            {
                _autoFullScreenOnStartup = value;
                OnPropertyChanged();
            }
        }

        // 报纸列表显示模式
        private NewspaperListDisplayMode _newspaperListDisplayMode = NewspaperListDisplayMode.TextList;
        public NewspaperListDisplayMode NewspaperListDisplayMode
        {
            get => _newspaperListDisplayMode;
            set
            {
                _newspaperListDisplayMode = value;
                OnPropertyChanged();
            }
        }

        // 报纸库
        private List<NewspaperInfo> _newspaperLibrary = new List<NewspaperInfo>();
        public List<NewspaperInfo> NewspaperLibrary
        {
            get => _newspaperLibrary;
            set
            {
                _newspaperLibrary = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public AppSettings()
        {
            // 初始化默认报纸库
            _newspaperLibrary = new List<NewspaperInfo>
            {
                new NewspaperInfo("人民日报", "https://paper.people.com.cn/rmrb/paperindex.htm"),
                new NewspaperInfo("讽刺与幽默", "https://paper.people.com.cn/fcyym/paperindex.htm"),
                new NewspaperInfo("松江报", "https://www.songjiangbao.com.cn/"),
                new NewspaperInfo("华西都市报", "https://www.wccdaily.com.cn/"),
                new NewspaperInfo("中国教师报", "https://www.chinateacher.com.cn/"),
                new NewspaperInfo("证券日报", "https://www.zqrb.cn/"),
                new NewspaperInfo("学习时报", "https://www.studytimes.cn/"),
                new NewspaperInfo("经济日报", "https://www.ce.cn/"),
                new NewspaperInfo("三峡都市报", "https://www.sxdsb.com.cn/"),
                new NewspaperInfo("重庆日报", "https://www.cqrb.cn/"),
                new NewspaperInfo("中国国防报", "https://www.81.cn/"),
                new NewspaperInfo("科技日报", "https://www.stdaily.com/")
            };
        }

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
