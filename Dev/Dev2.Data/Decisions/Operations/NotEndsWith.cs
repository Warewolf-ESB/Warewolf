using System;

namespace Dev2.Data.Decisions.Operations
{
    public class NotEndsWith : IDecisionOperation
    {
        public Enum HandlesType()
        {
            return enDecisionType.NotEndsWith;
        }

        public bool Invoke(string[] cols)
        {
            return (!cols[0].EndsWith(cols[1]));
        }
    }
}
