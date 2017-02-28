using System.Collections.Generic;

namespace Dev2.Common.Interfaces
{
    public interface ICircularDependency
    {
        List<IDependencyVisualizationNode> Nodes { get; }
    }
}