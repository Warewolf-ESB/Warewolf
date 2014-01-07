using System;

namespace Dev2.Data.Decisions.Operations
{
    public class IsError : IDecisionOperation
    {
        public bool Invoke(string[] cols)
        {
            return (cols[0].Length > 0);
        }

        public Enum HandlesType()
        {
            return enDecisionType.IsError;
        }
    }
}
