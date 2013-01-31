using System;
using System.IO;
using Dev2.Common.ExtMethods;

namespace Dev2.Data.Decisions.Operations
{
    public class IsNotAlphanumeric : IDecisionOperation
    {
        public bool Invoke(string[] cols)
        {
            //if (cols.Length < 1 || cols.Length > 1)
            //{
            //    throw new InvalidDataException("Wrong number of columns sent");
            //}

            if (!string.IsNullOrEmpty(cols[0]))
            {
                return !(cols[0].IsAlphaNumeric());
            }

            return false;
        }

        public Enum HandlesType()
        {
            return enDecisionType.IsNotAlphanumeric;
        }
    }
}
