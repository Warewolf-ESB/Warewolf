using System;
using System.IO;

namespace Dev2.Data.Decisions.Operations
{
    public class IsStartsWith : IDecisionOperation
    {
        public Enum HandlesType()
        {
            return enDecisionType.IsStartsWith;
        }

        public bool Invoke(string[] cols)
        {
            //if (cols.Length < 2 || cols.Length > 2)
            //{
            //    throw new InvalidDataException("Wrong number of columns sent");
            //}

            return (cols[0].StartsWith(cols[1]));
        }
    }
}
