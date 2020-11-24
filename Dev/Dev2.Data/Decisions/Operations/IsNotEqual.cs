/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;

namespace Dev2.Data.Decisions.Operations
{
    public class IsNotEqual : IDecisionOperation
    {
        public Enum HandlesType() => enDecisionType.IsNotEqual;

        public bool Invoke(string[] cols)
        {
            if(!string.IsNullOrEmpty(cols[0]))
            {
                var isString = DecisionUtils.IsNumericComparison(cols, out var tryGetNumber);

                //either int compare
                if (!isString)
                {
                    return tryGetNumber[0].CompareTo(tryGetNumber[1]) != 0;
                }

                //or string compare
            }

            // make it the same comparison
            return string.Compare(cols[0], cols[1], StringComparison.Ordinal) != 0;
        }
    }
}
