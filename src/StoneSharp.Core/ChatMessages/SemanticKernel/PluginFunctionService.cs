using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using System;

namespace StoneSharp.Core.ChatMessages.SemanticKernel
{
    internal class PluginFunctionService : IPluginFunctionService
    {
        // 事件声明
        public event EventHandler<FunctionInvokingEventArgs> FunctionInvoking;
        public event EventHandler<FunctionInvokedEventArgs> FunctionInvoked;
        public event EventHandler<FunctionRejectedEventArgs> FunctionRejected;
        public event EventHandler<FunctionErrorEventArgs> FunctionError;

        /// <summary>
        /// 构造函数
        /// </summary>
        public PluginFunctionService()
        {
        }

        // 触发事件的方法
        protected virtual void OnFunctionInvoking(FunctionInvokingEventArgs e)
        {
            FunctionInvoking?.Invoke(this, e);
        }

        protected virtual void OnFunctionInvoked(FunctionInvokedEventArgs e)
        {
            FunctionInvoked?.Invoke(this, e);
        }

        protected virtual void OnFunctionRejected(FunctionRejectedEventArgs e)
        {
            FunctionRejected?.Invoke(this, e);
        }

        protected virtual void OnFunctionError(FunctionErrorEventArgs e)
        {
            FunctionError?.Invoke(this, e);
        }

        // 实现接口方法 - 触发函数调用开始事件
        internal FunctionInvokingEventArgs RaiseFunctionInvoking(AutoFunctionInvocationContext context)
        {
            var arguments = new FunctionArguments(context.Arguments);
            object reasoningContent = null;
            context.ChatMessageContent.Metadata?.TryGetValue(OpenAIChatMessageContent.ReasoningContentProperty, out reasoningContent);

            var args = new FunctionInvokingEventArgs(
                context.Function.PluginName,
                context.Function.Name,
                context.ToolCallId,
                arguments);

            // 传递 reasoning content
            if (reasoningContent != null && reasoningContent is string rc)
            {
                args.ReasoningContent = rc;
            }

            // 触发事件，调用者可以在这里设置IsApproved
            OnFunctionInvoking(args);

            return args;
        }

        internal void RaiseFunctionInvoked(AutoFunctionInvocationContext context, DateTime invocationTime)
        {
            var arguments = new FunctionArguments(context.Arguments);

            var args = new FunctionInvokedEventArgs(
                context.Function.PluginName, 
                context.Function.Name,
                context.ToolCallId,
                arguments, 
                context.Result.ToString(),
                invocationTime);
            OnFunctionInvoked(args);
        }

        internal void RaiseFunctionRejected(AutoFunctionInvocationContext context, string rejectionReason = null)
        {
            var arguments = new FunctionArguments(context.Arguments);

            var args = new FunctionRejectedEventArgs(
                context.Function.PluginName, 
                context.Function.Name,
                context.ToolCallId,
                arguments, 
                rejectionReason);
            OnFunctionRejected(args);
        }

        internal void RaiseFunctionError(AutoFunctionInvocationContext context, DateTime invocationTime, Exception exception)
        {
            var arguments = new FunctionArguments(context.Arguments);

            var args = new FunctionErrorEventArgs(
                context.Function.PluginName, 
                context.Function.Name,
                context.ToolCallId,
                arguments, 
                invocationTime, 
                exception);
            OnFunctionError(args);
        }
    }
}