using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.SemanticKernel;

namespace StoneSharp.Core.Tools.BuiltIn;

/// <summary>
/// 网页内容抓取工具 - 基于WebFetchTool.ts重新实现
/// 从指定URL获取网页内容，支持缓存、权限控制、重定向安全检测
/// </summary>
public sealed class WebFetchTool
{
    private readonly HttpClient _httpClient;

    // 缓存 - 使用ConcurrentDictionary实现简易LRU缓存
    private static readonly ConcurrentDictionary<string, CacheEntry> UrlCache = new();
    private const int CacheTtlMinutes = 15;
    private const int MaxCacheEntries = 200;

    // 资源限制
    private const int MaxHttpContentLength = 10 * 1024 * 1024; // 10MB
    private const int FetchTimeoutMs = 60_000;
    private const int MaxRedirects = 10;
    private const int MaxMarkdownLength = 100_000;
    private const int MaxUrlLength = 2000;

    // 默认User-Agent
    private const string DefaultUserAgent =
        "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36";

    // 预授权域名白名单（代码/技术文档相关）
    private static readonly HashSet<string> PreapprovedHosts = new(StringComparer.OrdinalIgnoreCase)
    {
        // Anthropic
        "platform.claude.com",
        "code.claude.com",
        "modelcontextprotocol.io",
        "github.com",
        "agentskills.io",

        // 主流编程语言文档
        "docs.python.org",
        "en.cppreference.com",
        "docs.oracle.com",
        "learn.microsoft.com",
        "developer.mozilla.org",
        "go.dev",
        "pkg.go.dev",
        "www.php.net",
        "docs.swift.org",
        "kotlinlang.org",
        "ruby-doc.org",
        "doc.rust-lang.org",
        "www.typescriptlang.org",

        // Web & JavaScript 框架
        "react.dev",
        "angular.io",
        "vuejs.org",
        "nextjs.org",
        "expressjs.com",
        "nodejs.org",
        "bun.sh",

        // Python 框架
        "docs.djangoproject.com",
        "flask.palletsprojects.com",
        "fastapi.tiangolo.com",
        "pandas.pydata.org",
        "numpy.org",
        "www.tensorflow.org",
        "pytorch.org",
        "scikit-learn.org",

        // Java 生态
        "docs.spring.io",
        "hibernate.org",
        "gradle.org",
        "maven.apache.org",

        // .NET 生态
        "asp.net",
        "dotnet.microsoft.com",
        "nuget.org",

        // 云服务
        "docs.aws.amazon.com",
        "cloud.google.com",
        "kubernetes.io",
        "www.docker.com",

        // 数据库
        "www.postgresql.org",
        "dev.mysql.com",
        "www.sqlite.org",
        "redis.io",
        "www.mongodb.com",

        // 其他常用技术站点
        "git-scm.com",
        "nginx.org",
        "stackoverflow.com",
        "docs.github.com",
    };

    // HTML实体替换表
    private static readonly Dictionary<string, string> HtmlEntities = new(StringComparer.OrdinalIgnoreCase)
    {
        ["&amp;"] = "&",
        ["&lt;"] = "<",
        ["&gt;"] = ">",
        ["&quot;"] = "\"",
        ["&apos;"] = "'",
        ["&nbsp;"] = " ",
        ["&copy;"] = "©",
        ["&reg;"] = "®",
        ["&trade;"] = "™",
        ["&mdash;"] = "—",
        ["&ndash;"] = "–",
        ["&hellip;"] = "…",
        ["&laquo;"] = "«",
        ["&raquo;"] = "»",
        ["&bull;"] = "•",
        ["&middot;"] = "·",
        ["&deg;"] = "°",
        ["&plusmn;"] = "±",
        ["&times;"] = "×",
        ["&divide;"] = "÷",
        ["&frac12;"] = "½",
        ["&frac14;"] = "¼",
        ["&frac34;"] = "¾",
        ["&sect;"] = "§",
        ["&para;"] = "¶",
    };

