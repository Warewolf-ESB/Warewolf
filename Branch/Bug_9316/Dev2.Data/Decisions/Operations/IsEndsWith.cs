using System;

namespace Dev2.Data.Decisions.Operations
{
    public class IsEndsWith : IDecisionOperation
    {
        public Enum HandlesType()
        {
            return enDecisionType.IsEndsWith;
        }

        public bool Invoke(string[] cols)
        {
            //if (cols.Length < 2 || cols.Length > 2)
            //{
            //    throw new InvalidDataException("Wrong number of columns sent");
            //}

            return (cols[0].EndsWith(cols[1]));
        }
    }
}
