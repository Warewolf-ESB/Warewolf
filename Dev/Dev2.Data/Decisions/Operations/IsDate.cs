using System;

namespace Dev2.Data.Decisions.Operations
{
    public class IsDate : IDecisionOperation
    {
        public Enum HandlesType()
        {
            return enDecisionType.IsDate;
        }

        public bool Invoke(string[] cols)
        {
            DateTime date;

            return DateTime.TryParse(cols[0], out date);
        }
    }
}
