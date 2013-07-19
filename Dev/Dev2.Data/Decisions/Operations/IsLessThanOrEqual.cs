using System;
using System.IO;

namespace Dev2.Data.Decisions.Operations
{
    public class IsLessThanOrEqual : IDecisionOperation
    {
        public Enum HandlesType()
        {
            return enDecisionType.IsLessThanOrEqual;
        }

        public bool Invoke(string[] cols)
        {
            //if (cols.Length < 2 || cols.Length > 2)
            //{
            //    throw new InvalidDataException("Wrong number of columns sent");
            //}

            if(!string.IsNullOrEmpty(cols[0]))
            {
                int[] tryGetNumber;
                var isString = DecisionUtils.IsNumericComparison(cols, out tryGetNumber);

                //either int compare
                if (!isString)
                {
                    return (tryGetNumber[0].CompareTo(tryGetNumber[1]) <= 0);
                }

                //or string compare
                return (String.Compare(cols[0], cols[1], StringComparison.Ordinal) <= 0);
            }

            return false;
        }
    }
}
