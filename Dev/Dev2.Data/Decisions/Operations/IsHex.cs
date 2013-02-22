using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dev2.Data.Decisions.Operations
{
    public class IsHex : IDecisionOperation
    {
        public bool Invoke(string[] cols)
        {
            throw new NotImplementedException();
        }

        public Enum HandlesType()
        {
            return enDecisionType.IsHex;
        }
    }
}
