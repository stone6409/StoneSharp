// IAgentConfigurationProvider.cs
namespace StoneSharp.Core.Settings
{
    /// <summary>
    /// 配置提供者接口
    /// </summary>
    public interface IAgentConfigurationProvider
    {
        /// <summary>
        /// 获取AI模型配置
        /// </summary>
        AiModelSettings GetAiModelSettings();

        /// <summary>
        /// 获取工具配置
        /// </summary>
        ToolSettings GetToolSettings();

        /// <summary>
        /// 获取聊天配置
        /// </summary>
        ChatSettings GetChatSettings();

        /// <summary>
        /// 验证所有配置
        /// </summary>
        void ValidateAllSettings();
    }
}