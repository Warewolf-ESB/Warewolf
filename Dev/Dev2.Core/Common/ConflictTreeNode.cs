/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Collections.Generic;
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
        }

        public void AddChild(IConflictTreeNode node,string name)
        {
            if (Children == null)
            {
                Children = new List<(string name, IConflictTreeNode node)>();
            }
            Children.Add((name, node));
        }

        public bool Equals(IConflictTreeNode other)
        {
            if (other == null)
            {
                return false;
            }
            var equals = true;
            equals &= other.UniqueId == UniqueId;
            equals &= other.Activity.Equals(Activity);

            equals &= ChildrenEquals(other);

            return equals;
        }

        private bool ChildrenEquals(IConflictTreeNode other)
        {
            if (Children == null && other.Children == null)
            {
                return true;
            }
            if ((Children == null && other.Children != null) || (Children != null && other.Children == null))
            {
                return false;
            }
            if (Children.Count != other.Children.Count)
            {
                return false;
            }

            var equals = true;
            for (int i = 0; i < Children.Count; i++)
            {
                equals &= Equals(Children[i].node, other.Children[i].node);
                equals &= Equals(Children[i].uniqueId, other.Children[i].uniqueId);

                if (!equals)
                {
                    break;
                }
            }

            return equals;
        }

        public override bool Equals(object obj)
        {
            if (obj is ConflictTreeNode conflictTreeNode)
            {
                return Equals(conflictTreeNode);
            }
            return false;
        }

        public override int GetHashCode()
        {
            var hashCode = (397) ^ UniqueId.GetHashCode();
            hashCode = (hashCode * 397) ^ (Children != null ? Children.GetHashCode() : 0);
            hashCode = (hashCode * 397) ^ Activity.GetHashCode();
            return hashCode;
        }

        public List<(string uniqueId, IConflictTreeNode node)> Children { get; private set; }
        public string UniqueId { get; set; }
        public IDev2Activity Activity { get; }
        public Point Location { get; }
        public bool IsInConflict { get; set; }
    }
}