    /// <summary>
    /// 构造函数
    /// </summary>
    public WebFetchTool() : this(null)
    {
    }

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="httpClient">HTTP客户端实例</param>
    public WebFetchTool(HttpClient? httpClient)
    {
        _httpClient = httpClient ?? new HttpClient();
        _httpClient.Timeout = TimeSpan.FromMilliseconds(FetchTimeoutMs);
        _httpClient.DefaultRequestHeaders.UserAgent.TryParseAdd(DefaultUserAgent);
        _httpClient.DefaultRequestHeaders.Accept.ParseAdd("text/markdown, text/html, */*");
    }

    /// <summary>
    /// 获取网页内容并处理
    /// 功能类似于Anthropic的WebFetchTool，支持URL抓取、内容提取和缓存
    /// </summary>
    [KernelFunction, Description("从指定URL获取网页内容并返回处理后的Markdown文本")]
    public async Task<string> FetchWebContentAsync(
        [Description("要获取内容的完整URL")] string url,
        [Description("可选的提取提示词，描述需要从页面中提取什么信息")] string? prompt = null,
        [Description("超时时间（秒，可选，默认30秒）")] int timeout = 30,
        CancellationToken cancellationToken = default)
    {
        var sw = Stopwatch.StartNew();
        try
        {
            // 1. 验证URL
            if (!ValidateUrl(url))
            {
                return $"错误：URL格式无效或不受支持 - {url}";
            }

            // 2. 自动升级 http -> https
            url = UpgradeToHttps(url);

            // 3. 检查缓存
            var cacheKey = $"WebFetch_{url}";
            if (UrlCache.TryGetValue(cacheKey, out var cached) && !cached.IsExpired)
            {
                sw.Stop();
                return FormatResult(url, cached.Content, cached.Bytes, cached.Code, cached.CodeText,
                    cached.ContentType, sw.ElapsedMilliseconds, true);
            }

            // 4. 执行HTTP请求（含安全重定向处理）
            var (response, redirectInfo) = await FetchWithRedirectHandlingAsync(url, timeout, cancellationToken);

            // 5. 处理重定向结果
            if (redirectInfo != null)
            {
                sw.Stop();
                return FormatRedirectMessage(redirectInfo.Value);
            }

            // 6. 读取响应内容
            using (response)
            {
                var responseBytes = await response!.Content.ReadAsByteArrayAsync(cancellationToken);
                var bytes = responseBytes.Length;
                var statusCode = (int)response.StatusCode;
                var codeText = response.ReasonPhrase ?? GetDefaultStatusText(response.StatusCode);
                var contentType = response.Content.Headers.ContentType?.MediaType ?? "application/octet-stream";

                // 7. 处理内容
                string processedContent;
                if (contentType.Contains("text/html", StringComparison.OrdinalIgnoreCase))
                {
                    var html = Encoding.UTF8.GetString(responseBytes);
                    processedContent = HtmlToMarkdown(html);

                    // 截断过长内容
                    if (processedContent.Length > MaxMarkdownLength)
                    {
                        processedContent = processedContent[..MaxMarkdownLength] +
                            "\n\n[内容因过长已被截断...]";
                    }
                }
                else if (contentType.Contains("text/", StringComparison.OrdinalIgnoreCase))
                {
                    processedContent = Encoding.UTF8.GetString(responseBytes);
                }
                else
                {
                    // 二进制内容 - 仅记录元数据
                    processedContent =
                        $"[二进制内容] 类型: {contentType}, 大小: {FileSizeFormatter.FormatFileSize(bytes)}";
                }

                // 8. 写入缓存（附带过期清理）
                var cacheEntry = new CacheEntry
                {
                    Content = processedContent,
                    Bytes = bytes,
                    Code = statusCode,
                    CodeText = codeText,
                    ContentType = contentType,
                    CreatedAt = DateTime.UtcNow,
                };
                UrlCache[cacheKey] = cacheEntry;
                EvictExpiredEntriesIfNeeded();

                sw.Stop();
                return FormatResult(url, processedContent, bytes, statusCode, codeText,
                    contentType, sw.ElapsedMilliseconds, false);
            }
        }
        catch (OperationCanceledException)
        {
            return $"请求已取消: {url}";
        }
        catch (HttpRequestException ex)
        {
            return $"HTTP请求失败: {ex.Message}";
        }
        catch (Exception ex)
        {
            return $"获取网页内容时出错: {ex.Message}";
        }
    }

