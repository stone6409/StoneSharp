using Microsoft.SemanticKernel;
using System;

namespace StoneSharp.Core.ChatMessages.SemanticKernel
{
    /// <summary>
    /// 插件函数服务接口
    /// </summary>
    public interface IPluginFunctionService
    {
        /// <summary>
        /// 函数调用开始前触发 - 调用者可以在这里决定是否批准调用
        /// </summary>
        event EventHandler<FunctionInvokingEventArgs> FunctionInvoking;

        /// <summary>
        /// 函数调用完成后触发
        /// </summary>
        event EventHandler<FunctionInvokedEventArgs> FunctionInvoked;

        /// <summary>
        /// 函数调用被拒绝时触发
        /// </summary>
        event EventHandler<FunctionRejectedEventArgs> FunctionRejected;

        /// <summary>
        /// 函数调用出错时触发
        /// </summary>
        event EventHandler<FunctionErrorEventArgs> FunctionError;
    }
}