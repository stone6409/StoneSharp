using StoneSharp.Core.ChatMessages;
using StoneSharp.Core.Models;
using StoneSharp.Core.Models.ContextItems;
using System.Xml;

namespace StoneSharp.Core.Stores
{
    public static class ChatConversationXmlMaper
    {
        #region Write Object

        public static void WriteChatConversation(ChatConversation chatConversation, XmlNode xmlNode)
        {
            XmlElement xmlElement = xmlNode.OwnerDocument.CreateElement("ChatConversation");

            XmlAttributeHelper.WriteAttribute(xmlElement, "Id", chatConversation.Id, (string)null);
            WriteRequestMessage(chatConversation.RequestMessage, xmlElement);
            if (chatConversation.ReplyMessage != null)
            {
                WriteReplyMessage(chatConversation.ReplyMessage, xmlElement);
            }

            xmlNode.AppendChild(xmlElement);
        }

        public static void WriteRequestMessage(RequestMessage requestMessage, XmlNode xmlNode)
        {
            XmlElement xmlElement = xmlNode.OwnerDocument.CreateElement("RequestMessage");

            XmlAttributeHelper.WriteAttribute(xmlElement, "Time", requestMessage.Time);
            XmlAttributeHelper.WriteAttribute(xmlElement, "Prompt", requestMessage.Prompt);
            if (requestMessage.ContextItems.Count > 0)
            {
                WriteContextItems(requestMessage.ContextItems, xmlElement);
            }

            xmlNode.AppendChild(xmlElement);
        }

        public static void WriteContextItems(IEnumerable<ContextItem> contextItems, XmlNode xmlNode)
        {
            foreach (ContextItem contextItem in contextItems)
            {
                ContextItemXmlMaper.WriteContextItem(contextItem, xmlNode);
            }
        }

        public static void WriteReplyMessage(ReplyMessage replyMessage, XmlNode xmlNode)
        {
            XmlElement xmlElement = xmlNode.OwnerDocument.CreateElement("ReplyMessage");

            XmlAttributeHelper.WriteAttribute(xmlElement, "AiModel", replyMessage.AiModel);
            XmlAttributeHelper.WriteAttribute(xmlElement, "Time", replyMessage.Time);
            XmlAttributeHelper.WriteAttribute(xmlElement, "Result", replyMessage.Result);
            XmlAttributeHelper.WriteAttribute(xmlElement, "ReasoningContent", replyMessage.ReasoningContent);

            // 添加工具调用的序列化
            if (replyMessage.ToolCalls != null && replyMessage.ToolCalls.Count > 0)
            {
                WriteToolCalls(replyMessage.ToolCalls, xmlElement);
            }

            xmlNode.AppendChild(xmlElement);
        }

        // 添加工具调用的序列化方法
        public static void WriteToolCalls(IEnumerable<ToolCall> toolCalls, XmlNode xmlNode)
        {
            XmlElement toolCallsElement = xmlNode.OwnerDocument.CreateElement("ToolCalls");

            foreach (ToolCall toolCall in toolCalls)
            {
                WriteToolCall(toolCall, toolCallsElement);
            }

            xmlNode.AppendChild(toolCallsElement);
        }

        public static void WriteToolCall(ToolCall toolCall, XmlNode xmlNode)
        {
            XmlElement toolCallElement = xmlNode.OwnerDocument.CreateElement("ToolCall");

            XmlAttributeHelper.WriteAttribute(toolCallElement, "PluginName", toolCall.PluginName);
            XmlAttributeHelper.WriteAttribute(toolCallElement, "FunctionName", toolCall.FunctionName);
            XmlAttributeHelper.WriteAttribute(toolCallElement, "CallId", toolCall.CallId);
            XmlAttributeHelper.WriteAttribute(toolCallElement, "ReasoningContent", toolCall.ReasoningContent);

            // 序列化 FunctionArguments 为 JSON 字符串
            string argumentsJson = toolCall.Arguments?.ToJson(false) ?? "{}";
            XmlAttributeHelper.WriteAttribute(toolCallElement, "Arguments", argumentsJson);

            XmlAttributeHelper.WriteAttribute(toolCallElement, "Result", toolCall.Result);
            XmlAttributeHelper.WriteAttribute(toolCallElement, "Status", toolCall.Status);
            XmlAttributeHelper.WriteAttribute(toolCallElement, "Error", toolCall.Error);
            XmlAttributeHelper.WriteAttribute(toolCallElement, "StartTime", toolCall.StartTime);

            if (toolCall.EndTime.HasValue)
            {
                XmlAttributeHelper.WriteAttribute(toolCallElement, "EndTime", toolCall.EndTime.Value);
            }

            xmlNode.AppendChild(toolCallElement);
        }

        #endregion

        #region Read Object

        public static ChatConversation ReadChatConversation(XmlNode xmlNode)
        {
            ChatConversation chatConversation = new ChatConversation();
            chatConversation.Id = XmlAttributeHelper.ReadAttribute(xmlNode, "Id", (string)null);

            XmlNode requestMessageNode = xmlNode.SelectSingleNode("RequestMessage");
            if (requestMessageNode != null)
            {
                chatConversation.RequestMessage = ReadRequestMessage(requestMessageNode);
            }

            XmlNode replyMessageNode = xmlNode.SelectSingleNode("ReplyMessage");
            if (replyMessageNode != null)
            {
                chatConversation.ReplyMessage = ReadReplyMessage(replyMessageNode);
            }

            return chatConversation;
        }

