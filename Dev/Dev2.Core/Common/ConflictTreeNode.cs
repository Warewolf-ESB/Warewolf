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
using System.Linq;
using System.Windows;

namespace Dev2.Common
{
    public class ConflictTreeNode : IConflictTreeNode
    {
        public ConflictTreeNode(IDev2Activity act, Point location)
        {
            Activity = act;
            UniqueId = act.UniqueID;
            Location = location;
            IsInConflict = true;
        }

        public void AddChild(IConflictTreeNode node,string name)
        {
            if (Children == null)
            {
                Children = new List<(string name, IConflictTreeNode node)>();
            }
            Children.Add((name, node));
        }

#pragma warning disable S1541 // Methods and properties should not be too complex
        public bool Equals(IConflictTreeNode other)
#pragma warning restore S1541 // Methods and properties should not be too complex
        {
            if (other == null)
            {
                IsInConflict = true;
                return false;
            }
            var equals = true;
            equals &= other.UniqueId == UniqueId;
            equals &= other.Activity.Equals(Activity);
            equals &= (other.Children != null || Children == null);
            equals &= (other.Children == null || Children != null);
            equals &= (Children == null || Children.SequenceEqual(other.Children ?? new List<(string uniqueId, IConflictTreeNode node)>()));
            IsInConflict = !equals;
            other.IsInConflict = !equals;
            return equals;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }
            if (ReferenceEquals(this, obj))
            {
                return true;
            }
            if (obj.GetType() != GetType())
            {
                return false;
            }
            return Equals((ConflictTreeNode)obj);
        }

        public override int GetHashCode()
        {
            var hashCode = (397) ^ UniqueId.GetHashCode();
            hashCode = (hashCode * 397) ^ (Children != null ? Children.GetHashCode() : 0);
            hashCode = (hashCode * 397) ^ (Activity != null ? Activity.GetHashCode() : 0);
            return hashCode;
        }

        public List<(string uniqueId, IConflictTreeNode node)> Children { get; private set; }
        public string UniqueId { get; set; }
        public IDev2Activity Activity { get; }
        public Point Location { get; }
        public bool IsInConflict { get; set; }
    }
}
