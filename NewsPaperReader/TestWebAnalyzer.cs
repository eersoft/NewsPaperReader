using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace NewsPaperReader
{
    public class TestWebAnalyzer
    {
        private readonly HttpClient _httpClient;

        public TestWebAnalyzer()
        {
            _httpClient = new HttpClient();
            _httpClient.Timeout = TimeSpan.FromSeconds(30);
        }

        public async Task TestHuaxiMetroDaily()
        {
            string url = "https://www.wccdaily.com.cn/";
            Console.WriteLine($"测试华西都市报: {url}");

            try
            {
                // 获取网页内容
                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();
                string htmlContent = await response.Content.ReadAsStringAsync();

                // 保存HTML内容到文件，以便分析
                string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "huaxi.html");
                File.WriteAllText(filePath, htmlContent);
                Console.WriteLine($"HTML内容已保存到: {filePath}");

                // 解析HTML
                var doc = new HtmlAgilityPack.HtmlDocument();
                doc.LoadHtml(htmlContent);

                // 测试原来的选择器
                Console.WriteLine("\n测试原来的选择器:");
                var titleNodes = doc.DocumentNode.SelectNodes("//div[@class='ellipsis title-text']");
                var pdfNodes = doc.DocumentNode.SelectNodes("//div[@class='pdf-ico']/a");

                Console.WriteLine($"titleNodes数量: {titleNodes?.Count ?? 0}");
                Console.WriteLine($"pdfNodes数量: {pdfNodes?.Count ?? 0}");

                // 测试其他可能的选择器
                Console.WriteLine("\n测试其他可能的选择器:");
                
                // 查找所有包含pdf的链接
                var pdfLinks = doc.DocumentNode.SelectNodes("//a[contains(@href, '.pdf')]");
                Console.WriteLine($"包含.pdf的链接数量: {pdfLinks?.Count ?? 0}");
                if (pdfLinks != null && pdfLinks.Count > 0)
                {
                    for (int i = 0; i < Math.Min(pdfLinks.Count, 10); i++)
                    {
                        var link = pdfLinks[i];
                        Console.WriteLine($"  {i + 1}. {link.GetAttributeValue("href", "")} - {link.InnerText.Trim()}");
                    }
                }

                // 查找所有包含"pdf"或"PDF"的链接
                var pdfTextLinks = doc.DocumentNode.SelectNodes("//a[contains(text(), 'pdf') or contains(text(), 'PDF')]");
                Console.WriteLine($"\n包含'pdf'或'PDF'的链接数量: {pdfTextLinks?.Count ?? 0}");
                if (pdfTextLinks != null && pdfTextLinks.Count > 0)
                {
                    for (int i = 0; i < Math.Min(pdfTextLinks.Count, 10); i++)
                    {
                        var link = pdfTextLinks[i];
                        Console.WriteLine($"  {i + 1}. {link.GetAttributeValue("href", "")} - {link.InnerText.Trim()}");
                    }
                }

                // 查找所有包含"下载"的链接
                var downloadLinks = doc.DocumentNode.SelectNodes("//a[contains(text(), '下载')]");
                Console.WriteLine($"\n包含'下载'的链接数量: {downloadLinks?.Count ?? 0}");
                if (downloadLinks != null && downloadLinks.Count > 0)
                {
                    for (int i = 0; i < Math.Min(downloadLinks.Count, 10); i++)
                    {
                        var link = downloadLinks[i];
                        Console.WriteLine($"  {i + 1}. {link.GetAttributeValue("href", "")} - {link.InnerText.Trim()}");
                    }
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine($"测试失败: {ex.Message}");
            }
        }
    }
}