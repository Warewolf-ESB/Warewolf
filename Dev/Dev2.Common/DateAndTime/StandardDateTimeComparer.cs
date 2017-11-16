/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common.Interfaces.Core.Convertors.DateAndTime;
using System;
using System.Globalization;

namespace Dev2.Common.DateAndTime
{
    public class StandardDateTimeComparer : DateTimeComparer
    {
        public override bool TryCompare(IDateTimeDiffTO dateTimeDiffTo, out string result, out string error)
        {
            result = "";
            error = "";
            IDateTimeParser dateTimeParser = DateTimeConverterFactory.CreateStandardParser();
            var parsedCorreclty = DateTime.TryParseExact(dateTimeDiffTo.Input1, dateTimeDiffTo.InputFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedDate);
            if (parsedCorreclty)
            {
                _input1 = parsedDate;
                parsedCorreclty = DateTime.TryParseExact(dateTimeDiffTo.Input2, dateTimeDiffTo.InputFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedDate1);


                if (parsedCorreclty)
                {
                    _input2 = parsedDate1;
                    parsedCorreclty = OutputFormats.TryGetValue(dateTimeDiffTo.OutputType, out Func<DateTime, DateTime, double> returnedFunc);

                    if (returnedFunc != null)
                    {
                        double tmpAmount = returnedFunc.Invoke(_input1, _input2);
                        long wholeValue = Convert.ToInt64(Math.Floor(tmpAmount));
                        result = wholeValue.ToString(CultureInfo.InvariantCulture);
                    }
                }

                return parsedCorreclty;
            }
            error = Warewolf.Resource.Errors.ErrorResource.CannorParseInputDateTimeWithGivenFormat;
            return parsedCorreclty;
        }
    }
}