using System;

namespace Dev2.Data.Decisions.Operations
{
    public class IsNotEqual : IDecisionOperation
    {
        public Enum HandlesType()
        {
            return enDecisionType.IsNotEqual;
        }

        public bool Invoke(string[] cols)
        {
            return !(cols[0].Equals(cols[1], StringComparison.InvariantCulture));
        }
    }
}
