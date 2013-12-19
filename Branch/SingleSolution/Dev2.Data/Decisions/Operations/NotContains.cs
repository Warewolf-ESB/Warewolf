using System;

namespace Dev2.Data.Decisions.Operations
{
    public class NotContains : IDecisionOperation
    {
        public Enum HandlesType()
        {
            return enDecisionType.NotContains;
        }

        public bool Invoke(string[] cols)
        {
            return (!cols[0].Contains(cols[1]));
        }
    }
}
