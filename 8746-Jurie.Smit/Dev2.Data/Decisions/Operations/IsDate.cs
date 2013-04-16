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
            DateTime date = DateTime.MinValue;

            //if (cols.Length < 1 || cols.Length > 1)
            //{
            //    throw new InvalidDataException("Wrong number of columns sent");
            //}

            return DateTime.TryParse(cols[0], out date);
        }
    }
}
