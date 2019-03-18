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

using Dev2.Common.DateAndTime.TO;
using Dev2.Common.Interfaces.Core.Convertors.DateAndTime;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Dev2.Common.TimeZoneBuilder;
using Warewolf.Resource.Errors;




namespace Dev2.Common.DateAndTime
{
    public abstract class DateTimeParser : IDateTimeParser
    {
        public enum LiteralRegionStates
        {
            OutsideLiteralRegion,
            InsideLiteralRegion,
            InsideLiteralRegionWithEscape,
            InsideInferredLiteralRegion,
            InsideInferredLiteralRegionWithEscape,
        }

        public static readonly char DateLiteralCharacter = '\'';

        protected  static Dictionary<char, List<int>> _dateTimeFormatForwardLookups = new Dictionary<char, List<int>>();
        protected Dictionary<string, IDateTimeFormatPartTO> _dateTimeFormatsParts = new Dictionary<string, IDateTimeFormatPartTO>();
        protected Dictionary<string, List<IDateTimeFormatPartOptionTO>> _dateTimeFormatPartOptions = new Dictionary<string, List<IDateTimeFormatPartOptionTO>>();
        protected static Dictionary<string, List<IDateTimeFormatPartOptionTO>> _timeFormatPartOptions = new Dictionary<string, List<IDateTimeFormatPartOptionTO>>();
        public static Dictionary<string, ITimeZoneTO> TimeZones = new Dictionary<string, ITimeZoneTO>();
        static Dictionary<string, List<IDateTimeFormatPartOptionTO>> _dateTimeFormatPartOptionsForDotNet = new Dictionary<string, List<IDateTimeFormatPartOptionTO>>();
        static Dictionary<char, List<int>> _dateTimeFormatForwardLookupsForDotNet = new Dictionary<char, List<int>>();

        protected DateTimeParser()
        {
            InitializeBuilders();
        }

        void InitializeBuilders()
        {
            var timeZoneBuilder = new TimeZoneBuilder.TimeZoneBuilder();
            timeZoneBuilder.Build();
            TimeZones = timeZoneBuilder.TimeZones;

            var formatForwardLookup = new DateTimeFormatForwardLookup();
            formatForwardLookup.Build();
            _dateTimeFormatForwardLookups = formatForwardLookup.DateTimeFormatForwardLookups;

            var dateTimeFormatPart = new DateTimeFormatPart(TimeZones);
            dateTimeFormatPart.Build();
            _dateTimeFormatsParts = dateTimeFormatPart.DateTimeFormatsParts;
            _dateTimeFormatPartOptions = dateTimeFormatPart.DateTimeFormatPartOptions;

            var timeFormatPartBuilder = new TimeFormatPartBuilder(TimeZones);
            timeFormatPartBuilder.Build();
            _timeFormatPartOptions = timeFormatPartBuilder.TimeFormatPartOptions;

            var forwardLookupsForDotNet = new DateTimeFormatForwardLookupsForDotNet();
            forwardLookupsForDotNet.Build();
            _dateTimeFormatForwardLookupsForDotNet = forwardLookupsForDotNet.DateTimeFormatForwardLookupsForDotNetLu;

            var dateTimeFormatPartsForDotNet = new DateTimeFormatPartsForDotNet();
            dateTimeFormatPartsForDotNet.Build();
            _dateTimeFormatPartOptionsForDotNet = dateTimeFormatPartsForDotNet.DateTimeFormatPartOptionsForDotNet;
        }

        public bool TryParseDateTime(string dateTime, string inputFormat, out IDateTimeResultTO parsedDateTime, out string error) => TryParse(dateTime, inputFormat, false, out parsedDateTime, out error);

        public bool TryParseTime(string time, string inputFormat, out IDateTimeResultTO parsedTime, out string error) => TryParse(time, inputFormat, true, out parsedTime, out error);

