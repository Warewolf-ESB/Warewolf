using System.Collections.Generic;
using Dev2.Common.Interfaces.DB;

namespace Dev2.Common.Interfaces
{
    public interface IConstructor
    {
        IList<IServiceInput> Inputs { get; set; }
        string ReturnType { get; set; }
        IList<INameValue> Variables { get; set; }
        string ConstructorName { get; set; }
        string GetIdentifier();
    }

    public interface IPluginConstructor : IConstructor
    {

    }
}