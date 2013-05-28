using System;
using Dev2.Common;
using Dev2.Data.ServiceModel.Messages;
using Dev2.DynamicServices;

namespace Dev2.Runtime.Compiler.CompileRules
{
    /// <summary>
    /// Used to pick compile rules
    /// </summary>
    public interface IServiceCompileRule : ISpookyLoadable<enActionType>
    {

        CompileMessageTO ApplyRule(Guid serviceID, ServiceAction beforeAction, string afterAction);
    }
}
