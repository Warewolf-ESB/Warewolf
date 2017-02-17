/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using Dev2.Common.Interfaces;

namespace Dev2.Common.DependencyVisualization
{
    /// <summary>
    /// Represents a set of nodes that can be dependent upon each other,
    /// and will detect circular dependencies between its nodes.
    /// </summary>
    public class Graph : IGraph
    {
        #region Constructor

        public Graph(string title)
        {
            CircularDependencies = new List<ICircularDependency>();
            Nodes = new List<IDependencyVisualizationNode>();
            Title = title;
        }

        #endregion Constructor

        #region Properties

        public int GridColumn { get; set; }
        public int ColSpan { get; set; }

        public List<ICircularDependency> CircularDependencies { get; }

        public bool HasCircularDependency => CircularDependencies.Any();

        public List<IDependencyVisualizationNode> Nodes { get; }

        public string Title { get; }

        #endregion Properties

        #region Methods

        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        public new string ToString()
        {
            var result = new StringBuilder($"<graph title=\"{Title}\">");

            foreach (var node in Nodes)
            {
                result.Append(((DependencyVisualizationNode)node).ToString());
            }

            result.Append("</graph>");

            return result.ToString();
        }

        /// <summary>
        /// Inspects the graph's nodes for circular dependencies.
        /// </summary>
        public void CheckForCircularDependencies()
        {
            foreach (var node in Nodes)
            {
                var circularDependencies = node.FindCircularDependencies();
                if (circularDependencies != null)
                    ProcessCircularDependencies(circularDependencies);
            }

            CircularDependencies.Sort();
        }

        public void ProcessCircularDependencies(IEnumerable<ICircularDependency> circularDependencies)
        {
            foreach (var circularDependency in circularDependencies)
            {
                if (circularDependency.Nodes.Count == 0)
                    continue;

                if (CircularDependencies.Contains(circularDependency))
                    continue;

                // Arrange the nodes into the order in which they were discovered.
                circularDependency.Nodes.Reverse();

                CircularDependencies.Add(circularDependency);

                // Inform each node that it is a member of the circular dependency.
                foreach (var dependency in circularDependency.Nodes)
                    dependency.CircularDependencies.Add(circularDependency);
            }
        }

        #endregion Methods
    }
}