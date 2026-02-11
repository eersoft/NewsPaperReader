using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace TestHuaxi
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("开始测试华西都市报网页分析...");
            
            // 测试重定向后的桌面版URL
            await TestHuaxiMetroDaily("https://www.wccdaily.com.cn/shtml/index_hxdsb.shtml");
            
            // 测试可能的报纸页面
            await TestHuaxiMetroDaily("https://www.wccdaily.com.cn/hxdsb/html/");
            await TestHuaxiMetroDaily("https://www.wccdaily.com.cn/hxdsb/html/2026-02/11/node_1.htm");
            
            Console.WriteLine("\n测试完成，按任意键退出...");
            Console.ReadKey();
        }

        static async Task TestHuaxiMetroDaily(string url)
        {
            Console.WriteLine($"\n测试华西都市报: {url}");

            try
            {
                // 获取网页内容
                using var httpClient = new HttpClient();
                httpClient.Timeout = TimeSpan.FromSeconds(30);
                var response = await httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();
                string htmlContent = await response.Content.ReadAsStringAsync();

                // 保存HTML内容到文件，以便分析
                string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                string fileName = $"huaxi_{Guid.NewGuid().ToString().Substring(0, 8)}.html";
                string filePath = Path.Combine(desktopPath, fileName);
                File.WriteAllText(filePath, htmlContent);
                Console.WriteLine($"HTML内容已保存到: {filePath}");

                // 解析HTML
                var doc = new HtmlAgilityPack.HtmlDocument();
                doc.LoadHtml(htmlContent);

                // 测试原来的选择器
                Console.WriteLine("测试原来的选择器:");
                var titleNodes = doc.DocumentNode.SelectNodes("//div[@class='ellipsis title-text']");
                var pdfNodes = doc.DocumentNode.SelectNodes("//div[@class='pdf-ico']/a");

                Console.WriteLine($"titleNodes数量: {titleNodes?.Count ?? 0}");
                Console.WriteLine($"pdfNodes数量: {pdfNodes?.Count ?? 0}");

                // 测试其他可能的选择器
                Console.WriteLine("测试其他可能的选择器:");
                
                // 查找所有包含pdf的链接
                var pdfLinks = doc.DocumentNode.SelectNodes("//a[contains(@href, '.pdf')]");
                Console.WriteLine($"包含.pdf的链接数量: {pdfLinks?.Count ?? 0}");
                if (pdfLinks != null && pdfLinks.Count > 0)
                {
                    for (int i = 0; i < Math.Min(pdfLinks.Count, 5); i++)
                    {
                        var link = pdfLinks[i];
                        Console.WriteLine($"  {i + 1}. {link.GetAttributeValue("href", "")} - {link.InnerText.Trim()}");
                    }
                }

                // 查找所有包含"pdf"或"PDF"的链接
                var pdfTextLinks = doc.DocumentNode.SelectNodes("//a[contains(text(), 'pdf') or contains(text(), 'PDF')]");
                Console.WriteLine($"包含'pdf'或'PDF'的链接数量: {pdfTextLinks?.Count ?? 0}");

                // 查找所有包含"下载"的链接
                var downloadLinks = doc.DocumentNode.SelectNodes("//a[contains(text(), '下载')]");
                Console.WriteLine($"包含'下载'的链接数量: {downloadLinks?.Count ?? 0}");
                if (downloadLinks != null && downloadLinks.Count > 0)
                {
                    for (int i = 0; i < Math.Min(downloadLinks.Count, 5); i++)
                    {
                        var link = downloadLinks[i];
                        Console.WriteLine($"  {i + 1}. {link.GetAttributeValue("href", "")} - {link.InnerText.Trim()}");
                    }
                }

                // 查找所有包含"版面"的链接
                var sectionLinks = doc.DocumentNode.SelectNodes("//a[contains(text(), '版面')]");
                Console.WriteLine($"包含'版面'的链接数量: {sectionLinks?.Count ?? 0}");
                if (sectionLinks != null && sectionLinks.Count > 0)
                {
                    for (int i = 0; i < Math.Min(sectionLinks.Count, 5); i++)
                    {
                        var link = sectionLinks[i];
                        Console.WriteLine($"  {i + 1}. {link.GetAttributeValue("href", "")} - {link.InnerText.Trim()}");
                    }
                }

                // 查找所有包含"报纸"的链接
                var paperLinks = doc.DocumentNode.SelectNodes("//a[contains(text(), '报纸')]");
                Console.WriteLine($"包含'报纸'的链接数量: {paperLinks?.Count ?? 0}");
                if (paperLinks != null && paperLinks.Count > 0)
                {
                    for (int i = 0; i < Math.Min(paperLinks.Count, 5); i++)
                    {
                        var link = paperLinks[i];
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