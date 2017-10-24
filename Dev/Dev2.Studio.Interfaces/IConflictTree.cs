using System;

namespace Dev2.Studio.Interfaces
{
    public interface IConflictTree : IEquatable<IConflictTree>
    {
        IConflictTreeNode Start { get; }
    }
}