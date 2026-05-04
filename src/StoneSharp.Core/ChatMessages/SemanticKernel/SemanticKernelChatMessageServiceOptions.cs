namespace StoneSharp.Core.ChatMessages.SemanticKernel
{
    public class SemanticKernelChatMessageServiceOptions
    {
        public string ApiUrl { get; set; }

        public string ApiKey { get; set; }

        public string Model { get; set; }

        public int MaxTokens { get; set; } = 4096;

        public double Temperature { get; set; } = 0.7;

        public double TopP { get; set; } = 0.9;

        public IPluginFunctionService PluginFunctionService { get; }

        public SemanticKernelChatMessageServiceOptions(string apiUrl, string apiKey, string model)
        {
            ApiUrl = apiUrl;
            ApiKey = apiKey;
            Model = model;
        }

        public SemanticKernelChatMessageServiceOptions(string apiUrl, string apiKey, string model, int maxTokens)
        {
            ApiUrl = apiUrl;
            ApiKey = apiKey;
            Model = model;
            MaxTokens = maxTokens;
        }

        public SemanticKernelChatMessageServiceOptions(string apiUrl, string apiKey, string model, int maxTokens, double temperature, double topP, IPluginFunctionService pluginFunctionService = null)
        {
            ApiUrl = apiUrl;
            ApiKey = apiKey;
            Model = model;
            MaxTokens = maxTokens;
            Temperature = temperature;
            TopP = topP;
            PluginFunctionService = pluginFunctionService;
        }

        public override string ToString()
        {
            return $"Model: {Model}, MaxTokens: {MaxTokens}, Temperature: {Temperature}, TopP: {TopP}";
        }
    }
}