        public static RequestMessage ReadRequestMessage(XmlNode xmlNode)
        {
            RequestMessage requestMessage = new RequestMessage();

            requestMessage.Time = XmlAttributeHelper.ReadAttribute(xmlNode, "Time", default(DateTime));
            requestMessage.Prompt = XmlAttributeHelper.ReadAttribute(xmlNode, "Prompt", null);
            requestMessage.ContextItems = ReadContextItems(xmlNode);

            return requestMessage;
        }

        public static List<ContextItem> ReadContextItems(XmlNode xmlNode)
        {
            List<ContextItem> contextItems = new List<ContextItem>();

            foreach (XmlNode childNode in xmlNode.ChildNodes)
            {
                ContextItem contextItem = ContextItemXmlMaper.ReadContextItem(childNode);
                if (contextItem != null)
                {
                    contextItems.Add(contextItem);
                }
            }

            return contextItems;
        }

        public static ReplyMessage ReadReplyMessage(XmlNode xmlNode)
        {
            ReplyMessage replyMessage = new ReplyMessage();
            replyMessage.AiModel = XmlAttributeHelper.ReadAttribute(xmlNode, "AiModel", null);
            replyMessage.Time = XmlAttributeHelper.ReadAttribute(xmlNode, "Time", default(DateTime));
            replyMessage.Result = XmlAttributeHelper.ReadAttribute(xmlNode, "Result", null);
            replyMessage.ReasoningContent = XmlAttributeHelper.ReadAttribute(xmlNode, "ReasoningContent", null);

            // 读取工具调用数据
            XmlNode toolCallsNode = xmlNode.SelectSingleNode("ToolCalls");
            if (toolCallsNode != null)
            {
                replyMessage.ToolCalls = ReadToolCalls(toolCallsNode);
            }

            return replyMessage;
        }

        // 添加工具调用的反序列化方法
        public static List<ToolCall> ReadToolCalls(XmlNode xmlNode)
        {
            List<ToolCall> toolCalls = new List<ToolCall>();

            foreach (XmlNode childNode in xmlNode.ChildNodes)
            {
                if (childNode.Name == "ToolCall")
                {
                    ToolCall toolCall = ReadToolCall(childNode);
                    if (toolCall != null)
                    {
                        toolCalls.Add(toolCall);
                    }
                }
            }

            return toolCalls;
        }

        public static ToolCall ReadToolCall(XmlNode xmlNode)
        {
            ToolCall toolCall = new ToolCall();

            toolCall.PluginName = XmlAttributeHelper.ReadAttribute(xmlNode, "PluginName", null);
            toolCall.FunctionName = XmlAttributeHelper.ReadAttribute(xmlNode, "FunctionName", null);
            toolCall.CallId = XmlAttributeHelper.ReadAttribute(xmlNode, "CallId", null);
            toolCall.ReasoningContent = XmlAttributeHelper.ReadAttribute(xmlNode, "ReasoningContent", null);

            // 从 XML 读取 Arguments 字符串，支持旧格式和新 JSON 格式
            string argumentsStr = XmlAttributeHelper.ReadAttribute(xmlNode, "Arguments", "{}");
            toolCall.Arguments = ParseArgumentsString(argumentsStr);

            toolCall.Result = XmlAttributeHelper.ReadAttribute(xmlNode, "Result", null);
            toolCall.Status = XmlAttributeHelper.ReadAttribute(xmlNode, "Status", null);
            toolCall.Error = XmlAttributeHelper.ReadAttribute(xmlNode, "Error", null);
            toolCall.StartTime = XmlAttributeHelper.ReadAttribute(xmlNode, "StartTime", default(DateTime));

            string endTimeStr = XmlAttributeHelper.ReadAttribute(xmlNode, "EndTime", null);
            if (!string.IsNullOrEmpty(endTimeStr) && DateTime.TryParse(endTimeStr, out DateTime endTime))
            {
                toolCall.EndTime = endTime;
            }


            return toolCall;
        }

        /// <summary>
        /// 解析 Arguments 字符串，支持旧格式和新 JSON 格式
        /// </summary>
        private static FunctionArguments ParseArgumentsString(string argumentsStr)
        {
            // 空字符串或空白字符串直接返回空的 FunctionArguments
            if (string.IsNullOrWhiteSpace(argumentsStr))
            {
                return new FunctionArguments();
            }

            // 如果是空 JSON 对象，也返回空的 FunctionArguments
            if (argumentsStr == "{}")
            {
                return new FunctionArguments();
            }

            // 检查是否为有效的 JSON 对象格式（以 '{' 开头，以 '}' 结尾）
            if (argumentsStr.Length < 2 || argumentsStr[0] != '{' || argumentsStr[^1] != '}')
            {
                return new FunctionArguments();
            }

            // 尝试解析为 JSON 格式（新格式）
            if (FunctionArgumentsJsonExtensions.TryFromJson(argumentsStr, out var result))
            {
                return result;
            }

            // 如果 JSON 解析失败，尝试处理旧格式
            // 旧格式可能是简单的字符串或其他格式，这里创建一个空的 FunctionArguments
            // 或者根据旧格式的具体结构进行解析
            // 目前先返回空的 FunctionArguments，后续可以根据实际需求调整
            return new FunctionArguments();
        }

        #endregion
    }
}