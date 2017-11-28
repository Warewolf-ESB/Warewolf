/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

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
