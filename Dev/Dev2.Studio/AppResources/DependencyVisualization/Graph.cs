
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dev2.AppResources.DependencyVisualization
{
    /// <summary>
    /// Represents a set of nodes that can be dependent upon each other, 
    /// and will detect circular dependencies between its nodes.
    /// </summary>
    public class Graph
    {
        #region Constructor

        public Graph(string title)
        {
            CircularDependencies = new List<CircularDependency>();
            Nodes = new List<Node>();
            Title = title;
        }

        #endregion Constructor

        #region Properties

        public int GridColumn { get; set; }
        public int ColSpan { get; set; }

        public List<CircularDependency> CircularDependencies { get; private set; }

        public bool HasCircularDependency
        {
            get { return CircularDependencies.Any(); }
        }

        public List<Node> Nodes { get; private set; }

        public string Title { get; private set; }

        #endregion Properties

        #region Methods

        public new string ToString()
        {
            StringBuilder result = new StringBuilder(string.Format("<graph title=\"{0}\">",Title));

            foreach (var node in Nodes)
            {
                result.Append(node.ToString());
            }

            result.Append("</graph>");

            return result.ToString();
        } 

        /// <summary>
        /// Inspects the graph's nodes for circular dependencies.
        /// </summary>
        public void CheckForCircularDependencies()
        {
            foreach (Node node in Nodes)
            {
                var circularDependencies = node.FindCircularDependencies();
                if (circularDependencies != null)
                    ProcessCircularDependencies(circularDependencies);
            }

            CircularDependencies.Sort();
        }

        public void ProcessCircularDependencies(List<CircularDependency> circularDependencies)
        {
            foreach (CircularDependency circularDependency in circularDependencies)
            {
                if (circularDependency.Nodes.Count == 0)
                    continue;

                if (CircularDependencies.Contains(circularDependency))
                    continue;

                // Arrange the nodes into the order in which they were discovered.
                circularDependency.Nodes.Reverse();

                CircularDependencies.Add(circularDependency);

                // Inform each node that it is a member of the circular dependency.
                foreach (Node dependency in circularDependency.Nodes)
                    dependency.CircularDependencies.Add(circularDependency);
            }
        }

        public List<Node> GetAllUniqueNodesRecirsively()
        {
            return GetAllUniqueNodesRecirsively(new Stack<Node>(), Nodes);
        }

        public List<Node> GetAllUniqueNodesRecirsively(Stack<Node> nodeStack, List<Node> childNodes)
        {
            List<Node> nodes = new List<Node>();

            if (nodeStack == null || childNodes == null)
            {
                return nodes;
            }

            nodes.AddRange(childNodes.Where(childNode => !nodes.Any(n => childNode.ID == n.ID)));

            foreach (Node node in childNodes)
            {
                if (nodeStack.Contains(node))
                {
                    continue;
                }

                nodeStack.Push(node);
                nodes.AddRange(GetAllUniqueNodesRecirsively(nodeStack, node.NodeDependencies).Where(childNode => !nodes.Any(n => childNode.ID == n.ID)));
                nodeStack.Pop();
            }

            return nodes;
        }

        #endregion Methods
    }
}
