using System;
using System.Collections.Generic;
using System.Linq;

namespace CircularDependencyTool
{
    /// <summary>
    /// Represents a set of nodes in a graph that form a circular dependency.
    /// </summary>
    public class CircularDependency : IComparable<CircularDependency>
    {
        #region Constructor

        public CircularDependency(IEnumerable<Node> nodes)
        {
            this.Nodes = new List<Node>(nodes);
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
                other.Nodes.Count == this.Nodes.Count &&
                other.Nodes.Intersect(this.Nodes).Count() == this.Nodes.Count;
        }

        public override int GetHashCode()
        {
            return this.Nodes.GetHashCode();
        }

        #endregion // Base Class Overrides

        #region IComparable<CircularDependency> Members

        public int CompareTo(CircularDependency other)
        {
            return this.Nodes.Count.CompareTo(other.Nodes.Count);
        }

        #endregion // IComparable<CircularDependency> Members
    }
}