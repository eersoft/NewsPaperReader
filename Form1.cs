using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using PdfiumViewer;
using System.Net.Http;
using HtmlAgilityPack;
using HtmlDocument = HtmlAgilityPack.HtmlDocument;

using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms.VisualStyles;

namespace NewsPaperReader
{
    public partial class Form1 : Form
    {
        [DllImport("user32.dll")]
        [return:  MarshalAs(UnmanagedType.Bool)]
        static extern bool GetCursorPos(out Point lpPoint);

        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
        [DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        [DllImport("user32.dll")]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        private const int GWL_STYLE = -16;
        private const int WS_POPUP = 0x800000;

        private const int HOT_AREA_SIZE = 20; //自动显示工具栏的热区大小

        private HttpClient client=new HttpClient();
        private string mainHtml = "";
        private Dictionary<string,string> urlsPages = new Dictionary<string,string>();//版面URL
        private Dictionary<string,string> urlsPdfs = new Dictionary<string,string>();//版面pdfURL
        private Dictionary<string, string> pdfs = new Dictionary<string,string>();//已下载版面的pdf文件
        private string pathRoot = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop),"在线报纸"); //保存PDF文件的根目录,根/日期/报纸名称/*.pdf
        private string newsPaperName = "人民日报";
        private string baseUrl = null; //页面URL作为baseUrl（有些页面地址会重定向，不能用第一个链接）
        private int window_w,window_h,window_left,window_top; //窗口尺寸及位置
        private bool isLoading = false;
        private bool isReading = false;

        //工厂模式新代码
        private INewsPaper newsPaper;


