using System.Windows;
using Dev2;
using System;
using System.Collections.Generic;

namespace Dev2.Common
{
    public interface IConflictTreeNode : IEquatable<IConflictTreeNode>
    {
        IDev2Activity Activity { get; }
        List<(string uniqueId, IConflictTreeNode node)> Children { get; }
        List<IConflictTreeNode> NextNodes { get; }
        bool IsInConflict { get; set; }
        Point Location { get; }
        List<(string name, string uniqueId, IConflictTreeNode node)> Parents { get; }
        string UniqueId { get; set; }

        void AddChild(IConflictTreeNode node);
        void AddParent(IConflictTreeNode node, string name);
        void AddNext(IConflictTreeNode conflictTreeNode);
    }
}