        public string TranslateDotNetToDev2Format(string originalFormat, out string error)
        {
            TryGetDateTimeFormatParts(originalFormat, _dateTimeFormatForwardLookupsForDotNet,
                _dateTimeFormatPartOptionsForDotNet, out List<IDateTimeFormatPartTO> dotNetFormatParts, out error);
            dotNetFormatParts = ReplaceToken(dotNetFormatParts, "m", "Minutes");
            dotNetFormatParts = ReplaceToken(dotNetFormatParts, "mm", "Minutes");
            dotNetFormatParts = ReplaceToken(dotNetFormatParts, "M", "Month in single digit");
            dotNetFormatParts = ReplaceToken(dotNetFormatParts, "MM", "Month in 2 digits");
            dotNetFormatParts = ReplaceToken(dotNetFormatParts, "MMM", "Month text abbreviated");
            dotNetFormatParts = ReplaceToken(dotNetFormatParts, "MMMM", "Month text in full");
            dotNetFormatParts = ReplaceToken(dotNetFormatParts, "h", "Hours in 12 hour format");
            dotNetFormatParts = ReplaceToken(dotNetFormatParts, "hh", "Hours in 12 hour format");
            dotNetFormatParts = ReplaceToken(dotNetFormatParts, "H", "Hours in 24 hour format");
            dotNetFormatParts = ReplaceToken(dotNetFormatParts, "HH", "Hours in 24 hour format");
            dotNetFormatParts = ReplaceToken(dotNetFormatParts, "tt", "am or pm");
            dotNetFormatParts = ReplaceToken(dotNetFormatParts, "ddd", "Day of Week text abbreviated");
            dotNetFormatParts = ReplaceToken(dotNetFormatParts, "dddd", "Day of Week in full");
            dotNetFormatParts = ReplaceToken(dotNetFormatParts, "fff", "Split Seconds: 987");
            //
            // Get input format string for the dotnet parts
            //
            var dev2Format = "";
            foreach (IDateTimeFormatPartTO part in dotNetFormatParts)
            {
                if (part.Isliteral)
                {
                    dev2Format += "'" + part.Value + "'";
                }
                else
                {
                    dev2Format += part.Value;
                }
            }
            return dev2Format;
        }

        List<IDateTimeFormatPartTO> ReplaceToken(List<IDateTimeFormatPartTO> currentPartList,
            string findTokenValue, string describeReplaceWith)
        {
            var mPart = currentPartList.FindIndex(part => part.Value == findTokenValue);
            while (mPart > -1 && mPart < currentPartList.Count)
            {
                currentPartList[mPart] =
                    DateTimeFormatParts[
                        DateTimeFormatParts.FindIndex(part => part.Description.Contains(describeReplaceWith))];
                mPart = currentPartList.FindIndex(part => part.Value == findTokenValue);
            }
            return currentPartList;
        }

        public bool TryGetDateTimeFormatParts(string format, out List<IDateTimeFormatPartTO> formatParts,
            out string error) => TryGetDateTimeFormatParts(format, _dateTimeFormatForwardLookups, _dateTimeFormatPartOptions,
                out formatParts, out error);

