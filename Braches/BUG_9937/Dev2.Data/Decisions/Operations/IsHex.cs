using System;
using Dev2.Common.ExtMethods;

namespace Dev2.Data.Decisions.Operations
{
    /// <summary>
    /// Is Hex Decision
    /// </summary>
    public class IsHex : IDecisionOperation
    {
        public bool Invoke(string[] cols)
        {
            if (!string.IsNullOrEmpty(cols[0]))
            {
                return (cols[0].IsHex());
            }

            return false;
        }

        public Enum HandlesType()
        {
            return enDecisionType.IsHex;
        }
    }
}
