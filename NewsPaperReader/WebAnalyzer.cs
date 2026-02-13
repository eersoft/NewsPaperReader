using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HtmlDocument = HtmlAgilityPack.HtmlDocument;
using Microsoft.Web.WebView2.Core;
using System.Runtime.InteropServices;

namespace NewsPaperReader
{
    public class WebAnalyzer
    {
        private readonly HttpClient _httpClient;

        public WebAnalyzer()
        {
            _httpClient = new HttpClient();
            _httpClient.Timeout = TimeSpan.FromSeconds(30);
        }

        /// <summary>
        /// 智能分析网页，尝试获取报纸的标题目录及对应PDF链接
        /// </summary>
        /// <param name="url">报纸网站URL</param>
        /// <returns>版面标题和PDF链接的字典</returns>
        public async Task<Dictionary<string, string>> AnalyzeNewspaperPage(string url)
        {
            try
            {
                // 特殊处理华西都市报
                if (url.Contains("wccdaily.com.cn"))
                {
                    // 直接使用桌面版URL
                    var uri = new Uri(url);
                    string host = uri.GetLeftPart(UriPartial.Authority);
                    url = host + "/shtml/index_hxdsb.shtml";
                    
                    // 获取网页内容
                    var (wxHtmlContent, wxFinalUrl) = await GetHtmlContentWithRedirect(url);
                    if (string.IsNullOrWhiteSpace(wxHtmlContent))
                    {
                        return new Dictionary<string, string>();
                    }
                    
                    // 解析HTML
                    var wxDoc = new HtmlDocument();
                    wxDoc.LoadHtml(wxHtmlContent);
                    
                    // 华西都市报特殊处理
                    var result = await GetPdfLinksBySamePageStrategy(wxDoc, wxFinalUrl);
                    if (result.Count > 0)
                    {
                        return result;
                    }
                }
                // 特殊处理中国教师报
                else if (url.Contains("chinateacher.com.cn"))
                {
                    // 获取网页内容，启用JavaScript渲染
                    var (ctHtmlContent, ctFinalUrl) = await GetHtmlContentWithRedirect(url, true);
                    if (string.IsNullOrWhiteSpace(ctHtmlContent))
                    {
                        return new Dictionary<string, string>();
                    }
                    
                    // 解析HTML
                    var ctDoc = new HtmlDocument();
                    ctDoc.LoadHtml(ctHtmlContent);
                    
                    // 中国教师报特殊处理
                    var result = await GetPdfLinksForChinaTeacher(ctDoc, ctFinalUrl);
                    if (result.Count > 0)
                    {
                        return result;
                    }
                }

                // 获取网页内容，处理重定向
                var (htmlContent, finalUrl) = await GetHtmlContentWithRedirect(url);
                if (string.IsNullOrWhiteSpace(htmlContent))
                {
                    return new Dictionary<string, string>();
                }

                // 解析HTML
                var doc = new HtmlDocument();
                doc.LoadHtml(htmlContent);

                // 尝试多种策略获取PDF链接
                var strategies = new List<Func<HtmlDocument, string, Task<Dictionary<string, string>>>>
                {
                    GetPdfLinksBySamePageStrategy,   // 同一页面策略（适用于华西都市报等）
                    GetPdfLinksByMultiPageStrategy,  // 多页面策略（适用于人民日报等）
                    GetPdfLinksByInputValueStrategy, // Input值策略（适用于经济日报等）
                    GetPdfLinksByPdfExtension,       // PDF扩展名策略
                    GetPdfLinksByAnchorText,         // 链接文本策略
                    GetPdfLinksByDataAttributes,     // 数据属性策略
                    GetPdfLinksByFrameSrc,           // iframe源策略
                    GetPdfLinksByScriptContent,      // 脚本内容策略
                    GetPdfLinksByGenericListStrategy, // 通用列表策略
                    GetPdfLinksByTableStrategy,      // 表格策略
                    GetPdfLinksByCardStrategy        // 卡片式布局策略
                };

                // 存储所有策略的结果
                var allResults = new List<Dictionary<string, string>>();

                foreach (var strategy in strategies)
                {
                    var result = await strategy(doc, finalUrl);
                    if (result.Count > 0)
                    {
                        allResults.Add(result);
                    }
                }

                // 分析结果，选择最优的一个
                if (allResults.Count > 0)
                {
                    // 优先选择结果数量最多的
                    var bestResult = allResults.OrderByDescending(r => r.Count).First();
                    return bestResult;
                }

                return new Dictionary<string, string>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"分析网页时出错: {ex.Message}");
                return new Dictionary<string, string>();
            }
        }

