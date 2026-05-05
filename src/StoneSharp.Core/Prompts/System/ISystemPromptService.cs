using System.Threading.Tasks;

namespace StoneSharp.Core.Prompts.System
{
    public interface ISystemPromptService
    {
        /// <summary>
        /// 默认系统提示语
        /// </summary>
        /// <value>
        /// 当无法从文件加载提示语时使用的默认提示语内容
        /// </value>
        string DefaultSystemPrompt { get; set; }

        /// <summary>
        /// 异步获取系统提示语
        /// </summary>
        /// <returns>系统提示语内容，如果找不到文件则返回null</returns>
        Task<string> GetSystemPromptAsync();
        
        /// <summary>
        /// 同步获取系统提示语
        /// </summary>
        /// <returns>系统提示语内容，如果找不到文件则返回null</returns>
        string GetSystemPrompt();

        /// <summary>
        /// 异步获取增强的系统提示语
        /// </summary>
        /// <param name="skillPrompt">技能提示语</param>
        /// <param name="isPlanMode">是否为规划模式</param>
        /// <returns>增强的系统提示语内容</returns>
        Task<string> GetEnhancedSystemPromptAsync(string subjectPrompt, string skillPrompt, bool isPlanMode = false);

        /// <summary>
        /// 同步获取增强的系统提示语
        /// </summary>
        /// <param name="skillPrompt">技能提示语</param>
        /// <param name="isPlanMode">是否为规划模式</param>
        /// <returns>增强的系统提示语内容</returns>
        string GetEnhancedSystemPrompt(string subjectPrompt, string skillPrompt, bool isPlanMode = false);
    }
}