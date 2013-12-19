using Dev2.Common.ExtMethods;

namespace Dev2.Data.Decisions.Operations
{
    public class DecisionUtils
    {
        public static bool IsNumericComparison(string[] cols, out decimal[] tryGetNumber)
        {
            tryGetNumber = new decimal[2];
            var isString = false;
            for(var i = 0; i < 2; i++)
            {
                if (!cols[i].IsNumeric(out tryGetNumber[i]))
                {
                    isString = true;
                }
            }
            return isString;
        }
    }
}