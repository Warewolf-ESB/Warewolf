using System;
using Dev2.Common.ExtMethods;

namespace Dev2.Data.Decisions.Operations
{
    class IsEmail : IDecisionOperation
    {
        public Enum HandlesType()
        {
            return enDecisionType.IsEmail;
        }

        public bool Invoke(string[] cols)
        {
            return (cols[0].IsEmail());
        }
    }
}
