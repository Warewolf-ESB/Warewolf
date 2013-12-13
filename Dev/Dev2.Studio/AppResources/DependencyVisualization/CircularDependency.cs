using System;
using System.Collections.Generic;
using System.Linq;

namespace Dev2.AppResources.DependencyVisualization
{
    /// <summary>
    /// Represents a set of nodes in a graph that form a circular dependency.
    /// </summary>
    public class CircularDependency : IComparable<CircularDependency>
    {
        #region Constructor

        public CircularDependency(IEnumerable<Node> nodes)
        {
            Nodes = new List<Node>(nodes);
        }

        #endregion // Constructor

        #region Properties

        public List<Node> Nodes { get; private set; }

        #endregion // Properties

        #region Base Class Overrides

        public override bool Equals(object obj)
        {
            var other = obj as CircularDependency;
            return 
                other != null &&
                other.Nodes.Count == Nodes.Count &&
                other.Nodes.Intersect(Nodes).Count() == Nodes.Count;
        }

        public override int GetHashCode()
        {
            return Nodes.GetHashCode();
        }

        #endregion // Base Class Overrides

        #region IComparable<CircularDependency> Members

        public int CompareTo(CircularDependency other)
        {
            return Nodes.Count.CompareTo(other.Nodes.Count);
        }

        #endregion // IComparable<CircularDependency> Members
    }
}