    /// <summary>
    /// 执行带有安全重定向处理的HTTP请求
    /// </summary>
    private async Task<(HttpResponseMessage? Response, RedirectInfo? RedirectInfo)> FetchWithRedirectHandlingAsync(
        string url, int timeout, CancellationToken cancellationToken)
    {
        using var httpClient = CreateHttpClient(timeout);

        var currentUrl = url;
        var redirectDepth = 0;

        while (redirectDepth <= MaxRedirects)
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, currentUrl);

            var response = await httpClient.SendAsync(
                request,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken);

            // 检查是否为需要手动处理的重定向
            if (IsRedirectStatusCode(response.StatusCode))
            {
                var location = response.Headers.Location?.ToString();
                if (string.IsNullOrEmpty(location))
                {
                    response.Dispose();
                    throw new HttpRequestException("重定向响应缺少Location头");
                }

                var redirectUrl = new Uri(new Uri(currentUrl), location).ToString();

                // 安全检查：仅允许同源（±www）的重定向
                if (!IsPermittedRedirect(currentUrl, redirectUrl))
                {
                    var statusCode = (int)response.StatusCode;
                    var statusText = statusCode switch
                    {
                        301 => "Moved Permanently",
                        302 => "Found",
                        307 => "Temporary Redirect",
                        308 => "Permanent Redirect",
                        _ => "Redirect",
                    };

                    response.Dispose();

                    return (null, new RedirectInfo
                    {
                        OriginalUrl = currentUrl,
                        RedirectUrl = redirectUrl,
                        StatusCode = statusCode,
                        StatusText = statusText,
                    });
                }

                currentUrl = redirectUrl;
                redirectDepth++;
                response.Dispose();
                continue;
            }

