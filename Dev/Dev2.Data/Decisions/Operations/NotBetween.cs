/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.IO;
using Dev2.Common;
using Warewolf.Resource.Errors;

namespace Dev2.Data.Decisions.Operations
{
    public class NotBetween : IDecisionOperation
    {
        public Enum HandlesType()
        {
            return enDecisionType.NotBetween;
        }

        public bool Invoke(string[] cols)
        {
            double[] dVal = new double[3];
            DateTime[] dtVal = new DateTime[3];

            int pos = 0;
            bool anyDoubles = false;

            foreach (string c in cols)
            {
                bool isDouble = double.TryParse(c, out dVal[pos]);
                anyDoubles = anyDoubles || isDouble;
                if (!anyDoubles)
                {
                    try
                    {
                        if (DateTime.TryParse(c, out DateTime dt))
                        {
                            dtVal[pos] = dt;
                        }
                    }
                    catch(Exception ex)
                    {
                        Dev2Logger.Error(ex, GlobalConstants.WarewolfError);
                    }
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
                throw new InvalidDataException(ErrorResource.IsBetweenDataTypeMismatch);
            }

            return !(left > 0 && right < 0);
        }
    }
}