        //工厂模式代码结束

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //Pass
        }
      
        /// <summary>
        /// 载入PDF显示
        /// </summary>
        /// <param name="filePath"></param>
        private void LoadPdfFile(string filePath)
        {
            var pdfDocument = PdfDocument.Load(filePath);
            MainViewer.Document = pdfDocument;
            isLoading = false;
            isReading = true;
        }

        /// <summary>
        /// 旋转页面
        /// </summary>
        private void RRight90()
        {
            if (MainViewer.Document != null)
            {
                PdfRotation pdfRotation = new PdfRotation();
                switch (MainViewer.Renderer.Rotation)
                {
                    case PdfRotation.Rotate0:
                        pdfRotation = PdfRotation.Rotate90;
                        break;
                    case PdfRotation.Rotate90:
                        pdfRotation = PdfRotation.Rotate180;
                        break;
                    case PdfRotation.Rotate180:
                        pdfRotation = PdfRotation.Rotate270;
                        break;
                    case PdfRotation.Rotate270:
                        pdfRotation = PdfRotation.Rotate0;
                        break;
                    default:
                        pdfRotation = PdfRotation.Rotate0;
                        break;
                }
                MainViewer.Renderer.Rotation = pdfRotation;
            }
        }

        /// <summary>
        /// 旋转页面
        /// </summary>
        private void RLeft90()
        {
            if (MainViewer.Document != null)
            {
                PdfRotation pdfRotation = new PdfRotation();
                switch (MainViewer.Renderer.Rotation)
                {
                    case PdfRotation.Rotate0:
                        pdfRotation = PdfRotation.Rotate270;
                        break;
                    case PdfRotation.Rotate90:
                        pdfRotation = PdfRotation.Rotate0;
                        break;
                    case PdfRotation.Rotate180:
                        pdfRotation = PdfRotation.Rotate90;
                        break;
                    case PdfRotation.Rotate270:
                        pdfRotation = PdfRotation.Rotate180;
                        break;
                    default:
                        pdfRotation = PdfRotation.Rotate0;
                        break;
                }
                MainViewer.Renderer.Rotation = pdfRotation;
            }
        }

        /// <summary>
        /// 获取指定URL的html代码，自动处理重定向
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        private async Task GetHtml(string url)
        {
            try
            {
                mainHtml = null;
                //string html = await client.GetStringAsync(url);
                HttpResponseMessage response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();
                string html = await response.Content.ReadAsStringAsync();
                var doc = new HtmlDocument();
                doc.LoadHtml(html);
                var metaRefresh = doc.DocumentNode.SelectSingleNode("//meta[@http-equiv='REFRESH' or @http-equiv='Refresh']");
                if (metaRefresh != null)
                {
                    var content = metaRefresh.GetAttributeValue("content", string.Empty);
                    var contentParts = content.Split(';');
                    if (contentParts.Length > 1)
                    {
                        var urlPart = contentParts[1].Trim().Replace("URL=", "").Replace("'", "").Replace("\"", "");
                        if (Uri.IsWellFormedUriString(urlPart, UriKind.Relative))
                        {
                            url = new Uri(new Uri(url), urlPart).ToString();
                        }
                        else if (Uri.IsWellFormedUriString(urlPart, UriKind.Absolute))
                        {
                            url = urlPart;
                        }

                        // 重新请求新的URL
                        response = await client.GetAsync(url);
                        baseUrl = url.ToString();
                        response.EnsureSuccessStatusCode();
                        html = await response.Content.ReadAsStringAsync();
                    }
                }
                baseUrl = url;
                mainHtml = html;
            }
            catch (Exception)
            {
                mainHtml =null;
            }
        }

        /// <summary>
        /// 下载指定URL的PDF，保存到指定文件名
        /// </summary>
        /// <param name="url"></param>
        /// <param name="fn"></param>
        /// <returns></returns>
        private async Task GetPdf(string url,string fn)
        {
            try
            {
                using(HttpResponseMessage response=await client.GetAsync(url))
                {
                    response.EnsureSuccessStatusCode();
                    using(Stream contentStream = await response.Content.ReadAsStreamAsync())
                    {
                        using(FileStream fileStream=new FileStream(fn, FileMode.Create, FileAccess.Write, FileShare.Read))
                        {
                            await contentStream.CopyToAsync(fileStream);
                        }
                    }
                }
            }
            catch (HttpRequestException)
            {
                throw;
            }
        }

        /// <summary>
        /// 人民日报类页面获取，标题及PDFURL
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void btnGetPage_Click(object sender, EventArgs e)
        {
            string url = @"http://paper.people.com.cn/rmrb/html/2024-05/23/nbs.D110000renmrb_01.htm";
            url = @"http://paper.people.com.cn/rmrb/paperindex.htm";
            //url = txtURL.Text;
            string urlPage = null,urlPdf=null;
            await GetHtml(url);
            Uri xbaseUri = new Uri(baseUrl);
            if (mainHtml != null)
            {
                Console.Write(mainHtml);
                var doc = new HtmlDocument();
                doc.LoadHtml(mainHtml);
                var titles = doc.DocumentNode.SelectNodes("//*[@id=\"pageLink\"]");
                if (titles != null)
                {
                    urlsPages.Clear();

                    foreach (var title in titles)
                    {
                        urlPage = title.Attributes["href"] != null ? title.Attributes["href"].Value : null;
                        if (urlPage != null)
                        {
                            Uri absUri = new Uri(xbaseUri, urlPage);
                            urlPage = absUri.AbsoluteUri;
                            urlsPages.Add(title.InnerText, urlPage);
                        }
                    }
                    urlsPdfs.Clear();
                    this.lstPage.Items.Clear();
                    bool isFirst = true;
                    foreach (var page in urlsPages)
                    {
                        await GetHtml(page.Value);
                        if (mainHtml != null)
                        {
                            doc.LoadHtml(mainHtml);
                            xbaseUri = new Uri(page.Value);
                            var pdfUrl = doc.DocumentNode.SelectNodes("//*[@id=\"main\"]/div[1]/div[2]/p[2]/a");
                            if (pdfUrl != null)
                            {                                
                                foreach (var pdf in pdfUrl)
                                {
                                    urlPdf = pdf.Attributes["href"] != null ? pdf.Attributes["href"].Value : null;
                                    if (urlPdf != null)
                                    {
                                        Uri absUri = new Uri(xbaseUri, urlPdf);
                                        urlPdf = absUri.AbsoluteUri;
                                        urlsPdfs.Add(page.Key, urlPdf);
                                        lstPage.Items.Add(string.Format("{0}", page.Key));
                                        if (isFirst)
                                        {
                                            lstPage.SelectedIndex = 0;
                                            isFirst = false;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                MessageBox.Show("没有获取到报纸页面文件！", "Eersoft-提示");
                            }
                        }
                        else
                        {
                            MessageBox.Show("没有获取到报纸页面数据，可能是如下原因：\n1. 网络错误\n2. 报纸网页改版\n3. 时间太早了，今天的报纸还没有上线");
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("没有获取到内容，可能是网络错误，也可能是网页改版了。","Eersoft-提示");
            }
        }

        /// <summary>
        /// 根据URL访问报纸页面，解析得到版面列表及对应PDF下载链接，不同报纸需要不同的解析方式
        /// 这是人民日报页面类型的解析函数
        /// </summary>
        /// <param name="url"></param>
        private async void GetPageRMRB(string url)
        {
            string urlPage = null, urlPdf = null;
            await GetHtml(url);
            Uri xbaseUri = new Uri(baseUrl);
            if (mainHtml != null)
            {
                Console.Write(mainHtml);
                var doc = new HtmlDocument();
                doc.LoadHtml(mainHtml);
                var titles = doc.DocumentNode.SelectNodes("//*[@id=\"pageLink\"]");
                if (titles != null)
                {
                    urlsPages.Clear();

                    foreach (var title in titles)
                    {
                        urlPage = title.Attributes["href"] != null ? title.Attributes["href"].Value : null;
                        if (urlPage != null)
                        {
                            Uri absUri = new Uri(xbaseUri, urlPage);
                            urlPage = absUri.AbsoluteUri;
                            urlsPages.Add(title.InnerText, urlPage);
                        }
                    }
                    urlsPdfs.Clear();
                    this.lstPage.Items.Clear();
                    bool isFirst = true;
                    foreach (var page in urlsPages)
                    {
                        await GetHtml(page.Value);
                        if (mainHtml != null)
                        {
                            doc.LoadHtml(mainHtml);
                            xbaseUri = new Uri(page.Value);
                            var pdfUrl = doc.DocumentNode.SelectNodes("//*[@id=\"main\"]/div[1]/div[2]/p[2]/a");
                            if (pdfUrl != null)
                            {
                                foreach (var pdf in pdfUrl)
                                {
                                    urlPdf = pdf.Attributes["href"] != null ? pdf.Attributes["href"].Value : null;
                                    if (urlPdf != null)
                                    {
                                        Uri absUri = new Uri(xbaseUri, urlPdf);
                                        urlPdf = absUri.AbsoluteUri;
                                        urlsPdfs.Add(page.Key, urlPdf);
                                        lstPage.Items.Add(string.Format("{0}", page.Key));
                                        if (isFirst)
                                        {
                                            lstPage.SelectedIndex = 0;
                                            isFirst = false;
                                        }
                                        else
                                        {
                                            GetPDFbyNameUrl(page.Key, urlPdf);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                MessageBox.Show("没有获取到报纸页面文件！", "Eersoft-提示");
                            }
                        }
                        else
                        {
                            MessageBox.Show("没有获取到报纸页面数据，可能是如下原因：\n1. 网络错误\n2. 报纸网页改版\n3. 时间太早了，今天的报纸还没有上线");
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("没有获取到内容，可能是网络错误，也可能是网页改版了。", "Eersoft-提示");
            }
        }

        /// <summary>
        /// 根据URL访问报纸页面，解析得到版面列表及对应PDF下载链接，不同报纸需要不同的解析方式
        /// 这是讽刺与幽默页面类型的解析函数
        /// </summary>
        /// <param name="url"></param>
        private async void GetPageFCYYM(string url)
        {
            string urlPage = null, urlPdf = null;
            await GetHtml(url);
            Uri xbaseUri = new Uri(baseUrl);
            if (mainHtml != null)
            {
                Console.Write(mainHtml);
                var doc = new HtmlDocument();
                doc.LoadHtml(mainHtml);
                var titles = doc.DocumentNode.SelectNodes("//*[@id=\"pageLink\"]");
                if (titles != null)
                {
                    urlsPages.Clear();

                    foreach (var title in titles)
                    {
                        urlPage = title.Attributes["href"] != null ? title.Attributes["href"].Value : null;
                        if (urlPage != null)
                        {
                            Uri absUri = new Uri(xbaseUri, urlPage);
                            urlPage = absUri.AbsoluteUri;
                            urlsPages.Add(title.InnerText, urlPage);
                        }
                    }
                    urlsPdfs.Clear();
                    this.lstPage.Items.Clear();
                    bool isFirst = true;
                    foreach (var page in urlsPages)
                    {
                        await GetHtml(page.Value);
                        if (mainHtml != null)
                        {
                            doc.LoadHtml(mainHtml);
                            xbaseUri = new Uri(page.Value);
                            var pdfUrl = doc.DocumentNode.SelectNodes("//html/body/div[2]/div[1]/div[2]/p[2]/a");
                            if (pdfUrl != null)
                            {
                                foreach (var pdf in pdfUrl)
                                {
                                    urlPdf = pdf.Attributes["href"] != null ? pdf.Attributes["href"].Value : null;
                                    if (urlPdf != null)
                                    {
                                        Uri absUri = new Uri(xbaseUri, urlPdf);
                                        urlPdf = absUri.AbsoluteUri;
                                        urlsPdfs.Add(page.Key, urlPdf);
                                        lstPage.Items.Add(string.Format("{0}", page.Key));
                                        if (isFirst)
                                        {
                                            lstPage.SelectedIndex = 0;
                                            isFirst = false;
                                        }
                                        else
                                        {
                                            GetPDFbyNameUrl(page.Key, urlPdf);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                MessageBox.Show("没有获取到报纸页面文件！", "Eersoft-提示");
                            }
                        }
                        else
                        {
                            MessageBox.Show("没有获取到报纸页面数据，可能是如下原因：\n1. 网络错误\n2. 报纸网页改版\n3. 时间太早了，今天的报纸还没有上线");
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("没有获取到内容，可能是网络错误，也可能是网页改版了。", "Eersoft-提示");
            }
        }

        /// <summary>
        /// 根据URL访问报纸页面，解析得到版面列表及对应PDF下载链接，不同报纸需要不同的解析方式
        /// 这是松江报页面类型的解析函数,页面标题与URL分开，PDF在每页
        /// </summary>
        /// <param name="url"></param>
        private async void GetPageSJB(string url)
        {
            string urlPage = null, urlPdf = null;
            await GetHtml(url);
            Uri xbaseUri = new Uri(baseUrl);
            if (mainHtml != null)
            {
                Console.Write(mainHtml);
                var doc = new HtmlDocument();
                doc.LoadHtml(mainHtml);
                var titles = doc.DocumentNode.SelectNodes("//*[@class=\"titlebox\"]/span");
                var titles_url = doc.DocumentNode.SelectNodes("//*[@class=\"item\"]/a");
                if (titles != null && titles_url!=null)
                {
                    if (titles.Count == titles_url.Count)
                    {
                        urlsPages.Clear();
                        for (int i = 0; i < titles.Count; i++)
                        {
                            string key = titles[i].InnerText;
                            urlPage = titles_url[i].Attributes["href"] != null ? titles_url[i].Attributes["href"].Value : null;
                            if (urlPage != null)
                            {
                                Uri absUri = new Uri(xbaseUri,urlPage);
                                urlPage = absUri.AbsoluteUri;
                                urlsPages.Add(key, urlPage);
                            }
                        }
                        
                        urlsPdfs.Clear();
                        this.lstPage.Items.Clear();
                        bool isFirst = true;
                        foreach (var page in urlsPages)
                        {
                            await GetHtml(page.Value);
                            if (mainHtml != null)
                            {
                                doc.LoadHtml(mainHtml);
                                xbaseUri = new Uri(page.Value);
                                var pdfUrl = doc.DocumentNode.SelectNodes("//*[@id=\"pdf_toolbar\"]/a");
                                if (pdfUrl != null)
                                {
                                    foreach (var pdf in pdfUrl)
                                    {
                                        urlPdf = pdf.Attributes["href"] != null ? pdf.Attributes["href"].Value : null;
                                        if (urlPdf != null)
                                        {
                                            Uri absUri = new Uri(xbaseUri, urlPdf);
                                            urlPdf = absUri.AbsoluteUri;
                                            urlsPdfs.Add(page.Key, urlPdf);
                                            lstPage.Items.Add(string.Format("{0}", page.Key));
                                            if (isFirst)
                                            {
                                                lstPage.SelectedIndex = 0;
                                                isFirst = false;
                                            }
                                            else
                                            {
                                                GetPDFbyNameUrl(page.Key, urlPdf);
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    MessageBox.Show("没有获取到报纸页面文件！", "Eersoft-提示");
                                }
                            }
                            else
                            {
                                MessageBox.Show("没有获取到报纸页面数据，可能是如下原因：\n1. 网络错误\n2. 报纸网页改版\n3. 时间太早了，今天的报纸还没有上线");
                            }
                        }
                    }
                    else
                    {
                        MessageBox.Show("获取页面链接出现问题，可能是页面改版了。", "Eersoft-提示");
                    }
                }
            }
            else
            {
                MessageBox.Show("没有获取到内容，可能是网络错误，也可能是网页改版了。", "Eersoft-提示");
            }
        }

        /// <summary>
        /// 根据URL访问报纸页面，解析得到版面列表及对应PDF下载链接，不同报纸需要不同的解析方式
        /// 这是华西都市报页面类型的解析函数
        /// </summary>
        /// <param name="url"></param>
        private async void GetPageHXDSB(string url)
        {
            await GetHtml(url);
            if (mainHtml != null)
            {
                Console.Write(mainHtml);
                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(mainHtml);
                HtmlNodeCollection titles = doc.DocumentNode.SelectNodes("//div[@class='ellipsis title-text']");
                HtmlNodeCollection pdfs = doc.DocumentNode.SelectNodes("//div[@class='pdf-ico']/a");
                if (titles != null && pdfs!=null)
                {
                    if (titles.Count() == pdfs.Count())
                    {
                        urlsPages.Clear();
                        urlsPdfs.Clear();
                        bool isFirst = true;
                        this.lstPage.Items.Clear();
                        for (int i = 0; i < titles.Count(); ++i)
                        {
                            string key = titles[i].InnerText;
                            string pdfurl = pdfs[i].Attributes["href"] != null ? pdfs[i].Attributes["href"].Value : null;
                            if (pdfurl != null)
                            {
                                urlsPages.Add(key,"");
                                urlsPdfs.Add(key, pdfurl);
                                this.lstPage.Items.Add(key);
                                if (isFirst)
                                {
                                    this.lstPage.SelectedIndex = 0;
                                    isFirst = false;
                                }
                                else
                                {
                                    GetPDFbyNameUrl(key, pdfurl);
                                }
                            } 
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("没有获取到内容，可能是网络错误，也可能是网页改版了。", "Eersoft-提示");
            }
        }

        /// <summary>
        /// 根据URL访问报纸页面，解析得到版面列表及对应PDF下载链接，不同报纸需要不同的解析方式
        /// 这是中国教师报页面类型的解析函数
        /// </summary>
        /// <param name="url"></param>
        private async void GetPageZGJSB(string url)
        {
            await GetHtml(url);
            if (mainHtml != null)
            {
                Uri xbaseUri = new Uri(baseUrl);
                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(mainHtml);
                HtmlNodeCollection titles = doc.DocumentNode.SelectNodes("//div[@class=\"right_title-name\"]/a");
                HtmlNodeCollection pdfs = doc.DocumentNode.SelectNodes("//div[@class='right_title-pdf']/a");
                if (titles != null && pdfs != null)
                {
                    if (titles.Count() == pdfs.Count())
                    {
                        urlsPages.Clear();
                        urlsPdfs.Clear();
                        bool isFirst = true;
                        this.lstPage.Items.Clear();
                        for (int i = 0; i < titles.Count(); ++i)
                        {
                            string key = titles[i].InnerText;
                            string pdfurl = pdfs[i].Attributes["href"] != null ? pdfs[i].Attributes["href"].Value : null;
                            if (pdfurl != null)
                            {
                                Uri absUri = new Uri(xbaseUri, pdfurl);
                                pdfurl = absUri.AbsoluteUri;
                                urlsPages.Add(key, "");
                                urlsPdfs.Add(key, pdfurl);
                                this.lstPage.Items.Add(key);
                                if (isFirst)
                                {
                                    this.lstPage.SelectedIndex = 0;
                                    isFirst = false;
                                }
                                else
                                {
                                    GetPDFbyNameUrl(key, pdfurl);
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("没有获取到内容，可能是网络错误，也可能是网页改版了。", "Eersoft-提示");
            }
        }

        /// <summary>
        /// 根据URL访问报纸页面，解析得到版面列表及对应PDF下载链接，不同报纸需要不同的解析方式
        /// 这是证券日报页面类型的解析函数
        /// </summary>
        /// <param name="url"></param>
        private async void GetPageZQRB(string url)
        {
            await GetHtml(url);
            if (mainHtml != null)
            {
                Uri xbaseUri = new Uri(baseUrl);
                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(mainHtml);
                HtmlNodeCollection titles = doc.DocumentNode.SelectNodes("//*[@id=\"pageLink\"]");
                HtmlNodeCollection pdfs = doc.DocumentNode.SelectNodes("//*[@id=\"pgn\"]/table/tbody/tr[*]/td[2]/div/a");
                if (titles != null && pdfs != null)
                {
                    if (titles.Count() == pdfs.Count())
                    {
                        urlsPages.Clear();
                        urlsPdfs.Clear();
                        bool isFirst = true;
                        this.lstPage.Items.Clear();
                        for (int i = 0; i < titles.Count(); ++i)
                        {
                            string key = titles[i].InnerText;
                            string pdfurl = pdfs[i].Attributes["href"] != null ? pdfs[i].Attributes["href"].Value : null;
                            if (pdfurl != null)
                            {
                                Uri absUri = new Uri(xbaseUri, pdfurl);
                                pdfurl = absUri.AbsoluteUri;
                                urlsPages.Add(key, "");
                                urlsPdfs.Add(key, pdfurl);
                                this.lstPage.Items.Add(key);
                                if (isFirst)
                                {
                                    this.lstPage.SelectedIndex = 0;
                                    isFirst = false;
                                }
                                else
                                {
                                    GetPDFbyNameUrl(key, pdfurl);
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("没有获取到内容，可能是网络错误，也可能是网页改版了。", "Eersoft-提示");
            }
        }

        /// <summary>
        /// 根据URL访问报纸页面，解析得到版面列表及对应PDF下载链接，不同报纸需要不同的解析方式
        /// 这是学习时报页面类型的解析函数
        /// </summary>
        /// <param name="url"></param>
        private async void GetPageXXSB(string url)
        {
            await GetHtml(url);
            if (mainHtml != null)
            {
                Uri xbaseUri = new Uri(baseUrl);
                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(mainHtml);
                HtmlNodeCollection titles = doc.DocumentNode.SelectNodes("//*[@id=\"pageLink\"]");
                HtmlNodeCollection pdfs = doc.DocumentNode.SelectNodes("//*[@class=\"right_title-pdf\"]/a");
                if (titles != null && pdfs != null)
                {
                    if (titles.Count() == pdfs.Count())
                    {
                        urlsPages.Clear();
                        urlsPdfs.Clear();
                        bool isFirst = true;
                        this.lstPage.Items.Clear();
                        for (int i = 0; i < titles.Count(); ++i)
                        {
                            string key = titles[i].InnerText;
                            string pdfurl = pdfs[i].Attributes["href"] != null ? pdfs[i].Attributes["href"].Value : null;
                            if (pdfurl != null)
                            {
                                Uri absUri = new Uri(xbaseUri, pdfurl);
                                pdfurl = absUri.AbsoluteUri;
                                urlsPages.Add(key, "");
                                urlsPdfs.Add(key, pdfurl);
                                this.lstPage.Items.Add(key);
                                if (isFirst)
                                {
                                    this.lstPage.SelectedIndex = 0;
                                    isFirst = false;
                                }
                                else
                                {
                                    GetPDFbyNameUrl(key, pdfurl);
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("没有获取到内容，可能是网络错误，也可能是网页改版了。", "Eersoft-提示");
            }
        }

        /// <summary>
        /// 根据URL访问报纸页面，解析得到版面列表及对应PDF下载链接，不同报纸需要不同的解析方式
        /// 这是经济日报页面类型的解析函数,pdf在input节点的value中
        /// </summary>
        /// <param name="url"></param>
        private async void GetPageJJRB(string url)
        {
            await GetHtml(url);
            if (mainHtml != null)
            {
                Uri xbaseUri = new Uri(baseUrl);
                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(mainHtml);
                HtmlNodeCollection titles = doc.DocumentNode.SelectNodes("//*[@id=\"layoutlist\"]/li[*]/a");
                HtmlNodeCollection pdfs = doc.DocumentNode.SelectNodes("//li[@class='posRelative']/input");
                if (titles != null && pdfs != null)
                {
                    if (titles.Count() == pdfs.Count())
                    {
                        urlsPages.Clear();
                        urlsPdfs.Clear();
                        bool isFirst = true;
                        this.lstPage.Items.Clear();
                        for (int i = 0; i < titles.Count(); ++i)
                        {
                            string key = titles[i].InnerText;
                            string pdfurl = pdfs[i].Attributes["value"] != null ? pdfs[i].Attributes["value"].Value : null;
                            if (pdfurl != null)
                            {
                                Uri absUri = new Uri(xbaseUri, pdfurl);
                                pdfurl = absUri.AbsoluteUri;
                                urlsPages.Add(key, "");
                                urlsPdfs.Add(key, pdfurl);
                                this.lstPage.Items.Add(key);
                                if (isFirst)
                                {
                                    this.lstPage.SelectedIndex = 0;
                                    isFirst = false;
                                }
                                else
                                {
                                    GetPDFbyNameUrl(key, pdfurl);
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("没有获取到内容，可能是网络错误，也可能是网页改版了。", "Eersoft-提示");
            }
        }

        /// <summary>
        /// 根据URL访问报纸页面，解析得到版面列表及对应PDF下载链接，不同报纸需要不同的解析方式
        /// 这是三峡都市报页面类型的解析函数,跳转+js跳转，重庆晨报/重庆商报与该报采用同一套系统
        /// </summary>
        /// <param name="url"></param>
        private async void GetPageSXDSB(string url)
        {
            await GetHtml(url);

            if (mainHtml != null)
            {
                Uri xbaseUri = new Uri(baseUrl);
                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(mainHtml);
                HtmlNodeCollection titles = doc.DocumentNode.SelectNodes("//li[1]/a");
                string main_url = titles[0].Attributes["href"] != null ? titles[0].Attributes["href"].Value : null;
                if (main_url != null)
                {
                    Uri absUri = new Uri(xbaseUri, main_url);
                    main_url = absUri.AbsoluteUri;
                    await GetHtml(main_url);
                }
                if (mainHtml != null)
                {
                    xbaseUri = new Uri(baseUrl);
                    doc.LoadHtml(mainHtml);
                    var nodes = doc.DocumentNode.SelectNodes("//div[@class='Chunkiconlist']/p");

                    if (nodes != null)
                    {
                        urlsPages.Clear(); 
                        urlsPdfs.Clear();   
                        lstPage.Items.Clear();
                        bool isFirst = true;
                        foreach (var node in nodes)
                        {
                            var titleNode = node.SelectSingleNode("a[1]");
                            var pdfNode = node.SelectSingleNode("a[2]");

                            if (titleNode != null && pdfNode != null)
                            {
                                string title = titleNode.InnerText.Trim();
                                string pdfLink = pdfNode.GetAttributeValue("href", string.Empty);
                                Uri absUri = new Uri(xbaseUri, pdfLink);
                                pdfLink = absUri.AbsoluteUri;
                                urlsPdfs[title] = pdfLink;
                                lstPage.Items.Add(title);
                                GetPDFbyNameUrl(title, pdfLink);
                                if (isFirst)
                                {
                                    lstPage.SelectedIndex = 0;
                                    isFirst = false;
                                }
                            }
                        }
                    }
                    else
                    {
                        MessageBox.Show("没有获取到内容，可能是网络错误，也可能是网页改版了。", "Eersoft-提示");
                    }
                }
                else
                {
                    MessageBox.Show("没有获取到内容，可能是网络错误，也可能是网页改版了。", "Eersoft-提示");
                }
            }
            else
            {
                MessageBox.Show("没有获取到内容，可能是网络错误，也可能是网页改版了。", "Eersoft-提示");
            }
        }

        /// <summary>
        /// 根据URL访问报纸页面，解析得到版面列表及对应PDF下载链接，不同报纸需要不同的解析方式
        /// 这是重庆日报页面类型的解析函数,超多表格结构，乱七八糟
        /// </summary>
        /// <param name="url"></param>
        private async void GetPageCQRB(string url)
        {
            await GetHtml(url);
            if (mainHtml != null)
            {
                Uri xbaseUri = new Uri(baseUrl);
                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(mainHtml);

                urlsPdfs.Clear();
                urlsPages.Clear();
                lstPage.Items.Clear();
                bool isFirst = true;
                var nodes = doc.DocumentNode.SelectNodes("//tr");

                
                if (nodes != null)
                {
                    foreach (var node in nodes)
                    {
                        var titleNode = node.SelectSingleNode(".//span[contains(@class, 'default')]");
                        var pdfLinkNode = node.SelectSingleNode(".//a[contains(@href, '.pdf')]");

                        if (titleNode != null && pdfLinkNode != null)
                        {
                            string title = titleNode.InnerText.Trim();
                            string pdfLink = pdfLinkNode.GetAttributeValue("href", string.Empty);
                            Uri absUri = new Uri(xbaseUri, pdfLink);
                            pdfLink = absUri.AbsoluteUri;
                            if (!urlsPdfs.ContainsKey(title))
                            {
                                urlsPdfs.Add(title, pdfLink);
                                lstPage.Items.Add(title);
                                if (isFirst)
                                {
                                    lstPage.SelectedIndex = 0;
                                    isFirst = false;
                                }
                                else
                                {
                                    GetPDFbyNameUrl(title, pdfLink);
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("没有获取到内容，可能是网络错误，也可能是网页改版了。", "Eersoft-提示");
            }
        }

        /// <summary>
        /// 根据URL访问报纸页面，解析得到版面列表及对应PDF下载链接，不同报纸需要不同的解析方式
        /// 这是中国国防报页面类型的解析函数
        /// </summary>
        /// <param name="url"></param>
        private async void GetPageZGGFB(string url)
        {
            string urlPage = null, urlPdf = null;
            await GetHtml(url);
            Uri xbaseUri = new Uri(baseUrl);
            if (mainHtml != null)
            {
                Console.Write(mainHtml);
                var doc = new HtmlDocument();
                doc.LoadHtml(mainHtml);
                var titles = doc.DocumentNode.SelectNodes("//*[@id=\"APP-SectionNav\"]/li[*]/a");
                if (titles != null)
                {
                    urlsPages.Clear();

                    foreach (var title in titles)
                    {
                        urlPage = title.Attributes["href"] != null ? title.Attributes["href"].Value : null;
                        if (urlPage != null)
                        {
                            Uri absUri = new Uri(xbaseUri, urlPage);
                            urlPage = absUri.AbsoluteUri;
                            urlsPages.Add(title.InnerText, urlPage);
                        }
                    }
                    urlsPdfs.Clear();
                    this.lstPage.Items.Clear();
                    bool isFirst = true;
                    foreach (var page in urlsPages)
                    {
                        await GetHtml(page.Value);
                        if (mainHtml != null)
                        {
                            doc.LoadHtml(mainHtml);
                            xbaseUri = new Uri(page.Value);
                            var pdfUrl = doc.DocumentNode.SelectNodes("//*[@id=\"APP-Pdf\"]");
                            if (pdfUrl != null)
                            {
                                foreach (var pdf in pdfUrl)
                                {
                                    urlPdf = pdf.Attributes["href"] != null ? pdf.Attributes["href"].Value : null;
                                    if (urlPdf != null)
                                    {
                                        Uri absUri = new Uri(xbaseUri, urlPdf);
                                        urlPdf = absUri.AbsoluteUri;
                                        urlsPdfs.Add(page.Key, urlPdf);
                                        lstPage.Items.Add(string.Format("{0}", page.Key));
                                        if (isFirst)
                                        {
                                            lstPage.SelectedIndex = 0;
                                            isFirst = false;
                                        }
                                        else
                                        {
                                            GetPDFbyNameUrl(page.Key, urlPdf);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                MessageBox.Show("没有获取到报纸页面文件！", "Eersoft-提示");
                            }
                        }
                        else
                        {
                            MessageBox.Show("没有获取到报纸页面数据，可能是如下原因：\n1. 网络错误\n2. 报纸网页改版\n3. 时间太早了，今天的报纸还没有上线");
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("没有获取到内容，可能是网络错误，也可能是网页改版了。", "Eersoft-提示");
            }
        }

        /// <summary>
        /// 根据URL访问报纸页面，解析得到版面列表及对应PDF下载链接，不同报纸需要不同的解析方式
        /// 这是科技日报页面类型的解析函数
        /// </summary>
        /// <param name="url"></param>
        private async void GetPageKJRB(string url)
        {
            string urlPage = null, urlPdf = null;
            await GetHtml(url);
            Uri xbaseUri = new Uri(baseUrl);
            if (mainHtml != null)
            {
                Console.Write(mainHtml);
                var doc = new HtmlDocument();
                doc.LoadHtml(mainHtml);
                var titles = doc.DocumentNode.SelectNodes("//*[@id=\"pageLink\"]");
                if (titles != null)
                {
                    urlsPages.Clear();

                    foreach (var title in titles)
                    {
                        urlPage = title.Attributes["href"] != null ? title.Attributes["href"].Value : null;
                        if (urlPage != null)
                        {
                            Uri absUri = new Uri(xbaseUri, urlPage);
                            urlPage = absUri.AbsoluteUri;
                            urlsPages.Add(title.InnerText, urlPage);
                        }
                    }
                    urlsPdfs.Clear();
                    this.lstPage.Items.Clear();
                    bool isFirst = true;
                    foreach (var page in urlsPages)
                    {
                        await GetHtml(page.Value);
                        if (mainHtml != null)
                        {
                            doc.LoadHtml(mainHtml);
                            xbaseUri = new Uri(page.Value);
                            var pdfUrl = doc.DocumentNode.SelectNodes("//*[@class=\"pdf\"]/a");
                            if (pdfUrl != null)
                            {
                                foreach (var pdf in pdfUrl)
                                {
                                    urlPdf = pdf.Attributes["href"] != null ? pdf.Attributes["href"].Value : null;
                                    if (urlPdf != null)
                                    {
                                        Uri absUri = new Uri(xbaseUri, urlPdf);
                                        urlPdf = absUri.AbsoluteUri;
                                        urlsPdfs.Add(page.Key, urlPdf);
                                        lstPage.Items.Add(string.Format("{0}", page.Key));
                                        if (isFirst)
                                        {
                                            lstPage.SelectedIndex = 0;
                                            isFirst = false;
                                        }
                                        else
                                        {
                                            GetPDFbyNameUrl(page.Key, urlPdf);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                MessageBox.Show("没有获取到报纸页面文件！", "Eersoft-提示");
                            }
                        }
                        else
                        {
                            MessageBox.Show("没有获取到报纸页面数据，可能是如下原因：\n1. 网络错误\n2. 报纸网页改版\n3. 时间太早了，今天的报纸还没有上线");
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("没有获取到内容，可能是网络错误，也可能是网页改版了。", "Eersoft-提示");
            }
        }

        /// <summary>
        /// 根据报纸名称和url下载PDF
        /// </summary>
        /// <param name="name">版面标题</param>
        /// <param name="url">pdf链接</param>
        private async void GetPDFbyNameUrl(string caption, string url)
        {
            string path = Path.Combine(pathRoot, DateTime.Now.ToString("yyyy-MM-dd"), newsPaperName);
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            string fn = Path.Combine(path, caption + ".pdf");
            try
            {
                if (!File.Exists(fn))
                {
                    await GetPdf(url, fn);
                }
            }
            catch (Exception)
            {
                MessageBox.Show("读取页面出现问题！", "Eersoft-提示");
            }
        }

        /// <summary>
        /// 点击版面列表切换版面
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void lstPage_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(lstPage.SelectedIndex != -1) {
                string caption = lstPage.SelectedItem.ToString();
                //对应PDF放入对应路径，需要检测路径存在性
                string path = Path.Combine(pathRoot,DateTime.Now.ToString("yyyy-MM-dd"),newsPaperName);
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                string fn = Path.Combine(path, caption+".pdf");
                try
                {
                    if (!File.Exists(fn))
                    {
                        await GetPdf(urlsPdfs[caption], fn);
                    }
                    LoadPdfFile(fn);
                    this.Text = string.Format("Eersoft在线报纸阅读器-{0}-{1}", newsPaperName, caption);
                }
                catch (Exception)
                {
                    MessageBox.Show("读取页面出现问题！","Eersoft-提示");
                }

            }
        }

        /// <summary>
        /// 全屏视图及退出
        /// </summary>
        private void SwitchFullScreen()
        {
            if (tbFullScreen.Text == "全屏")
            {
                // 保存窗口位置与尺寸
                window_h = this.Height;
                window_w = this.Width;
                window_left = this.Left;
                window_top = this.Top;
                this.FormBorderStyle = FormBorderStyle.None;
                // 设置窗口为全屏
                int style = GetWindowLong(this.Handle, GWL_STYLE);
                SetWindowLong(this.Handle, GWL_STYLE, (style & ~WS_POPUP) | WS_POPUP);
                SetWindowPos(this.Handle, IntPtr.Zero, 0, 0, Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height, 0);
                tbFullScreen.Text = "退出全屏";
            }else
            {
                // 退出全屏
                int style = GetWindowLong(this.Handle, GWL_STYLE);
                SetWindowLong(this.Handle, GWL_STYLE, style & ~WS_POPUP);
                SetWindowPos(this.Handle, IntPtr.Zero, 0, 0, Screen.PrimaryScreen.WorkingArea.Width, Screen.PrimaryScreen.WorkingArea.Height, 0);
                this.FormBorderStyle = FormBorderStyle.Sizable;
                // 恢复窗口位置与尺寸
                this.Height = window_h;
                this.Width = window_w;
                this.Top = window_top;
                this.Left = window_left;
                tbFullScreen.Text = "全屏";
            }
        }

        /// <summary>
        /// 根据鼠标位置显示/隐藏工具栏
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timer1_Tick(object sender, EventArgs e)
        {
            Point point = new Point();
            if(GetCursorPos(out point))
            {
                Point clientPoint = this.PointToClient(new Point(point.X, point.Y));
                if (this.ClientRectangle.Contains(clientPoint))
                {
                    Rectangle panelBounds = new Rectangle(this.fPanel.Left, this.fPanel.Top, HOT_AREA_SIZE, this.fPanel.Height);
                    this.fPanel.Visible = panelBounds.Contains(clientPoint) || clientPoint.X<=this.fPanel.Width;

                    // 计算工具栏的完整边界
                    Rectangle toolbarBounds = new Rectangle(this.panel_Top.Left, 
                                                           this.panel_Top.Top, 
                                                           this.panel_Top.Width, 
                                                           this.panel_Top.Height);
                    // 计算热区边界
                    Rectangle hotAreaBounds = new Rectangle(this.panel_Top.Left + this.panel_Top.Width - HOT_AREA_SIZE, 
                                                           this.panel_Top.Top, 
                                                           HOT_AREA_SIZE, 
                                                           HOT_AREA_SIZE);
                    // 当鼠标位于热区或工具栏上时，显示工具栏
                    this.panel_Top.Visible = hotAreaBounds.Contains(clientPoint) || toolbarBounds.Contains(clientPoint);
                }
            }
            if (isReading)
            {
                if (isLoading)
                {
                    panelCenter.Visible = true;
                    labInfo.Text = "载入中...";
                }
                else
                {
                    panelCenter.Visible = false;
                }
            }
            else
            {
                panelCenter.Visible = true;
                labInfo.Text = "准备就绪...";
            }
        }

        /// <summary>
        /// 人民日报 OK
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnTest_Click(object sender, EventArgs e)
        {
            isLoading = true;
            newsPaperName = "人民日报";
            string url = @"http://paper.people.com.cn/rmrb/paperindex.htm";
            GetPageRMRB(url);
        }

        /// <summary>
        /// 中国国防报 OK
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button6_Click(object sender, EventArgs e)
        {
            isLoading = true;
            newsPaperName = "中国国防报";
            string url = @"http://www.chinamil.com.cn/gfbmap/paperindex.htm";
            GetPageZGGFB(url);
        }
        
        /// <summary>
        /// 证券日报 OK
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button7_Click(object sender, EventArgs e)
        {
            isLoading = true;
            newsPaperName = "证券日报";
            string url = @"http://epaper.zqrb.cn/";
            GetPageZQRB(url);
        }

        /// <summary>
        /// 中国教师报 OK
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button8_Click(object sender, EventArgs e)
        {
            isLoading = true;
            newsPaperName = "中国教师报";
            string url = @"http://paper.chinateacher.com.cn/zgjsb/paperindex.htm";
            GetPageZGJSB(url);
        }

        /// <summary>
        /// 科技日报 OK
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button4_Click(object sender, EventArgs e)
        {
            isLoading = true;
            newsPaperName = "科技日报";
            string url = @"http://digitalpaper.stdaily.com/http_www.kjrb.com/kjrb/paperindex.htm";
            GetPageKJRB(url);
        }

        /// <summary>
        /// 学习时报 OK
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button5_Click(object sender, EventArgs e)
        {
            isLoading = true;
            newsPaperName = "学习时报";
            string url = @"https://paper.cntheory.com/";
            GetPageXXSB(url);
        }

        /// <summary>
        /// 经济日报 OK
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button11_Click(object sender, EventArgs e)
        {
            isLoading = true;
            newsPaperName = "经济日报";
            string MM = DateTime.Today.ToString("yyyyMM");
            string DD = DateTime.Today.ToString("dd");
            string url = string.Format("http://paper.ce.cn/pc/layout/{0}/{1}/node_01.html", MM, DD);
            GetPageJJRB(url);
        }

        /// <summary>
        /// 向左旋转90°
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tbLeft90_Click(object sender, EventArgs e)
        {
            RLeft90();
        }

        /// <summary>
        /// 全屏按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tbFullScreen_Click(object sender, EventArgs e)
        {
            SwitchFullScreen();
        }

        /// <summary>
        /// 向右旋转90°
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tbRight90_Click(object sender, EventArgs e)
        {
            RRight90();
        }

        private void Form1_Load_1(object sender, EventArgs e)
        {
            isReading = false;
            isLoading = false;
            ResetLogo();
        }

        /// <summary>
        /// 工具栏放大
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tbZoomIn_Click(object sender, EventArgs e)
        {
            if (MainViewer.Document != null)
            {
                MainViewer.Renderer.ZoomIn();
            }
        }

        /// <summary>
        /// 工具栏缩小
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tbZoomOut_Click(object sender, EventArgs e)
        {
            if(MainViewer.Document != null) { MainViewer.Renderer.ZoomOut(); }  
        }

        /// <summary>
        /// 工具栏适应宽度
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tbToWidth_Click(object sender, EventArgs e)
        {
            if (MainViewer.Document != null)
            {
                MainViewer.Renderer.Zoom = 1;
                MainViewer.ZoomMode = PdfViewerZoomMode.FitWidth;
            }
        }

        /// <summary>
        /// 工具栏适应高度
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tbToHeight_Click(object sender, EventArgs e)
        {
            if (MainViewer.Document != null)
            {
                MainViewer.Renderer.Zoom = 1;
                MainViewer.ZoomMode = PdfViewerZoomMode.FitHeight;
            }
        }

        /// <summary>
        /// 重庆日报OK
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button15_Click(object sender, EventArgs e)
        {
            isLoading = true;
            newsPaperName = "重庆日报";
            string url = @"https://epaper.cqrb.cn/html/cqrb/";
            GetPageCQRB(url);
        }

        /// <summary>
        /// 三峡都市报OK
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button16_Click(object sender, EventArgs e)
        {
            isLoading = true;
            newsPaperName = "三峡都市报";
            string url = @"http://dpaper.sxcm.net/sxdsb/html/";
            GetPageSXDSB(url);
        }

        /// <summary>
        /// 重庆晨报OK与三峡都市报相同
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button17_Click(object sender, EventArgs e)
        {
            isLoading = true;
            newsPaperName = "重庆晨报";
            string url = @"https://epaper.cqcb.com/html/index.html";
            GetPageSXDSB(url);
        }

        /// <summary>
        /// 重庆商报OK与三峡都市报相同
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button18_Click(object sender, EventArgs e)
        {
            isLoading = true;
            newsPaperName = "重庆商报";
            string url = @"https://e.chinacqsb.com/html/";
            GetPageSXDSB(url);
        }

        private void linkEersoft_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            string url = "http://www.eersoft.com";
            System.Diagnostics.Process.Start(url);
        }

        private void button19_Click(object sender, EventArgs e)
        {
            string url = "";
            DialogResult ret = MessageBox.Show("想要给作者发送电子邮件添加您想看的报纸吗？", "Eersoft-提示", MessageBoxButtons.YesNo);
            switch (ret)
            {
                case DialogResult.Yes:
                    url = "mailto:eersoft@msn.com";
                    System.Diagnostics.Process.Start(url);
                    break;
                case DialogResult.No:
                    ret=MessageBox.Show("你也可以访问http://www.eersoft.com通过其他方式联系作者添加或删除报纸。需要现在帮你打开Eersoft网站吗？", "Eersoft-提示", MessageBoxButtons.YesNo);
                    if (ret==DialogResult.Yes)
                    {
                        url = "http://www.eersoft.com";
                        System.Diagnostics.Process.Start(url);
                    }
                    break;
            }
        }

        private void Form1_SizeChanged(object sender, EventArgs e)
        {
            ResetLogo();
        }

        private void ResetLogo()
        {
            panelCenter.Left = (this.ClientSize.Width - panelCenter.Width) / 2;
            panelCenter.Top = (this.ClientSize.Height - panelCenter.Height) / 2;
        }

        /// <summary>
        /// 工厂模式测试按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void button1_Click(object sender, EventArgs e)
        {
            string url = @"http://paper.people.com.cn/rmrb/paperindex.htm";
            newsPaper = NewsPaperFactory.CreateNewsPaper("人民日报");

            HtmlReturn result;
            result = await newsPaper.GetHtml(url);
            urlsPdfs = newsPaper.ParsePage(result);

            foreach (var item in urlsPdfs)
            {
                string filePath = Path.Combine(pathRoot, item.Key + ".pdf");
                await newsPaper.DownloadPdf(item.Value, filePath);
                // 显示PDF文件
                LoadPdfFile(filePath);
            }
        }

        /// <summary>
        /// 讽刺与幽默 OK
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button12_Click(object sender, EventArgs e)
        {
            isLoading = true;
            newsPaperName = "讽刺与幽默";
            string url = @"http://paper.people.com.cn/fcyym/paperindex.htm";
            GetPageFCYYM(url);
        }

        /// <summary>
        /// 松江报OK
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button13_Click(object sender, EventArgs e)
        {
            isLoading = true;
            newsPaperName = "松江报";
            string url = @"http://www.shsjb.com/";
            GetPageSJB(url);
        }

        /// <summary>
        /// 华西都市报 OK
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button14_Click(object sender, EventArgs e)
        {
            isLoading = true;
            newsPaperName = "华西都市报";
            string url = @"http://www.wccdaily.com.cn/shtml/index_hxdsb.shtml";
            GetPageHXDSB(url);
        }
    }
}
