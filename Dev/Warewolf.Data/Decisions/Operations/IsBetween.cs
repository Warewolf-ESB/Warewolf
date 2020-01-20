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
    /// <summary>
    /// Is Between Operator
    /// </summary>
    public class IsBetween : IDecisionOperation
    {
        public Enum HandlesType() => enDecisionType.IsBetween;

        public bool Invoke(string[] cols)
        {
            var dVal = new double[3];
            var dtVal = new DateTime[3];

            var pos = 0;
            var isDateTimeCompare = false;
            foreach (string c in cols)
            {
                if(!double.TryParse(c, out dVal[pos]))
                {
                    isDateTimeCompare = TryParseAsDatetimeCompare(dtVal, pos, isDateTimeCompare, c);
                }
                pos++;
            }

            double left;
            double right;
            if (isDateTimeCompare)
            {
                left = dtVal[0].Ticks - dtVal[1].Ticks;
                right = dtVal[0].Ticks - dtVal[2].Ticks;
            }
            else
            {
                left = dVal[0] - dVal[1];
                right = dVal[0] - dVal[2];
            }
            return left >= 0 && right <= 0 || left <= 0 && right >= 0;
        }

        private static bool TryParseAsDatetimeCompare(DateTime[] dtVal, int pos, bool isDateTimeCompare, string c)
        {
            if (DateTime.TryParse(c, out DateTime dt))
            {
                dtVal[pos] = dt;
                return true;
            }
            return isDateTimeCompare;
        }
    }
}
