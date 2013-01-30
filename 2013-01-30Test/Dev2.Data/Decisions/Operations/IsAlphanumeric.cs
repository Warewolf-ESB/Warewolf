using System;
using Dev2.Common.ExtMethods;

namespace Dev2.Data.Decisions.Operations
{
    public class IsAlphanumeric : IDecisionOperation
    {

        public bool Invoke(string[] cols)
        {
            //if (cols.Length < 1 || cols.Length > 1)
            //{
            //    throw new InvalidDataException("Wrong number of columns sent");
            //}

            return cols[0].IsAlphaNumeric();
        }

        public Enum HandlesType()
        {
            return enDecisionType.IsAlphanumeric;
        }
    }
}
