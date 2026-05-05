// ChatSettings.cs
namespace StoneSharp.Core.Settings
{
    /// <summary>
    /// 聊天配置设置
    /// </summary>
    public class ChatSettings
    {
        /// <summary>
        /// 聊天文件夹路径
        /// </summary>
        public string ChatFolderPath { get; set; } = "chats";

        /// <summary>
        /// 验证配置是否有效
        /// </summary>
        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(ChatFolderPath))
                throw new InvalidOperationException("聊天文件夹路径未配置");
        }
    }
}