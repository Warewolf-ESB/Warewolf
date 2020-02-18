/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using Warewolf.Options;

namespace Warewolf.Data.Decisions.Operations
{
    public class IsGreaterThanOrEqual : IDecisionOperation
    {
        public Enum HandlesType() => enDecisionType.IsGreaterThanOrEqual;

        public bool Invoke(string[] cols)
        {
            if(!string.IsNullOrEmpty(cols[0]))
            {
                var isString = DecisionUtils.IsNumericComparison(cols, out decimal[] tryGetNumber);

                //either int compare
                if (!isString)
                {
                    return tryGetNumber[0].CompareTo(tryGetNumber[1]) >= 0;
                }

                //or string compare
                return String.Compare(cols[0], cols[1], StringComparison.Ordinal) >= 0;
            }

            return false;
        }
    }
}
