using System;
using System.IO;
using Dev2.Common.ExtMethods;

namespace Dev2.Data.Decisions.Operations
{
    public class IsNotNumeric : IDecisionOperation
    {

        public bool Invoke(string[] cols)
        {
            return (!cols[0].IsNumeric());
        }

        public Enum HandlesType()
        {
            return enDecisionType.IsNotNumeric;
        }
    }
}
