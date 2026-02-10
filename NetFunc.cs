using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using HtmlDocument = HtmlAgilityPack.HtmlDocument;

namespace NewsPaperReader
{
    /// <summary>
    /// GetHtml返回类型，需要返回当前页面的url和对应的html，
    /// 因为传入的url可能在重定向时变化
    /// </summary>
    public class HtmlReturn
    {
        private string _baseUrl;
        private string _pageHtml;

        public string BaseUrl
        {
            get { return _baseUrl; }
            set { _baseUrl = value; }
        }

        public string PageHtml
        {
            get { return _pageHtml; }
            set { _pageHtml = value; }
        }

        public HtmlReturn(string baseUrl, string pageHtml)
        {
            _baseUrl = baseUrl;
            _pageHtml = pageHtml;
        }
    }

    public class NetFunc
    {
        private HttpClient client = new HttpClient();        

        /// <summary>
        /// 获取页面html代码，需要处理META重定向
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public async Task<HtmlReturn> GetHtml(string url)
        {
            string html = null, baseUrl = null;
            try
            {
                HttpResponseMessage response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();
                html = await response.Content.ReadAsStringAsync();
                var doc = new HtmlDocument();
                if (html != null)
                {
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
                            return await GetHtml(url);
                        }
                        else
                        {
                            return new HtmlReturn(null, html);
                        }
                    }
                    else
                    {
                        baseUrl = url;
                        return new HtmlReturn(baseUrl, html);
                    }
                }
                else
                {
                    return new HtmlReturn(null, null);
                }                
            }
            catch (Exception)
            {
                return new HtmlReturn(null, null);
            }
        }

        /// <summary>
        /// 获取页面html代码，处理js重定向(1次)
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public async Task<HtmlReturn> GetHtml_js(string url)
        {
            HtmlReturn htmlReturn = null;
            htmlReturn = await GetHtml(url);
            string html=htmlReturn.PageHtml,baseUrl=htmlReturn.BaseUrl;

            if (html != null && baseUrl !=null)
            {
                Uri xbaseUri = new Uri(baseUrl);
                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(html);
                HtmlNodeCollection titles = doc.DocumentNode.SelectNodes("//li[1]/a"); //目前遇到的js重定向目标是第一个<li>
                string main_url = titles[0].Attributes["href"]?.Value;
                if (main_url != null)
                {
                    Uri absUri = new Uri(xbaseUri, main_url);
                    main_url = absUri.AbsoluteUri;
                    return await GetHtml(main_url);
                }                
                else
                {
                    return new HtmlReturn(null, null);
                }
            }
            else
            {
                return new HtmlReturn(null, null);
            }
        }

        /// <summary>
        /// 下载指定URL的PDF，保存到指定文件名
        /// </summary>
        /// <param name="pdfUrl"></param>
        /// <param name="savePath"></param>
        /// <returns></returns>
        public async Task<bool> DownloadPDF(string pdfUrl, string savePath)
        {
            try
            {
                using (HttpResponseMessage response = await client.GetAsync(pdfUrl))
                {
                    response.EnsureSuccessStatusCode();
                    using (Stream contentStream = await response.Content.ReadAsStreamAsync())
                    {
                        using (FileStream fileStream = new FileStream(savePath, FileMode.Create, FileAccess.Write, FileShare.Read))
                        {
                            await contentStream.CopyToAsync(fileStream);
                            return true;
                        }
                    }
                }
            }
            catch (HttpRequestException)
            {
                return false;
            }
        }
    }
}
