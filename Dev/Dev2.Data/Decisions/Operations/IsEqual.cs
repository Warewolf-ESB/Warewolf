using System;

namespace Dev2.Data.Decisions.Operations
{
    public class IsEqual : IDecisionOperation
    {
        public bool Invoke(string[] cols)
        {
            if(!string.IsNullOrEmpty(cols[0]))
            {
                decimal[] tryGetNumber;
                var isString = DecisionUtils.IsNumericComparison(cols, out tryGetNumber);

                //either int compare
                if(!isString)
                {
                    return (tryGetNumber[0].CompareTo(tryGetNumber[1]) == 0);
                }

                //or string compare
                return (String.Compare(cols[0], cols[1], StringComparison.Ordinal) == 0);
            }

            // make it the same comparison
            return (String.Compare(cols[0], cols[1], StringComparison.Ordinal) == 0);
        }

        public Enum HandlesType()
        {
            return enDecisionType.IsEqual;
        }
    }
}
