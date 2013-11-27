using System;
using System.IO;

namespace Dev2.Data.Decisions.Operations
{
    public class IsContains : IDecisionOperation
    {
        public Enum HandlesType()
        {
            return enDecisionType.IsContains;
        }

        public bool Invoke(string[] cols)
        {

            return (cols[0].Contains(cols[1]));
        }
    }
}
