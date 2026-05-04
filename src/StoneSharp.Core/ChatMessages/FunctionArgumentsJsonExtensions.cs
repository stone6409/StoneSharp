using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace StoneSharp.Core.ChatMessages
{
    /// <summary>
    /// FunctionArguments 的 JSON 序列化扩展方法
    /// </summary>
    public static class FunctionArgumentsJsonExtensions
    {
        private static readonly JsonSerializerOptions _defaultJsonOptions = new()
        {
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            Converters = { new FunctionArgumentsJsonConverter() }
        };

        private static readonly JsonSerializerOptions _compactJsonOptions = new()
        {
            WriteIndented = false,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            Converters = { new FunctionArgumentsJsonConverter() }
        };

        /// <summary>
        /// 将 FunctionArguments 序列化为 JSON 字符串
        /// </summary>
        /// <param name="arguments">要序列化的 FunctionArguments 实例</param>
        /// <param name="indented">是否格式化输出（缩进）</param>
        /// <returns>JSON 字符串</returns>
        public static string ToJson(this FunctionArguments arguments, bool indented = true)
        {
            if (arguments == null)
                return "null";

            var options = indented ? _defaultJsonOptions : _compactJsonOptions;
            return JsonSerializer.Serialize(arguments.GetAll(), options);
        }

        /// <summary>
        /// 将 FunctionArguments 序列化为 Base64 编码的字符串
        /// </summary>
        /// <param name="arguments">要序列化的 FunctionArguments 实例</param>
        /// <returns>Base64 编码的 JSON 字符串</returns>
        public static string ToBase64Json(this FunctionArguments arguments)
        {
            if (arguments == null)
                return string.Empty;

            var json = arguments.ToJson(false);
            var bytes = Encoding.UTF8.GetBytes(json);
            return Convert.ToBase64String(bytes);
        }

        /// <summary>
        /// 从 JSON 字符串创建 FunctionArguments
        /// </summary>
        /// <param name="json">JSON 字符串</param>
        /// <returns>FunctionArguments 实例</returns>
        public static FunctionArguments FromJson(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return new FunctionArguments();

            try
            {
                var dictionary = JsonSerializer.Deserialize<Dictionary<string, object>>(json, _defaultJsonOptions);
                return new FunctionArguments(dictionary ?? new Dictionary<string, object>());
            }
            catch (JsonException)
            {
                // 如果反序列化失败，返回空实例
                return new FunctionArguments();
            }
        }

        /// <summary>
        /// 从 Base64 编码的 JSON 字符串创建 FunctionArguments
        /// </summary>
        /// <param name="base64Json">Base64 编码的 JSON 字符串</param>
        /// <returns>FunctionArguments 实例</returns>
        public static FunctionArguments FromBase64Json(string base64Json)
        {
            if (string.IsNullOrWhiteSpace(base64Json))
                return new FunctionArguments();

            try
            {
                var bytes = Convert.FromBase64String(base64Json);
                var json = Encoding.UTF8.GetString(bytes);
                return FromJson(json);
            }
            catch (FormatException)
            {
                // 如果 Base64 格式错误，返回空实例
                return new FunctionArguments();
            }
        }

        /// <summary>
        /// 尝试从 JSON 字符串创建 FunctionArguments
        /// </summary>
        /// <param name="json">JSON 字符串</param>
        /// <param name="result">创建的 FunctionArguments 实例</param>
        /// <returns>是否成功</returns>
        public static bool TryFromJson(string json, out FunctionArguments result)
        {
            result = null;

            if (string.IsNullOrWhiteSpace(json))
            {
                result = new FunctionArguments();
                return true;
            }

            try
            {
                var dictionary = JsonSerializer.Deserialize<Dictionary<string, object>>(json, _defaultJsonOptions);
                result = new FunctionArguments(dictionary ?? new Dictionary<string, object>());
                return true;
            }
            catch (JsonException)
            {
                return false;
            }
        }

        /// <summary>
        /// 尝试从 Base64 编码的 JSON 字符串创建 FunctionArguments
        /// </summary>
        /// <param name="base64Json">Base64 编码的 JSON 字符串</param>
        /// <param name="result">创建的 FunctionArguments 实例</param>
        /// <returns>是否成功</returns>
        public static bool TryFromBase64Json(string base64Json, out FunctionArguments result)
        {
            result = null;

            if (string.IsNullOrWhiteSpace(base64Json))
            {
                result = new FunctionArguments();
                return true;
            }

            try
            {
                var bytes = Convert.FromBase64String(base64Json);
                var json = Encoding.UTF8.GetString(bytes);
                return TryFromJson(json, out result);
            }
            catch (FormatException)
            {
                return false;
            }
        }
    }

    /// <summary>
    /// FunctionArguments 的 JSON 转换器
    /// </summary>
    internal class FunctionArgumentsJsonConverter : JsonConverter<object>
    {
        public override bool CanConvert(Type typeToConvert)
        {
            return true;
        }

        public override object Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return ReadValue(ref reader, options);
        }

        public override void Write(Utf8JsonWriter writer, object value, JsonSerializerOptions options)
        {
            WriteValue(writer, value, options);
        }

        private object ReadValue(ref Utf8JsonReader reader, JsonSerializerOptions options)
        {
            switch (reader.TokenType)
            {
                case JsonTokenType.Null:
                    return null;

                case JsonTokenType.True:
                    return true;

                case JsonTokenType.False:
                    return false;

                case JsonTokenType.Number:
                    if (reader.TryGetInt32(out int intValue))
                        return intValue;
                    if (reader.TryGetInt64(out long longValue))
                        return longValue;
                    if (reader.TryGetDouble(out double doubleValue))
                        return doubleValue;
                    return reader.GetDecimal();

                case JsonTokenType.String:
                    return reader.GetString();

                case JsonTokenType.StartArray:
                    var list = new List<object>();
                    while (reader.Read())
                    {
                        if (reader.TokenType == JsonTokenType.EndArray)
                            break;
                        list.Add(ReadValue(ref reader, options));
                    }
                    return list;

                case JsonTokenType.StartObject:
                    var dict = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
                    while (reader.Read())
                    {
                        if (reader.TokenType == JsonTokenType.EndObject)
                            break;

                        if (reader.TokenType != JsonTokenType.PropertyName)
                            throw new JsonException();

                        var propertyName = reader.GetString();
                        reader.Read();
                        dict[propertyName] = ReadValue(ref reader, options);
                    }
                    return dict;

                default:
                    throw new JsonException($"Unsupported token type: {reader.TokenType}");
            }
        }

        private void WriteValue(Utf8JsonWriter writer, object value, JsonSerializerOptions options)
        {
            if (value == null)
            {
                writer.WriteNullValue();
                return;
            }

            var type = value.GetType();

            // 处理基本类型
            if (type == typeof(string))
            {
                writer.WriteStringValue((string)value);
            }
            else if (type == typeof(bool))
            {
                writer.WriteBooleanValue((bool)value);
            }
            else if (type == typeof(int))
            {
                writer.WriteNumberValue((int)value);
            }
            else if (type == typeof(long))
            {
                writer.WriteNumberValue((long)value);
            }
            else if (type == typeof(double))
            {
                writer.WriteNumberValue((double)value);
            }
            else if (type == typeof(decimal))
            {
                writer.WriteNumberValue((decimal)value);
            }
            else if (type == typeof(float))
            {
                writer.WriteNumberValue((float)value);
            }
            else if (type == typeof(DateTime))
            {
                writer.WriteStringValue(((DateTime)value).ToString("O")); // ISO 8601
            }
            else if (type == typeof(DateTimeOffset))
            {
                writer.WriteStringValue(((DateTimeOffset)value).ToString("O")); // ISO 8601
            }
            else if (type == typeof(TimeSpan))
            {
                writer.WriteStringValue(((TimeSpan)value).ToString());
            }
            else if (type == typeof(Guid))
            {
                writer.WriteStringValue(((Guid)value).ToString());
            }
            else if (value is IDictionary dictionary)
            {
                writer.WriteStartObject();
                foreach (DictionaryEntry entry in dictionary)
                {
                    writer.WritePropertyName(entry.Key.ToString());
                    WriteValue(writer, entry.Value, options);
                }
                writer.WriteEndObject();
            }
            else if (value is IEnumerable enumerable && !(value is string))
            {
                writer.WriteStartArray();
                foreach (var item in enumerable)
                {
                    WriteValue(writer, item, options);
                }
                writer.WriteEndArray();
            }
            else
            {
                // 对于其他类型，尝试序列化为字符串
                writer.WriteStringValue(value.ToString());
            }
        }
    }
}