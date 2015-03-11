/*
*  Warewolf - The Easy Service Bus
*  Copyright 2015 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Globalization;
using Dev2.Common.Interfaces.Core.Convertors.DateAndTime;

namespace Dev2.Converters.DateAndTime
{
    public class DateTimeComparer : IDateTimeComparer
    {
        #region Class Members

        private static readonly Dictionary<string, Func<DateTime, DateTime, double>> OutputFormats =
            new Dictionary<string, Func<DateTime, DateTime, double>>();

        private DateTime _input1;
        private DateTime _input2;

        #endregion Class Members

        #region Properties

        public static List<string> OutputFormatTypes { get; private set; }

        #endregion Properties

        #region Ctor

        static DateTimeComparer()
        {
            CreateOutputFormatTypes();
        }

        #endregion Ctor

        #region Methods

        public bool TryCompare(IDateTimeDiffTO dateTimeDiffTo, out string result, out string error)
        {
            //local variable declarations

            result = "";
            //Creation of parser to get the DateTime Objects
            IDateTimeParser dateTimeParser = DateTimeConverterFactory.CreateParser();
            IDateTimeResultTO tmpRes;

            //try create the first DateTime object
            bool noErrorOccured = dateTimeParser.TryParseDateTime(dateTimeDiffTo.Input1, dateTimeDiffTo.InputFormat,
                out tmpRes, out error);
            if (noErrorOccured)
            {
                //Set the first DateTime object
                _input1 = tmpRes.ToDateTime();
                //try create the second DateTime object
                noErrorOccured = dateTimeParser.TryParseDateTime(dateTimeDiffTo.Input2, dateTimeDiffTo.InputFormat,
                    out tmpRes, out error);
            }

            if (noErrorOccured)
            {
                //Set the first DateTime object
                _input2 = tmpRes.ToDateTime();

                //Try get the function according to what the OutputType is
                Func<DateTime, DateTime, double> returnedFunc;
                noErrorOccured = OutputFormats.TryGetValue(dateTimeDiffTo.OutputType, out returnedFunc);

                if (returnedFunc != null)
                {
                    //Invoke the function the return the difference
                    double tmpAmount = returnedFunc.Invoke(_input1, _input2);
                    //Splits the double that is returned into a whole number and to a string
                    long wholeValue = Convert.ToInt64(Math.Floor(tmpAmount));
                    result = wholeValue.ToString(CultureInfo.InvariantCulture);
                }
            }
            return noErrorOccured;
        }

        #endregion Methods

        #region Private Methods

        /// <summary>
        ///     Creates a list of all valid Output Formats
        /// </summary>
        private static void CreateOutputFormatTypes()
        {
            OutputFormats.Add("Years", ReturnYears);
            OutputFormats.Add("Months", ReturnMonths);
            OutputFormats.Add("Days", ReturnDays);
            OutputFormats.Add("Weeks", ReturnWeeks);
            OutputFormats.Add("Hours", ReturnHours);
            OutputFormats.Add("Minutes", ReturnMinutes);
            OutputFormats.Add("Seconds", ReturnSeconds);
            OutputFormats.Add("Split Secs", ReturnSplitSeconds);
            OutputFormatTypes = new List<string>(OutputFormats.Keys);
        }

        #endregion Private Methods

        #region OutputFormat Methods

        /// <summary>
        ///     Returns the difference in years between two DateTime object
        /// </summary>
        /// <param name="input1"></param>
        /// <param name="input2"></param>
        /// <returns></returns>
        private static double ReturnYears(DateTime input1, DateTime input2)
        {
            int result = 0;
            if (input2.Year != input1.Year)
            {
                result = input2.Year - input1.Year;
                input1 = input1.AddYears(result);
                if (input2 < input1)
                {
                    if (result < 0)
                    {
                        result++;
                    }
                    else if (result > 0)
                    {
                        result--;
                    }
                }
            }
            return result;
        }

        /// <summary>
        ///     Returns the difference in months between two DateTime object
        /// </summary>
        /// <param name="input1"></param>
        /// <param name="input2"></param>
        /// <returns></returns>
        private static double ReturnMonths(DateTime input1, DateTime input2)
        {
            int tmpYears = input2.Year - input1.Year;

            int result = input2.Month - input1.Month;
            input1 = input1.AddMonths(result);
            input1 = input1.AddYears(tmpYears);
            if (input2 < input1)
            {
                if (result < 0)
                {
                    result++;
                }
                else if (result > 0)
                {
                    result--;
                }
            }

            result = (result) + (12*(tmpYears));
            return result;
        }

        /// <summary>
        ///     Returns the difference in days between two DateTime object
        /// </summary>
        /// <param name="input1"></param>
        /// <param name="input2"></param>
        /// <returns></returns>
        private static double ReturnDays(DateTime input1, DateTime input2)
        {
            TimeSpan timeDiff = input2 - input1;
            double result = timeDiff.TotalDays;
            return result;
        }

        /// <summary>
        ///     Returns the difference in weeks between two DateTime object
        /// </summary>
        /// <param name="input1"></param>
        /// <param name="input2"></param>
        /// <returns></returns>
        private static double ReturnWeeks(DateTime input1, DateTime input2)
        {
            TimeSpan timeDiff = input2 - input1;
            double result = (timeDiff.TotalDays/7);
            return result;
        }

        /// <summary>
        ///     Returns the difference in hours between two DateTime object
        /// </summary>
        /// <param name="input1"></param>
        /// <param name="input2"></param>
        /// <returns></returns>
        private static double ReturnHours(DateTime input1, DateTime input2)
        {
            TimeSpan timeDiff = input2 - input1;
            double result = timeDiff.TotalHours;
            return result;
        }

        /// <summary>
        ///     Returns the difference in minutes between two DateTime object
        /// </summary>
        /// <param name="input1"></param>
        /// <param name="input2"></param>
        /// <returns></returns>
        private static double ReturnMinutes(DateTime input1, DateTime input2)
        {
            TimeSpan timeDiff = input2 - input1;
            double result = timeDiff.TotalMinutes;
            return result;
        }

        /// <summary>
        ///     Returns the difference in seconds between two DateTime object
        /// </summary>
        /// <param name="input1"></param>
        /// <param name="input2"></param>
        /// <returns></returns>
        private static double ReturnSeconds(DateTime input1, DateTime input2)
        {
            TimeSpan timeDiff = input2 - input1;
            double result = timeDiff.TotalSeconds;
            return result;
        }

        /// <summary>
        ///     Returns the difference in split seconds between two DateTime object
        /// </summary>
        /// <param name="input1"></param>
        /// <param name="input2"></param>
        /// <returns></returns>
        private static double ReturnSplitSeconds(DateTime input1, DateTime input2)
        {
            TimeSpan timeDiff = input2 - input1;
            double result = timeDiff.TotalMilliseconds;
            return result;
        }

        #endregion OutputFormat Methods
    }
}