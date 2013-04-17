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

            //if (cols.Length < 1 || cols.Length > 1)
            //{
            //    throw new InvalidDataException("Wrong number of columns sent");
            //}

            return !(DateTime.TryParse(cols[0], out date));
        }
    }
}
