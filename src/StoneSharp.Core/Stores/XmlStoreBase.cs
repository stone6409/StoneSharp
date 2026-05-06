using System;
using System.IO;
using System.Xml;

namespace StoneSharp.Core.Stores
{
    public abstract class XmlStoreBase
    {
        String _configFileName;

        XmlDocument _xmlDocument;

        public XmlStoreBase(string configFileName, bool ensureExist = false)
        {
            _configFileName = configFileName;

            if (ensureExist)
            {
                EnsureConfigFileExist();
            }
        }

        public string ConfigFileName
        {
            get
            {
                return _configFileName;
            }
        }

        public void Load()
        {
            _xmlDocument = null;
            _xmlDocument = GetXmlDocument();
        }

        protected XmlDocument GetXmlDocument()
        {
            if (_xmlDocument == null)
            {
                XmlReaderSettings readerSettings = new XmlReaderSettings();
                readerSettings.IgnoreComments = true;
                using (XmlReader reader = XmlReader.Create(ConfigFileName, readerSettings))
                {
                    XmlDocument xmlDocument = new XmlDocument();
                    xmlDocument.Load(reader);

                    return xmlDocument;
                }
            }

            return _xmlDocument;
        }

        protected void SaveXmlDocument(XmlDocument xmlDocument)
        {
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.IndentChars = "\t";
            settings.OmitXmlDeclaration = true;

            using (XmlWriter writer = XmlWriter.Create(ConfigFileName, settings))
            {
                xmlDocument.Save(writer);
            }
        }

        /// <summary>
        /// 原子保存XML文档，确保写入过程中断时不会损坏原文件
        /// </summary>
        /// <param name="xmlDocument">要保存的XML文档</param>
        protected void SaveXmlDocumentSafely(XmlDocument xmlDocument)
        {
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.IndentChars = "\t";
            settings.OmitXmlDeclaration = true;

            // 使用临时文件进行原子写入
            string tempFileName = ConfigFileName + ".tmp";

            try
            {
                // 1. 清理XML文档中的非法字符
                SanitizeXmlDocument(xmlDocument);

                // 2. 写入到临时文件
                using (XmlWriter writer = XmlWriter.Create(tempFileName, settings))
                {
                    xmlDocument.Save(writer);
                }

                // 2. 验证临时文件内容（可选但推荐）
                if (!ValidateXmlFile(tempFileName))
                {
                    throw new InvalidOperationException("临时文件验证失败");
                }

                // 3. 原子替换原文件
                File.Replace(tempFileName, ConfigFileName, ConfigFileName + ".bak");
            }
            catch
            {
                // 清理临时文件
                if (File.Exists(tempFileName))
                {
                    try { File.Delete(tempFileName); } catch { }
                }
                throw;
            }
            finally
            {
                // 清理备份文件（可选）
                string backupFile = ConfigFileName + ".bak";
                if (File.Exists(backupFile))
                {
                    try { File.Delete(backupFile); } catch { }
                }
            }
        }

        /// <summary>
        /// 清理XML文档中所有节点和属性的非法字符（如0x00空字符）
        /// </summary>
        private static void SanitizeXmlDocument(XmlDocument xmlDocument)
        {
            if (xmlDocument == null) return;

            SanitizeXmlNode(xmlDocument.DocumentElement);
        }

        private static void SanitizeXmlNode(XmlNode node)
        {
            if (node == null) return;

            // 清理属性值
            if (node.Attributes != null)
            {
                foreach (XmlAttribute attribute in node.Attributes)
                {
                    if (attribute.Value != null)
                    {
                        string sanitized = RemoveInvalidXmlChars(attribute.Value);
                        if (sanitized != attribute.Value)
                        {
                            attribute.Value = sanitized;
                        }
                    }
                }
            }

            // 清理文本节点
            if (node is XmlText textNode)
            {
                if (textNode.Value != null)
                {
                    string sanitized = RemoveInvalidXmlChars(textNode.Value);
                    if (sanitized != textNode.Value)
                    {
                        textNode.Value = sanitized;
                    }
                }
            }

            // 递归处理子节点
            if (node.ChildNodes != null)
            {
                for (int i = 0; i < node.ChildNodes.Count; i++)
                {
                    SanitizeXmlNode(node.ChildNodes[i]);
                }
            }
        }

        /// <summary>
        /// 移除字符串中的非法XML字符（XML 1.0标准）
        /// </summary>
        private static string RemoveInvalidXmlChars(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;

            int len = text.Length;
            char[] result = new char[len];
            int resultIndex = 0;

            for (int i = 0; i < len; i++)
            {
                char ch = text[i];

                // 检查是否为有效的XML 1.0字符
                // Char ::= #x9 | #xA | #xD | [#x20-#xD7FF] | [#xE000-#xFFFD] | [#x10000-#x10FFFF]
                bool isValid =
                    ch == 0x9 ||
                    ch == 0xA ||
                    ch == 0xD ||
                    (ch >= 0x20 && ch <= 0xD7FF) ||
                    (ch >= 0xE000 && ch <= 0xFFFD);

                if (isValid)
                {
                    result[resultIndex++] = ch;
                }
                else
                {

                }
            }

            return new string(result, 0, resultIndex);
        }

        private bool ValidateXmlFile(string filePath)
        {
            try
            {
                // 尝试加载XML文件来验证其完整性
                XmlReaderSettings readerSettings = new XmlReaderSettings();
                readerSettings.IgnoreComments = true;

                using (XmlReader reader = XmlReader.Create(filePath, readerSettings))
                {
                    XmlDocument tempDoc = new XmlDocument();
                    tempDoc.Load(reader);
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        void EnsureConfigFileExist()
        {
            if (!File.Exists(_configFileName))
            {
                XmlDocument xmlDocument = new XmlDocument();
                OnCreateXmlDocument(xmlDocument);

                xmlDocument.Save(_configFileName);
            }
        }

        protected abstract void OnCreateXmlDocument(XmlDocument xmlDocument);
    }
}

