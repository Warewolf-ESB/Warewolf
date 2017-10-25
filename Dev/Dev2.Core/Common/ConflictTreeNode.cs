using System;
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
        }

        public void AddChild(IConflictTreeNode node)
        {
            if (Children == null)
            {
                Children = new List<(string uniqueId, IConflictTreeNode node)>();
            }
            Children.Add((node.UniqueId, node));
        }

        public void AddParent(IConflictTreeNode node,string name)
        {
            if (Parents == null)
            {
                Parents = new List<(string name,string uniqueId, IConflictTreeNode node)>();
            }
            Parents.Add((name,node.UniqueId, node));
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
            hashCode = (hashCode * 397) ^ (Parents != null ? Parents.GetHashCode() : 0);
            hashCode = (hashCode * 397) ^ (Children != null ? Children.GetHashCode() : 0);
            hashCode = (hashCode * 397) ^ (Activity != null ? Activity.GetHashCode() : 0);
            return hashCode;
        }

        public List<(string name, string uniqueId, IConflictTreeNode node)> Parents { get; private set; }
        public List<(string uniqueId, IConflictTreeNode node)> Children { get; private set; }
        public List<IConflictTreeNode> NextNodes { get; private set; }
        public string UniqueId { get; set; }
        public IDev2Activity Activity { get; }
        public Point Location { get; }
        public bool IsInConflict { get; set; }

        public void AddNext(IConflictTreeNode conflictTreeNode)
        {
            if (NextNodes == null)
            {
                NextNodes = new List<IConflictTreeNode>();
            }
            NextNodes.Add(conflictTreeNode);
        }
    }
}
