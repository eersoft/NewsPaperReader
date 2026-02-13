using System;
using System.Net.Http;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace NewsPaperReader
{
    class Program
    {
        static async Task Main(string[] args)
        {
            string url = "https://www.chinateacher.com.cn/";
            await GetHtmlContent(url);
        }

        static async Task GetHtmlContent(string url)
        {
            try
            {
                using var client = new HttpClient();
                client.Timeout = TimeSpan.FromSeconds(30);
                var response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();
                string html = await response.Content.ReadAsStringAsync();

                // 保存HTML到文件以便分析
                System.IO.File.WriteAllText("chinateacher.html", html);
                Console.WriteLine("HTML content saved to chinateacher.html");

                // 简单分析HTML结构
                var doc = new HtmlDocument();
                doc.LoadHtml(html);

                // 查找所有a标签
                var links = doc.DocumentNode.SelectNodes("//a");
                if (links != null)
                {
                    Console.WriteLine($"Found {links.Count} links");
                    // 查找包含PDF的链接
                    int pdfCount = 0;
                    foreach (var link in links)
                    {
                        var href = link.GetAttributeValue("href", string.Empty);
                        var text = link.InnerText.Trim();
                        if (href.Contains(".pdf", StringComparison.OrdinalIgnoreCase))
                        {
                            Console.WriteLine($"PDF link found: {text} - {href}");
                            pdfCount++;
                        }
                    }
                    Console.WriteLine($"Found {pdfCount} PDF links");
                }

                // 查找所有可能的版面标题
                var titleNodes = doc.DocumentNode.SelectNodes("//h1|//h2|//h3|//h4|//h5|//h6|//div[@class='title']|//div[@class='headline']|//span[@class='title']");
                if (titleNodes != null)
                {
                    Console.WriteLine($"Found {titleNodes.Count} possible title nodes");
                    // 打印前10个标题
                    int count = 0;
                    foreach (var node in titleNodes)
                    {
                        if (count < 10)
                        {
                            Console.WriteLine($"Title: {node.InnerText.Trim()}");
                        }
                        count++;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }
}