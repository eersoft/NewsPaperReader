using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HtmlDocument = HtmlAgilityPack.HtmlDocument;

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
                    GetPdfLinksByScriptContent       // 脚本内容策略
                };

                foreach (var strategy in strategies)
                {
                    var result = await strategy(doc, finalUrl);
                    if (result.Count > 0)
                    {
                        return result;
                    }
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
        /// <returns>(HTML内容, 最终URL)</returns>
        private async Task<(string?, string)> GetHtmlContentWithRedirect(string url)
        {
            try
            {
                // 处理HTTP重定向
                var response = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
                response.EnsureSuccessStatusCode();
                
                // 获取最终的URL（跟随HTTP重定向后的地址）
                string finalUrl = response.RequestMessage?.RequestUri?.ToString() ?? url;
                
                // 获取HTML内容
                string html = await response.Content.ReadAsStringAsync();
                var doc = new HtmlDocument();
                doc.LoadHtml(html);
                
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
                            finalUrl = new Uri(new Uri(finalUrl), urlPart).ToString();
                        }
                        else if (Uri.IsWellFormedUriString(urlPart, UriKind.Absolute))
                        {
                            finalUrl = urlPart;
                        }

                        // 重新请求新的URL
                        response = await _httpClient.GetAsync(finalUrl);
                        response.EnsureSuccessStatusCode();
                        html = await response.Content.ReadAsStringAsync();
                    }
                }
                
                // 处理简单的JavaScript重定向
                string jsRedirectUrl = ExtractJavaScriptRedirect(html, finalUrl);
                if (!string.IsNullOrWhiteSpace(jsRedirectUrl) && jsRedirectUrl != finalUrl)
                {
                    // 重新请求JavaScript重定向的URL
                    response = await _httpClient.GetAsync(jsRedirectUrl);
                    response.EnsureSuccessStatusCode();
                    html = await response.Content.ReadAsStringAsync();
                    finalUrl = jsRedirectUrl;
                }
                
                return (html, finalUrl);
            }
            catch
            {
                return (null, url);
            }
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
                    @"window\.location\.href\s*=\s*[""']([^""']+)[""']", 
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
                    @"window\.location\s*=\s*[""']([^""']+)[""']", 
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
                    @"location\.href\s*=\s*[""']([^""']+)[""']", 
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
                    @"location\s*=\s*[""']([^""']+)[""']", 
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
                    @"document\.location\s*=\s*[""']([^""']+)[""']", 
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
                var pdfRegex = new Regex(@"https?://[^""']*\.pdf[^""']*");
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
                    "//a[contains(text(), '版')]"
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
                                "//a[contains(text(), '下载')]"
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
                        "//a[contains(text(), '下载')]"
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