using System;
using System.Net.Http;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace NewsPaperReader
{
    class TestScraper
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("中国教师报页面抓取测试");
            Console.WriteLine("======================");
            
            string url = "https://www.chinateacher.com.cn";
            
            try
            {
                using var httpClient = new HttpClient();
                httpClient.Timeout = TimeSpan.FromSeconds(30);
                
                // 获取页面内容
                Console.WriteLine($"正在抓取页面: {url}");
                var response = await httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();
                string html = await response.Content.ReadAsStringAsync();
                
                // 保存源代码到文件
                string fileName = $"chinateacher_{DateTime.Now:yyyyMMddHHmmss}.html";
                System.IO.File.WriteAllText(fileName, html);
                Console.WriteLine($"页面源代码已保存到: {fileName}");
                
                // 分析页面结构
                var doc = new HtmlAgilityPack.HtmlDocument();
                doc.LoadHtml(html);
                
                // 查找版面目录相关内容
                Console.WriteLine("\n分析页面结构...");
                
                // 查找包含"版"的文本
                var editionTexts = doc.DocumentNode.SelectNodes("//text()[contains(., '第') and contains(., '版')]");
                if (editionTexts != null)
                {
                    Console.WriteLine($"找到 {editionTexts.Count} 个版面相关文本:");
                    for (int i = 0; i < editionTexts.Count; i++)
                    {
                        var text = editionTexts[i].InnerText.Trim();
                        var parent = editionTexts[i].ParentNode;
                        Console.WriteLine($"{i+1}. {text}");
                        Console.WriteLine($"   父节点: {parent.Name}");
                        if (parent.Attributes.Count > 0)
                        {
                            Console.WriteLine("   属性:");
                            foreach (var attr in parent.Attributes)
                            {
                                Console.WriteLine($"     {attr.Name}: {attr.Value}");
                            }
                        }
                    }
                }
                else
                {
                    Console.WriteLine("未找到版面相关文本");
                }
                
                // 查找PDF链接
                var pdfLinks = doc.DocumentNode.SelectNodes("//a[contains(@href, '.pdf')]");
                if (pdfLinks != null)
                {
                    Console.WriteLine($"\n找到 {pdfLinks.Count} 个PDF链接:");
                    for (int i = 0; i < pdfLinks.Count; i++)
                    {
                        var href = pdfLinks[i].Attributes["href"]?.Value;
                        var text = pdfLinks[i].InnerText.Trim();
                        var parent = pdfLinks[i].ParentNode;
                        Console.WriteLine($"{i+1}. {href}");
                        Console.WriteLine($"   文本: {text}");
                        Console.WriteLine($"   父节点: {parent.Name}");
                    }
                }
                else
                {
                    Console.WriteLine("\n未找到PDF链接");
                }
                
                // 查找可能的版面列表容器
                var listContainers = doc.DocumentNode.SelectNodes("//ul|//ol|//div[@class='list']|//div[@class='edition-list']|//div[@class='section-list']");
                if (listContainers != null)
                {
                    Console.WriteLine($"\n找到 {listContainers.Count} 个可能的列表容器:");
                    for (int i = 0; i < Math.Min(10, listContainers.Count); i++)
                    {
                        var container = listContainers[i];
                        Console.WriteLine($"{i+1}. {container.Name}");
                        if (container.Attributes.Count > 0)
                        {
                            Console.WriteLine("   属性:");
                            foreach (var attr in container.Attributes)
                            {
                                Console.WriteLine($"     {attr.Name}: {attr.Value}");
                            }
                        }
                        // 显示容器内的前几个子节点
                        var children = container.ChildNodes;
                        int childCount = 0;
                        foreach (var child in children)
                        {
                            if (child.NodeType == HtmlAgilityPack.HtmlNodeType.Element)
                            {
                                Console.WriteLine($"   子节点: {child.Name}");
                                childCount++;
                                if (childCount >= 3) break;
                            }
                        }
                    }
                }
                
            }
            catch (Exception ex)
            {
                Console.WriteLine($"错误: {ex.Message}");
                Console.WriteLine($"堆栈跟踪: {ex.StackTrace}");
            }
            
            Console.WriteLine("\n按任意键退出...");
            Console.ReadKey();
        }
    }
}