using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using HtmlDocument = HtmlAgilityPack.HtmlDocument;

namespace NewsPaperReader
{    
    internal interface INewsPaper
    {
        // 获取页面html
        Task<HtmlReturn> GetHtml(string url);

        // 解析页面，获取版面标题以及pdf链接
        Dictionary<string,string> ParsePage(HtmlReturn htmlResult);

        // 下载pdf到指定路径
        Task DownloadPdf(string pdfUrl,string filePath);

        /*// 版面列表
        Editions Editions();*/
    }

    /// <summary>
    /// 人民日报
    /// </summary>
    public class RenMinRiBao : INewsPaper
    {
        HtmlReturn htmlResult;
        NetHelper netFunc=new NetHelper();
        Editions _editions=new Editions();

        
        private HttpClient _client = new HttpClient();

        public async Task<HtmlReturn> GetHtml(string url)
        {
            htmlResult =await netFunc.GetHtml(url);
            return htmlResult;
        }

        public Dictionary<string,string> ParsePage(HtmlReturn htmlReturn)
        {
            var urlsPdfs = new Dictionary<string, string>();
            var doc = new HtmlDocument();
            doc.LoadHtml(htmlReturn.PageHtml);
            var titles = doc.DocumentNode.SelectNodes("//*[@id=\"pageLink\"]");
            if (titles != null)
            {
                foreach (var title in titles)
                {
                    var urlPdf = title.Attributes["href"] != null ? title.Attributes["href"].Value : null;
                    if (urlPdf != null)
                    {
                        urlsPdfs.Add(title.InnerText, urlPdf);
                    }
                }
            }
            return urlsPdfs;
        }

        public async Task DownloadPdf(string pdfUrl,string filePath)
        {
            HttpResponseMessage response = await _client.GetAsync(pdfUrl);
            response.EnsureSuccessStatusCode();
            using (Stream contentStream = await response.Content.ReadAsStreamAsync())
            using (FileStream fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.Read))
            {
                await contentStream.CopyToAsync(fileStream);
            }
        }

        /*public async Task<Editions> GetEditions()
        {
            
            return new Editions();
        }*/
    }
}
