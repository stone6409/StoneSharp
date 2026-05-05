using System;

namespace StoneSharp.Core.Models
{
    /// <summary>
    /// 表示一个工具的基本信息
    /// </summary>
    public class Tool
    {
        /// <summary>
        /// 工具的唯一标识符
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// 工具的名称（显示名称）
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 工具的描述信息
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 工具类别（用于分组）
        /// </summary>
        public string Category { get; set; }

        /// <summary>
        /// 工具类型
        /// </summary>
        public Type Type { get; set; }

        /// <summary>
        /// 是否在规划模式下允许使用
        /// </summary>
        public bool IsAllowedInPlanMode { get; set; } = true;

        /// <summary>
        /// 默认构造函数
        /// </summary>
        public Tool()
        {
        }

        /// <summary>
        /// 带参数的构造函数
        /// </summary>
        /// <param name="id">工具ID</param>
        /// <param name="name">工具名称</param>
        /// <param name="description">工具描述</param>
        /// <param name="isEnabled">是否启用</param>
        public Tool(string id, string name, Type type)
        {
            Id = id ?? throw new ArgumentNullException(nameof(id));
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Type = type;
        }

        /// <summary>
        /// 带参数的构造函数（包含规划模式设置）
        /// </summary>
        /// <param name="id">工具ID</param>
        /// <param name="name">工具名称</param>
        /// <param name="type">工具类型</param>
        /// <param name="isAllowedInPlanMode">是否在规划模式下允许使用</param>
        public Tool(string id, string name, Type type, bool isAllowedInPlanMode = false)
        {
            Id = id ?? throw new ArgumentNullException(nameof(id));
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Type = type;
            IsAllowedInPlanMode = isAllowedInPlanMode;
        }

        /// <summary>
        /// 复制工具对象
        /// </summary>
        /// <returns>新的工具实例</returns>
        public Tool Clone()
        {
            return new Tool
            {
                Id = Id,
                Name = Name,
                Description = Description,
                Category = Category,
                Type = Type,
                IsAllowedInPlanMode = IsAllowedInPlanMode
            };
        }

        /// <summary>
        /// 比较两个工具是否相同
        /// </summary>
        /// <param name="obj">要比较的对象</param>
        /// <returns>如果相同返回true，否则返回false</returns>
        public override bool Equals(object obj)
        {
            if (obj is Tool other)
            {
                return Id == other.Id;
            }
            return false;
        }

        /// <summary>
        /// 获取哈希码
        /// </summary>
        /// <returns>哈希码</returns>
        public override int GetHashCode()
        {
            return Id?.GetHashCode() ?? 0;
        }
    }
}