namespace StoneSharp.Core.ChatMessages
{
    /// <summary>
    /// 函数事件参数基类
    /// </summary>
    public abstract class FunctionEventArgs : EventArgs
    {
        /// <summary>
        /// 插件名称
        /// </summary>
        public string PluginName { get; }

        /// <summary>
        /// 函数名称
        /// </summary>
        public string FunctionName { get; }

        /// <summary>
        /// 调用ID
        /// </summary>
        public string CallId { get; }

        /// <summary>
        /// 函数参数
        /// </summary>
        public FunctionArguments Arguments { get; }

        /// <summary>
        /// (DeepSeek models only) The reasoning content associated with the message, used in thinking mode.
        /// </summary>
        public string ReasoningContent { get; set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        protected FunctionEventArgs(string pluginName, string functionName, string callId, FunctionArguments arguments)
        {
            PluginName = pluginName;
            FunctionName = functionName;
            CallId = callId;
            Arguments = arguments;
        }
    }

    /// <summary>
    /// 函数调用开始事件参数
    /// </summary>
    public class FunctionInvokingEventArgs : FunctionEventArgs
    {
        /// <summary>
        /// 是否批准调用（默认为true，调用者可以在事件处理中设置）
        /// </summary>
        public bool IsApproved { get; set; } = true;

        /// <summary>
        /// 拒绝原因（如果IsApproved为false）
        /// </summary>
        public string RejectionReason { get; set; }

        public FunctionInvokingEventArgs(string pluginName, string functionName, string callId, FunctionArguments arguments)
            : base(pluginName, functionName, callId, arguments)
        {
        }
    }

    /// <summary>
    /// 函数调用完成事件参数
    /// </summary>
    public class FunctionInvokedEventArgs : FunctionEventArgs
    {
        public string Result { get; }

        public FunctionInvokedEventArgs(string pluginName, string functionName, string callId, FunctionArguments arguments, string result, DateTime invocationTime)
            : base(pluginName, functionName, callId, arguments)
        {
            Result = result;
        }
    }

    /// <summary>
    /// 函数调用被拒绝事件参数
    /// </summary>
    public class FunctionRejectedEventArgs : FunctionEventArgs
    {
        public string RejectionReason { get; }

        public FunctionRejectedEventArgs(string pluginName, string functionName, string callId, FunctionArguments arguments, string rejectionReason = null)
            : base(pluginName, functionName, callId, arguments)
        {
            RejectionReason = rejectionReason;
        }
    }

    /// <summary>
    /// 函数调用出错事件参数
    /// </summary>
    public class FunctionErrorEventArgs : FunctionEventArgs
    {
        public Exception Exception { get; }

        public FunctionErrorEventArgs(string pluginName, string functionName, string callId, FunctionArguments arguments, DateTime invocationTime, Exception exception)
            : base(pluginName, functionName, callId, arguments)
        {
            Exception = exception;
        }
    }
}