            // 非重定向响应，返回结果
            response.EnsureSuccessStatusCode();
            return (response, null);
        }

        throw new HttpRequestException($"重定向次数过多（超过{MaxRedirects}次限制）");
    }

    /// <summary>
    /// 判断是否为重定向状态码
    /// </summary>
    private static bool IsRedirectStatusCode(HttpStatusCode statusCode)
    {
        return statusCode is HttpStatusCode.Redirect or HttpStatusCode.MovedPermanently
            or HttpStatusCode.TemporaryRedirect or HttpStatusCode.PermanentRedirect
            or (HttpStatusCode)307 or (HttpStatusCode)308;
    }

    /// <summary>
    /// 检查重定向是否安全（仅允许同源 ±www 的重定向）
    /// </summary>
    private static bool IsPermittedRedirect(string originalUrl, string redirectUrl)
    {
        try
        {
            var parsedOriginal = new Uri(originalUrl);
            var parsedRedirect = new Uri(redirectUrl);

            // 协议必须一致
            if (parsedRedirect.Scheme != parsedOriginal.Scheme)
                return false;

            // 端口必须一致
            if (parsedRedirect.Port != parsedOriginal.Port)
                return false;

            // 不允许携带认证信息
            if (!string.IsNullOrEmpty(parsedRedirect.UserInfo))
                return false;

            // Hostname检查：去掉www.前缀后比较
            var stripWww = (string host) => host.StartsWith("www.", StringComparison.OrdinalIgnoreCase)
                ? host[4..] : host;

            return string.Equals(
                stripWww(parsedRedirect.Host),
                stripWww(parsedOriginal.Host),
                StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// 将HTML转换为Markdown（简化版）
    /// 处理常用的HTML元素，生成可读的文本
    /// </summary>
    private static string HtmlToMarkdown(string html)
    {
        if (string.IsNullOrEmpty(html))
            return string.Empty;

        var text = html;

        // 移除脚本和样式
        text = Regex.Replace(text, @"<script[^>]*>.*?</script>", string.Empty,
            RegexOptions.Singleline | RegexOptions.IgnoreCase);
        text = Regex.Replace(text, @"<style[^>]*>.*?</style>", string.Empty,
            RegexOptions.Singleline | RegexOptions.IgnoreCase);
        text = Regex.Replace(text, @"<!--.*?-->", string.Empty,
            RegexOptions.Singleline);

        // 标题: <h1>~<h6> → ## 文本
        text = Regex.Replace(text, @"<h1[^>]*>(.*?)</h1>", "# $1\n\n",
            RegexOptions.Singleline | RegexOptions.IgnoreCase);
        text = Regex.Replace(text, @"<h2[^>]*>(.*?)</h2>", "## $1\n\n",
            RegexOptions.Singleline | RegexOptions.IgnoreCase);
        text = Regex.Replace(text, @"<h3[^>]*>(.*?)</h3>", "### $1\n\n",
            RegexOptions.Singleline | RegexOptions.IgnoreCase);
        text = Regex.Replace(text, @"<h4[^>]*>(.*?)</h4>", "#### $1\n\n",
            RegexOptions.Singleline | RegexOptions.IgnoreCase);
        text = Regex.Replace(text, @"<h5[^>]*>(.*?)</h5>", "##### $1\n\n",
            RegexOptions.Singleline | RegexOptions.IgnoreCase);
        text = Regex.Replace(text, @"<h6[^>]*>(.*?)</h6>", "###### $1\n\n",
            RegexOptions.Singleline | RegexOptions.IgnoreCase);

        // 链接: <a href="...">text</a> → [text](url)
        text = Regex.Replace(text,
            @"<a[^>]*href\s*=\s*[""']([^""']*)[""'][^>]*>(.*?)</a>",
            m =>
            {
                var href = m.Groups[1].Value;
                var linkText = m.Groups[2].Value;
                return $"[{linkText}]({href})";
            },
            RegexOptions.Singleline | RegexOptions.IgnoreCase);

        // 图片: <img src="..." alt="..."> → ![alt](src)
        text = Regex.Replace(text,
            @"<img[^>]*src\s*=\s*[""']([^""']*)[""'][^>]*alt\s*=\s*[""']([^""']*)[""'][^>]*>",
            m => $"![{m.Groups[2].Value}]({m.Groups[1].Value})",
            RegexOptions.Singleline | RegexOptions.IgnoreCase);
        text = Regex.Replace(text,
            @"<img[^>]*alt\s*=\s*[""']([^""']*)[""'][^>]*src\s*=\s*[""']([^""']*)[""'][^>]*>",
            m => $"![{m.Groups[1].Value}]({m.Groups[2].Value})",
            RegexOptions.Singleline | RegexOptions.IgnoreCase);
        text = Regex.Replace(text,
            @"<img[^>]*src\s*=\s*[""']([^""']*)[""'][^>]*>",
            m => $"![Image]({m.Groups[1].Value})",
            RegexOptions.Singleline | RegexOptions.IgnoreCase);

        // 粗体和斜体
        text = Regex.Replace(text, @"<(strong|b)[^>]*>(.*?)</\1>", "**$2**",
            RegexOptions.Singleline | RegexOptions.IgnoreCase);
        text = Regex.Replace(text, @"<(em|i)[^>]*>(.*?)</\1>", "*$2*",
            RegexOptions.Singleline | RegexOptions.IgnoreCase);

        // 代码块: <pre><code>...</code></pre> → ```\n...\n```
        text = Regex.Replace(text, @"<pre><code[^>]*>(.*?)</code></pre>",
            m => "```\n" + DecodeHtmlEntities(m.Groups[1].Value) + "\n```\n\n",
            RegexOptions.Singleline | RegexOptions.IgnoreCase);
        // 行内代码: <code>...</code> → `...`
        text = Regex.Replace(text, @"<code[^>]*>(.*?)</code>",
            m => "`" + DecodeHtmlEntities(m.Groups[1].Value) + "`",
            RegexOptions.Singleline | RegexOptions.IgnoreCase);

        // 段落
        text = Regex.Replace(text, @"<p[^>]*>(.*?)</p>", "$1\n\n",
            RegexOptions.Singleline | RegexOptions.IgnoreCase);

        // 换行
        text = Regex.Replace(text, @"<br\s*/?>", "\n", RegexOptions.IgnoreCase);
        text = Regex.Replace(text, @"<hr\s*/?>", "---\n\n", RegexOptions.IgnoreCase);

        // 列表
        text = Regex.Replace(text, @"<li[^>]*>(.*?)</li>", "- $1\n",
            RegexOptions.Singleline | RegexOptions.IgnoreCase);
        text = Regex.Replace(text, @"</?ul[^>]*>", "\n",
            RegexOptions.Singleline | RegexOptions.IgnoreCase);
        text = Regex.Replace(text, @"</?ol[^>]*>", "\n",
            RegexOptions.Singleline | RegexOptions.IgnoreCase);

        // 删除线
        text = Regex.Replace(text, @"<del[^>]*>(.*?)</del>", "~~$1~~",
            RegexOptions.Singleline | RegexOptions.IgnoreCase);

        // 引用
        text = Regex.Replace(text, @"<blockquote[^>]*>(.*?)</blockquote>",
            m => "> " + m.Groups[1].Value.Trim().Replace("\n", "\n> ") + "\n\n",
            RegexOptions.Singleline | RegexOptions.IgnoreCase);

        // 表格 - 简化为文本
        text = Regex.Replace(text,
            @"<table[^>]*>.*?<thead>.*?</thead>.*?<tbody>(.*?)</tbody>.*?</table>",
            m =>
            {
                var rows = Regex.Matches(m.Groups[1].Value, @"<tr[^>]*>(.*?)</tr>",
                    RegexOptions.Singleline | RegexOptions.IgnoreCase);
                var sb = new StringBuilder();
                foreach (Match row in rows)
                {
                    var cells = Regex.Matches(row.Groups[1].Value, @"<t[dh][^>]*>(.*?)</t[dh]>",
                        RegexOptions.Singleline | RegexOptions.IgnoreCase);
                    sb.AppendLine("| " + string.Join(" | ",
                        cells.Select(c => StripHtmlTags(c.Groups[1].Value).Trim())) + " |");
                }
                return sb.ToString() + "\n";
            },
            RegexOptions.Singleline | RegexOptions.IgnoreCase);

        // 移除所有剩余的HTML标签
        text = StripHtmlTags(text);

        // 解码HTML实体
        text = DecodeHtmlEntities(text);

        // 清理多余的空行（保留最多2个连续换行）
        text = Regex.Replace(text, @"\n{3,}", "\n\n");

        // 清理行首尾空白
        text = Regex.Replace(text, @"^[ \t]+|[ \t]+$", string.Empty, RegexOptions.Multiline);

        return text.Trim();
    }

    /// <summary>
    /// 移除HTML标签
    /// </summary>
    private static string StripHtmlTags(string html)
    {
        return Regex.Replace(html, @"<[^>]+>", " ");
    }

    /// <summary>
    /// 解码HTML实体
    /// </summary>
    private static string DecodeHtmlEntities(string text)
    {
        if (string.IsNullOrEmpty(text))
            return text;

        // 替换命名实体
        foreach (var (entity, replacement) in HtmlEntities)
        {
            text = text.Replace(entity, replacement);
        }

        // 替换数字实体 &#123; 和 &#x1F;
        text = Regex.Replace(text, @"&#(\d+);", m =>
        {
            var codePoint = int.Parse(m.Groups[1].Value);
            return char.ConvertFromUtf32(codePoint);
        });

        text = Regex.Replace(text, @"&#x([0-9a-fA-F]+);", m =>
        {
            var codePoint = Convert.ToInt32(m.Groups[1].Value, 16);
            return char.ConvertFromUtf32(codePoint);
        });

        return text;
    }

    /// <summary>
    /// 验证URL格式
    /// </summary>
    private static bool ValidateUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return false;

        if (url.Length > MaxUrlLength)
            return false;

        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
            return false;

        if (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
            return false;

        // 不允许URL中包含认证信息
        if (!string.IsNullOrEmpty(uri.UserInfo))
            return false;

        // 主机名必须包含至少一个点（排除内部主机名如localhost）
        var hostname = uri.Host;
        var parts = hostname.Split('.');
        if (parts.Length < 2)
            return false;

        return true;
    }

    /// <summary>
    /// 升级http到https
    /// </summary>
    private static string UpgradeToHttps(string url)
    {
        if (url.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
        {
            return "https://" + url[7..];
        }
        return url;
    }

    /// <summary>
    /// 格式化成功结果
    /// </summary>
    private static string FormatResult(string url, string content, long bytes, int code,
        string codeText, string contentType, long durationMs, bool fromCache)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"URL: {url}");
        sb.AppendLine($"状态码: {code} {codeText}");
        sb.AppendLine($"内容类型: {contentType}");
        sb.AppendLine($"内容大小: {FileSizeFormatter.FormatFileSize(bytes)}");
        sb.AppendLine($"耗时: {durationMs}ms");
        sb.AppendLine($"来源: {(fromCache ? "缓存" : "网络请求")}");
        sb.AppendLine(new string('=', 80));
        sb.AppendLine();
        sb.Append(content);
        sb.AppendLine();
        sb.AppendLine(new string('=', 80));

        if (fromCache)
        {
            sb.AppendLine("（内容来自缓存）");
        }

        return sb.ToString();
    }

    /// <summary>
    /// 格式化重定向消息
    /// </summary>
    private static string FormatRedirectMessage(RedirectInfo redirect)
    {
        return $@"重定向检测：URL重定向到了不同的主机。

原始URL: {redirect.OriginalUrl}
重定向URL: {redirect.RedirectUrl}
状态码: {redirect.StatusCode} {redirect.StatusText}

如需完成请求，请使用重定向后的URL重新获取。";
    }

    /// <summary>
    /// 获取默认HTTP状态文本
    /// </summary>
    private static string GetDefaultStatusText(HttpStatusCode statusCode)
    {
        return statusCode switch
        {
            HttpStatusCode.OK => "OK",
            HttpStatusCode.NotFound => "Not Found",
            HttpStatusCode.Forbidden => "Forbidden",
            HttpStatusCode.Unauthorized => "Unauthorized",
            HttpStatusCode.InternalServerError => "Internal Server Error",
            HttpStatusCode.BadGateway => "Bad Gateway",
            HttpStatusCode.ServiceUnavailable => "Service Unavailable",
            HttpStatusCode.MovedPermanently => "Moved Permanently",
            HttpStatusCode.Found => "Found",
            _ => statusCode.ToString(),
        };
    }

    /// <summary>
    /// 创建带超时的HttpClient
    /// </summary>
    private static HttpClient CreateHttpClient(int timeoutSeconds)
    {
        var client = new HttpClient();
        client.Timeout = TimeSpan.FromSeconds(timeoutSeconds);
        client.DefaultRequestHeaders.UserAgent.TryParseAdd(DefaultUserAgent);
        client.DefaultRequestHeaders.Accept.ParseAdd("text/markdown, text/html, */*");
        return client;
    }

    /// <summary>
    /// 在缓存条目过多时淘汰过期条目
    /// </summary>
    private static void EvictExpiredEntriesIfNeeded()
    {
        if (UrlCache.Count <= MaxCacheEntries)
            return;

        var now = DateTime.UtcNow;
        var expiredKeys = UrlCache
            .Where(kvp => kvp.Value.IsExpired)
            .Select(kvp => kvp.Key)
            .ToList();

        foreach (var key in expiredKeys)
        {
            UrlCache.TryRemove(key, out _);
        }
    }

    /// <summary>
    /// 清除所有缓存
    /// </summary>
    [KernelFunction, Description("清除网页抓取缓存")]
    public static void ClearCache()
    {
        UrlCache.Clear();
    }

    #region 内部类型

    /// <summary>
    /// 缓存条目
    /// </summary>
    private sealed class CacheEntry
    {
        public string Content { get; set; } = string.Empty;
        public long Bytes { get; set; }
        public int Code { get; set; }
        public string CodeText { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// 判断缓存是否已过期
        /// </summary>
        public bool IsExpired => DateTime.UtcNow - CreatedAt > TimeSpan.FromMinutes(CacheTtlMinutes);
    }

    /// <summary>
    /// 重定向信息
    /// </summary>
    private readonly struct RedirectInfo
    {
        public readonly string OriginalUrl { get; init; }
        public readonly string RedirectUrl { get; init; }
        public readonly int StatusCode { get; init; }
        public readonly string StatusText { get; init; }
    }

    #endregion
}
