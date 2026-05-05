// AgentConfigurationProvider.cs
using Microsoft.Extensions.Configuration;
using System;

namespace StoneSharp.Core.Settings
{
    /// <summary>
    /// 基于appsettings.json的配置提供者
    /// </summary>
    public class AgentConfigurationProvider : IAgentConfigurationProvider
    {
        private readonly IConfiguration _configuration;

        public AgentConfigurationProvider(IConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public AiModelSettings GetAiModelSettings()
        {
            var settings = new AiModelSettings();
            _configuration.GetSection("AiModelSettings").Bind(settings);
            return settings;
        }

        public ToolSettings GetToolSettings()
        {
            var settings = new ToolSettings();
            _configuration.GetSection("ToolSettings").Bind(settings);
            return settings;
        }

        public ChatSettings GetChatSettings()
        {
            var settings = new ChatSettings();
            _configuration.GetSection("ChatSettings").Bind(settings);
            return settings;
        }

        public void ValidateAllSettings()
        {
            GetAiModelSettings().Validate();
            GetToolSettings().Validate();
            GetChatSettings().Validate();
        }
    }
}