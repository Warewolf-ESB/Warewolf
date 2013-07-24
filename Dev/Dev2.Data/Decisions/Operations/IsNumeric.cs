using System;
using Dev2.Common.ExtMethods;

namespace Dev2.Data.Decisions.Operations
{
    /// <summary>
    /// The is numeric decision function
    /// </summary>
    public class IsNumeric : IDecisionOperation
    {
        public bool Invoke(string[] cols)
        {
            return cols[0].IsNumeric();
        }

        public Enum HandlesType()
        {
            return enDecisionType.IsNumeric;
        }

    }
}