        bool TryGetDateTimeFormatParts(string format, Dictionary<char, List<int>> dateTimeFormatForwardLookups, Dictionary<string, List<IDateTimeFormatPartOptionTO>> dateTimeFormatPartOptions, out List<IDateTimeFormatPartTO> formatParts, out string error)
        {
            var nothingDied = true;

            formatParts = new List<IDateTimeFormatPartTO>();
            error = "";

            var formatArray = format.ToArray();
            var literalRegionState = LiteralRegionStates.OutsideLiteralRegion;
            var count = 0;

            var currentValue = "";
            while (count < formatArray.Length && nothingDied)
            {
                var forwardLookupLength = 0;
                var currentChar = formatArray[count];

                if (literalRegionState == LiteralRegionStates.OutsideLiteralRegion)
                {
                    forwardLookupLength = DateTimeLiteralProcessor.ProcessOutsideLiteral(dateTimeFormatForwardLookups, dateTimeFormatPartOptions, formatParts, ref error, currentChar, formatArray, count, forwardLookupLength, ref literalRegionState, ref currentValue);
                }
                else if (literalRegionState == LiteralRegionStates.InsideInferredLiteralRegion)
                {
                    forwardLookupLength = DateTimeLiteralProcessor.ProcessInsideInferredLiteral(dateTimeFormatForwardLookups, dateTimeFormatPartOptions, formatParts, ref error, currentChar, formatArray, count, forwardLookupLength, ref currentValue, ref literalRegionState);
                }
                else if (literalRegionState == LiteralRegionStates.InsideInferredLiteralRegionWithEscape)
                {
                    literalRegionState = DateTimeLiteralProcessor.ProcessInsideInferredEscapedLiteral(ref error, currentChar, literalRegionState, ref currentValue, ref nothingDied);
                }
                else if (literalRegionState == LiteralRegionStates.InsideLiteralRegion)
                {
                    forwardLookupLength = DateTimeLiteralProcessor.ProcessInsideLiteral(formatParts, ref error, currentChar, formatArray, count, forwardLookupLength, ref currentValue, ref literalRegionState);
                }
                else if (literalRegionState == LiteralRegionStates.InsideLiteralRegionWithEscape)
                {
                    literalRegionState = DateTimeLiteralProcessor.ProcessInsideEscapedLiteral(ref error, currentChar, literalRegionState, ref currentValue, ref nothingDied);
                }
                else
                {
                    throw new ArgumentOutOfRangeException("Unrecognized literal region state: " + literalRegionState);
                }

                count++;
                if (forwardLookupLength > 0)
                {
                    count += forwardLookupLength - 1;
                }
            }

            if (currentValue.Length > 0 && literalRegionState != LiteralRegionStates.InsideLiteralRegion &&
                literalRegionState != LiteralRegionStates.InsideLiteralRegionWithEscape)
            {
                formatParts.Add(new DateTimeFormatPartTO(currentValue, true, ""));
            }
            else if (currentValue.Length > 0)
            {
                nothingDied = false;
                error = "A \' character defines a start or end of a non date time region, there appears to be a extra \' character.";
            }
            else
            {
                //valid
            }

            return nothingDied;
        }

        bool TryParse(string data, string inputFormat, bool parseAsTime, out IDateTimeResultTO result,
            out string error)
        {
            var nothingDied = true;

            result = new DateTimeResultTO();
            error = "";
            var originalInputFormat = inputFormat;
            var originalData = data;
            var culturesTried = 0;
            const int MaxAttempts = 8;
            if (string.IsNullOrWhiteSpace(data))
            {
                originalData = DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToLongTimeString();
            }

            if (string.IsNullOrWhiteSpace(inputFormat))
            {
                originalInputFormat =
                    TranslateDotNetToDev2Format(
                        GlobalConstants.Dev2DotNetDefaultDateTimeFormat.Replace("ss", "ss.fff"), out error);
            }
            else
            {
                culturesTried = MaxAttempts;
            }
            while (culturesTried <= MaxAttempts)
            {
                var dateTimeArray = originalData.ToArray();
                var position = 0;


                nothingDied = TryGetDateTimeFormatParts(originalInputFormat, _dateTimeFormatForwardLookups, _dateTimeFormatPartOptions, out List<IDateTimeFormatPartTO> formatParts, out error);
                if (!string.IsNullOrEmpty(error))
                {
                    return false;
                }
                if (nothingDied)
                {
                    TryCulture(parseAsTime, ref result, ref error, ref nothingDied, ref originalInputFormat, ref culturesTried, MaxAttempts, dateTimeArray, ref position, formatParts);
                }
                else
                {
                    culturesTried++;
                }
            }

            return nothingDied;
        }

