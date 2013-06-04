using System.Collections.Generic;
using System.Linq;

namespace CircularDependencyTool
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
            this.CircularDependencies = new List<CircularDependency>();
            this.Nodes = new List<Node>();
            this.Title = title;
        }

        #endregion Constructor

        #region Properties

        public int GridColumn { get; set; }
        public int ColSpan { get; set; }

        public List<CircularDependency> CircularDependencies { get; private set; }

        public bool HasCircularDependency
        {
            get { return this.CircularDependencies.Any(); }
        }

        public List<Node> Nodes { get; private set; }

        public string Title { get; private set; }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Inspects the graph's nodes for circular dependencies.
        /// </summary>
        public void CheckForCircularDependencies()
        {
            foreach (Node node in this.Nodes)
            {
                var circularDependencies = node.FindCircularDependencies();
                if (circularDependencies != null)
                    this.ProcessCircularDependencies(circularDependencies);
            }

            this.CircularDependencies.Sort();
        }

        public void ProcessCircularDependencies(List<CircularDependency> circularDependencies)
        {
            foreach (CircularDependency circularDependency in circularDependencies)
            {
                if (circularDependency.Nodes.Count == 0)
                    continue;

                if (this.CircularDependencies.Contains(circularDependency))
                    continue;

                // Arrange the nodes into the order in which they were discovered.
                circularDependency.Nodes.Reverse();

                this.CircularDependencies.Add(circularDependency);

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