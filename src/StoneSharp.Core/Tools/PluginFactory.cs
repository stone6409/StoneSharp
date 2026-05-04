namespace StoneSharp.Core.Tools
{
    /// <summary>
    /// 插件工厂
    /// </summary>
    public class PluginFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public PluginFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        /// <summary>
        /// 创建插件实例
        /// </summary>
        public object CreatePluginInstance(Type pluginType)
        {
            if (pluginType == null)
                return null;

            try
            {
                // 尝试从服务容器获取
                var instance = _serviceProvider.GetService(pluginType);
                if (instance != null)
                    return instance;

                // 回退到无参构造函数
                return Activator.CreateInstance(pluginType);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"创建插件实例失败: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// 根据工具ID创建插件实例
        /// </summary>
        public object CreatePluginInstance(string toolId)
        {
            var toolType = ToolUtility.GetToolType(toolId);
            return CreatePluginInstance(toolType);
        }
    }
}