        private void TryCulture(bool parseAsTime, ref IDateTimeResultTO result, ref string error, ref bool nothingDied, ref string originalInputFormat, ref int culturesTried, int MaxAttempts, char[] dateTimeArray, ref int position, List<IDateTimeFormatPartTO> formatParts)
        {
            var count = 0;
            while (count < formatParts.Count && nothingDied && position < dateTimeArray.Length)
            {
                var formatPart = formatParts[count];

                if (TryGetDataFromDateTime(dateTimeArray, position, formatPart, result, parseAsTime,
                    out int resultLength, out error))
                {
                    position += resultLength;
                }
                else
                {
                    //clear invalid result!
                    result = new DateTimeResultTO();
                    nothingDied = false;
                }

                count++;
            }
            if (!nothingDied)
            {
                originalInputFormat = MatchInputFormatToCulture(ref error, culturesTried);

                if (culturesTried >= MaxAttempts)
                {
                    if (!IsBlankResult(result))
                    {
                        //Return the result if it isn't blank
                        nothingDied = true;
                    }
                    else
                    {
                        //no result, throw error
                        error = string.Format(ErrorResource.CannorParseInputDateTimeWithGivenFormat, error);
                    }
                }
                else
                {
                    nothingDied = true;
                }

                culturesTried++;
            }
            else
            {
                //Stop trying different formats
                culturesTried = MaxAttempts + 1;
            }
        }

        string MatchInputFormatToCulture(ref string error, int culturesTried)
        {
            var inputFormat = "";
            switch (culturesTried)
            {
                case 0:
                    inputFormat =
                        TranslateDotNetToDev2Format(
                            CultureInfo.CurrentUICulture.DateTimeFormat.FullDateTimePattern, out error);
                    break;

                case 1:
                    inputFormat =
                        TranslateDotNetToDev2Format(
                            CultureInfo.InvariantCulture.DateTimeFormat.FullDateTimePattern, out error);
                    break;

                case 2:
                    inputFormat =
                        TranslateDotNetToDev2Format(
                            CultureInfo.InvariantCulture.DateTimeFormat.ShortDatePattern + " " +
                            CultureInfo.InvariantCulture.DateTimeFormat.LongTimePattern, out error);
                    break;

                case 3:
                    inputFormat =
                        TranslateDotNetToDev2Format(
                            new CultureInfo("en-ZA").DateTimeFormat.FullDateTimePattern, out error);
                    break;

                case 4:
                    inputFormat =
                        TranslateDotNetToDev2Format(
                            new CultureInfo("en-ZA").DateTimeFormat.ShortDatePattern + " " +
                            new CultureInfo("en-ZA").DateTimeFormat.LongTimePattern, out error);
                    break;

                case 5:
                    inputFormat =
                        TranslateDotNetToDev2Format(
                            new CultureInfo("en-US").DateTimeFormat.FullDateTimePattern, out error);
                    break;

                case 6:
                    inputFormat =
                        TranslateDotNetToDev2Format(
                            new CultureInfo("en-US").DateTimeFormat.ShortDatePattern + " " +
                            new CultureInfo("en-US").DateTimeFormat.LongTimePattern, out error);
                    break;

                case 7:
                    var shortPattern = CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern;
                    var longPattern = CultureInfo.CurrentCulture.DateTimeFormat.LongTimePattern;
                    var finalPattern = shortPattern + " " + longPattern;
                    if (finalPattern.Contains("ss"))
                    {
                        finalPattern =
                            finalPattern.Insert(finalPattern.IndexOf("ss", StringComparison.Ordinal) + 2,
                                ".fff");
                    }
                    inputFormat = TranslateDotNetToDev2Format(finalPattern, out error);
                    break;
                default:
                    break;
            }
            return inputFormat;
        }

        static bool IsBlankResult(IDateTimeResultTO result) => result.AmPm == DateTimeAmPm.am &&
                   result.Days == 0 &&
                   result.DaysOfWeek == 0 || result.DaysOfWeek == 1 &&
                   result.DaysOfYear == 0 &&
                   result.Era == null &&
                   result.Hours == 0 &&
                   !result.Is24H &&
                   result.Milliseconds == 0 &&
                   result.Minutes == 0 &&
                   result.Months == 0 &&
                   result.Seconds == 0 &&
                   result.Weeks == 0 &&
                   result.Years == 0;

