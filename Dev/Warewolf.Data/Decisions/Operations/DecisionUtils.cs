/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

namespace Warewolf.Data
{
    public static class DecisionUtils
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
