/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
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

// ReSharper disable NonLocalizedString
// ReSharper disable CollectionNeverQueried.Local

namespace Dev2.Common.DateAndTime
{
    public class DateTimeParser : IDateTimeParser
    {
        /// <summary>
        ///     used to describe the position of the parser relative to escaped regions
        /// </summary>
        public enum LiteralRegionStates
        {
            OutsideLiteralRegion,
            InsideLiteralRegion,
            InsideLiteralRegionWithEscape,
            InsideInferredLiteralRegion,
            InsideInferredLiteralRegionWithEscape,
        }

        public const char DateLiteralCharacter = '\'';

        private static Dictionary<char, List<int>> _dateTimeFormatForwardLookups = new Dictionary<char, List<int>>();
        private static Dictionary<string, IDateTimeFormatPartTO> _dateTimeFormatsParts = new Dictionary<string, IDateTimeFormatPartTO>();
        private static Dictionary<string, List<IDateTimeFormatPartOptionTO>> _dateTimeFormatPartOptions = new Dictionary<string, List<IDateTimeFormatPartOptionTO>>();
        private static Dictionary<string, List<IDateTimeFormatPartOptionTO>> _timeFormatPartOptions =new Dictionary<string, List<IDateTimeFormatPartOptionTO>>();
        public static Dictionary<string, ITimeZoneTO> TimeZones = new Dictionary<string, ITimeZoneTO>();
        private static Dictionary<string, List<IDateTimeFormatPartOptionTO>> _dateTimeFormatPartOptionsForDotNet= new Dictionary<string, List<IDateTimeFormatPartOptionTO>>();
        private static Dictionary<char, List<int>> _dateTimeFormatForwardLookupsForDotNet =new Dictionary<char, List<int>>();

        static DateTimeParser()
        {
            InitializeBuilders();
        }

        private static void InitializeBuilders()
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

        /// <summary>
        ///     Creates a DateTime instance from a specified string and format.
        /// </summary>
        public bool TryParseDateTime(string dateTime, string inputFormat, out IDateTimeResultTO result, out string error)
        {
            bool nothingDied = TryParse(dateTime, inputFormat, false, out result, out error);

            return nothingDied;
        }

        /// <summary>
        ///     Creates a TimeSpan instance from a specified string and format.
        /// </summary>
        public bool TryParseTime(string time, string inputFormat, out IDateTimeResultTO parsedTime, out string error)
        {
            return TryParse(time, inputFormat, true, out parsedTime, out error);
        }

        public string TranslateDotNetToDev2Format(string originalFormat, out string error)
        {
            List<IDateTimeFormatPartTO> dotNetFormatParts;
            TryGetDateTimeFormatParts(originalFormat, _dateTimeFormatForwardLookupsForDotNet,
                _dateTimeFormatPartOptionsForDotNet, out dotNetFormatParts, out error);
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
            string dev2Format = "";
            foreach (IDateTimeFormatPartTO part in dotNetFormatParts)
            {
                if (part.Isliteral)
                    dev2Format += "'" + part.Value + "'";
                else
                    dev2Format += part.Value;
            }
            return dev2Format;
        }

        private List<IDateTimeFormatPartTO> ReplaceToken(List<IDateTimeFormatPartTO> currentPartList,
            string findTokenValue, string describeReplaceWith)
        {
            int mPart = currentPartList.FindIndex(part => part.Value == findTokenValue);
            while (mPart > -1 && mPart < currentPartList.Count)
            {
                currentPartList[mPart] =
                    DateTimeFormatParts[
                        DateTimeFormatParts.FindIndex(part => part.Description.Contains(describeReplaceWith))];
                mPart = currentPartList.FindIndex(part => part.Value == findTokenValue);
            }
            return currentPartList;
        }

        /// <summary>
        ///     Breaks a date time format up into parts
        /// </summary>
        internal static bool TryGetDateTimeFormatParts(string format, out List<IDateTimeFormatPartTO> formatParts,
            out string error)
        {
            return TryGetDateTimeFormatParts(format, _dateTimeFormatForwardLookups, _dateTimeFormatPartOptions,
                out formatParts, out error);
        }

