using System.Collections.Generic;

namespace Dev2.Common.Interfaces
{
    public interface IGraph
    {
        int GridColumn { get; set; }
        int ColSpan { get; set; }
        List<ICircularDependency> CircularDependencies { get; }
        bool HasCircularDependency { get; }
        List<IDependencyVisualizationNode> Nodes { get; }
        string Title { get; }

        /// <summary>
        /// Inspects the graph's nodes for circular dependencies.
        /// </summary>
        void CheckForCircularDependencies();

        void ProcessCircularDependencies(IEnumerable<ICircularDependency> circularDependencies);
        string ToString();
    }
}