using System;

namespace Dev2.Common
{
    public interface IConflictTree : IEquatable<IConflictTree>
    {
        IConflictTreeNode Start { get; }
    }
}