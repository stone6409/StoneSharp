// AiModelSettings.cs
using System;

namespace StoneSharp.Core.Settings
{
    /// <summary>
    /// AI模型配置设置
    /// </summary>
    public class AiModelSettings
    {
        /// <summary>
        /// API地址
        /// </summary>
        public string ApiUrl { get; set; } = "https://api.deepseek.com/";

        /// <summary>
        /// API密钥
        /// </summary>
        public string ApiKey { get; set; } = string.Empty;

        /// <summary>
        /// 模型名称
        /// </summary>
        public string Model { get; set; } = "deepseek-chat";

        /// <summary>
        /// 最大令牌数
        /// </summary>
        public int MaxTokens { get; set; } = 8192;

        /// <summary>
        /// 温度参数
        /// </summary>
        public double Temperature { get; set; } = 0.7;

        /// <summary>
        /// TopP参数
        /// </summary>
        public double TopP { get; set; } = 1.0;

        /// <summary>
        /// 验证配置是否有效
        /// </summary>
        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(ApiKey))
                throw new InvalidOperationException("API Key未配置");

            if (string.IsNullOrWhiteSpace(ApiUrl))
                throw new InvalidOperationException("API URL未配置");

            if (string.IsNullOrWhiteSpace(Model))
                throw new InvalidOperationException("模型名称未配置");

            if (MaxTokens <= 0)
                throw new InvalidOperationException("MaxTokens必须大于0");

            if (Temperature < 0 || Temperature > 2)
                throw new InvalidOperationException("Temperature必须在0-2之间");

            if (TopP < 0 || TopP > 1)
                throw new InvalidOperationException("TopP必须在0-1之间");
        }
    }
}