using System;
using Dev2.Common.Interfaces.Core.Graph;

namespace Dev2.Common.Interfaces
{
    public interface ISimpePlugin: IEquatable<ISimpePlugin>
    {
        IPluginAction Method { get; set; }
        INamespaceItem Namespace { get; set; }
        IOutputDescription OutputDescription { get; set; }
    }
}