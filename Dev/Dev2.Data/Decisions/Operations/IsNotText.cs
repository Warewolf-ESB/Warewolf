using System;
using Dev2.Common.ExtMethods;

namespace Dev2.Data.Decisions.Operations
{
    public class IsNotText : IDecisionOperation
    {
        public bool Invoke(string[] cols)
        {
            if(!string.IsNullOrEmpty(cols[0]))
            {
                return (!cols[0].IsAlpha());
            }

            return true; // blank is not Text
        }

        public Enum HandlesType()
        {
            return enDecisionType.IsNotText;
        }
    }
}
