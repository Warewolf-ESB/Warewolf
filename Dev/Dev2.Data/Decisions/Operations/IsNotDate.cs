using System;

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
            DateTime date;

            return !(DateTime.TryParse(cols[0], out date));
        }
    }
}
