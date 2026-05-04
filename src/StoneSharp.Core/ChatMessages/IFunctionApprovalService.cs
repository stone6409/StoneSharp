namespace StoneSharp.Core.ChatMessages
{
    public interface IFunctionApprovalService
    {
        bool IsInvocationApproved(string pluginName, string functionName, FunctionArguments arguments);
    }
}
