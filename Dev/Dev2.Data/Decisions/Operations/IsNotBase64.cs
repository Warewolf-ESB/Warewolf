using System;

namespace Dev2.Data.Decisions.Operations
{
    public class IsNotBase64 : IDecisionOperation
    {
        public bool Invoke(string[] cols)
        {
            throw new NotImplementedException();
        }

        public Enum HandlesType()
        {
            return enDecisionType.IsNotBase64;
        }
    }
}
