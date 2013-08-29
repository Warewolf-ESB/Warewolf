using System;
using System.IO;

namespace Dev2.Data.Decisions.Operations
{
    public class IsNotDate : IDecisionOperation
    {
        public Enum HandlesType()
        {
            return enDecisionType.IsNotDate;
        }

        public bool Invoke(string[] cols)
        {
            DateTime date = DateTime.MinValue;

            return !(DateTime.TryParse(cols[0], out date));
        }
    }
}
