namespace Dev2.Data.Decisions.Operations
{
    public class DecisionUtils
    {
        public static bool IsNumericComparison(string[] cols, out int[] tryGetNumber)
        {
            tryGetNumber = new int[2];
            var isString = false;
            for(var i = 0; i < 2; i++)
            {
                if(!int.TryParse(cols[i], out tryGetNumber[i]))
                {
                    isString = true;
                }
            }
            return isString;
        }
    }
}