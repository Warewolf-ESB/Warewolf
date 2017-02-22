using System.Text;

namespace Dev2.Common.Interfaces
{
    public interface IDependencyGraphGenerator
    {
        /// <summary>
        /// Builds the graph.
        /// </summary>
        /// <param name="xmlData">The XML data.</param>
        /// <param name="modelName">Name of the model.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="nestingLevel">How deep should the graph show.</param>
        /// <returns></returns>
        IGraph BuildGraph(StringBuilder xmlData, string modelName, double width, double height, int nestingLevel);
    }
}