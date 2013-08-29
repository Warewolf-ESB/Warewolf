using System;

namespace Dev2.Data.Decisions.Operations
{
    public class IsEndsWith : IDecisionOperation
    {
        public Enum HandlesType()
        {
            return enDecisionType.IsEndsWith;
        }

        public bool Invoke(string[] cols)
        {
            return (cols[0].EndsWith(cols[1]));
        }
    }
}
