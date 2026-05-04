using Microsoft.SemanticKernel;
using System;

namespace StoneSharp.Core.ChatMessages.SemanticKernel
{
    public sealed class PluginFunctionFilter : IAutoFunctionInvocationFilter
    {
        private readonly PluginFunctionService _pluginFunctionService;

        public PluginFunctionFilter(IPluginFunctionService pluginFunctionService)
        {
            _pluginFunctionService = pluginFunctionService as PluginFunctionService
                ?? throw new ArgumentNullException(nameof(pluginFunctionService));
        }

        public async Task OnAutoFunctionInvocationAsync(AutoFunctionInvocationContext context, Func<AutoFunctionInvocationContext, Task> next)
        {
            // 触发函数调用开始事件，并获取事件参数
            var invokingArgs = _pluginFunctionService.RaiseFunctionInvoking(context);

            // 检查是否被批准
            if (invokingArgs.IsApproved)
            {
                var invocationTime = DateTime.Now;

                try
                {
                    // 执行函数调用
                    await next(context);

                    // 触发函数调用完成事件
                    _pluginFunctionService.RaiseFunctionInvoked(context, invocationTime);
                }
                catch (Exception ex)
                {
                    // 触发函数调用错误事件
                    _pluginFunctionService.RaiseFunctionError(context, invocationTime, ex);
                    throw;
                }
            }
            else
            {
                // 触发函数调用被拒绝事件
                _pluginFunctionService.RaiseFunctionRejected(context, invokingArgs.RejectionReason);

                // 返回拒绝结果
                context.Result = new FunctionResult(context.Result,
                    invokingArgs.RejectionReason ?? "Operation was rejected by user.");
            }
        }
    }
}
