using System;

namespace Dev2.Data.Decisions.Operations
{
    public class IsBinary : IDecisionOperation
    {
        public bool Invoke(string[] cols)
        {
            throw new NotImplementedException();
        }

        public Enum HandlesType()
        {
            return enDecisionType.IsBinary;
        }
    }
}
