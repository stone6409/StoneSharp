using System;
using System.Collections.Generic;
using System.Xml;

namespace StoneSharp.Core.Stores
{
    public static partial class XmlAttributeHelper
    {
        #region bool property

        public static bool ReadAttribute(XmlNode xmlNode, string name, bool defaultValue)
        {
            XmlAttribute xmlAttribute = xmlNode.Attributes[name];
            if (xmlAttribute != null)
            {
                return ReadAttribute(xmlAttribute.Value, defaultValue);
            }

            return defaultValue;
        }

        public static void WriteAttribute(XmlElement xmlElement, string name, bool value, bool defaultValue = false)
        {
            if (value != defaultValue)
            {
                xmlElement.SetAttribute(name, value.ToString());
            }
        }

        public static bool ReadAttribute(string stringValue, bool defaultValue)
        {
            bool result = defaultValue;
            bool.TryParse(stringValue, out result);
            return result;
        }

        #endregion

        #region int property

        public static int ReadAttribute(XmlNode xmlNode, string name, int defaultValue)
        {
            XmlAttribute xmlAttribute = xmlNode.Attributes[name];
            if (xmlAttribute != null)
            {
                int result = defaultValue;
                int.TryParse(xmlAttribute.Value, out result);
                return result;
            }

            return defaultValue;
        }

        public static int ReadAttribute(string stringValue, int defaultValue)
        {
            int result = defaultValue;
            int.TryParse(stringValue, out result);
            return result;
        }

        public static void WriteAttribute(XmlElement xmlElement, string name, int value, int defaultValue = 0)
        {
            if (!value.Equals(defaultValue))
            {
                xmlElement.SetAttribute(name, value.ToString());
            }
        }

        #endregion

        #region uint property

        public static uint ReadAttribute(XmlNode xmlNode, string name, uint defaultValue)
        {
            XmlAttribute xmlAttribute = xmlNode.Attributes[name];
            if (xmlAttribute != null)
            {
                uint result = defaultValue;
                uint.TryParse(xmlAttribute.Value, out result);
                return result;
            }

            return defaultValue;
        }

        public static uint ReadAttribute(string stringValue, uint defaultValue)
        {
            uint result = defaultValue;
            uint.TryParse(stringValue, out result);
            return result;
        }

        public static void WriteAttribute(XmlElement xmlElement, string name, uint value, uint defaultValue = 0)
        {
            if (!value.Equals(defaultValue))
            {
                xmlElement.SetAttribute(name, value.ToString());
            }
        }

        #endregion

        #region float property

        public static float ReadAttribute(XmlNode xmlNode, string name, float defaultValue)
        {
            XmlAttribute xmlAttribute = xmlNode.Attributes[name];
            if (xmlAttribute != null)
            {
                float result = defaultValue;
                float.TryParse(xmlAttribute.Value, out result);
                return result;
            }

            return defaultValue;
        }

        public static float ReadAttribute(string stringValue, float defaultValue)
        {
            float result = defaultValue;
            float.TryParse(stringValue, out result);
            return result;
        }

        public static void WriteAttribute(XmlElement xmlElement, string name, float value, float defaultValue = 0)
        {
            if (!value.Equals(defaultValue))
            {
                xmlElement.SetAttribute(name, value.ToString());
            }
        }

        #endregion

        #region double property

        public static double ReadAttribute(XmlNode xmlNode, string name, double defaultValue)
        {
            XmlAttribute xmlAttribute = xmlNode.Attributes[name];
            if (xmlAttribute != null)
            {
                double result = defaultValue;
                double.TryParse(xmlAttribute.Value, out result);
                return result;
            }

            return defaultValue;
        }

        public static double ReadAttribute(string stringValue, double defaultValue)
        {
            double result = defaultValue;
            double.TryParse(stringValue, out result);
            return result;
        }

        public static void WriteAttribute(XmlElement xmlElement, string name, double value, double defaultValue = 0)
        {
            if (!value.Equals(defaultValue))
            {
                xmlElement.SetAttribute(name, value.ToString());
            }
        }