        /// <summary>
        /// 获取网页HTML内容，处理重定向
        /// </summary>
        /// <param name="url">网页URL</param>
        /// <param name="requireJsRendering">是否需要JavaScript渲染</param>
        /// <returns>(HTML内容, 最终URL)</returns>
        private async Task<(string?, string)> GetHtmlContentWithRedirect(string url, bool requireJsRendering = false)
        {
            try
            {
                if (requireJsRendering)
                {
                    // 使用WebView2获取渲染后的HTML内容
                    var (jsHtml, jsFinalUrl) = await GetHtmlContentWithWebView2(url);
                    return (jsHtml, jsFinalUrl);
                }
                
                // 处理HTTP重定向
                var response = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
                response.EnsureSuccessStatusCode();
                
                // 获取最终的URL（跟随HTTP重定向后的地址）
                string redirectFinalUrl = response.RequestMessage?.RequestUri?.ToString() ?? url;
                
                // 获取HTML内容
                string redirectHtml = await response.Content.ReadAsStringAsync();
                var doc = new HtmlDocument();
                doc.LoadHtml(redirectHtml);
                
                // 处理meta refresh重定向
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
                            redirectFinalUrl = new Uri(new Uri(redirectFinalUrl), urlPart).ToString();
                        }
                        else if (Uri.IsWellFormedUriString(urlPart, UriKind.Absolute))
                        {
                            redirectFinalUrl = urlPart;
                        }

                        // 重新请求新的URL
                        response = await _httpClient.GetAsync(redirectFinalUrl);
                        response.EnsureSuccessStatusCode();
                        redirectHtml = await response.Content.ReadAsStringAsync();
                    }
                }
                
                // 处理简单的JavaScript重定向
                string jsRedirectUrl = ExtractJavaScriptRedirect(redirectHtml, redirectFinalUrl);
                if (!string.IsNullOrWhiteSpace(jsRedirectUrl) && jsRedirectUrl != redirectFinalUrl)
                {
                    // 重新请求JavaScript重定向的URL
                    response = await _httpClient.GetAsync(jsRedirectUrl);
                    response.EnsureSuccessStatusCode();
                    redirectHtml = await response.Content.ReadAsStringAsync();
                    redirectFinalUrl = jsRedirectUrl;
                }
                
                return (redirectHtml, redirectFinalUrl);
            }
            catch
            {
                return (null, url);
            }
        }
        
        /// <summary>
        /// 使用WebView2获取渲染后的HTML内容
        /// </summary>
        /// <param name="url">网页URL</param>
        /// <returns>(渲染后的HTML内容, 最终URL)</returns>
        private async Task<(string?, string)> GetHtmlContentWithWebView2(string url)
        {
            try
            {
                // 创建WebView2环境
                var environment = await CoreWebView2Environment.CreateAsync();
                
                // 创建WebView2控制器
                var controller = await environment.CreateCoreWebView2ControllerAsync(IntPtr.Zero);
                
                // 等待导航完成
                var navigationCompletedTcs = new System.Threading.Tasks.TaskCompletionSource<bool>();
                controller.CoreWebView2.NavigationCompleted += (sender, e) =>
                {
                    navigationCompletedTcs.TrySetResult(e.IsSuccess);
                };
                
                // 导航到URL
                controller.CoreWebView2.Navigate(url);
                
                // 等待导航完成，最多等待30秒
                var navigationSuccess = await System.Threading.Tasks.Task.WhenAny(
                    navigationCompletedTcs.Task,
                    System.Threading.Tasks.Task.Delay(TimeSpan.FromSeconds(30))
                ) == navigationCompletedTcs.Task && await navigationCompletedTcs.Task;
                
                if (!navigationSuccess)
                {
                    // 导航失败或超时，使用后备方案
                    var response = await _httpClient.GetAsync(url);
                    response.EnsureSuccessStatusCode();
                    string fallbackHtml = await response.Content.ReadAsStringAsync();
                    string fallbackFinalUrl = response.RequestMessage?.RequestUri?.ToString() ?? url;
                    
                    // 尝试执行一些简单的JavaScript模拟
                    fallbackHtml = ExecuteSimpleJavaScript(fallbackHtml);
                    
                    return (fallbackHtml, fallbackFinalUrl);
                }
                
                // 获取渲染后的HTML内容
                string htmlContent = await controller.CoreWebView2.ExecuteScriptAsync("document.documentElement.outerHTML");
                
                // 移除JavaScript字符串引号和转义
                if (!string.IsNullOrEmpty(htmlContent) && htmlContent.StartsWith("\""))
                {
                    htmlContent = htmlContent.Substring(1, htmlContent.Length - 2);
                    htmlContent = System.Text.RegularExpressions.Regex.Unescape(htmlContent);
                }
                
                // 获取最终URL
                string finalUrl = controller.CoreWebView2.Source;
                
                return (htmlContent, finalUrl);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"WebView2渲染出错: {ex.Message}");
                
                // 出错时使用后备方案
                try
                {
                    var response = await _httpClient.GetAsync(url);
                    response.EnsureSuccessStatusCode();
                    string errorHtml = await response.Content.ReadAsStringAsync();
                    string errorFinalUrl = response.RequestMessage?.RequestUri?.ToString() ?? url;
                    
                    // 尝试执行一些简单的JavaScript模拟
                    errorHtml = ExecuteSimpleJavaScript(errorHtml);
                    
                    return (errorHtml, errorFinalUrl);
                }
                catch
                {
                    return (null, url);
                }
            }
        }
        
        /// <summary>
        /// 执行简单的JavaScript模拟
        /// </summary>
        /// <param name="html">原始HTML内容</param>
        /// <returns>处理后的HTML内容</returns>
        private string ExecuteSimpleJavaScript(string html)
        {
            // 尝试处理一些简单的JavaScript
            // 例如，处理document.write调用
            var doc = new HtmlDocument();
            doc.LoadHtml(html);
            
            // 查找并处理script标签
            var scripts = doc.DocumentNode.SelectNodes("//script");
            if (scripts != null)
            {
                foreach (var script in scripts)
                {
                    var content = script.InnerText;
                    if (!string.IsNullOrWhiteSpace(content))
                    {
                        // 尝试处理document.write
                        if (content.Contains("document.write"))
                        {
                            // 简单处理document.write调用
                            var docWriteRegex = new Regex(@"document\.write\(['""]([^'""]+)['""]\);", RegexOptions.IgnoreCase);
                            var matches = docWriteRegex.Matches(content);
                            foreach (Match match in matches)
                            {
                                if (match.Groups.Count > 1)
                                {
                                    var writeContent = match.Groups[1].Value;
                                    // 替换script标签为写入的内容
                                    script.ParentNode.ReplaceChild(HtmlNode.CreateNode(writeContent), script);
                                }
                            }
                        }
                    }
                }
            }
            
            return doc.DocumentNode.OuterHtml;
        }

        /// <summary>
        /// 从HTML内容中提取JavaScript重定向URL
        /// </summary>
        /// <param name="html">HTML内容</param>
        /// <param name="baseUrl">基础URL</param>
        /// <returns>重定向URL，如果没有找到则返回空字符串</returns>
        private string ExtractJavaScriptRedirect(string html, string baseUrl)
        {
            try
            {
                // 查找window.location.href重定向
                var locationRegex = new System.Text.RegularExpressions.Regex(
                    @"window\.location\.href\s*=\s*['""]([^'""]+)['""]", 
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase
                );
                var locationMatch = locationRegex.Match(html);
                if (locationMatch.Success)
                {
                    string redirectUrl = locationMatch.Groups[1].Value;
                    return MakeAbsoluteUrl(redirectUrl, baseUrl);
                }

                // 查找window.location重定向
                var locationRegex2 = new System.Text.RegularExpressions.Regex(
                    @"window\.location\s*=\s*['""]([^'""]+)['""]", 
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase
                );
                var locationMatch2 = locationRegex2.Match(html);
                if (locationMatch2.Success)
                {
                    string redirectUrl = locationMatch2.Groups[1].Value;
                    return MakeAbsoluteUrl(redirectUrl, baseUrl);
                }

                // 查找location.href重定向
                var locationRegex3 = new System.Text.RegularExpressions.Regex(
                    @"location\.href\s*=\s*['""]([^'""]+)['""]", 
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase
                );
                var locationMatch3 = locationRegex3.Match(html);
                if (locationMatch3.Success)
                {
                    string redirectUrl = locationMatch3.Groups[1].Value;
                    return MakeAbsoluteUrl(redirectUrl, baseUrl);
                }

                // 查找location重定向
                var locationRegex4 = new System.Text.RegularExpressions.Regex(
                    @"location\s*=\s*['""]([^'""]+)['""]", 
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase
                );
                var locationMatch4 = locationRegex4.Match(html);
                if (locationMatch4.Success)
                {
                    string redirectUrl = locationMatch4.Groups[1].Value;
                    return MakeAbsoluteUrl(redirectUrl, baseUrl);
                }

                // 查找document.location重定向
                var locationRegex5 = new System.Text.RegularExpressions.Regex(
                    @"document\.location\s*=\s*['""]([^'""]+)['""]", 
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase
                );
                var locationMatch5 = locationRegex5.Match(html);
                if (locationMatch5.Success)
                {
                    string redirectUrl = locationMatch5.Groups[1].Value;
                    return MakeAbsoluteUrl(redirectUrl, baseUrl);
                }

                // 特殊处理华西都市报的重定向
                if (html.Contains("wccdaily.com.cn"))
                {
                    // 华西都市报的重定向逻辑
                    var uri = new Uri(baseUrl);
                    string host = uri.GetLeftPart(UriPartial.Authority);
                    // 直接返回桌面版URL
                    return host + "/shtml/index_hxdsb.shtml";
                }

                return string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// 获取网页HTML内容
        /// </summary>
        /// <param name="url">网页URL</param>
        /// <returns>HTML内容</returns>
        private async Task<string?> GetHtmlContent(string url)
        {
            try
            {
                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStringAsync();
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 通过脚本内容获取PDF链接
        /// </summary>
        private async Task<Dictionary<string, string>> GetPdfLinksByScriptContent(HtmlDocument doc, string baseUrl)
        {
            var result = new Dictionary<string, string>();
            var scripts = doc.DocumentNode.SelectNodes("//script");
            
            if (scripts != null)
            {
                var pdfRegex = new Regex(@"https?://[^'""]*\.pdf[^'""]*");
                int index = 1;
                
                foreach (var script in scripts)
                {
                    var content = script.InnerText;
                    if (!string.IsNullOrWhiteSpace(content))
                    {
                        var matches = pdfRegex.Matches(content);
                        foreach (Match match in matches)
                        {
                            var pdfUrl = match.Value;
                            var title = $"PDF文件 {index}";
                            index++;
                            
                            if (!result.ContainsKey(title))
                            {
                                result.Add(title, pdfUrl);
                            }
                        }
                    }
                }
            }
            
            return result;
        }

        /// <summary>
        /// 多页面策略（适用于人民日报等）：先获取版面列表，然后逐个页面获取PDF链接
        /// </summary>
        private async Task<Dictionary<string, string>> GetPdfLinksByMultiPageStrategy(HtmlDocument doc, string baseUrl)
        {
            var result = new Dictionary<string, string>();
            
            // 尝试多种版面链接的XPath
            var pageLinkXPaths = new[]
            {
                "//*[@id='pageLink']",
                "//*[@id='APP-SectionNav']/li[*]/a",
                "//*[@class='titlebox']/span",
                "//li[1]/a",
                "//a[contains(@href, 'node')]",
                "//a[contains(@href, 'page')]",
                "//a[contains(text(), '版')]",
                "//ul[@class='section-list']/li/a",
                "//ul[@class='page-list']/li/a",
                "//div[@class='section-nav']/a",
                "//div[@class='page-nav']/a",
                "//nav[@class='section-nav']/a",
                "//nav[@class='page-nav']/a"
            };
            
            // 尝试所有可能的版面链接XPath，收集所有版面链接
            var allPageLinks = new Dictionary<string, string>();
            foreach (var xpath in pageLinkXPaths)
            {
                var titles = doc.DocumentNode.SelectNodes(xpath);
                if (titles != null && titles.Count > 0)
                {
                    foreach (var title in titles)
                    {
                        var href = title.Attributes["href"]?.Value;
                        if (!string.IsNullOrWhiteSpace(href))
                        {
                            var absoluteUrl = MakeAbsoluteUrl(href, baseUrl);
                            var linkText = title.InnerText.Trim();
                            if (!string.IsNullOrWhiteSpace(linkText) && !allPageLinks.ContainsKey(linkText))
                            {
                                allPageLinks.Add(linkText, absoluteUrl);
                            }
                        }
                    }
                }
            }
            
            // 逐个版面获取PDF链接
            if (allPageLinks.Count > 0)
            {
                foreach (var page in allPageLinks)
                {
                    var pageHtml = await GetHtmlContent(page.Value);
                    if (!string.IsNullOrWhiteSpace(pageHtml))
                    {
                        var pageDoc = new HtmlDocument();
                        pageDoc.LoadHtml(pageHtml);
                        
                        // 尝试多种PDF链接的XPath
                        var pdfLinkXPaths = new[]
                        {
                            "//*[@id='main']/div[1]/div[2]/p[2]/a",
                            "//html/body/div[2]/div[1]/div[2]/p[2]/a",
                            "//*[@id='pdf_toolbar']/a",
                            "//*[@class='pdf']/a",
                            "//*[@id='APP-Pdf']",
                            "//a[contains(@href, '.pdf')]",
                            "//a[contains(text(), 'PDF')]",
                            "//a[contains(text(), 'pdf')]",
                            "//a[contains(text(), '下载')]",
                            "//div[@class='pdf-download']/a",
                            "//div[@class='download-pdf']/a",
                            "//span[@class='pdf']/a",
                            "//span[@class='download']/a"
                        };
                        
                        foreach (var pdfXpath in pdfLinkXPaths)
                        {
                            var pdfLinks = pageDoc.DocumentNode.SelectNodes(pdfXpath);
                            if (pdfLinks != null && pdfLinks.Count > 0)
                            {
                                foreach (var pdfLink in pdfLinks)
                                {
                                    var pdfHref = pdfLink.Attributes["href"]?.Value;
                                    if (!string.IsNullOrWhiteSpace(pdfHref))
                                    {
                                        var absolutePdfUrl = MakeAbsoluteUrl(pdfHref, page.Value);
                                        if (!result.ContainsKey(page.Key))
                                        {
                                            result.Add(page.Key, absolutePdfUrl);
                                        }
                                    }
                                }
                                break;
                            }
                        }
                    }
                }
            }
            
            // 如果没有找到版面链接，尝试直接在当前页面查找PDF链接
            if (result.Count == 0)
            {
                var pdfLinkXPaths = new[]
                {
                    "//a[contains(@href, '.pdf')]",
                    "//a[contains(text(), 'PDF')]",
                    "//a[contains(text(), 'pdf')]",
                    "//a[contains(text(), '下载')]",
                    "//div[@class='pdf-download']/a",
                    "//div[@class='download-pdf']/a",
                    "//span[@class='pdf']/a",
                    "//span[@class='download']/a"
                };
                
                foreach (var pdfXpath in pdfLinkXPaths)
                {
                    var pdfLinks = doc.DocumentNode.SelectNodes(pdfXpath);
                    if (pdfLinks != null && pdfLinks.Count > 0)
                    {
                        foreach (var pdfLink in pdfLinks)
                        {
                            var pdfHref = pdfLink.Attributes["href"]?.Value;
                            var linkText = pdfLink.InnerText.Trim();
                            if (!string.IsNullOrWhiteSpace(pdfHref) && !string.IsNullOrWhiteSpace(linkText))
                            {
                                var absolutePdfUrl = MakeAbsoluteUrl(pdfHref, baseUrl);
                                if (!result.ContainsKey(linkText))
                                {
                                    result.Add(linkText, absolutePdfUrl);
                                }
                            }
                        }
                    }
                }
            }
            
            return result;
        }

        /// <summary>
        /// 同一页面策略（适用于华西都市报等）：所有版面和PDF链接都在同一页面
        /// </summary>
        private async Task<Dictionary<string, string>> GetPdfLinksBySamePageStrategy(HtmlDocument doc, string baseUrl)
        {
            var result = new Dictionary<string, string>();
            
            // 尝试多种同一页面的结构
            
            // 结构1: 标题和PDF链接分开的列表
            var titleNodes = doc.DocumentNode.SelectNodes("//div[@class='ellipsis title-text']");
            var pdfNodes = doc.DocumentNode.SelectNodes("//div[@class='pdf-ico']/a");
            if (titleNodes != null && pdfNodes != null && titleNodes.Count == pdfNodes.Count)
            {
                for (int i = 0; i < titleNodes.Count; i++)
                {
                    var title = titleNodes[i].InnerText.Trim();
                    var pdfHref = pdfNodes[i].Attributes["href"]?.Value;
                    if (!string.IsNullOrWhiteSpace(title) && !string.IsNullOrWhiteSpace(pdfHref))
                    {
                        var absolutePdfUrl = MakeAbsoluteUrl(pdfHref, baseUrl);
                        if (!result.ContainsKey(title))
                        {
                            result.Add(title, absolutePdfUrl);
                        }
                    }
                }
                if (result.Count > 0)
                {
                    return result;
                }
            }
            
            // 结构2: 右侧标题和PDF链接
            titleNodes = doc.DocumentNode.SelectNodes("//div[@class='right_title-name']/a");
            pdfNodes = doc.DocumentNode.SelectNodes("//div[@class='right_title-pdf']/a");
            if (titleNodes != null && pdfNodes != null && titleNodes.Count == pdfNodes.Count)
            {
                for (int i = 0; i < titleNodes.Count; i++)
                {
                    var title = titleNodes[i].InnerText.Trim();
                    var pdfHref = pdfNodes[i].Attributes["href"]?.Value;
                    if (!string.IsNullOrWhiteSpace(title) && !string.IsNullOrWhiteSpace(pdfHref))
                    {
                        var absolutePdfUrl = MakeAbsoluteUrl(pdfHref, baseUrl);
                        if (!result.ContainsKey(title))
                        {
                            result.Add(title, absolutePdfUrl);
                        }
                    }
                }
                if (result.Count > 0)
                {
                    return result;
                }
            }
            
            // 结构3: Chunkiconlist结构（适用于三峡都市报等）
            var chunkNodes = doc.DocumentNode.SelectNodes("//div[@class='Chunkiconlist']/p");
            if (chunkNodes != null)
            {
                foreach (var node in chunkNodes)
                {
                    var titleNode = node.SelectSingleNode("a[1]");
                    var pdfNode = node.SelectSingleNode("a[2]");
                    if (titleNode != null && pdfNode != null)
                    {
                        var title = titleNode.InnerText.Trim();
                        var pdfHref = pdfNode.Attributes["href"]?.Value;
                        if (!string.IsNullOrWhiteSpace(title) && !string.IsNullOrWhiteSpace(pdfHref))
                        {
                            var absolutePdfUrl = MakeAbsoluteUrl(pdfHref, baseUrl);
                            if (!result.ContainsKey(title))
                            {
                                result.Add(title, absolutePdfUrl);
                            }
                        }
                    }
                }
                if (result.Count > 0)
                {
                    return result;
                }
            }
            
            // 结构4: 表格结构（适用于重庆日报等）
            var trNodes = doc.DocumentNode.SelectNodes("//tr");
            if (trNodes != null)
            {
                foreach (var node in trNodes)
                {
                    var titleNode = node.SelectSingleNode(".//span[contains(@class, 'default')]");
                    var pdfLinkNode = node.SelectSingleNode(".//a[contains(@href, '.pdf')]");
                    if (titleNode != null && pdfLinkNode != null)
                    {
                        var title = titleNode.InnerText.Trim();
                        var pdfHref = pdfLinkNode.Attributes["href"]?.Value;
                        if (!string.IsNullOrWhiteSpace(title) && !string.IsNullOrWhiteSpace(pdfHref))
                        {
                            var absolutePdfUrl = MakeAbsoluteUrl(pdfHref, baseUrl);
                            if (!result.ContainsKey(title))
                            {
                                result.Add(title, absolutePdfUrl);
                            }
                        }
                    }
                }
                if (result.Count > 0)
                {
                    return result;
                }
            }
            
            return result;
        }

        /// <summary>
        /// Input值策略（适用于经济日报等）：PDF链接存储在input元素的value属性中
        /// </summary>
        private async Task<Dictionary<string, string>> GetPdfLinksByInputValueStrategy(HtmlDocument doc, string baseUrl)
        {
            var result = new Dictionary<string, string>();
            
            // 经济日报结构
            var titles = doc.DocumentNode.SelectNodes("//*[@id='layoutlist']/li[*]/a");
            var pdfInputs = doc.DocumentNode.SelectNodes("//li[@class='posRelative']/input");
            if (titles != null && pdfInputs != null && titles.Count == pdfInputs.Count)
            {
                for (int i = 0; i < titles.Count; i++)
                {
                    var title = titles[i].InnerText.Trim();
                    var pdfValue = pdfInputs[i].Attributes["value"]?.Value;
                    if (!string.IsNullOrWhiteSpace(title) && !string.IsNullOrWhiteSpace(pdfValue))
                    {
                        var absolutePdfUrl = MakeAbsoluteUrl(pdfValue, baseUrl);
                        if (!result.ContainsKey(title))
                        {
                            result.Add(title, absolutePdfUrl);
                        }
                    }
                }
                if (result.Count > 0)
                {
                    return result;
                }
            }
            
            return result;
        }

        /// <summary>
        /// 通过PDF文件扩展名获取链接
        /// </summary>
        private async Task<Dictionary<string, string>> GetPdfLinksByPdfExtension(HtmlDocument doc, string baseUrl)
        {
            var result = new Dictionary<string, string>();
            var links = doc.DocumentNode.SelectNodes("//a[contains(@href, '.pdf')]");
            
            if (links != null)
            {
                foreach (var link in links)
                {
                    var href = link.GetAttributeValue("href", string.Empty);
                    var text = link.InnerText.Trim();
                    
                    if (!string.IsNullOrWhiteSpace(href) && !string.IsNullOrWhiteSpace(text))
                    {
                        var absoluteUrl = MakeAbsoluteUrl(href, baseUrl);
                        if (!result.ContainsKey(text))
                        {
                            result.Add(text, absoluteUrl);
                        }
                    }
                }
            }
            
            return result;
        }

        /// <summary>
        /// 通过链接文本获取PDF链接
        /// </summary>
        private async Task<Dictionary<string, string>> GetPdfLinksByAnchorText(HtmlDocument doc, string baseUrl)
        {
            var result = new Dictionary<string, string>();
            var keywords = new[] { "pdf", "PDF", "下载", "Download", "版面", "Edition" };
            
            foreach (var keyword in keywords)
            {
                var links = doc.DocumentNode.SelectNodes($"//a[contains(text(), '{keyword}')]");
                if (links != null)
                {
                    foreach (var link in links)
                    {
                        var href = link.GetAttributeValue("href", string.Empty);
                        var text = link.InnerText.Trim();
                        
                        if (!string.IsNullOrWhiteSpace(href))
                        {
                            var absoluteUrl = MakeAbsoluteUrl(href, baseUrl);
                            if (!result.ContainsKey(text))
                            {
                                result.Add(text, absoluteUrl);
                            }
                        }
                    }
                }
            }
            
            return result;
        }

        /// <summary>
        /// 通过数据属性获取PDF链接
        /// </summary>
        private async Task<Dictionary<string, string>> GetPdfLinksByDataAttributes(HtmlDocument doc, string baseUrl)
        {
            var result = new Dictionary<string, string>();
            var links = doc.DocumentNode.SelectNodes("//a[@data-pdf or @data-file or @data-document]");
            
            if (links != null)
            {
                foreach (var link in links)
                {
                    var href = link.GetAttributeValue("href", string.Empty);
                    var dataPdf = link.GetAttributeValue("data-pdf", string.Empty);
                    var dataFile = link.GetAttributeValue("data-file", string.Empty);
                    var dataDocument = link.GetAttributeValue("data-document", string.Empty);
                    var text = link.InnerText.Trim();
                    
                    var pdfUrl = string.IsNullOrWhiteSpace(href) ? 
                        (string.IsNullOrWhiteSpace(dataPdf) ? 
                            (string.IsNullOrWhiteSpace(dataFile) ? dataDocument : dataFile) : dataPdf) : href;
                    
                    if (!string.IsNullOrWhiteSpace(pdfUrl) && !string.IsNullOrWhiteSpace(text))
                    {
                        var absoluteUrl = MakeAbsoluteUrl(pdfUrl, baseUrl);
                        if (!result.ContainsKey(text))
                        {
                            result.Add(text, absoluteUrl);
                        }
                    }
                }
            }
            
            return result;
        }

        /// <summary>
        /// 通过iframe的src属性获取PDF链接
        /// </summary>
        private async Task<Dictionary<string, string>> GetPdfLinksByFrameSrc(HtmlDocument doc, string baseUrl)
        {
            var result = new Dictionary<string, string>();
            var frames = doc.DocumentNode.SelectNodes("//iframe[contains(@src, '.pdf')]");
            
            if (frames != null)
            {
                int index = 1;
                foreach (var frame in frames)
                {
                    var src = frame.GetAttributeValue("src", string.Empty);
                    if (!string.IsNullOrWhiteSpace(src))
                    {
                        var absoluteUrl = MakeAbsoluteUrl(src, baseUrl);
                        var title = frame.GetAttributeValue("title", string.Empty);
                        if (string.IsNullOrWhiteSpace(title))
                        {
                            title = $"PDF文件 {index}";
                            index++;
                        }
                        
                        if (!result.ContainsKey(title))
                        {
                            result.Add(title, absoluteUrl);
                        }
                    }
                }
            }
            
            return result;
        }

        /// <summary>
        /// 通用列表策略：处理常见的列表结构
        /// </summary>
        private async Task<Dictionary<string, string>> GetPdfLinksByGenericListStrategy(HtmlDocument doc, string baseUrl)
        {
            var result = new Dictionary<string, string>();
            
            // 尝试多种常见的列表结构
            var listSelectors = new[]
            {
                "//ul[@class='list']",
                "//ul[@class='news-list']",
                "//ul[@class='article-list']",
                "//div[@class='list']",
                "//div[@class='news-list']",
                "//div[@class='article-list']",
                "//div[@class='section-list']",
                "//div[@class='layout-list']",
                "//div[@class='page-list']"
            };
            
            foreach (var selector in listSelectors)
            {
                var listNodes = doc.DocumentNode.SelectNodes(selector);
                if (listNodes != null)
                {
                    foreach (var listNode in listNodes)
                    {
                        // 查找列表中的所有项
                        var items = listNode.SelectNodes(".//li|.//div[@class='item']|.//div[@class='news-item']|.//div[@class='article-item']");
                        if (items != null)
                        {
                            foreach (var item in items)
                            {
                                // 查找标题和PDF链接
                                var titleNode = item.SelectSingleNode(".//a[1]");
                                var pdfNode = item.SelectSingleNode(".//a[contains(@href, '.pdf')]|.//a[contains(text(), 'PDF')]|.//a[contains(text(), 'pdf')]|.//a[contains(text(), '下载')]");
                                
                                if (titleNode != null && pdfNode != null)
                                {
                                    var title = titleNode.InnerText.Trim();
                                    var pdfHref = pdfNode.Attributes["href"]?.Value;
                                    
                                    if (!string.IsNullOrWhiteSpace(title) && !string.IsNullOrWhiteSpace(pdfHref))
                                    {
                                        var absolutePdfUrl = MakeAbsoluteUrl(pdfHref, baseUrl);
                                        if (!result.ContainsKey(title))
                                        {
                                            result.Add(title, absolutePdfUrl);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            
            return result;
        }
        
        /// <summary>
        /// 表格策略：处理表格布局
        /// </summary>
        private async Task<Dictionary<string, string>> GetPdfLinksByTableStrategy(HtmlDocument doc, string baseUrl)
        {
            var result = new Dictionary<string, string>();
            
            // 查找所有表格
            var tables = doc.DocumentNode.SelectNodes("//table");
            if (tables != null)
            {
                foreach (var table in tables)
                {
                    // 查找表格中的所有行
                    var rows = table.SelectNodes(".//tr");
                    if (rows != null)
                    {
                        foreach (var row in rows)
                        {
                            // 查找行中的所有单元格
                            var cells = row.SelectNodes(".//td|.//th");
                            if (cells != null)
                            {
                                // 查找标题和PDF链接
                                var titleNode = row.SelectSingleNode(".//a[1]");
                                var pdfNode = row.SelectSingleNode(".//a[contains(@href, '.pdf')]|.//a[contains(text(), 'PDF')]|.//a[contains(text(), 'pdf')]|.//a[contains(text(), '下载')]");
                                
                                if (titleNode != null && pdfNode != null)
                                {
                                    var title = titleNode.InnerText.Trim();
                                    var pdfHref = pdfNode.Attributes["href"]?.Value;
                                    
                                    if (!string.IsNullOrWhiteSpace(title) && !string.IsNullOrWhiteSpace(pdfHref))
                                    {
                                        var absolutePdfUrl = MakeAbsoluteUrl(pdfHref, baseUrl);
                                        if (!result.ContainsKey(title))
                                        {
                                            result.Add(title, absolutePdfUrl);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            
            return result;
        }
        
        /// <summary>
        /// 卡片式布局策略：处理现代卡片式布局
        /// </summary>
        private async Task<Dictionary<string, string>> GetPdfLinksByCardStrategy(HtmlDocument doc, string baseUrl)
        {
            var result = new Dictionary<string, string>();
            
            // 尝试多种常见的卡片结构
            var cardSelectors = new[]
            {
                "//div[@class='card']",
                "//div[@class='news-card']",
                "//div[@class='article-card']",
                "//div[@class='section-card']",
                "//div[@class='page-card']",
                "//div[@class='card-item']",
                "//div[@class='news-item']",
                "//div[@class='article-item']"
            };
            
            foreach (var selector in cardSelectors)
            {
                var cardNodes = doc.DocumentNode.SelectNodes(selector);
                if (cardNodes != null)
                {
                    foreach (var cardNode in cardNodes)
                    {
                        // 查找标题和PDF链接
                        var titleNode = cardNode.SelectSingleNode(".//h2|.//h3|.//h4|.//h5|.//h6|.//div[@class='title']|.//a[1]");
                        var pdfNode = cardNode.SelectSingleNode(".//a[contains(@href, '.pdf')]|.//a[contains(text(), 'PDF')]|.//a[contains(text(), 'pdf')]|.//a[contains(text(), '下载')]");
                        
                        if (titleNode != null && pdfNode != null)
                        {
                            var title = titleNode.InnerText.Trim();
                            var pdfHref = pdfNode.Attributes["href"]?.Value;
                            
                            if (!string.IsNullOrWhiteSpace(title) && !string.IsNullOrWhiteSpace(pdfHref))
                            {
                                var absolutePdfUrl = MakeAbsoluteUrl(pdfHref, baseUrl);
                                if (!result.ContainsKey(title))
                                {
                                    result.Add(title, absolutePdfUrl);
                                }
                            }
                        }
                    }
                }
            }
            
            return result;
        }
        
        /// <summary>
        /// 中国教师报特殊处理
        /// </summary>
        private async Task<Dictionary<string, string>> GetPdfLinksForChinaTeacher(HtmlDocument doc, string baseUrl)
        {
            var result = new Dictionary<string, string>();
            
            // 打印调试信息
            Console.WriteLine("开始处理中国教师报页面");
            Console.WriteLine($"当前URL: {baseUrl}");
            
            // 保存页面源代码到文件，以便分析
            try
            {
                string fileName = $"chinateacher_source_{DateTime.Now:yyyyMMddHHmmss}.html";
                System.IO.File.WriteAllText(fileName, doc.DocumentNode.OuterHtml);
                Console.WriteLine($"页面源代码已保存到: {fileName}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"保存页面源代码时出错: {ex.Message}");
            }
            
            // 尝试多种可能的结构
            
            // 结构1: 常见的版面列表结构 - 增强版
            var sectionListXPaths = new[]
            {
                "//ul[@class='section-list']",
                "//ul[@class='page-list']",
                "//div[@class='section-list']",
                "//div[@class='page-list']",
                "//div[@class='layout-list']",
                "//div[@class='news-list']",
                "//ul[@class='list']",
                "//div[@class='list']",
                "//div[@class='container']",
                "//div[@class='content']",
                "//div[@class='main']",
                "//div[@class='epaper']",
                "//div[@class='paper']",
                "//div[@class='edition']",
                "//div[@class='editions']",
                "//ul[@class='epaper-list']",
                "//ul[@class='paper-list']",
                "//ul[@class='edition-list']",
                "//div[@class='epaper-list']",
                "//div[@class='paper-list']",
                "//div[@class='edition-list']",
                "//div[@class='edu-paper']",
                "//div[@class='teacher-paper']",
                "//ul[@class='edu-paper-list']",
                "//ul[@class='teacher-paper-list']",
                "//div[@class='epaper-content']",
                "//div[@class='paper-content']",
                "//div[@class='edition-content']",
                "//div[@class='news-content']",
                "//div[@class='article-content']",
                "//div[@class='content-container']",
                "//div[@class='main-content']",
                "//div[@class='body-content']",
                "//div[@class='site-content']",
                "//div[@class='page-content']",
                "//div[@class='editions-content']",
                "//div[@class='papers-content']",
                "//div[@class='sections-content']",
                "//div[@class='layout-content']"
            };
            
            foreach (var xpath in sectionListXPaths)
            {
                var listNodes = doc.DocumentNode.SelectNodes(xpath);
                if (listNodes != null)
                {
                    Console.WriteLine($"找到 {listNodes.Count} 个匹配 {xpath} 的节点");
                    foreach (var listNode in listNodes)
                    {
                        var items = listNode.SelectNodes(".//li|.//div[@class='item']|.//div[@class='news-item']|.//div[@class='article-item']|.//div[@class='section-item']|.//div[@class='epaper-item']|.//div[@class='paper-item']|.//div[@class='edition-item']|.//div[@class='edu-item']|.//div[@class='teacher-item']|.//div[@class='list-item']|.//div[@class='content-item']");
                        if (items != null)
                        {
                            Console.WriteLine($"在 {xpath} 中找到 {items.Count} 个列表项");
                            foreach (var item in items)
                            {
                                // 查找标题节点 - 针对"第01版 标题"格式
                                var titleNode = item.SelectSingleNode(".//a[1]|.//h2|.//h3|.//h4|.//h5|.//h6|.//div[@class='title']|.//div[@class='name']|.//div[@class='edition']|.//div[@class='epaper']|.//div[@class='paper']|.//div[@class='edu-title']|.//div[@class='teacher-title']|.//span[@class='title']|.//p[@class='title']|.//div[@class='section-title']|.//div[@class='page-title']|.//span[@class='edition']|.//span[@class='epaper']|.//span[@class='paper']|.//p[@class='edition']|.//p[@class='epaper']|.//p[@class='paper']");
                                
                                // 查找PDF链接节点 - 针对PDF图标超链接
                                var pdfNode = item.SelectSingleNode(".//a[contains(@href, '.pdf')]|.//a[contains(text(), 'PDF')]|.//a[contains(text(), 'pdf')]|.//a[contains(text(), '下载')]|.//a[contains(@class, 'pdf')]|.//a[contains(@class, 'download')]|.//a[contains(@class, 'epaper')]|.//a[contains(@class, 'paper')]|.//a[contains(@class, 'edition')]|.//a[contains(@class, 'edu-pdf')]|.//a[contains(@class, 'teacher-pdf')]|.//a[contains(@title, 'PDF')]|.//a[contains(@title, 'pdf')]|.//a[contains(@title, '下载')]|.//a[contains(@alt, 'PDF')]|.//a[contains(@alt, 'pdf')]|.//a[contains(@alt, '下载')]|.//a[contains(@data-type, 'pdf')]|.//a[contains(@data-file, 'pdf')]");
                                
                                if (titleNode != null && pdfNode != null)
                                {
                                    var title = titleNode.InnerText.Trim();
                                    var pdfHref = pdfNode.Attributes["href"]?.Value;
                                    
                                    // 特别处理"第01版 标题"格式
                                    if (title.Contains("版") && !string.IsNullOrWhiteSpace(pdfHref))
                                    {
                                        var absolutePdfUrl = MakeAbsoluteUrl(pdfHref, baseUrl);
                                        if (!result.ContainsKey(title))
                                        {
                                            result.Add(title, absolutePdfUrl);
                                            Console.WriteLine($"找到版面: {title}, PDF链接: {absolutePdfUrl}");
                                        }
                                    }
                                    // 处理其他标题格式
                                    else if (!string.IsNullOrWhiteSpace(title) && !string.IsNullOrWhiteSpace(pdfHref))
                                    {
                                        var absolutePdfUrl = MakeAbsoluteUrl(pdfHref, baseUrl);
                                        if (!result.ContainsKey(title))
                                        {
                                            result.Add(title, absolutePdfUrl);
                                            Console.WriteLine($"找到版面: {title}, PDF链接: {absolutePdfUrl}");
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            
            // 结构2: 导航菜单中的版面链接 - 增强版
            var navXPaths = new[]
            {
                "//nav",
                "//div[@class='nav']",
                "//div[@class='navigation']",
                "//ul[@class='nav']",
                "//ul[@class='navigation']",
                "//div[@class='menu']",
                "//ul[@class='menu']",
                "//div[@class='header-nav']",
                "//nav[@class='main-nav']",
                "//div[@class='epaper-nav']",
                "//div[@class='paper-nav']",
                "//div[@class='edition-nav']",
                "//div[@class='edu-nav']",
                "//div[@class='teacher-nav']",
                "//div[@class='nav-container']",
                "//div[@class='nav-menu']",
                "//ul[@class='nav-menu']",
                "//div[@class='top-nav']",
                "//div[@class='main-navigation']",
                "//nav[@class='site-nav']",
                "//nav[@class='main-navigation']"
            };
            
            foreach (var xpath in navXPaths)
            {
                var navNodes = doc.DocumentNode.SelectNodes(xpath);
                if (navNodes != null)
                {
                    Console.WriteLine($"找到 {navNodes.Count} 个匹配 {xpath} 的导航节点");
                    foreach (var navNode in navNodes)
                    {
                        var links = navNode.SelectNodes(".//a");
                        if (links != null)
                        {
                            foreach (var link in links)
                            {
                                var href = link.Attributes["href"]?.Value;
                                var text = link.InnerText.Trim();
                                
                                if (!string.IsNullOrWhiteSpace(href) && !string.IsNullOrWhiteSpace(text))
                                {
                                    // 检查链接文本是否包含版面信息
                                    if (text.Contains("第") && text.Contains("版"))
                                    {
                                        Console.WriteLine($"找到版面链接: {text}, URL: {href}");
                                        // 尝试访问链接页面，查找PDF
                                        var linkUrl = MakeAbsoluteUrl(href, baseUrl);
                                        var linkHtml = await GetHtmlContent(linkUrl);
                                        
                                        if (!string.IsNullOrWhiteSpace(linkHtml))
                                        {
                                            var linkDoc = new HtmlDocument();
                                            linkDoc.LoadHtml(linkHtml);
                                            
                                            // 在链接页面中查找PDF
                                            var pdfLinks = linkDoc.DocumentNode.SelectNodes("//a[contains(@href, '.pdf')]|//a[contains(text(), 'PDF')]|//a[contains(text(), 'pdf')]|//a[contains(text(), '下载')]|//a[contains(@class, 'pdf')]|//a[contains(@class, 'download')]|//a[contains(@class, 'epaper')]|//a[contains(@class, 'paper')]|//a[contains(@class, 'edition')]|//a[contains(@class, 'edu-pdf')]|//a[contains(@class, 'teacher-pdf')]");
                                            if (pdfLinks != null)
                                            {
                                                foreach (var pdfLink in pdfLinks)
                                                {
                                                    var pdfHref = pdfLink.Attributes["href"]?.Value;
                                                    if (!string.IsNullOrWhiteSpace(pdfHref))
                                                    {
                                                        var absolutePdfUrl = MakeAbsoluteUrl(pdfHref, linkUrl);
                                                        if (!result.ContainsKey(text))
                                                        {
                                                            result.Add(text, absolutePdfUrl);
                                                            Console.WriteLine($"从导航链接找到版面: {text}, PDF链接: {absolutePdfUrl}");
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            
            // 结构3: 直接在当前页面查找所有PDF链接 - 增强版
            var directPdfLinks = doc.DocumentNode.SelectNodes("//a[contains(@href, '.pdf')]");
            if (directPdfLinks != null)
            {
                Console.WriteLine($"找到 {directPdfLinks.Count} 个直接PDF链接");
                int index = 1;
                foreach (var link in directPdfLinks)
                {
                    var href = link.Attributes["href"]?.Value;
                    var text = link.InnerText.Trim();
                    
                    if (!string.IsNullOrWhiteSpace(href))
                    {
                        // 尝试查找链接附近的标题文本，针对"第01版 标题    PDF图标"格式
                        string title = text;
                        
                        // 查找父节点中的标题
                        var parentItem = link.ParentNode.SelectSingleNode(".//div[@class='title']|.//h2|.//h3|.//h4|.//h5|.//h6|.//span[@class='title']|.//p[@class='title']|.//div[@class='edu-title']|.//div[@class='teacher-title']|.//div[@class='section-title']|.//div[@class='page-title']|.//span[@class='edition']|.//p[@class='edition']");
                        if (parentItem != null)
                        {
                            title = parentItem.InnerText.Trim();
                        }
                        // 查找前一个兄弟节点作为标题
                        else if (link.PreviousSibling != null && link.PreviousSibling.NodeType == HtmlAgilityPack.HtmlNodeType.Element)
                        {
                            var prevSibling = link.PreviousSibling;
                            if (prevSibling.InnerText.Trim().Contains("版"))
                            {
                                title = prevSibling.InnerText.Trim();
                            }
                        }
                        // 查找前一个兄弟文本节点作为标题
                        else if (link.PreviousSibling != null && link.PreviousSibling.NodeType == HtmlAgilityPack.HtmlNodeType.Text)
                        {
                            var prevText = link.PreviousSibling.InnerText.Trim();
                            if (prevText.Contains("版"))
                            {
                                title = prevText;
                            }
                        }
                        // 查找包含"版"的文本节点
                        else
                        {
                            var textNodes = link.ParentNode.SelectNodes(".//text()[contains(., '版')]");
                            if (textNodes != null && textNodes.Count > 0)
                            {
                                title = textNodes[0].InnerText.Trim();
                            }
                        }
                        
                        // 如果仍然没有标题，使用默认格式
                        if (string.IsNullOrWhiteSpace(title) || title.Equals("PDF", StringComparison.OrdinalIgnoreCase) || title.Equals("pdf", StringComparison.OrdinalIgnoreCase) || title.Equals("下载", StringComparison.OrdinalIgnoreCase))
                        {
                            title = $"版面 {index}";
                            index++;
                        }
                        
                        var absolutePdfUrl = MakeAbsoluteUrl(href, baseUrl);
                        if (!result.ContainsKey(title))
                        {
                            result.Add(title, absolutePdfUrl);
                            Console.WriteLine($"从直接PDF链接找到版面: {title}, PDF链接: {absolutePdfUrl}");
                        }
                    }
                }
            }
            
            // 结构4: 尝试直接访问常见的PDF页面 - 增强版
            var commonPdfPaths = new[]
            {
                "/pdf",
                "/PDF",
                "/download",
                "/Download",
                "/epaper",
                "/Epaper",
                "/paper",
                "/Paper",
                "/edition",
                "/Edition",
                "/editions",
                "/Editions",
                "/edu",
                "/Edu",
                "/teacher",
                "/Teacher",
                "/epaper/pdf",
                "/paper/pdf",
                "/edition/pdf",
                "/editions/pdf",
                "/edu/pdf",
                "/teacher/pdf",
                "/pdf/download",
                "/pdf/epaper",
                "/pdf/paper",
                "/pdf/edition",
                "/download/pdf",
                "/download/epaper",
                "/download/paper",
                "/download/edition",
                "/epaper/download",
                "/paper/download",
                "/edition/download",
                "/editions/download"
            };
            
            foreach (var path in commonPdfPaths)
            {
                try
                {
                    var pdfUrl = baseUrl.TrimEnd('/') + path;
                    Console.WriteLine($"尝试访问PDF页面: {pdfUrl}");
                    var pdfHtml = await GetHtmlContent(pdfUrl);
                    
                    if (!string.IsNullOrWhiteSpace(pdfHtml))
                    {
                        var pdfDoc = new HtmlDocument();
                        pdfDoc.LoadHtml(pdfHtml);
                        
                        var pdfLinks = pdfDoc.DocumentNode.SelectNodes("//a[contains(@href, '.pdf')]|//a[contains(text(), 'PDF')]|//a[contains(text(), 'pdf')]|//a[contains(text(), '下载')]|//a[contains(@class, 'pdf')]|//a[contains(@class, 'download')]|//a[contains(@class, 'epaper')]|//a[contains(@class, 'paper')]|//a[contains(@class, 'edition')]|//a[contains(@class, 'edu-pdf')]|//a[contains(@class, 'teacher-pdf')]");
                        if (pdfLinks != null)
                        {
                            Console.WriteLine($"在 {path} 中找到 {pdfLinks.Count} 个PDF链接");
                            foreach (var pdfLink in pdfLinks)
                            {
                                var href = pdfLink.Attributes["href"]?.Value;
                                var text = pdfLink.InnerText.Trim();
                                
                                if (!string.IsNullOrWhiteSpace(href) && !string.IsNullOrWhiteSpace(text))
                                {
                                    var absolutePdfUrl = MakeAbsoluteUrl(href, pdfUrl);
                                    if (!result.ContainsKey(text))
                                    {
                                        result.Add(text, absolutePdfUrl);
                                        Console.WriteLine($"从 {path} 找到版面: {text}, PDF链接: {absolutePdfUrl}");
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"尝试访问 {path} 时出错: {ex.Message}");
                }
            }
            
            // 结构5: 查找所有可能的链接，然后尝试从中提取PDF - 增强版
            var allLinks = doc.DocumentNode.SelectNodes("//a");
            if (allLinks != null)
            {
                Console.WriteLine($"找到 {allLinks.Count} 个链接");
                foreach (var link in allLinks)
                {
                    var href = link.Attributes["href"]?.Value;
                    var text = link.InnerText.Trim();
                    
                    if (!string.IsNullOrWhiteSpace(href) && !string.IsNullOrWhiteSpace(text))
                    {
                        // 检查链接是否直接指向PDF
                        if (href.Contains(".pdf", StringComparison.OrdinalIgnoreCase))
                        {
                            var absolutePdfUrl = MakeAbsoluteUrl(href, baseUrl);
                            if (!result.ContainsKey(text))
                            {
                                result.Add(text, absolutePdfUrl);
                                Console.WriteLine($"找到直接PDF链接: {text}, URL: {absolutePdfUrl}");
                            }
                        }
                        // 检查链接文本是否包含PDF相关词汇或版面信息
                        else if (text.Contains("PDF", StringComparison.OrdinalIgnoreCase) || text.Contains("pdf", StringComparison.OrdinalIgnoreCase) || text.Contains("下载", StringComparison.OrdinalIgnoreCase) || text.Contains("电子版", StringComparison.OrdinalIgnoreCase) || text.Contains("电子报", StringComparison.OrdinalIgnoreCase) || text.Contains("版面", StringComparison.OrdinalIgnoreCase) || (text.Contains("第", StringComparison.OrdinalIgnoreCase) && text.Contains("版", StringComparison.OrdinalIgnoreCase)))
                        {
                            var linkUrl = MakeAbsoluteUrl(href, baseUrl);
                            var linkHtml = await GetHtmlContent(linkUrl);
                            
                            if (!string.IsNullOrWhiteSpace(linkHtml))
                            {
                                var linkDoc = new HtmlDocument();
                                linkDoc.LoadHtml(linkHtml);
                                
                                var pdfLinks = linkDoc.DocumentNode.SelectNodes("//a[contains(@href, '.pdf')]|//a[contains(text(), 'PDF')]|//a[contains(text(), 'pdf')]|//a[contains(text(), '下载')]");
                                if (pdfLinks != null)
                                {
                                    foreach (var pdfLink in pdfLinks)
                                    {
                                        var pdfHref = pdfLink.Attributes["href"]?.Value;
                                        if (!string.IsNullOrWhiteSpace(pdfHref))
                                        {
                                            var absolutePdfUrl = MakeAbsoluteUrl(pdfHref, linkUrl);
                                            if (!result.ContainsKey(text))
                                            {
                                                result.Add(text, absolutePdfUrl);
                                                Console.WriteLine($"从链接 {text} 找到PDF: {absolutePdfUrl}");
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            
            // 结构6: 从脚本内容中提取PDF链接
            var scripts = doc.DocumentNode.SelectNodes("//script");
            if (scripts != null)
            {
                foreach (var script in scripts)
                {
                    var content = script.InnerText;
                    if (!string.IsNullOrWhiteSpace(content))
                    {
                        // 尝试从脚本中提取PDF链接
                        var pdfRegex = new Regex(@"https?://[^'""]*\.pdf[^'""]*");
                        var matches = pdfRegex.Matches(content);
                        foreach (Match match in matches)
                        {
                            var pdfUrl = match.Value;
                            var title = "PDF文件";
                            if (!result.ContainsKey(title))
                            {
                                result.Add(title, pdfUrl);
                            }
                        }
                    }
                }
            }
            
            // 结构7: 查找所有可能的容器，然后递归搜索PDF链接
            var containers = doc.DocumentNode.SelectNodes("//div|//section|//article|//main|//body");
            if (containers != null)
            {
                foreach (var container in containers)
                {
                    var pdfLinks = container.SelectNodes(".//a[contains(@href, '.pdf')]");
                    if (pdfLinks != null)
                    {
                        foreach (var pdfLink in pdfLinks)
                        {
                            var href = pdfLink.Attributes["href"]?.Value;
                            var text = pdfLink.InnerText.Trim();
                            
                            if (!string.IsNullOrWhiteSpace(href))
                            {
                                // 尝试查找容器中的标题文本，针对"第01版 标题    PDF图标"格式
                                string title = text;
                                
                                // 查找容器中的标题
                                var titleNode = container.SelectSingleNode(".//div[@class='title']|.//h2|.//h3|.//h4|.//h5|.//h6|.//span[@class='title']|.//p[@class='title']|.//div[@class='edu-title']|.//div[@class='teacher-title']|.//text()[contains(., '版')]");
                                if (titleNode != null)
                                {
                                    title = titleNode.InnerText.Trim();
                                }
                                
                                var absolutePdfUrl = MakeAbsoluteUrl(href, baseUrl);
                                if (!result.ContainsKey(title))
                                {
                                    result.Add(title, absolutePdfUrl);
                                }
                            }
                        }
                    }
                }
            }
            
            // 结构8: 专门针对中国教师报的"第01版 标题"格式
            var editionPatterns = doc.DocumentNode.SelectNodes("//text()[contains(., '第') and contains(., '版')]");
            if (editionPatterns != null)
            {
                foreach (var textNode in editionPatterns)
                {
                    var title = textNode.InnerText.Trim();
                    
                    // 查找包含该文本节点的父元素，然后在其中查找PDF链接
                    var parentElement = textNode.ParentNode;
                    while (parentElement != null && !parentElement.Name.Equals("body", StringComparison.OrdinalIgnoreCase))
                    {
                        var pdfLink = parentElement.SelectSingleNode(".//a[contains(@href, '.pdf')]|.//a[contains(text(), 'PDF')]|.//a[contains(text(), 'pdf')]|.//a[contains(text(), '下载')]|.//a[contains(@class, 'pdf')]|.//a[contains(@class, 'download')]");
                        if (pdfLink != null)
                        {
                            var pdfHref = pdfLink.Attributes["href"]?.Value;
                            if (!string.IsNullOrWhiteSpace(pdfHref))
                            {
                                var absolutePdfUrl = MakeAbsoluteUrl(pdfHref, baseUrl);
                                if (!result.ContainsKey(title))
                                {
                                    result.Add(title, absolutePdfUrl);
                                }
                            }
                            break;
                        }
                        parentElement = parentElement.ParentNode;
                    }
                }
            }
            
            // 结构9: 专门针对中国教师报的"第01版: 要闻"格式（基于用户提供的图片）
            var editionListItems = doc.DocumentNode.SelectNodes("//li|//div[@class='item']|//div[@class='edition-item']|//div[@class='paper-item']");
            if (editionListItems != null)
            {
                foreach (var listItem in editionListItems)
                {
                    // 查找包含"第01版: 要闻"格式的文本
                    var editionText = listItem.SelectSingleNode(".//text()[contains(., '第') and contains(., '版:')]");
                    if (editionText != null)
                    {
                        var title = editionText.InnerText.Trim();
                        
                        // 查找该列表项中的PDF链接
                        var pdfLink = listItem.SelectSingleNode(".//a[contains(@href, '.pdf')]|.//a[contains(@class, 'pdf')]|.//a[contains(@class, 'download')]|.//a[contains(@title, 'PDF')]|.//a[contains(@title, 'pdf')]");
                        if (pdfLink != null)
                        {
                            var pdfHref = pdfLink.Attributes["href"]?.Value;
                            if (!string.IsNullOrWhiteSpace(pdfHref))
                            {
                                var absolutePdfUrl = MakeAbsoluteUrl(pdfHref, baseUrl);
                                if (!result.ContainsKey(title))
                                {
                                    result.Add(title, absolutePdfUrl);
                                }
                            }
                        }
                    }
                }
            }
            
            // 结构10: 针对中国教师报的列表结构（基于用户提供的图片）
            var sectionLists = doc.DocumentNode.SelectNodes("//ul|//ol|//div[@class='list']|//div[@class='edition-list']|//div[@class='section-list']");
            if (sectionLists != null)
            {
                foreach (var list in sectionLists)
                {
                    // 查找列表中的所有项
                    var items = list.SelectNodes(".//li|.//div[@class='item']");
                    if (items != null)
                    {
                        foreach (var item in items)
                        {
                            // 查找包含版面信息的文本
                            var textContent = item.InnerText.Trim();
                            if (textContent.Contains("第") && textContent.Contains("版:"))
                            {
                                // 提取标题
                                var title = textContent;
                                
                                // 查找PDF链接
                                var pdfLink = item.SelectSingleNode(".//a[contains(@href, '.pdf')]|.//a[contains(@class, 'pdf')]|.//a[contains(@class, 'download')]|.//a[contains(@title, 'PDF')]|.//a[contains(@title, 'pdf')]|.//a[contains(@alt, 'PDF')]|.//a[contains(@alt, 'pdf')]");
                                if (pdfLink != null)
                                {
                                    var pdfHref = pdfLink.Attributes["href"]?.Value;
                                    if (!string.IsNullOrWhiteSpace(pdfHref))
                                    {
                                        var absolutePdfUrl = MakeAbsoluteUrl(pdfHref, baseUrl);
                                        if (!result.ContainsKey(title))
                                        {
                                            result.Add(title, absolutePdfUrl);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            
            // 结构11: 针对中国教师报的PDF图标链接 - 增强版
            var pdfIcons = doc.DocumentNode.SelectNodes("//a[contains(@href, '.pdf')]|//a[contains(@class, 'pdf')]|//a[contains(@class, 'download')]|//a[contains(@class, 'epaper')]|//a[contains(@class, 'paper')]|//a[contains(@class, 'edition')]");
            if (pdfIcons != null)
            {
                Console.WriteLine($"找到 {pdfIcons.Count} 个PDF图标链接");
                foreach (var pdfIcon in pdfIcons)
                {
                    var pdfHref = pdfIcon.Attributes["href"]?.Value;
                    if (!string.IsNullOrWhiteSpace(pdfHref))
                    {
                        // 查找该PDF链接对应的版面标题
                        var parentItem = pdfIcon.ParentNode;
                        while (parentItem != null && !parentItem.Name.Equals("body", StringComparison.OrdinalIgnoreCase))
                        {
                            // 查找包含版面信息的文本
                            var editionText = parentItem.SelectSingleNode(".//text()[contains(., '第') and contains(., '版:')]|.//text()[contains(., '第') and contains(., '版')]");
                            if (editionText != null)
                            {
                                var title = editionText.InnerText.Trim();
                                var absolutePdfUrl = MakeAbsoluteUrl(pdfHref, baseUrl);
                                if (!result.ContainsKey(title))
                                {
                                    result.Add(title, absolutePdfUrl);
                                    Console.WriteLine($"找到版面: {title}, PDF链接: {absolutePdfUrl}");
                                }
                                break;
                            }
                            parentItem = parentItem.ParentNode;
                        }
                    }
                }
            }
            
            // 结构12: 针对中国教师报的动态加载内容 - 增强版
            // 尝试执行一些可能的JavaScript，模拟动态加载
            var dynamicContent = ExecuteSimpleJavaScript(doc.DocumentNode.OuterHtml);
            var dynamicDoc = new HtmlDocument();
            dynamicDoc.LoadHtml(dynamicContent);
            
            var dynamicPdfLinks = dynamicDoc.DocumentNode.SelectNodes("//a[contains(@href, '.pdf')]");
            if (dynamicPdfLinks != null)
            {
                Console.WriteLine($"从动态内容中找到 {dynamicPdfLinks.Count} 个PDF链接");
                foreach (var link in dynamicPdfLinks)
                {
                    var href = link.Attributes["href"]?.Value;
                    var text = link.InnerText.Trim();
                    
                    if (!string.IsNullOrWhiteSpace(href))
                    {
                        var absolutePdfUrl = MakeAbsoluteUrl(href, baseUrl);
                        if (!result.ContainsKey(text))
                        {
                            result.Add(text, absolutePdfUrl);
                            Console.WriteLine($"从动态内容找到PDF链接: {text}, URL: {absolutePdfUrl}");
                        }
                    }
                }
            }
            
            Console.WriteLine($"处理完成，找到 {result.Count} 个版面和PDF链接");
            
            return result;
        }

        /// <summary>
        /// 将相对URL转换为绝对URL
        /// </summary>
        private string MakeAbsoluteUrl(string relativeUrl, string baseUrl)
        {
            if (string.IsNullOrWhiteSpace(relativeUrl))
            {
                return string.Empty;
            }
            
            if (relativeUrl.StartsWith("http://") || relativeUrl.StartsWith("https://"))
            {
                return relativeUrl;
            }
            
            try
            {
                var baseUri = new Uri(baseUrl);
                var absoluteUri = new Uri(baseUri, relativeUrl);
                return absoluteUri.AbsoluteUri;
            }
            catch
            {
                return relativeUrl;
            }
        }
    }
}