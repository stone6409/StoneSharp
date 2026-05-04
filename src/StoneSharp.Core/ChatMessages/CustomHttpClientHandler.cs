namespace StoneSharp.Core.ChatMessages
{
    public class CustomHttpClientHandler : HttpClientHandler
    {
        /// <summary>
        /// 用于OpenAI或Azure OpenAI请求时重定向的模型基础URL。
        /// </summary>
        private readonly string _modelUrl;
        private static readonly string[] sourceArray = new string[] { "api.openai.com", "openai.azure.com" };

        /// <summary>
        /// 使用指定的模型URL初始化<see cref="CustomHttpClientHandler"/>类的新实例。
        /// </summary>
        /// <param name="modelUrl">用于OpenAI或Azure OpenAI请求的基础URL。</param>
        public CustomHttpClientHandler(string modelUrl)
        {
            // 确保modelUrl不是null或空
            if (string.IsNullOrWhiteSpace(modelUrl))
                throw new ArgumentException("模型URL不能为空或空白。", nameof(modelUrl));

            _modelUrl = modelUrl;
        }

        /// <summary>
        /// 异步发送HTTP请求，对于OpenAI或Azure OpenAI服务的请求，将URL重定向到指定的模型URL。
        /// </summary>
        /// <param name="request">要发送的HTTP请求消息。</param>
        /// <param name="cancellationToken">可以用来取消操作的取消令牌。</param>
        /// <returns>表示异步操作的任务对象。</returns>
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // 修改 User-Agent
            ModifyUserAgent(request, "MyCustomUserAgent/1.0");

            // 调用封装的方法输出请求内容

#if DEBUG
            await LogRequestAsync(request);
#endif
            // 检查请求是否针对OpenAI或Azure OpenAI服务
            if (request.RequestUri != null &&
                sourceArray.Contains(request.RequestUri.Host))
            {
                // 修改请求URI，以包含模型URL
                request.RequestUri = new Uri(_modelUrl + request.RequestUri.PathAndQuery);
            }
            // 调用基类方法实际发送HTTP请求
            return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
        }

        // 修改 User-Agent 的方法
        private void ModifyUserAgent(HttpRequestMessage request, string userAgent)
        {
            // 如果 User-Agent 已经存在，先移除
            if (request.Headers.UserAgent.Any())
            {
                request.Headers.UserAgent.Clear();
            }
            // 添加新的 User-Agent
            request.Headers.TryAddWithoutValidation("User-Agent", userAgent);
        }

        private async Task LogRequestAsync(HttpRequestMessage request)
        {
            Console.WriteLine("Request URI: " + request.RequestUri);
            Console.WriteLine("Request Method: " + request.Method);
            if (request.Content != null)
            {
                string content = await request.Content.ReadAsStringAsync();
                Console.WriteLine("Request Content: " + content);
            }
            Console.WriteLine("Request Headers:");
            foreach (var header in request.Headers)
            {
                Console.WriteLine($"{header.Key}: {string.Join(", ", header.Value)}");
            }
        }
    }
}
