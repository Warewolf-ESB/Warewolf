using Dev2.Common;

namespace Dev2.Common
{
    public class ConflictTree : IConflictTree
    {

        public ConflictTree(IConflictTreeNode startNode)
        {
            Start = startNode;
        }

        public IConflictTreeNode Start { get; }

        public bool Equals(IConflictTree other)
        {
            switch (other)
            {
                case null when Start == null:
                    {
                        return true;
                    }
                case null when Start != null:
                    {
                        return false;
                    }
                default:
                    {
                        return Start.Equals(other.Start);
                    }
            }
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
            return Equals((IConflictTree)obj);
        }

        public override int GetHashCode()
        {
            return (Start != null ? Start.GetHashCode() : 0);
        }
    }
}
