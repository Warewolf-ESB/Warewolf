using System.Collections.Generic;
using Dev2.Common.Interfaces.DB;

namespace Dev2.Common.Interfaces
{
    public interface IEnhancedPlugin
    {
        INamespaceItem Namespace { get; set; }
        IPluginConstructor Constructor { get; set; }
        List<IPluginAction> MethodsToRun { get; set; }
        List<IServiceInput> ConstructorInputs { get; set; }
    }
}