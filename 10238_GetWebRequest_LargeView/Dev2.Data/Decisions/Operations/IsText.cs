using System;
using System.IO;
using Dev2.Common.ExtMethods;

namespace Dev2.Data.Decisions.Operations
{
    public class IsText : IDecisionOperation
    {
        public bool Invoke(string[] cols)
        {
            if(!string.IsNullOrEmpty(cols[0]))
            {
                return (cols[0].IsAlpha());
            }

            return false; // blank is not Text

        }

        public Enum HandlesType()
        {
            return enDecisionType.IsText;
        }
    }
}
