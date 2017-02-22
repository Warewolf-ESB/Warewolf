/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Linq;
using Dev2.Common.Interfaces;

namespace Dev2.Common.DependencyVisualization
{
    /// <summary>
    /// Represents a set of nodes in a graph that form a circular dependency.
    /// </summary>
    public class CircularDependency : IComparable<CircularDependency>, ICircularDependency
    {
        #region Constructor

        public CircularDependency(IEnumerable<IDependencyVisualizationNode> nodes)
        {
            Nodes = new List<IDependencyVisualizationNode>(nodes);
        }

        #endregion Constructor

        #region Properties

        public List<IDependencyVisualizationNode> Nodes { get; private set; }

        #endregion Properties

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

        #endregion Base Class Overrides

        #region IComparable<CircularDependency> Members

        public int CompareTo(CircularDependency other)
        {
            return Nodes.Count.CompareTo(other.Nodes.Count);
        }

        #endregion IComparable<CircularDependency> Members
    }
}