        /// <summary>
        ///     Breaks a date time format up into parts
        /// </summary>
        static bool TryGetDateTimeFormatParts(string format, Dictionary<char, List<int>> dateTimeFormatForwardLookups, Dictionary<string, List<IDateTimeFormatPartOptionTO>> dateTimeFormatPartOptions, out List<IDateTimeFormatPartTO> formatParts, out string error)
        {
            bool nothingDied = true;

            formatParts = new List<IDateTimeFormatPartTO>();
            error = "";

            char[] formatArray = format.ToArray();
            var literalRegionState = LiteralRegionStates.OutsideLiteralRegion;
            int count = 0;

            string currentValue = "";
            while (count < formatArray.Length && nothingDied)
            {
                int forwardLookupLength = 0;
                char currentChar = formatArray[count];

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
                error = "A \' character defines a start or end of a non date time region, there apears to be a extra \' character.";
            }

            return nothingDied;
        }
        /// <summary>
        ///     Parses the given data using the specified format
        /// </summary>
        private bool TryParse(string data, string inputFormat, bool parseAsTime, out IDateTimeResultTO result,
            out string error)
        {
            bool nothingDied = true;

            result = new DateTimeResultTO();
            error = "";

            int culturesTried = 0;
            const int MaxAttempts = 8;
            if (string.IsNullOrWhiteSpace(data))
            {
                data = DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToLongTimeString();
            }

            if (string.IsNullOrWhiteSpace(inputFormat))
            {
                inputFormat =
                    TranslateDotNetToDev2Format(
                        GlobalConstants.Dev2DotNetDefaultDateTimeFormat.Replace("ss", "ss.fff"), out error);
            }
            else
            {
                culturesTried = MaxAttempts;
            }
            while (culturesTried <= MaxAttempts)
            {
                char[] dateTimeArray = data.ToArray();
                List<IDateTimeFormatPartTO> formatParts;
                int position = 0;


                nothingDied = TryGetDateTimeFormatParts(inputFormat, _dateTimeFormatForwardLookups, _dateTimeFormatPartOptions, out formatParts, out error);
                if (!string.IsNullOrEmpty(error))
                {
                    return false;
                }
                if (nothingDied)
                {

                    int count = 0;
                    while (count < formatParts.Count && nothingDied && position < dateTimeArray.Length)
                    {
                        IDateTimeFormatPartTO formatPart = formatParts[count];

                        int resultLength;
                        if (TryGetDataFromDateTime(dateTimeArray, position, formatPart, result, parseAsTime,
                            out resultLength, out error))
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
                        inputFormat = MatchInputFormatToCulture(ref error, culturesTried);

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
                else
                {
                    culturesTried++;
                }
            }

            return nothingDied;
        }

        private string MatchInputFormatToCulture(ref string error, int culturesTried)
        {
            string inputFormat = "";
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
                    string shortPattern = CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern;
                    string longPattern = CultureInfo.CurrentCulture.DateTimeFormat.LongTimePattern;
                    string finalPattern = shortPattern + " " + longPattern;
                    if (finalPattern.Contains("ss"))
                    {
                        finalPattern =
                            finalPattern.Insert(finalPattern.IndexOf("ss", StringComparison.Ordinal) + 2,
                                ".fff");
                    }
                    inputFormat = TranslateDotNetToDev2Format(finalPattern, out error);
                    break;
            }
            return inputFormat;
        }

        private static bool IsBlankResult(IDateTimeResultTO result)
        {
            return result.AmPm == DateTimeAmPm.am &&
                   result.Days == 0 &&
                   result.DaysOfWeek == 0 || result.DaysOfWeek == 1 &&
                   result.DaysOfYear == 0 &&
                   result.Era == null &&
                   result.Hours == 0 &&
                   result.Is24H == false &&
                   result.Milliseconds == 0 &&
                   result.Minutes == 0 &&
                   result.Months == 0 &&
                   result.Seconds == 0 &&
                   result.Weeks == 0 &&
                   result.Years == 0;
        }

        /// <summary>
        ///     Extracts data from a date time text given a potision and a date time format part. This data is then assigned to the
        ///     given result.
        /// </summary>
        private static bool TryGetDataFromDateTime(char[] dateTimeArray, int startPosition, IDateTimeFormatPartTO part, IDateTimeResultTO result, bool passAsTime, out int resultLength, out string error)
        {
            bool nothingDied = true;

            error = "";
            resultLength = 0;

            bool dataFound = false;

            if (part.Isliteral)
            {
                string forwardLookupResult = ForwardLookup(dateTimeArray, startPosition, part.Value.Length);

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
                    int partOptionsCount = 0;

                    //
                    // Try get a value for each option
                    //
                    while (partOptionsCount < partOptions.Count)
                    {
                        IDateTimeFormatPartOptionTO partOption = partOptions[partOptionsCount];

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

        /// <summary>
        ///     Does a forward lookup on the given array and returns the resulting string
        /// </summary>
        public static string ForwardLookup(char[] formatArray, int startPosition, int lookupLength)
        {
            string result = "";
            int position = startPosition;

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



