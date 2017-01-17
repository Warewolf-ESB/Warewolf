using System.Collections.Generic;
using Dev2.Common.Interfaces.DB;

namespace Dev2.Common.Interfaces
{
    public interface IConstructor
    {
        IList<IServiceInput> Inputs { get; set; }
        string ReturnObject { get; set; }
        string ConstructorName { get; set; }
        string GetIdentifier();
    }

    public interface IPluginConstructor : IConstructor
    {
        bool IsExistingObject { get; set; }
    }
}