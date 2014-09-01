using System;
using System.Text;
using Dev2.Common.Interfaces.Patterns;
using Dev2.Data.ServiceModel.Messages;

namespace Dev2.Runtime.Compiler.CompileRules
{
    /// <summary>
    /// Used to pick compile rules
    /// </summary>
    public interface IServiceCompileRule : ISpookyLoadable<ServerCompileMessageType>
    {
        CompileMessageTO ApplyRule(Guid serviceID, StringBuilder beforeAction, StringBuilder afterAction);
    }
}
