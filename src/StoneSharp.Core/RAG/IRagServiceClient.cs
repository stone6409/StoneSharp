namespace StoneSharp.Core.RAG
{
    /// <summary>
    /// RAG 服务客户端接口
    /// </summary>
    public interface IRagServiceClient
    {
        /// <summary>
        /// 调用 RAG 服务进行搜索
        /// </summary>
        /// <param name="query">用户输入的查询</param>
        /// <param name="index">可选参数，指定索引名称</param>
        /// <returns>RAG 服务的响应结果</returns>
        Task<string> SearchAsync(string query, string? index = null);

        /// <summary>
        /// 获取所有可用的索引
        /// </summary>
        /// <returns>索引列表</returns>
        Task<IEnumerable<string>> ListIndexesAsync();
    }
}