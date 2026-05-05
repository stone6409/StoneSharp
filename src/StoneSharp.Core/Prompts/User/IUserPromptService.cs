using StoneSharp.Core.Models.ContextItems;

namespace StoneSharp.Core.Prompts.User
{
    /// <summary>
    /// 提示语构建器接口
    /// </summary>
    public interface IUserPromptService
    {
        /// <summary>
        /// 从上下文项构建组合提示语（异步）
        /// </summary>
        Task<string> BuildCombinedPromptFromContextItemsAsync(string input, IEnumerable<ContextItem> contextItems, bool includeFileContent = true);

        // 同步方法（向后兼容）
        string BuildCombinedPromptFromContextItems(string input, IEnumerable<ContextItem> contextItems, bool includeFileContent = true);

        /// <summary>
        /// 构建独立的上下文项描述（异步）
        /// </summary>
        Task<IEnumerable<string>> BuildIndividualContextItemsAsync(string input, IEnumerable<ContextItem> contextItems, bool includeFileContent = true);

        /// <summary>
        /// 构建独立的上下文项描述（同步）
        /// </summary>
        IEnumerable<string> BuildIndividualContextItems(string input, IEnumerable<ContextItem> contextItems, bool includeFileContent = true);
    }
}