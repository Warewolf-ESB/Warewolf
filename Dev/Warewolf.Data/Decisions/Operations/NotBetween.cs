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
using System.IO;
using Warewolf.Options;

namespace Warewolf.Data.Decisions.Operations
{
    public class NotBetween : IDecisionOperation
    {
        public Enum HandlesType() => enDecisionType.NotBetween;

        public bool Invoke(string[] cols)
        {
            var dVal = new double[3];
            var dtVal = new DateTime[3];

            var pos = 0;
            var anyDoubles = false;

            foreach (string c in cols)
            {
                var isDouble = double.TryParse(c, out dVal[pos]);
                anyDoubles = anyDoubles || isDouble;
                if (!anyDoubles)
                {
                    TryParseColum(dtVal, pos, c);
                }

                pos++;
            }

            double left;
            double right;

            if(anyDoubles)
            {
                left = dVal[0] - dVal[1];
                right = dVal[0] - dVal[2];
            }
            else if(dtVal.Length == 3)
            {
                left = dtVal[0].Ticks - dtVal[1].Ticks;
                right = dtVal[0].Ticks - dtVal[2].Ticks;
            }
            else
            {
                throw new InvalidDataException("IsBetween Numeric and DateTime mis-match");
            }

            return !(left > 0 && right < 0);
        }

        private static void TryParseColum(DateTime[] dtVal, int pos, string c)
        {
            if (DateTime.TryParse(c, out DateTime dt))
            {
                dtVal[pos] = dt;
            }
        }
    }
}
