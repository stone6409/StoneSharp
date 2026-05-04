using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace StoneSharp.Core.ChatMessages
{
    /// <summary>
    /// 简化的函数参数类型，只提供读取功能
    /// </summary>
    public class FunctionArguments : IReadOnlyDictionary<string, object>
    {
        private readonly Dictionary<string, object> _arguments;

        /// <summary>
        /// 构造函数
        /// </summary>
        public FunctionArguments()
        {
            _arguments = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// 从现有字典创建
        /// </summary>
        public FunctionArguments(IDictionary<string, object> source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            
            _arguments = new Dictionary<string, object>(source, StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// 从KernelArguments创建
        /// </summary>
        public FunctionArguments(Microsoft.SemanticKernel.KernelArguments kernelArguments)
        {
            if (kernelArguments == null)
                throw new ArgumentNullException(nameof(kernelArguments));
            
            _arguments = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            
            foreach (var kvp in kernelArguments)
            {
                _arguments[kvp.Key] = kvp.Value;
            }
        }

        /// <summary>
        /// 转换为KernelArguments
        /// </summary>
        public Microsoft.SemanticKernel.KernelArguments ToKernelArguments()
        {
            var kernelArgs = new Microsoft.SemanticKernel.KernelArguments();
            
            foreach (var kvp in _arguments)
            {
                kernelArgs[kvp.Key] = kvp.Value;
            }
            
            return kernelArgs;
        }

        /// <summary>
        /// 获取参数值
        /// </summary>
        public object this[string key] => _arguments[key];

        /// <summary>
        /// 参数数量
        /// </summary>
        public int Count => _arguments.Count;

        /// <summary>
        /// 所有参数名
        /// </summary>
        public IEnumerable<string> Keys => _arguments.Keys;

        /// <summary>
        /// 所有参数值
        /// </summary>
        public IEnumerable<object> Values => _arguments.Values;

        /// <summary>
        /// 检查是否包含指定参数
        /// </summary>
        public bool ContainsKey(string key) => _arguments.ContainsKey(key);

        /// <summary>
        /// 尝试获取参数值
        /// </summary>
        public bool TryGetValue(string key, out object value) => _arguments.TryGetValue(key, out value);

        /// <summary>
        /// 获取枚举器
        /// </summary>
        public IEnumerator<KeyValuePair<string, object>> GetEnumerator() => _arguments.GetEnumerator();

        /// <summary>
        /// 获取枚举器
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator() => _arguments.GetEnumerator();

        /// <summary>
        /// 获取参数值，如果不存在则返回默认值
        /// </summary>
        public T GetValue<T>(string key, T defaultValue = default)
        {
            if (_arguments.TryGetValue(key, out var value) && value is T typedValue)
            {
                return typedValue;
            }
            return defaultValue;
        }

        /// <summary>
        /// 获取字符串参数值
        /// </summary>
        public string GetString(string key, string defaultValue = "")
        {
            return GetValue(key, defaultValue);
        }

        /// <summary>
        /// 获取整数参数值
        /// </summary>
        public int GetInt(string key, int defaultValue = 0)
        {
            return GetValue(key, defaultValue);
        }

        /// <summary>
        /// 获取布尔参数值
        /// </summary>
        public bool GetBool(string key, bool defaultValue = false)
        {
            return GetValue(key, defaultValue);
        }

        /// <summary>
        /// 获取所有参数
        /// </summary>
        public IReadOnlyDictionary<string, object> GetAll() => new Dictionary<string, object>(_arguments);

        /// <summary>
        /// 返回参数的字符串表示形式
        /// </summary>
        public override string ToString()
        {
            if (_arguments.Count == 0)
            {
                return "Empty";
            }

            var sb = new StringBuilder();
            sb.Append("[");
            
            bool isFirst = true;
            foreach (var kvp in _arguments)
            {
                if (!isFirst)
                {
                    sb.Append(", ");
                }
                
                sb.Append(kvp.Key);
                sb.Append(": ");
                
                // 处理不同类型的值显示
                if (kvp.Value == null)
                {
                    sb.Append("null");
                }
                else if (kvp.Value is string str)
                {
                    // 对字符串进行截断，避免过长
                    if (str.Length > 128)
                    {
                        sb.Append('"');
                        sb.Append(str.Substring(0, 128));
                        sb.Append("...");
                        sb.Append('"');
                    }
                    else
                    {
                        sb.Append('"');
                        sb.Append(str);
                        sb.Append('"');
                    }
                }
                else if (kvp.Value is IEnumerable enumerable && !(kvp.Value is string))
                {
                    // 处理集合类型
                    sb.Append("[");
                    bool firstItem = true;
                    int count = 0;
                    foreach (var item in enumerable)
                    {
                        if (!firstItem)
                        {
                            sb.Append(", ");
                        }
                        
                        if (count >= 5) // 只显示前5个元素
                        {
                            sb.Append("...");
                            break;
                        }
                        
                        sb.Append(FormatValue(item));
                        firstItem = false;
                        count++;
                    }
                    sb.Append("]");
                }
                else
                {
                    sb.Append(FormatValue(kvp.Value));
                }
                
                isFirst = false;
            }
            
            sb.Append("]");
            return sb.ToString();
        }

        /// <summary>
        /// 格式化单个值
        /// </summary>
        private string FormatValue(object value)
        {
            if (value == null)
                return "null";
            
            if (value is string str)
            {
                // 对字符串进行截断
                if (str.Length > 30)
                {
                    return $"\"{str.Substring(0, 27)}...\"";
                }
                return $"\"{str}\"";
            }
            
            if (value is DateTime dateTime)
            {
                return dateTime.ToString("yyyy-MM-dd HH:mm:ss");
            }
            
            if (value is DateTimeOffset dateTimeOffset)
            {
                return dateTimeOffset.ToString("yyyy-MM-dd HH:mm:ss zzz");
            }
            
            if (value is TimeSpan timeSpan)
            {
                return timeSpan.ToString();
            }
            
            if (value is bool boolean)
            {
                return boolean ? "true" : "false";
            }
            
            if (value is int || value is long || value is short || value is byte ||
                value is uint || value is ulong || value is ushort || value is sbyte ||
                value is float || value is double || value is decimal)
            {
                return value.ToString();
            }
            
            // 对于其他类型，使用类型名
            return $"{value.GetType().Name}: {value}";
        }
    }
}