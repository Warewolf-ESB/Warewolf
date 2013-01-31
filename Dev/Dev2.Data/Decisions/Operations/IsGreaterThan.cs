using System;
using System.IO;

namespace Dev2.Data.Decisions.Operations
{
    public class IsGreaterThan : IDecisionOperation
    {
        public Enum HandlesType()
        {
            return enDecisionType.IsGreaterThan;
        }

        public bool Invoke(string[] cols)
        {
            //if (cols.Length < 2 || cols.Length > 2)
            //{
            //    throw new InvalidDataException("Wrong number of columns sent");
            //}

            if(!string.IsNullOrEmpty(cols[0]))
            {
                return (String.Compare(cols[0], cols[1], StringComparison.Ordinal) > 0);
            }

            return false;
        }
    }
}
