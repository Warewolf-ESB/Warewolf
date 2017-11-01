using System.Windows;
using System;
using System.Collections.Generic;

namespace Dev2.Common
{
    public interface IConflictTreeNode : IEquatable<IConflictTreeNode>
    {
        IDev2Activity Activity { get; }
        List<(string uniqueId, IConflictTreeNode node)> Children { get; }
        bool IsInConflict { get; set; }
        Point Location { get; }
        string UniqueId { get; set; }

        void AddChild(IConflictTreeNode node,string name);
    }
}