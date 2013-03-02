using System;
using Dev2.Common.ExtMethods;

namespace Dev2.Data.Decisions.Operations
{
    /// <summary>
    /// Is Not Bse64 Decision
    /// </summary>
    public class IsNotBase64 : IDecisionOperation
    {
        public bool Invoke(string[] cols)
        {
            if (!string.IsNullOrEmpty(cols[0]))
            {
                return !(cols[0].IsBase64());
            }

            return false;
        }

        public Enum HandlesType()
        {
            return enDecisionType.IsNotBase64;
        }
    }
}
