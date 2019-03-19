#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
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
        public CircularDependency(IEnumerable<IDependencyVisualizationNode> nodes)
        {
            Nodes = new List<IDependencyVisualizationNode>(nodes);
        }

        public List<IDependencyVisualizationNode> Nodes { get; private set; }
        
        public override bool Equals(object obj)
        {
            var other = obj as CircularDependency;
            return
                other != null &&
                other.Nodes.Count == Nodes.Count &&
                other.Nodes.Intersect(Nodes).Count() == Nodes.Count;
        }

        public override int GetHashCode() => Nodes.GetHashCode();

        public int CompareTo(CircularDependency other) => Nodes.Count.CompareTo(other.Nodes.Count);
    }
}