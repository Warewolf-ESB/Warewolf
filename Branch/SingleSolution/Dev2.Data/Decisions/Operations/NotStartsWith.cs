using System;

namespace Dev2.Data.Decisions.Operations
{
    public class NotStartsWith : IDecisionOperation
    {
        public Enum HandlesType()
        {
            return enDecisionType.DoesntStartWith;
        }

        public bool Invoke(string[] cols)
        {
            return (!cols[0].StartsWith(cols[1]));
        }
    }
}