        bool TryGetDataFromDateTime(char[] dateTimeArray, int startPosition, IDateTimeFormatPartTO part, IDateTimeResultTO result, bool passAsTime, out int resultLength, out string error)
        {
            var nothingDied = true;

            error = "";
            resultLength = 0;

            var dataFound = false;

            if (part.Isliteral)
            {
                var forwardLookupResult = ForwardLookup(dateTimeArray, startPosition, part.Value.Length);

                if (forwardLookupResult != part.Value)
                {
                    nothingDied = false;
                    error = string.Concat("Literal expressed from index ", startPosition,
                        " doesn't match what is specified in the input format.");
                }
                else
                {
                    resultLength = forwardLookupResult.Length;
                }
            }
            else
            {
                //
                // Get the possible options for part
                //
                List<IDateTimeFormatPartOptionTO> partOptions;

                if (passAsTime)
                {
                    if (!_timeFormatPartOptions.TryGetValue(part.Value, out partOptions))
                    {
                        nothingDied = false;
                        error = string.Format(ErrorResource.UnrecognisedFormatPart, part.Value);
                    }
                }
                else
                {
                    if (!_dateTimeFormatPartOptions.TryGetValue(part.Value, out partOptions))
                    {
                        nothingDied = false;
                        error = string.Format(ErrorResource.UnrecognisedFormatPart, part.Value);
                    }
                }

                if (nothingDied)
                {
                    TryGetAValueForEachOption(dateTimeArray, startPosition, result, passAsTime, ref resultLength, ref dataFound, partOptions);

                    //
                    // If no viable data was found set error
                    //
                    if (!dataFound)
                    {
                        nothingDied = false;
                        error = string.Format(ErrorResource.UnexpectedValueAtIndex, startPosition);
                    }
                }
            }

            return nothingDied;
        }

        private static void TryGetAValueForEachOption(char[] dateTimeArray, int startPosition, IDateTimeResultTO result, bool passAsTime, ref int resultLength, ref bool dataFound, List<IDateTimeFormatPartOptionTO> partOptions)
        {
            var partOptionsCount = 0;

            //
            // Try get a value for each option
            //
            while (partOptionsCount < partOptions.Count)
            {
                var partOption = partOptions[partOptionsCount];

                string forwardLookupResult;
                bool predicateRun;

                if (partOption.Length != partOption.ResultLength)
                {
                    forwardLookupResult = ForwardLookup(dateTimeArray, startPosition, partOption.ResultLength);
                    predicateRun = partOption.Predicate(forwardLookupResult, passAsTime);
                    if (!predicateRun)
                    {
                        forwardLookupResult = ForwardLookup(dateTimeArray, startPosition, partOption.Length);
                        predicateRun = partOption.Predicate(forwardLookupResult, passAsTime);
                    }
                }
                else
                {
                    forwardLookupResult = ForwardLookup(dateTimeArray, startPosition, partOption.Length);

                    predicateRun = partOption.Predicate(forwardLookupResult, passAsTime);
                }

                //
                // Check length of forward lookup is correct
                //
                if ((forwardLookupResult.Length == partOption.Length ||
                     forwardLookupResult.Length == partOption.ResultLength) &&
                    (partOption.Predicate == null || predicateRun))
                {
                    //
                    // Set exit and result length
                    //
                    partOptionsCount = partOptions.Count;
                    resultLength = forwardLookupResult.Length;
                    dataFound = true;

                    //
                    // Decide on the correct value to use
                    //
                    IConvertible value;
                    if (partOption.ActualValue != null)
                    {
                        value = partOption.ActualValue;
                    }
                    else if (partOption.IsNumeric)
                    {
                        value = Convert.ToInt32(forwardLookupResult);
                    }
                    else
                    {
                        value = forwardLookupResult;
                    }
                    partOption.AssignAction?.Invoke(result, passAsTime, value);
                }

                partOptionsCount++;
            }
        }

        /// <summary>
        ///     Does a forward lookup on the given array and returns the resulting string
        /// </summary>
        public static string ForwardLookup(char[] formatArray, int startPosition, int lookupLength)
        {
            var result = "";
            var position = startPosition;

            while (position >= 0 && position < formatArray.Length && position < startPosition + lookupLength)
            {
                result += formatArray[position];
                position++;
            }
            return result;
        }

        public List<IDateTimeFormatPartTO> DateTimeFormatParts => _dateTimeFormatsParts.Values.ToList();

    }
}



