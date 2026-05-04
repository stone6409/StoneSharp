using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using StoneSharp.Core.Tools;

namespace StoneSharp.Core.ChatMessages.SemanticKernel
{
    // https://api-docs.deepseek.com/zh-cn/guides/thinking_mode
    public class SemanticKernelChatMessageService : IChatMessageService
    {
        int _maxTokens = 4096;
        bool _isPluginsEnable = false;
        double _temperature = 0.7;
        double _topP = 1.0;

        private IChatCompletionService _chatCompletionService;
        private Kernel _kernel;
        private ChatHistory _chatHistory = new ChatHistory();
        private List<string> _allowedTools { get; set; } = new List<string>();

        private PluginFactory _pluginFactory;
        private IPluginFunctionService _pluginFunctionService;

        private SemanticKernelChatMessageServiceOptions _options;

        // 新的构造方法，使用参数类
        public SemanticKernelChatMessageService(SemanticKernelChatMessageServiceOptions options, IServiceProvider serviceProvider = null)
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            _options = options;
            _maxTokens = options.MaxTokens;
            _temperature = options.Temperature;
            _topP = options.TopP;

            // 创建插件工厂
            _pluginFactory = new PluginFactory(serviceProvider);

            // 使用传入的 PluginFunctionService 或创建默认的
            _pluginFunctionService = _options.PluginFunctionService ?? CreateDefaultPluginFunctionService();
        }

        /// <summary>
        /// 获取插件函数服务实例，允许外部订阅事件
        /// </summary>
        public IPluginFunctionService PluginFunctionService => _pluginFunctionService;

        /// <summary>
        /// 创建默认的插件函数服务
        /// </summary>
        private IPluginFunctionService CreateDefaultPluginFunctionService()
        {
            return new PluginFunctionService();
        }

        /// <summary>
        /// 初始化聊天服务（封装了原构造方法的核心逻辑）
        /// </summary>
        private void InitializeChatService()
        {
            if (_chatCompletionService != null && _kernel != null)
                return;

            string apiEndpoint = _options.ApiUrl;
            string apiKey = _options.ApiKey;
            string model = _options.Model;

            // 如果端点以 /v1/ 结尾，则去掉 /v1/
            if (apiEndpoint.EndsWith("/v1/"))
            {
                apiEndpoint = apiEndpoint.Substring(0, apiEndpoint.Length - 4);
            }

            var openAICustomHandler = new CustomHttpClientHandler(apiEndpoint);
            HttpClient client = new HttpClient(openAICustomHandler);

            // 创建 Kernel 并添加 OpenAI 聊天完成服务
            var builder = Kernel.CreateBuilder().AddOpenAIChatCompletion(model, apiKey, httpClient: client);

            // Add a plugin (the LightsPlugin class is defined below)
            if (_allowedTools.Count > 0)
            {
                // 注册服务
                builder.Services.AddSingleton(_pluginFunctionService);
                builder.Services.AddSingleton<IAutoFunctionInvocationFilter, PluginFunctionFilter>();

                AddPluginsBasedOnAllowedTools(builder, _allowedTools);
            }

            // 构建 Kernel
            _kernel = builder.Build();
            _chatCompletionService = _kernel.GetRequiredService<IChatCompletionService>();
        }

        private OpenAIPromptExecutionSettings CreateOpenAIPromptExecutionSettings()
        {
            return new OpenAIPromptExecutionSettings()
            {
                FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(),
                MaxTokens = _maxTokens,
                Temperature = _temperature,
                TopP = _topP,
                //ExtensionData = new Dictionary<string, object>
                //{
                //    ["extra_body"] = new Dictionary<string, object>
                //    {
                //        ["thinking"] = new { type = "disabled" }
                //    }
                //}
                //ExtensionData = new Dictionary<string, object>
                //{
                //    ["thinking"] = new { type = "disabled" }
                //},
                //ExtensionData = new Dictionary<string, object>
                //{
                //    ["extra_body"] = new { thinking = new { type = "disabled" } }
                //}
                //ExtensionData = new Dictionary<string, object>
                //{
                //    ["extra_body"] = "{\"thinking\": {\"type\": \"disabled\"}}"
                //}
                //Metadata = new Dictionary<string, string>
                //{
                //    ["extra_body"] = "{\"thinking\": {\"type\": \"disabled\"}}"
                //}
            };
        }

        public void AddMessage(AuthorRole authorRole, string content, string? reasoningContent = null)
        {
            Dictionary<string, object?> metadata = null;
            if (!string.IsNullOrEmpty(reasoningContent))
            {
                metadata = new Dictionary<string, object?>
                {
                    [OpenAIChatMessageContent.ReasoningContentProperty] = reasoningContent
                };
            }

            Microsoft.SemanticKernel.ChatCompletion.AuthorRole skAuthorRole = new Microsoft.SemanticKernel.ChatCompletion.AuthorRole(authorRole.Label);
            _chatHistory.AddMessage(skAuthorRole, content, metadata: metadata);
        }

