using System;
using Dev2.Common.ExtMethods;

namespace Dev2.Data.Decisions.Operations
{
    public class IsAlphanumeric : IDecisionOperation
    {

        public bool Invoke(string[] cols)
        {
            if(!string.IsNullOrEmpty(cols[0]))
            {
                return cols[0].IsAlphaNumeric();
            }

            return false;
        }

        public Enum HandlesType()
        {
            return enDecisionType.IsAlphanumeric;
        }
    }
}
