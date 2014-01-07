using System;
using Dev2.Common.ExtMethods;

namespace Dev2.Data.Decisions.Operations
{
    public class IsNotEmail : IDecisionOperation
    {
        public bool Invoke(string[] cols)
        {
            return !(cols[0].IsEmail());
        }

        public Enum HandlesType()
        {
            return enDecisionType.IsNotEmail;
        }
    }
}