        #endregion

        #region decimal property

        public static decimal ReadAttribute(XmlNode xmlNode, string name, decimal defaultValue)
        {
            XmlAttribute xmlAttribute = xmlNode.Attributes[name];
            if (xmlAttribute != null)
            {
                decimal result = defaultValue;
                decimal.TryParse(xmlAttribute.Value, out result);
                return result;
            }

            return defaultValue;
        }

        public static void WriteAttribute(XmlElement xmlElement, string name, decimal value, decimal defaultValue = 0)
        {
            if (!value.Equals(defaultValue))
            {
                xmlElement.SetAttribute(name, value.ToString());
            }
        }

        #endregion

        #region string property

        public static string ReadAttribute(XmlNode xmlNode, string name, string defaultValue)
        {
            XmlAttribute xmlAttribute = xmlNode.Attributes[name];
            if (xmlAttribute != null)
            {
                return xmlAttribute.Value;
            }

            return defaultValue;
        }

        public static void WriteAttribute(XmlElement xmlElement, string name, string value, string defaultValue = null)
        {
            if (value != defaultValue)
            {
                xmlElement.SetAttribute(name, value.ToString());
            }
        }

        #endregion

        #region DateTime property

        public static DateTime ReadAttribute(XmlNode xmlNode, string name, DateTime defaultValue)
        {
            XmlAttribute xmlAttribute = xmlNode.Attributes[name];
            if (xmlAttribute != null)
            {
                DateTime result = defaultValue;

                // 优先尝试使用包含毫秒的精确格式
                if (DateTime.TryParseExact(xmlAttribute.Value,
                    new string[] {
                        "yyyy-MM-dd HH:mm:ss.fff",
                        "yyyy-MM-ddTHH:mm:ss.fff",
                        "yyyy/MM/dd HH:mm:ss.fff",
                        "yyyy/MM/ddTHH:mm:ss.fff"
                    },
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.None,
                    out result))
                {
                    return result;
                }

                // 如果精确格式失败，尝试通用解析
                if (DateTime.TryParse(xmlAttribute.Value, out result))
                {
                    return result;
                }
            }

            return defaultValue;
        }

        public static void WriteAttribute(XmlElement xmlElement, string name, DateTime value, DateTime defaultValue = default(DateTime))
        {
            if (value != defaultValue)
            {
                // 使用包含毫秒的 ISO 8601 格式，确保精确到毫秒
                xmlElement.SetAttribute(name, value.ToString("yyyy-MM-dd HH:mm:ss.fff"));
            }
        }

        #endregion

        #region Enum property

        public static TEnum ReadAttribute<TEnum>(XmlNode xmlNode, string name, TEnum defaultValue) where TEnum : struct
        {
            XmlAttribute xmlAttribute = xmlNode.Attributes[name];
            if (xmlAttribute != null)
            {
                TEnum result = defaultValue;
                Enum.TryParse<TEnum>(xmlAttribute.Value, true, out result);
                return result;
            }

            return defaultValue;
        }

        public static TEnum ReadAttribute<TEnum>(string stringValue, TEnum defaultValue) where TEnum : struct
        {
            TEnum result = defaultValue;
            Enum.TryParse<TEnum>(stringValue, true, out result);
            return result;
        }

        public static void WriteAttribute<TEnum>(XmlElement xmlElement, string name, TEnum value, TEnum defaultValue) where TEnum : Enum
        {
            if (!EqualityComparer<TEnum>.Default.Equals(value, defaultValue))
            {
                xmlElement.SetAttribute(name, value.ToString());
            }
        }

        #endregion

        public static XmlElement EnsureGetEmptyXmlElement(XmlNode parentNode, string name)
        {
            XmlElement xmlElement = parentNode[name];
            if (xmlElement == null)
            {
                xmlElement = parentNode.OwnerDocument.CreateElement(name);
                parentNode.AppendChild(xmlElement);
            }
            else
            {
                xmlElement.RemoveAll();
            }

            return xmlElement;
        }
    }
}
