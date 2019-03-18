#pragma warning disable CC0091, S1226, S100, CC0044, CC0045, CC0021, S1449, S1541, S1067, S3235, CC0015, S107, S2292, S1450, S105, CC0074, S1135, S101, S3776, CS0168, S2339, CC0031, S3240, CC0020, CS0108, S1694, S1481, CC0008, AD0001, S2328, S2696, S1643, CS0659, CS0067, S104, CC0030, CA2202, S3376, S1185, CS0219, S3253, S1066, CC0075, S3459, S1871, S1125, CS0649, S2737, S1858, CC0082, CC0001, S3241, S2223, S1301, CC0013, S2955, S1944, CS4014, S3052, S2674, S2344, S1939, S1210, CC0033, CC0002, S3458, S3254, S3220, S2197, S1905, S1699, S1659, S1155, CS0105, CC0019, S3626, S3604, S3440, S3256, S2692, S2345, S1109, FS0058, CS1998, CS0661, CS0660, CS0162, CC0089, CC0032, CC0011, CA1001
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common.Interfaces.Core.Convertors.DateAndTime;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Dev2.Common.DateAndTime
{
    public class DateTimeComparer : IDateTimeComparer
    {
        protected static readonly Dictionary<string, Func<DateTime, DateTime, double>> OutputFormats =
            new Dictionary<string, Func<DateTime, DateTime, double>>();

        protected DateTime _input1;
        protected DateTime _input2;

        public static List<string> OutputFormatTypes { get; private set; }

        static DateTimeComparer()
        {
            CreateOutputFormatTypes();
        }

        public virtual bool TryCompare(IDateTimeDiffTO dateTimeDiffTo, out string result, out string error)
        {
            //local variable declarations
            result = "";
            //Creation of parser to get the DateTime Objects
            var dateTimeParser = DateTimeConverterFactory.CreateParser();

            //try create the first DateTime object
            var noErrorOccured = dateTimeParser.TryParseDateTime(dateTimeDiffTo.Input1, dateTimeDiffTo.InputFormat, out IDateTimeResultTO tmpRes, out error);
            if (noErrorOccured)
            {
                //Set the first DateTime object
                _input1 = tmpRes.ToDateTime();
                //try create the second DateTime object
                noErrorOccured = dateTimeParser.TryParseDateTime(dateTimeDiffTo.Input2, dateTimeDiffTo.InputFormat, out tmpRes, out error);
            }

            if (noErrorOccured)
            {
                //Set the first DateTime object
                _input2 = tmpRes.ToDateTime();

                //Try get the function according to what the OutputType is
                noErrorOccured = OutputFormats.TryGetValue(dateTimeDiffTo.OutputType, out Func<DateTime, DateTime, double> returnedFunc);

                if (returnedFunc != null)
                {
                    //Invoke the function the return the difference
                    var tmpAmount = returnedFunc.Invoke(_input1, _input2);
                    //Splits the double that is returned into a whole number and to a string
                    var wholeValue = Convert.ToInt64(Math.Floor(tmpAmount));
                    result = wholeValue.ToString(CultureInfo.InvariantCulture);
                }
            }
            return noErrorOccured;
        }

        private static void CreateOutputFormatTypes()
        {
            OutputFormats.Add("Years", ReturnYears);
            OutputFormats.Add("Months", ReturnMonths);
            OutputFormats.Add("Days", ReturnDays);
            OutputFormats.Add("Weeks", ReturnWeeks);
            OutputFormats.Add("Hours", ReturnHours);
            OutputFormats.Add("Minutes", ReturnMinutes);
            OutputFormats.Add("Seconds", ReturnSeconds);
            OutputFormats.Add("Milliseconds", ReturnMilliseconds);
            OutputFormatTypes = new List<string>(OutputFormats.Keys);
        }

        private static double ReturnYears(DateTime input1, DateTime input2)
        {
            var result = 0;
            if (input2.Year != input1.Year)
            {
                result = input2.Year - input1.Year;
                input1 = input1.AddYears(result);
                if (input2 < input1)
                {
                    result = Touch(result);
                }
            }
            return result;
        }

        private static int Touch(int result)
        {
            if (result < 0)
            {
                result++;
            }
            else
            {
                if (result > 0)
                {
                    result--;
                }
            }

            return result;
        }

        private static double ReturnMonths(DateTime input1, DateTime input2)
        {
            var tmpYears = input2.Year - input1.Year;

            var result = input2.Month - input1.Month;
            input1 = input1.AddMonths(result);
            input1 = input1.AddYears(tmpYears);
            if (input2 < input1)
            {
                if (result < 0)
                {
                    result++;
                }
                else
                {
                    if (result > 0)
                    {
                        result--;
                    }
                }
            }

            result = result + 12 * tmpYears;
            return result;
        }

        private static double ReturnDays(DateTime input1, DateTime input2)
        {
            var timeDiff = input2 - input1;
            var result = timeDiff.TotalDays;
            return result;
        }

        private static double ReturnWeeks(DateTime input1, DateTime input2)
        {
            var timeDiff = input2 - input1;
            var result = timeDiff.TotalDays / 7;
            return result;
        }

        private static double ReturnHours(DateTime input1, DateTime input2)
        {
            var timeDiff = input2 - input1;
            var result = timeDiff.TotalHours;
            return result;
        }

        private static double ReturnMinutes(DateTime input1, DateTime input2)
        {
            var timeDiff = input2 - input1;
            var result = timeDiff.TotalMinutes;
            return result;
        }

        private static double ReturnSeconds(DateTime input1, DateTime input2)
        {
            var timeDiff = input2 - input1;
            var result = timeDiff.TotalSeconds;
            return result;
        }

        private static double ReturnMilliseconds(DateTime input1, DateTime input2)
        {
            var timeDiff = input2 - input1;
            var result = timeDiff.TotalMilliseconds;
            return result;
        }
    }
}