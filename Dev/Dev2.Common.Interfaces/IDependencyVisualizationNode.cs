using System.Collections.Generic;

namespace Dev2.Common.Interfaces
{
    public interface IDependencyVisualizationNode
    {
        List<ICircularDependency> CircularDependencies { get; }
        bool HasCircularDependency { get; }
        string ID { get; }
        bool IsTargetNode { get; set; }
        string ErrorImagePath { get; }
        double LocationX { get; set; }
        double LocationY { get; set; }
        List<IDependencyVisualizationNode> NodeDependencies { get; }
        double NodeWidth { get; }
        double NodeHeight { get; }
        bool IsBroken { get; }
        List<ICircularDependency> FindCircularDependencies();
        string ToString();
    }
}