        public void AddFunctionCallMessage(string functionName, string? pluginName = null, string? callId = null, FunctionArguments? arguments = null, string? reasoningContent = null)
        {
            Dictionary<string, object?> metadata = null;
            // CUSTOM: Add reasoning content to metadata for DeepSeek API thinking mode support
            if (!string.IsNullOrEmpty(reasoningContent))
            {
                metadata = new Dictionary<string, object?>
                {
                    [OpenAIChatMessageContent.ReasoningContentProperty] = reasoningContent
                };
            }

            Microsoft.SemanticKernel.KernelArguments kernelArguments = arguments != null ? arguments.ToKernelArguments() : null;
            var message = new ChatMessageContent(Microsoft.SemanticKernel.ChatCompletion.AuthorRole.Assistant, [new FunctionCallContent(functionName, pluginName, callId, kernelArguments)], metadata: metadata);

            _chatHistory.Add(message);
        }

        public void AddFunctionResultMessage(string? functionName = null, string? pluginName = null, string? callId = null, object? result = null)
        {
            _chatHistory.Add(new ChatMessageContent(Microsoft.SemanticKernel.ChatCompletion.AuthorRole.Tool, [new FunctionResultContent(functionName, pluginName, callId, result)]));
        }

        public void AddAllowedTool(string tool)
        {
            if (!string.IsNullOrEmpty(tool))
            {
                if (!_allowedTools.Contains(tool))
                {
                    _allowedTools.Add(tool);
                }
            }
        }

        public async Task<ChatMessageContentResult> GetChatMessageContentAsync(CancellationToken cancellationToken = default)
        {
            // 确保聊天服务已初始化
            InitializeChatService();

            ChatMessageContent result = await _chatCompletionService.GetChatMessageContentAsync(
                    _chatHistory,
                    executionSettings: CreateOpenAIPromptExecutionSettings(),
                    _kernel,
                    cancellationToken).ConfigureAwait(false);

            // 从 Metadata 中提取 reasoning_content
            string? reasoningContent = null;
            if (result?.Metadata?.TryGetValue(OpenAIChatMessageContent.ReasoningContentProperty, out var rc) is true && rc is string rcStr)
            {
                reasoningContent = rcStr;
            }

            return new ChatMessageContentResult(result?.Content ?? string.Empty, reasoningContent);
        }

        public async IAsyncEnumerable<StreamingMessage> GetStreamingChatMessageContentsAsync(CancellationToken cancellationToken = default)
        {
            // 确保聊天服务已初始化
            InitializeChatService();

            var streamingMessages = _chatCompletionService.GetStreamingChatMessageContentsAsync(
                    _chatHistory,
                    executionSettings: CreateOpenAIPromptExecutionSettings(),
                    _kernel,
                    cancellationToken);

            await foreach (var message in streamingMessages.WithCancellation(cancellationToken).ConfigureAwait(false))
            {
                // 从 Metadata 中提取 reasoning_content（每个 chunk 的增量片段）
                string? reasoningContent = null;
                if (message.Metadata?.TryGetValue(OpenAIChatMessageContent.ReasoningContentProperty, out var rc) is true && rc is string rcStr)
                {
                    reasoningContent = rcStr;
                }

                yield return new StreamingMessage(message.Content ?? string.Empty, reasoningContent);
            }
        }

        /// <summary>
        /// 根据允许的工具列表添加插件
        /// </summary>
        /// <param name="builder">Kernel构建器</param>
        /// <param name="allowedTools">允许的工具字符串</param>
        private void AddPluginsBasedOnAllowedTools(IKernelBuilder builder, List<string> allowedTools)
        {
            // 添加允许的工具
            foreach (string toolId in allowedTools)
            {
                AddPluginByToolId(builder, toolId);
            }
        }

        /// <summary>
        /// 根据工具ID添加插件
        /// </summary>
        /// <param name="builder">Kernel构建器</param>
        /// <param name="toolId">工具ID</param>
        private void AddPluginByToolId(IKernelBuilder builder, string toolId)
        {
            var toolType = ToolUtility.GetToolType(toolId);

            if (toolType == null)
            {
                return;
            }

            try
            {
                var pluginInstance = _pluginFactory.CreatePluginInstance(toolType);
                if (pluginInstance != null)
                {
                    builder.Plugins.AddFromObject(pluginInstance, toolId);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"添加插件失败: {toolId}, 错误: {ex.Message}");
            }
        }
    }
}