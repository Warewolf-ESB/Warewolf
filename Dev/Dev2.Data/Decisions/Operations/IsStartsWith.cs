using System;

namespace Dev2.Data.Decisions.Operations
{
    public class IsStartsWith : IDecisionOperation
    {
        public Enum HandlesType()
        {
            return enDecisionType.IsStartsWith;
        }

        public bool Invoke(string[] cols)
        {
            return (cols[0].StartsWith(cols[1]));
        }
    }
}
