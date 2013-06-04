using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dev2.Converters.DateAndTime.Interfaces;
using System.Globalization;
using Microsoft.VisualBasic;

namespace Dev2.Converters.DateAndTime
{
    public class DateTimeComparer : IDateTimeComparer
    {
        #region Class Members

        private static Dictionary<string, Func<DateTime, DateTime, double>> _outputFormats = new Dictionary<string, Func<DateTime, DateTime, double>>();
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

        public bool TryCompare(IDateTimeDiffTO dateTimeDiffTO, out string result, out string error)
        {
            //local variable declarations
            bool noErrorOccured = true;
            result = "";
            error = "";
            //Creation of praser to get the DateTime Objects
            IDateTimeParser dateTimeParser = DateTimeConverterFactory.CreateParser();
            IDateTimeResultTO tmpRes;
            Func<DateTime, DateTime, double> returnedFunc;

            //try create the first DateTime object
            noErrorOccured = dateTimeParser.TryParseDateTime(dateTimeDiffTO.Input1, dateTimeDiffTO.InputFormat, out tmpRes, out error);
            if (noErrorOccured)
            {
                //Set the first DateTime object
                _input1 = tmpRes.ToDateTime();
                //try create the second DateTime object
                noErrorOccured = dateTimeParser.TryParseDateTime(dateTimeDiffTO.Input2, dateTimeDiffTO.InputFormat, out tmpRes, out error);

            }

            if (noErrorOccured)
            {
                //Set the first DateTime object
                _input2 = tmpRes.ToDateTime(); 

                //Try get the function according to what the OutputType is
                noErrorOccured = _outputFormats.TryGetValue(dateTimeDiffTO.OutputType, out returnedFunc);

                if (returnedFunc != null && noErrorOccured)
                {
                    //Invoke the function the return the difference
                    double tmpAmount = returnedFunc.Invoke(_input1, _input2);
                    //Splits the double that is returned into a whole number and to a string
                    string[] splitArray = tmpAmount.ToString().Split('.');
                    if (splitArray[0] == "-0")
                    {
                        splitArray[0] = "0";
                    }
                    result = splitArray[0];
                }
            }
            return noErrorOccured;
        }

        #endregion Methods

        #region Private Methods

        /// <summary>
        /// Creates a list of all valid Output Formats
        /// </summary>
        private static void CreateOutputFormatTypes()
        {    
            _outputFormats.Add("Years", ReturnYears);
            _outputFormats.Add("Months", ReturnMonths);
            _outputFormats.Add("Days", ReturnDays);
            _outputFormats.Add("Weeks", ReturnWeeks);
            _outputFormats.Add("Hours", ReturnHours);
            _outputFormats.Add("Minutes", ReturnMinutes);
            _outputFormats.Add("Seconds", ReturnSeconds);
            _outputFormats.Add("Split Secs", ReturnSplitSeconds);
            OutputFormatTypes = new List<string>(_outputFormats.Keys);
        }

        #endregion Private Methods

        #region OutputFormat Methods

        /// <summary>
        /// Returns the difference in years between two DateTime object
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
            return (double)result;
        }

        /// <summary>
        /// Returns the difference in months between two DateTime object
        /// </summary>
        /// <param name="input1"></param>
        /// <param name="input2"></param>
        /// <returns></returns>
        private static double ReturnMonths(DateTime input1, DateTime input2)
        {
            int result = 0;
            int tmpYears = input2.Year - input1.Year;

            result = input2.Month - input1.Month;
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

            result = (result) + (12 * (tmpYears));
            return (double)result;
        }

        /// <summary>
        /// Returns the difference in days between two DateTime object
        /// </summary>
        /// <param name="input1"></param>
        /// <param name="input2"></param>
        /// <returns></returns>
        private static double ReturnDays(DateTime input1, DateTime input2)
        {
            TimeSpan _timeDiff = input2 - input1;
            double result = 0;
            result = _timeDiff.TotalDays;
            return result;
        }

        /// <summary>
        /// Returns the difference in weeks between two DateTime object
        /// </summary>
        /// <param name="input1"></param>
        /// <param name="input2"></param>
        /// <returns></returns>
        private static double ReturnWeeks(DateTime input1, DateTime input2)
        {
            TimeSpan _timeDiff = input2 - input1;
            double result = 0;
            result = (_timeDiff.TotalDays / 7);
            return result;
        }

        /// <summary>
        /// Returns the difference in hours between two DateTime object
        /// </summary>
        /// <param name="input1"></param>
        /// <param name="input2"></param>
        /// <returns></returns>
        private static double ReturnHours(DateTime input1, DateTime input2)
        {
            TimeSpan _timeDiff = input2 - input1;
            double result = 0;
            result = _timeDiff.TotalHours;
            return result;
        }

        /// <summary>
        /// Returns the difference in minutes between two DateTime object
        /// </summary>
        /// <param name="input1"></param>
        /// <param name="input2"></param>
        /// <returns></returns>
        private static double ReturnMinutes(DateTime input1, DateTime input2)
        {
            TimeSpan _timeDiff = input2 - input1;
            double result = 0;
            result = _timeDiff.TotalMinutes;
            return result;
        }

        /// <summary>
        /// Returns the difference in seconds between two DateTime object
        /// </summary>
        /// <param name="input1"></param>
        /// <param name="input2"></param>
        /// <returns></returns>
        private static double ReturnSeconds(DateTime input1, DateTime input2)
        {
            TimeSpan _timeDiff = input2 - input1;
            double result = 0;
            result = _timeDiff.TotalSeconds;
            return result;
        }

        /// <summary>
        /// Returns the difference in split seconds between two DateTime object
        /// </summary>
        /// <param name="input1"></param>
        /// <param name="input2"></param>
        /// <returns></returns>
        private static double ReturnSplitSeconds(DateTime input1, DateTime input2)
        {
            TimeSpan _timeDiff = input2 - input1;
            double result = 0;
            result = _timeDiff.TotalMilliseconds;
            return result;
        }



        #endregion OutputFormat Methods

    }
}
