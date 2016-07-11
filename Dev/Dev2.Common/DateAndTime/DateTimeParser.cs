/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2016 by Warewolf Ltd <alpha@warewolf.io>
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
using Dev2.Common.Interfaces;
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
        private const char TimeLiteralCharacter = ':';
        private static readonly IDatetimeParserHelper DatetimeParserHelper = new DateTimeParserHelper();
        private static readonly IAssignManager AssignManager = new AssignManager();

        private static readonly Dictionary<char, List<int>> DateTimeFormatForwardLookups =
            new Dictionary<char, List<int>>();

        private static readonly Dictionary<string, IDateTimeFormatPartTO> DateTimeFormatsParts =
            new Dictionary<string, IDateTimeFormatPartTO>();

        private static readonly Dictionary<string, List<IDateTimeFormatPartOptionTO>> DateTimeFormatPartOptions =
            new Dictionary<string, List<IDateTimeFormatPartOptionTO>>();

        private static readonly Dictionary<string, List<IDateTimeFormatPartOptionTO>> TimeFormatPartOptions =
            new Dictionary<string, List<IDateTimeFormatPartOptionTO>>();

        public static readonly Dictionary<string, ITimeZoneTO> TimeZones = new Dictionary<string, ITimeZoneTO>();

        private static readonly Dictionary<string, IDateTimeFormatPartTO> DateTimeFormatPartsForDotNet =
            new Dictionary<string, IDateTimeFormatPartTO>();

        private static readonly Dictionary<string, List<IDateTimeFormatPartOptionTO>> DateTimeFormatPartOptionsForDotNet
            = new Dictionary<string, List<IDateTimeFormatPartOptionTO>>();

        private static readonly Dictionary<char, List<int>> DateTimeFormatForwardLookupsForDotNet =
            new Dictionary<char, List<int>>();

        static DateTimeParser()
        {
            CreateTimeZones();
            CreateDateTimeFormatForwardLookups();
            CreateDateTimeFormatParts();
            CreateTimeFormatParts();
            CreateDateTimeFormatForwardLookupsForDotNet();
            CreateDateTimeFormatPartsForDotNet();
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
            TryGetDateTimeFormatParts(originalFormat, DateTimeFormatForwardLookupsForDotNet,
                DateTimeFormatPartOptionsForDotNet, out dotNetFormatParts, out error);
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
            return TryGetDateTimeFormatParts(format, DateTimeFormatForwardLookups, DateTimeFormatPartOptions,
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


                nothingDied = TryGetDateTimeFormatParts(inputFormat, DateTimeFormatForwardLookups, DateTimeFormatPartOptions, out formatParts, out error);
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
                    if (!TimeFormatPartOptions.TryGetValue(part.Value, out partOptions))
                    {
                        nothingDied = false;
                        error = string.Format(ErrorResource.UnrecognisedFormatPart, part.Value);
                    }
                }
                else
                {
                    if (!DateTimeFormatPartOptions.TryGetValue(part.Value, out partOptions))
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

        //Ashley make a .net version of this method Only populate the _dateTimeFormatPartOptions dictionary, and all values should be null
        /// <summary>
        ///     Creates a list of all valid date time format parts
        /// </summary>
        private static void CreateDateTimeFormatPartsForDotNet()
        {
            DateTimeFormatPartsForDotNet.Add("d", new DateTimeFormatPartTO("d", false, "Day in 1 or 2 digits: 8"));
            DateTimeFormatPartOptionsForDotNet.Add("d", null);
            DateTimeFormatPartsForDotNet.Add("dd", new DateTimeFormatPartTO("dd", false, "Day in 2 digits: 8"));
            DateTimeFormatPartOptionsForDotNet.Add("dd", null);
            DateTimeFormatPartsForDotNet.Add("ddd",
                new DateTimeFormatPartTO("ddd", false, "The abbreviated name of the day of the week: Mon"));
            DateTimeFormatPartOptionsForDotNet.Add("ddd", null);
            DateTimeFormatPartsForDotNet.Add("dddd",
                new DateTimeFormatPartTO("dddd", false, "The full name of the day of the week: Monday"));
            DateTimeFormatPartOptionsForDotNet.Add("dddd", null);

            DateTimeFormatPartsForDotNet.Add("f", new DateTimeFormatPartTO("f", false, "The tenths of a second: 6"));
            DateTimeFormatPartOptionsForDotNet.Add("f", null);
            DateTimeFormatPartsForDotNet.Add("ff",
                new DateTimeFormatPartTO("ff", false, "The hundredths of a second in a date and time value: 6"));
            DateTimeFormatPartOptionsForDotNet.Add("ff", null);
            DateTimeFormatPartsForDotNet.Add("fff",
                new DateTimeFormatPartTO("fff", false, "The milliseconds in a date and time value: 6"));
            DateTimeFormatPartOptionsForDotNet.Add("fff", null);
            DateTimeFormatPartsForDotNet.Add("ffff",
                new DateTimeFormatPartTO("ffff", false, "The ten thousandths of a second in a date and time value: 6"));
            DateTimeFormatPartOptionsForDotNet.Add("ffff", null);
            DateTimeFormatPartsForDotNet.Add("fffff",
                new DateTimeFormatPartTO("fffff", false,
                    "The hundred thousandths of a second in a date and time value: 6"));
            DateTimeFormatPartOptionsForDotNet.Add("fffff", null);
            DateTimeFormatPartsForDotNet.Add("ffffff",
                new DateTimeFormatPartTO("ffffff", false, "The millionths of a second in a date and time value: 6"));
            DateTimeFormatPartOptionsForDotNet.Add("ffffff", null);
            DateTimeFormatPartsForDotNet.Add("fffffff",
                new DateTimeFormatPartTO("ffffffff", false, "The ten millionths of a second in a date and time value: 6"));
            DateTimeFormatPartOptionsForDotNet.Add("fffffff", null);

            DateTimeFormatPartsForDotNet.Add("F",
                new DateTimeFormatPartTO("f", false, "If non-zero, the tenths of a second: 6"));
            DateTimeFormatPartOptionsForDotNet.Add("F", null);
            DateTimeFormatPartsForDotNet.Add("FF",
                new DateTimeFormatPartTO("FF", false,
                    "If non-zero, the hundredths of a second in a date and time value: 6"));
            DateTimeFormatPartOptionsForDotNet.Add("FF", null);
            DateTimeFormatPartsForDotNet.Add("FFF",
                new DateTimeFormatPartTO("FFF", false, "If non-zero, the milliseconds in a date and time value: 6"));
            DateTimeFormatPartOptionsForDotNet.Add("FFF", null);
            DateTimeFormatPartsForDotNet.Add("FFFF",
                new DateTimeFormatPartTO("FFFF", false,
                    "If non-zero, the ten thousandths of a second in a date and time value: 6"));
            DateTimeFormatPartOptionsForDotNet.Add("FFFF", null);
            DateTimeFormatPartsForDotNet.Add("FFFFF",
                new DateTimeFormatPartTO("FFFFF", false,
                    "If non-zero, the hundred thousandths of a second in a date and time value: 6"));
            DateTimeFormatPartOptionsForDotNet.Add("FFFFF", null);
            DateTimeFormatPartsForDotNet.Add("FFFFFF",
                new DateTimeFormatPartTO("FFFFFF", false,
                    "If non-zero, the millionths of a second in a date and time value: 6"));
            DateTimeFormatPartOptionsForDotNet.Add("FFFFFF", null);

            DateTimeFormatPartsForDotNet.Add("g", new DateTimeFormatPartTO("g", false, "The period or era: A.D."));
            DateTimeFormatPartOptionsForDotNet.Add("g", null);
            DateTimeFormatPartsForDotNet.Add("gg", new DateTimeFormatPartTO("gg", false, "The period or era: A.D."));
            DateTimeFormatPartOptionsForDotNet.Add("gg", null);

            DateTimeFormatPartsForDotNet.Add("h",
                new DateTimeFormatPartTO("h", false, "The hour, using a 12-hour clock from 1 to 12: 1"));
            DateTimeFormatPartOptionsForDotNet.Add("h", null);
            DateTimeFormatPartsForDotNet.Add("hh",
                new DateTimeFormatPartTO("hh", false, "The hour, using a 12-hour clock from 1 to 12: 01"));
            DateTimeFormatPartOptionsForDotNet.Add("hh", null);
            DateTimeFormatPartsForDotNet.Add("H",
                new DateTimeFormatPartTO("H", false, "The hour, using a 24-hour clock from 1 to 24: 01"));
            DateTimeFormatPartOptionsForDotNet.Add("H", null);
            DateTimeFormatPartsForDotNet.Add("HH",
                new DateTimeFormatPartTO("HH", false, "The hour, using a 24-hour clock from 1 to 24: 13"));
            DateTimeFormatPartOptionsForDotNet.Add("HH", null);

            DateTimeFormatPartsForDotNet.Add("K", new DateTimeFormatPartTO("K", false, "Time zone information: +02:00"));
            DateTimeFormatPartOptionsForDotNet.Add("K", null);

            DateTimeFormatPartsForDotNet.Add("m", new DateTimeFormatPartTO("m", false, "Minute in 1 or 2 digits: 9"));
            DateTimeFormatPartOptionsForDotNet.Add("m", null);
            DateTimeFormatPartsForDotNet.Add("mm", new DateTimeFormatPartTO("mm", false, "Minute in two digits: 09"));
            DateTimeFormatPartOptionsForDotNet.Add("mm", null);

            DateTimeFormatPartsForDotNet.Add("M", new DateTimeFormatPartTO("M", false, "Month in 1 or 2 digits: 6"));
            DateTimeFormatPartOptionsForDotNet.Add("M", null);
            DateTimeFormatPartsForDotNet.Add("MM", new DateTimeFormatPartTO("MM", false, "Month in two digits: 06"));
            DateTimeFormatPartOptionsForDotNet.Add("MM", null);
            DateTimeFormatPartsForDotNet.Add("MMM",
                new DateTimeFormatPartTO("MMM", false, "Abbreviated name of the month: Jun"));
            DateTimeFormatPartOptionsForDotNet.Add("MMM", null);
            DateTimeFormatPartsForDotNet.Add("MMMM",
                new DateTimeFormatPartTO("MMM", false, "Full name of the month: June"));
            DateTimeFormatPartOptionsForDotNet.Add("MMMM", null);

            DateTimeFormatPartsForDotNet.Add("s", new DateTimeFormatPartTO("s", false, "Second in 1 or 2 digits: 9"));
            DateTimeFormatPartOptionsForDotNet.Add("s", null);
            DateTimeFormatPartsForDotNet.Add("ss", new DateTimeFormatPartTO("ss", false, "Second in two digits: 09"));
            DateTimeFormatPartOptionsForDotNet.Add("ss", null);

            DateTimeFormatPartsForDotNet.Add("t",
                new DateTimeFormatPartTO("t", false, "First character of the AM/PM designator: 9"));
            DateTimeFormatPartOptionsForDotNet.Add("t", null);
            DateTimeFormatPartsForDotNet.Add("tt", new DateTimeFormatPartTO("tt", false, "AM/PM designator: 09"));
            DateTimeFormatPartOptionsForDotNet.Add("tt", null);

            DateTimeFormatPartsForDotNet.Add("y", new DateTimeFormatPartTO("y", false, "Year in 1 or 2 digits: 13"));
            DateTimeFormatPartOptionsForDotNet.Add("y", null);
            DateTimeFormatPartsForDotNet.Add("yy", new DateTimeFormatPartTO("yy", false, "Year in two digits: 13"));
            DateTimeFormatPartOptionsForDotNet.Add("yy", null);
            DateTimeFormatPartsForDotNet.Add("yyy",
                new DateTimeFormatPartTO("yyy", false, "Year, with a minimum of three digits: 2013"));
            DateTimeFormatPartOptionsForDotNet.Add("yyy", null);
            DateTimeFormatPartsForDotNet.Add("yyyy",
                new DateTimeFormatPartTO("yyyy", false, "Year in four digits: 2013"));
            DateTimeFormatPartOptionsForDotNet.Add("yyyy", null);
            DateTimeFormatPartsForDotNet.Add("yyyyy",
                new DateTimeFormatPartTO("yyyyy", false, "Year in five digits: 02013"));
            DateTimeFormatPartOptionsForDotNet.Add("yyyyy", null);

            DateTimeFormatPartsForDotNet.Add("z",
                new DateTimeFormatPartTO("z", false, "Hours offset from UTC, with no leading zeros: +2"));
            DateTimeFormatPartOptionsForDotNet.Add("z", null);
            DateTimeFormatPartsForDotNet.Add("zz",
                new DateTimeFormatPartTO("zz", false, "Hours offset from UTC in two digits: +02"));
            DateTimeFormatPartOptionsForDotNet.Add("zz", null);
            DateTimeFormatPartsForDotNet.Add("zzz",
                new DateTimeFormatPartTO("zzz", false, "Hours and minutes offset from UTC: +02:00"));
            DateTimeFormatPartOptionsForDotNet.Add("zzz", null);

            DateTimeFormatPartsForDotNet.Add(TimeLiteralCharacter.ToString(CultureInfo.InvariantCulture),
                new DateTimeFormatPartTO(TimeLiteralCharacter.ToString(CultureInfo.InvariantCulture), false,
                    "The time separator: '" + TimeLiteralCharacter + "'"));
            DateTimeFormatPartOptionsForDotNet.Add(TimeLiteralCharacter.ToString(CultureInfo.InvariantCulture), null);
            DateTimeFormatPartsForDotNet.Add(DateLiteralCharacter.ToString(CultureInfo.InvariantCulture),
                new DateTimeFormatPartTO(DateLiteralCharacter.ToString(CultureInfo.InvariantCulture), false,
                    "The date separator: " + DateLiteralCharacter));
            DateTimeFormatPartOptionsForDotNet.Add(DateLiteralCharacter.ToString(CultureInfo.InvariantCulture), null);
        }

        /// <summary>
        ///     Creates a list of all valid date time format parts
        /// </summary>
        private static void CreateDateTimeFormatParts()
        {
            AddYearParts();
            AddMonthParts();
            AddDayParts();
            AddWeekParts();
            AddHourParts();
            AddMinuteParts();
            AddSecondParts();
            AddOffsetParts();
            AddEraParts();
        }

        private static void AddEraParts()
        {
            DateTimeFormatsParts.Add("Era", new DateTimeFormatPartTO("Era", false, "A.D."));

            DateTimeFormatPartOptions.Add("era",
                new List<IDateTimeFormatPartOptionTO>
                {
                    new DateTimeFormatPartOptionTO(4, (data, treatAsTim) => data.ToLower().Equals("a.d."), false, "A.D.",
                        AssignManager.AssignEra),
                    new DateTimeFormatPartOptionTO(3, (data, treatAsTim) => data.ToLower().Equals("a.d"), false, "A.D",
                        AssignManager.AssignEra),
                    new DateTimeFormatPartOptionTO(2, (data, treatAsTim) => data.ToLower().Equals("ad"), false, "AD",
                        AssignManager.AssignEra)
                });

            DateTimeFormatPartOptions.Add("Era",
                new List<IDateTimeFormatPartOptionTO>
                {
                    new DateTimeFormatPartOptionTO(4, (data, treatAsTim) => data.ToLower().Equals("a.d."), false, "A.D.",
                        AssignManager.AssignEra),
                    new DateTimeFormatPartOptionTO(3, (data, treatAsTim) => data.ToLower().Equals("a.d"), false, "A.D",
                        AssignManager.AssignEra),
                    new DateTimeFormatPartOptionTO(2, (data, treatAsTim) => data.ToLower().Equals("ad"), false, "AD",
                        AssignManager.AssignEra)
                });

            DateTimeFormatPartOptions.Add("ERA",
                new List<IDateTimeFormatPartOptionTO>
                {
                    new DateTimeFormatPartOptionTO(4, (data, treatAsTim) => data.ToLower().Equals("a.d."), false, "A.D.",
                        AssignManager.AssignEra),
                    new DateTimeFormatPartOptionTO(3, (data, treatAsTim) => data.ToLower().Equals("a.d"), false, "A.D",
                        AssignManager.AssignEra),
                    new DateTimeFormatPartOptionTO(2, (data, treatAsTim) => data.ToLower().Equals("ad"), false, "AD",
                       AssignManager. AssignEra)
                });
        }

        private static void AddOffsetParts()
        {
            DateTimeFormatsParts.Add("am/pm", new DateTimeFormatPartTO("am/pm", false, "am or pm"));
            DateTimeFormatPartOptions.Add("am/pm",
                new List<IDateTimeFormatPartOptionTO>
                {
                    new DateTimeFormatPartOptionTO(4, CompareTextValueToDateTimePart.IsTextAmPm, false, null, AssignManager.AssignAmPm),
                    new DateTimeFormatPartOptionTO(3, CompareTextValueToDateTimePart.IsTextAmPm, false, null, AssignManager.AssignAmPm),
                    new DateTimeFormatPartOptionTO(2, CompareTextValueToDateTimePart.IsTextAmPm, false, null, AssignManager.AssignAmPm),
                });

            DateTimeFormatsParts.Add("Z",
                new DateTimeFormatPartTO("Z", false, "Time zone in short format: GMT (if available on the system)"));
            DateTimeFormatPartOptions.Add("Z", TimeZones.Select(k =>
            {
                IDateTimeFormatPartOptionTO dateTimeFormatPartOptionTo = new DateTimeFormatPartOptionTO(k.Key.Length, CompareTextValueToDateTimePart.IsTextTimeZone, false, null, AssignTimeZone);
                return dateTimeFormatPartOptionTo;
            }).OrderByDescending(k => k.Length).ToList());

            DateTimeFormatsParts.Add("ZZ", new DateTimeFormatPartTO("ZZ", false, "Time zone: Grenwich mean time"));
            DateTimeFormatPartOptions.Add("ZZ", TimeZones.Select(k =>
            {
                IDateTimeFormatPartOptionTO dateTimeFormatPartOptionTo = new DateTimeFormatPartOptionTO(k.Key.Length, CompareTextValueToDateTimePart.IsTextTimeZone, false, null, AssignTimeZone);
                return dateTimeFormatPartOptionTo;
            }).OrderByDescending(k => k.Length).ToList());

            DateTimeFormatsParts.Add("ZZZ", new DateTimeFormatPartTO("ZZZ", false, "Time zone offset: GMT + 02:00"));
            DateTimeFormatPartOptions.Add("ZZZ", TimeZones.Select(k =>
            {
                IDateTimeFormatPartOptionTO dateTimeFormatPartOptionTo = new DateTimeFormatPartOptionTO(k.Key.Length, CompareTextValueToDateTimePart.IsTextTimeZone, false, null, AssignTimeZone);
                return dateTimeFormatPartOptionTo;
            }).OrderByDescending(k => k.Length).ToList());
        }

        private static void AddSecondParts()
        {
            DateTimeFormatsParts.Add("ss", new DateTimeFormatPartTO("ss", false, "Seconds: 29"));
            DateTimeFormatPartOptions.Add("ss",
                new List<IDateTimeFormatPartOptionTO>
                {
                    new DateTimeFormatPartOptionTO(2, DatetimeParserHelper.IsNumberSeconds, true, null, AssignManager.AssignSeconds),
                    new DateTimeFormatPartOptionTO(1, DatetimeParserHelper.IsNumberSeconds, true, null,  AssignManager.AssignSeconds),
                });

            DateTimeFormatsParts.Add("sp", new DateTimeFormatPartTO("sp", false, "Split Seconds: 987"));
            DateTimeFormatPartOptions.Add("sp",
                new List<IDateTimeFormatPartOptionTO>
                {
                    new DateTimeFormatPartOptionTO(3, DatetimeParserHelper.IsNumberMilliseconds, true, null, AssignManager. AssignMilliseconds),
                    new DateTimeFormatPartOptionTO(2, DatetimeParserHelper.IsNumberMilliseconds, true, null, AssignManager. AssignMilliseconds),
                    new DateTimeFormatPartOptionTO(1, DatetimeParserHelper.IsNumberMilliseconds, true, null, AssignManager. AssignMilliseconds),
                });
        }

        private static void AddMinuteParts()
        {
            DateTimeFormatsParts.Add("min", new DateTimeFormatPartTO("min", false, "Minutes: 30"));
            DateTimeFormatPartOptions.Add("min",
                new List<IDateTimeFormatPartOptionTO>
                {
                    new DateTimeFormatPartOptionTO(2, DatetimeParserHelper.IsNumberMinutes, true, null, AssignManager.AssignMinutes),
                    new DateTimeFormatPartOptionTO(1, DatetimeParserHelper.IsNumberMinutes, true, null, AssignManager.AssignMinutes),
                });
        }

        private static void AddHourParts()
        {
            DateTimeFormatsParts.Add("24h", new DateTimeFormatPartTO("24h", false, "Hours in 24 hour format: 15"));
            DateTimeFormatPartOptions.Add("24h",
                new List<IDateTimeFormatPartOptionTO>
                {
                    new DateTimeFormatPartOptionTO(2, DatetimeParserHelper.IsNumber24H, true, null, AssignManager.Assign24Hours)
                });

            DateTimeFormatsParts.Add("12h", new DateTimeFormatPartTO("12h", false, "Hours in 12 hour format: 3"));
            DateTimeFormatPartOptions.Add("12h",
                new List<IDateTimeFormatPartOptionTO>
                {
                    new DateTimeFormatPartOptionTO(2, DatetimeParserHelper.IsNumber12H, true, null, AssignManager.Assign12Hours),
                    new DateTimeFormatPartOptionTO(1, DatetimeParserHelper.IsNumber12H, true, null, AssignManager.Assign12Hours),
                });
        }

        private static void AddWeekParts()
        {
            DateTimeFormatsParts.Add("ww", new DateTimeFormatPartTO("ww", false, "Week of year: 09"));
            DateTimeFormatPartOptions.Add("ww",
                new List<IDateTimeFormatPartOptionTO>
                {
                    new DateTimeFormatPartOptionTO(2, DatetimeParserHelper.IsNumberWeekOfYear, true, null, AssignManager.AssignWeeks),
                });

            DateTimeFormatsParts.Add("w", new DateTimeFormatPartTO("w", false, "Week of year: 9"));
            DateTimeFormatPartOptions.Add("w",
                new List<IDateTimeFormatPartOptionTO>
                {
                    new DateTimeFormatPartOptionTO(2, DatetimeParserHelper.IsNumberWeekOfYear, true, null, AssignManager.AssignWeeks),
                    new DateTimeFormatPartOptionTO(1, DatetimeParserHelper.IsNumberWeekOfYear, true, null, AssignManager.AssignWeeks),
                });
        }

        private static void AddDayParts()
        {
            DateTimeFormatsParts.Add("d", new DateTimeFormatPartTO("d", false, "Day of month in single digit: 6"));
            DateTimeFormatPartOptions.Add("d",
                new List<IDateTimeFormatPartOptionTO>
                {
                    new DateTimeFormatPartOptionTO(2, DatetimeParserHelper.IsNumberDay, true, null, AssignManager.AssignDays),
                    new DateTimeFormatPartOptionTO(1, DatetimeParserHelper.IsNumberDay, true, null, AssignManager.AssignDays)
                });

            DateTimeFormatsParts.Add("dd", new DateTimeFormatPartTO("dd", false, "Day of month in 2 digits: 06"));
            DateTimeFormatPartOptions.Add("dd",
                new List<IDateTimeFormatPartOptionTO>
                {
                    new DateTimeFormatPartOptionTO(2, DatetimeParserHelper.IsNumberDay, true, null, AssignManager.AssignDays)
                });

            DateTimeFormatsParts.Add("DW", new DateTimeFormatPartTO("DW", false, "Day of Week in full: Thursday"));
            DateTimeFormatPartOptions.Add("DW",
                new List<IDateTimeFormatPartOptionTO>
                {
                    new DateTimeFormatPartOptionTO(CultureInfo.CurrentCulture.DateTimeFormat.DayNames[0].Length, CompareTextValueToDateTimePart.IsTextSunday, false, 7, AssignManager.AssignDaysOfWeek),
                    new DateTimeFormatPartOptionTO(CultureInfo.CurrentCulture.DateTimeFormat.DayNames[1].Length, CompareTextValueToDateTimePart.IsTextMonday, false, 1, AssignManager.AssignDaysOfWeek),
                    new DateTimeFormatPartOptionTO(CultureInfo.CurrentCulture.DateTimeFormat.DayNames[2].Length, CompareTextValueToDateTimePart.IsTextTuesday, false, 2, AssignManager.AssignDaysOfWeek),
                    new DateTimeFormatPartOptionTO(CultureInfo.CurrentCulture.DateTimeFormat.DayNames[3].Length, CompareTextValueToDateTimePart.IsTextWednesday, false, 3, AssignManager.AssignDaysOfWeek),
                    new DateTimeFormatPartOptionTO(CultureInfo.CurrentCulture.DateTimeFormat.DayNames[4].Length, CompareTextValueToDateTimePart.IsTextThursday, false, 4, AssignManager.AssignDaysOfWeek),
                    new DateTimeFormatPartOptionTO(CultureInfo.CurrentCulture.DateTimeFormat.DayNames[5].Length, CompareTextValueToDateTimePart.IsTextFriday, false, 5, AssignManager.AssignDaysOfWeek),
                    new DateTimeFormatPartOptionTO(CultureInfo.CurrentCulture.DateTimeFormat.DayNames[6].Length, CompareTextValueToDateTimePart.IsTextSaturday, false, 6, AssignManager.AssignDaysOfWeek),
                });

            DateTimeFormatsParts.Add("dW", new DateTimeFormatPartTO("dW", false, "Day of Week text abbreviated: Thu"));
            DateTimeFormatPartOptions.Add("dW",
                new List<IDateTimeFormatPartOptionTO>
                {
                    new DateTimeFormatPartOptionTO(
                        CultureInfo.CurrentCulture.DateTimeFormat.AbbreviatedDayNames[0].Length, CompareTextValueToDateTimePart.IsTextSunday, false, 7,
                        AssignManager.AssignDaysOfWeek, 6),
                    new DateTimeFormatPartOptionTO(
                        CultureInfo.CurrentCulture.DateTimeFormat.AbbreviatedDayNames[1].Length, CompareTextValueToDateTimePart.IsTextMonday, false, 1,
                        AssignManager.AssignDaysOfWeek, 6),
                    new DateTimeFormatPartOptionTO(
                        CultureInfo.CurrentCulture.DateTimeFormat.AbbreviatedDayNames[2].Length, CompareTextValueToDateTimePart.IsTextTuesday, false, 2,
                        AssignManager.AssignDaysOfWeek, 7),
                    new DateTimeFormatPartOptionTO(
                        CultureInfo.CurrentCulture.DateTimeFormat.AbbreviatedDayNames[3].Length, CompareTextValueToDateTimePart.IsTextWednesday, false,
                        3, AssignManager.AssignDaysOfWeek, 9),
                    new DateTimeFormatPartOptionTO(
                        CultureInfo.CurrentCulture.DateTimeFormat.AbbreviatedDayNames[4].Length, CompareTextValueToDateTimePart.IsTextThursday, false,
                        4, AssignManager.AssignDaysOfWeek, 8),
                    new DateTimeFormatPartOptionTO(
                        CultureInfo.CurrentCulture.DateTimeFormat.AbbreviatedDayNames[5].Length, CompareTextValueToDateTimePart.IsTextFriday, false, 5,
                        AssignManager.AssignDaysOfWeek, 6),
                    new DateTimeFormatPartOptionTO(
                        CultureInfo.CurrentCulture.DateTimeFormat.AbbreviatedDayNames[6].Length, CompareTextValueToDateTimePart.IsTextSaturday, false,
                        6, AssignManager.AssignDaysOfWeek, 7),
                });

            DateTimeFormatsParts.Add("dw", new DateTimeFormatPartTO("dw", false, "Day of Week number: 4"));
            DateTimeFormatPartOptions.Add("dw",
                new List<IDateTimeFormatPartOptionTO>
                {
                    new DateTimeFormatPartOptionTO(1, DatetimeParserHelper.IsNumberDayOfWeek, true, null, AssignManager.AssignDaysOfWeek),
                });

            DateTimeFormatsParts.Add("dy", new DateTimeFormatPartTO("dy", false, "Day of year: 66"));
            DateTimeFormatPartOptions.Add("dy",
                new List<IDateTimeFormatPartOptionTO>
                {
                    new DateTimeFormatPartOptionTO(3, DatetimeParserHelper.IsNumberDayOfYear, true, null, AssignManager.AssignDaysOfYear),
                    new DateTimeFormatPartOptionTO(2, DatetimeParserHelper.IsNumberDayOfYear, true, null, AssignManager.AssignDaysOfYear),
                    new DateTimeFormatPartOptionTO(1, DatetimeParserHelper.IsNumberDayOfYear, true, null, AssignManager.AssignDaysOfYear)
                });
        }

        private static void AddMonthParts()
        {
            DateTimeFormatsParts.Add("mm", new DateTimeFormatPartTO("mm", false, "Month in 2 digits: 03"));
            DateTimeFormatPartOptions.Add("mm",
                new List<IDateTimeFormatPartOptionTO>
                {
                    new DateTimeFormatPartOptionTO(2, DatetimeParserHelper.IsNumberMonth, true, null, AssignManager.AssignMonths)
                });

            DateTimeFormatsParts.Add("m", new DateTimeFormatPartTO("m", false, "Month in single digit: 3"));
            DateTimeFormatPartOptions.Add("m",
                new List<IDateTimeFormatPartOptionTO>
                {
                    new DateTimeFormatPartOptionTO(2, DatetimeParserHelper.IsNumberMonth, true, null, AssignManager.AssignMonths),
                    new DateTimeFormatPartOptionTO(1, DatetimeParserHelper.IsNumberMonth, true, null, AssignManager.AssignMonths),
                });

            DateTimeFormatsParts.Add("M", new DateTimeFormatPartTO("M", false, "Month text abbreviated: Mar"));
            DateTimeFormatPartOptions.Add("M",
                new List<IDateTimeFormatPartOptionTO>
                {
                    new DateTimeFormatPartOptionTO(
                        CultureInfo.CurrentCulture.DateTimeFormat.AbbreviatedMonthNames[0].Length, CompareTextValueToDateTimePart.IsTextJanuary, false,
                        1, AssignManager.AssignMonths),
                    new DateTimeFormatPartOptionTO(
                        CultureInfo.CurrentCulture.DateTimeFormat.AbbreviatedMonthNames[1].Length, CompareTextValueToDateTimePart.IsTextFebuary, false,
                        2, AssignManager.AssignMonths),
                    new DateTimeFormatPartOptionTO(
                        CultureInfo.CurrentCulture.DateTimeFormat.AbbreviatedMonthNames[2].Length, CompareTextValueToDateTimePart.IsTextMarch, false, 3,
                        AssignManager.AssignMonths),
                    new DateTimeFormatPartOptionTO(
                        CultureInfo.CurrentCulture.DateTimeFormat.AbbreviatedMonthNames[3].Length, CompareTextValueToDateTimePart.IsTextApril, false, 4,
                        AssignManager.AssignMonths),
                    new DateTimeFormatPartOptionTO(
                        CultureInfo.CurrentCulture.DateTimeFormat.AbbreviatedMonthNames[4].Length, CompareTextValueToDateTimePart.IsTextMay, false, 5,
                        AssignManager.AssignMonths),
                    new DateTimeFormatPartOptionTO(
                        CultureInfo.CurrentCulture.DateTimeFormat.AbbreviatedMonthNames[5].Length, CompareTextValueToDateTimePart.IsTextJune, false, 6,
                        AssignManager.AssignMonths),
                    new DateTimeFormatPartOptionTO(
                        CultureInfo.CurrentCulture.DateTimeFormat.AbbreviatedMonthNames[6].Length, CompareTextValueToDateTimePart.IsTextJuly, false, 7,
                        AssignManager.AssignMonths),
                    new DateTimeFormatPartOptionTO(
                        CultureInfo.CurrentCulture.DateTimeFormat.AbbreviatedMonthNames[7].Length, CompareTextValueToDateTimePart.IsTextAugust, false,
                        8, AssignManager.AssignMonths),
                    new DateTimeFormatPartOptionTO(
                        CultureInfo.CurrentCulture.DateTimeFormat.AbbreviatedMonthNames[8].Length, CompareTextValueToDateTimePart.IsTextSeptember,
                        false, 9, AssignManager.AssignMonths),
                    new DateTimeFormatPartOptionTO(
                        CultureInfo.CurrentCulture.DateTimeFormat.AbbreviatedMonthNames[9].Length, CompareTextValueToDateTimePart.IsTextOctober, false,
                        10, AssignManager.AssignMonths),
                    new DateTimeFormatPartOptionTO(
                        CultureInfo.CurrentCulture.DateTimeFormat.AbbreviatedMonthNames[10].Length, CompareTextValueToDateTimePart.IsTextNovember,
                        false, 11, AssignManager.AssignMonths),
                    new DateTimeFormatPartOptionTO(
                        CultureInfo.CurrentCulture.DateTimeFormat.AbbreviatedMonthNames[11].Length, CompareTextValueToDateTimePart.IsTextDecember,
                        false, 12, AssignManager.AssignMonths),
                });

            DateTimeFormatsParts.Add("MM", new DateTimeFormatPartTO("MM", false, "Month text in full: March"));
            DateTimeFormatPartOptions.Add("MM",
                new List<IDateTimeFormatPartOptionTO>
                {
                    new DateTimeFormatPartOptionTO(CultureInfo.CurrentCulture.DateTimeFormat.MonthNames[0].Length, CompareTextValueToDateTimePart.IsTextJanuary, false, 1, AssignManager.AssignMonths),
                    new DateTimeFormatPartOptionTO(CultureInfo.CurrentCulture.DateTimeFormat.MonthNames[1].Length, CompareTextValueToDateTimePart.IsTextFebuary, false, 2, AssignManager.AssignMonths),
                    new DateTimeFormatPartOptionTO(CultureInfo.CurrentCulture.DateTimeFormat.MonthNames[2].Length, CompareTextValueToDateTimePart.IsTextMarch, false, 3, AssignManager.AssignMonths),
                    new DateTimeFormatPartOptionTO(CultureInfo.CurrentCulture.DateTimeFormat.MonthNames[3].Length, CompareTextValueToDateTimePart.IsTextApril, false, 4, AssignManager.AssignMonths),
                    new DateTimeFormatPartOptionTO(CultureInfo.CurrentCulture.DateTimeFormat.MonthNames[4].Length, CompareTextValueToDateTimePart.IsTextMay, false, 5, AssignManager.AssignMonths),
                    new DateTimeFormatPartOptionTO(CultureInfo.CurrentCulture.DateTimeFormat.MonthNames[5].Length, CompareTextValueToDateTimePart.IsTextJune, false, 6, AssignManager.AssignMonths),
                    new DateTimeFormatPartOptionTO(CultureInfo.CurrentCulture.DateTimeFormat.MonthNames[6].Length, CompareTextValueToDateTimePart.IsTextJuly, false, 7, AssignManager.AssignMonths),
                    new DateTimeFormatPartOptionTO(CultureInfo.CurrentCulture.DateTimeFormat.MonthNames[7].Length, CompareTextValueToDateTimePart.IsTextAugust, false, 8, AssignManager.AssignMonths),
                    new DateTimeFormatPartOptionTO(CultureInfo.CurrentCulture.DateTimeFormat.MonthNames[8].Length, CompareTextValueToDateTimePart.IsTextSeptember, false, 9, AssignManager.AssignMonths),
                    new DateTimeFormatPartOptionTO(CultureInfo.CurrentCulture.DateTimeFormat.MonthNames[9].Length, CompareTextValueToDateTimePart.IsTextOctober, false, 10, AssignManager.AssignMonths),
                    new DateTimeFormatPartOptionTO(CultureInfo.CurrentCulture.DateTimeFormat.MonthNames[10].Length, CompareTextValueToDateTimePart.IsTextNovember, false, 11, AssignManager.AssignMonths),
                    new DateTimeFormatPartOptionTO(CultureInfo.CurrentCulture.DateTimeFormat.MonthNames[11].Length, CompareTextValueToDateTimePart.IsTextDecember, false, 12, AssignManager.AssignMonths),
                });
        }

        private static void AddYearParts()
        {
            DateTimeFormatsParts.Add("yy", new DateTimeFormatPartTO("yy", false, "Year in 2 digits: 08"));
            DateTimeFormatPartOptions.Add("yy", new List<IDateTimeFormatPartOptionTO>
            {
                new DateTimeFormatPartOptionTO(2, CompareTextValueToDateTimePart.IsTextNumeric, true, null, AssignManager.AssignYears)
            });

            DateTimeFormatsParts.Add("yyyy", new DateTimeFormatPartTO("yyyy", false, "Year in 4 digits: 2008"));
            DateTimeFormatPartOptions.Add("yyyy",
                new List<IDateTimeFormatPartOptionTO>
                {
                    new DateTimeFormatPartOptionTO(4, CompareTextValueToDateTimePart.IsTextNumeric, true, null, AssignManager.AssignYears)
                });
        }

        /// <summary>
        ///     Creates a list of all valid time format parts
        /// </summary>
        private static void CreateTimeFormatParts()
        {
            TimeFormatPartOptions.Add("yy",
                new List<IDateTimeFormatPartOptionTO>
                {
                    new DateTimeFormatPartOptionTO(2, CompareTextValueToDateTimePart.IsTextNumeric, true, null, AssignManager.AssignYears)
                });

            TimeFormatPartOptions.Add("yyyy",
                new List<IDateTimeFormatPartOptionTO>
                {
                    new DateTimeFormatPartOptionTO(4, CompareTextValueToDateTimePart.IsTextNumeric, true, null, AssignManager.AssignYears)
                });

            TimeFormatPartOptions.Add("mm",
                new List<IDateTimeFormatPartOptionTO>
                {
                    new DateTimeFormatPartOptionTO(2, DatetimeParserHelper.IsNumberMonth, true, null, AssignManager.AssignMonths)
                });

            TimeFormatPartOptions.Add("m",
                new List<IDateTimeFormatPartOptionTO>
                {
                    new DateTimeFormatPartOptionTO(2, DatetimeParserHelper.IsNumberMonth, true, null, AssignManager.AssignMonths),
                    new DateTimeFormatPartOptionTO(1, DatetimeParserHelper.IsNumberMonth, true, null,AssignManager. AssignMonths),
                });

            TimeFormatPartOptions.Add("MM",
                new List<IDateTimeFormatPartOptionTO>
                {
                    new DateTimeFormatPartOptionTO(2, DatetimeParserHelper.IsNumberMonth, true, null, AssignManager.AssignMonths)
                });

            TimeFormatPartOptions.Add("M",
                new List<IDateTimeFormatPartOptionTO>
                {
                    new DateTimeFormatPartOptionTO(2, DatetimeParserHelper.IsNumberMonth, true, null, AssignManager.AssignMonths),
                    new DateTimeFormatPartOptionTO(1, DatetimeParserHelper.IsNumberMonth, true, null, AssignManager.AssignMonths),
                });

            TimeFormatPartOptions.Add("d",
                new List<IDateTimeFormatPartOptionTO>
                {
                    new DateTimeFormatPartOptionTO(2, DatetimeParserHelper.IsNumberDay, true, null, AssignManager.AssignDays),
                    new DateTimeFormatPartOptionTO(1, DatetimeParserHelper.IsNumberDay, true, null, AssignManager.AssignDays)
                });

            TimeFormatPartOptions.Add("dd",
                new List<IDateTimeFormatPartOptionTO>
                {
                    new DateTimeFormatPartOptionTO(2, DatetimeParserHelper.IsNumberDay, true, null, AssignManager.AssignDays)
                });

            TimeFormatPartOptions.Add("DW",
                new List<IDateTimeFormatPartOptionTO>
                {
                    new DateTimeFormatPartOptionTO(1, DatetimeParserHelper.IsNumberDayOfWeek, true, null, AssignManager.AssignDaysOfWeek),
                });

            TimeFormatPartOptions.Add("dW",
                new List<IDateTimeFormatPartOptionTO>
                {
                    new DateTimeFormatPartOptionTO(1, DatetimeParserHelper.IsNumberDayOfWeek, true, null, AssignManager.AssignDaysOfWeek),
                });

            TimeFormatPartOptions.Add("dw",
                new List<IDateTimeFormatPartOptionTO>
                {
                    new DateTimeFormatPartOptionTO(1, DatetimeParserHelper.IsNumberDayOfWeek, true, null, AssignManager.AssignDaysOfWeek),
                });

            TimeFormatPartOptions.Add("dy",
                new List<IDateTimeFormatPartOptionTO>
                {
                    new DateTimeFormatPartOptionTO(3, DatetimeParserHelper.IsNumberDayOfYear, true, null, AssignManager.AssignDaysOfYear),
                    new DateTimeFormatPartOptionTO(2, DatetimeParserHelper.IsNumberDayOfYear, true, null, AssignManager.AssignDaysOfYear),
                    new DateTimeFormatPartOptionTO(1, DatetimeParserHelper.IsNumberDayOfYear, true, null, AssignManager.AssignDaysOfYear)
                });

            TimeFormatPartOptions.Add("w",
                new List<IDateTimeFormatPartOptionTO>
                {
                    new DateTimeFormatPartOptionTO(2, DatetimeParserHelper.IsNumberWeekOfYear, true, null, AssignManager.AssignWeeks),
                    new DateTimeFormatPartOptionTO(1, DatetimeParserHelper.IsNumberWeekOfYear, true, null, AssignManager.AssignWeeks),
                });

            TimeFormatPartOptions.Add("24h",
                new List<IDateTimeFormatPartOptionTO>
                {
                    new DateTimeFormatPartOptionTO(2, DatetimeParserHelper.IsNumber24H, true, null, AssignManager.Assign24Hours)
                });

            TimeFormatPartOptions.Add("12h",
                new List<IDateTimeFormatPartOptionTO>
                {
                    new DateTimeFormatPartOptionTO(2, DatetimeParserHelper.IsNumber12H, true, null, AssignManager.Assign12Hours),
                    new DateTimeFormatPartOptionTO(1, DatetimeParserHelper.IsNumber12H, true, null, AssignManager.Assign12Hours),
                });

            TimeFormatPartOptions.Add("min",
                new List<IDateTimeFormatPartOptionTO>
                {
                    new DateTimeFormatPartOptionTO(2, DatetimeParserHelper.IsNumberMinutes, true, null, AssignManager. AssignMinutes),
                    new DateTimeFormatPartOptionTO(1, DatetimeParserHelper.IsNumberMinutes, true, null, AssignManager. AssignMinutes),
                });

            TimeFormatPartOptions.Add("ss",
                new List<IDateTimeFormatPartOptionTO>
                {
                    new DateTimeFormatPartOptionTO(2, DatetimeParserHelper.IsNumberSeconds, true, null,  AssignManager.AssignSeconds),
                    new DateTimeFormatPartOptionTO(1, DatetimeParserHelper.IsNumberSeconds, true, null,  AssignManager.AssignSeconds),
                });

            TimeFormatPartOptions.Add("sp",
                new List<IDateTimeFormatPartOptionTO>
                {
                    new DateTimeFormatPartOptionTO(3, DatetimeParserHelper.IsNumberMilliseconds, true, null, AssignManager. AssignMilliseconds),
                    new DateTimeFormatPartOptionTO(2, DatetimeParserHelper.IsNumberMilliseconds, true, null, AssignManager. AssignMilliseconds),
                    new DateTimeFormatPartOptionTO(1, DatetimeParserHelper.IsNumberMilliseconds, true, null, AssignManager. AssignMilliseconds),
                });

            TimeFormatPartOptions.Add("am/pm",
                new List<IDateTimeFormatPartOptionTO>
                {
                    new DateTimeFormatPartOptionTO(4, CompareTextValueToDateTimePart.IsTextAmPm, false, null, AssignManager.AssignAmPm),
                    new DateTimeFormatPartOptionTO(3, CompareTextValueToDateTimePart.IsTextAmPm, false, null, AssignManager.AssignAmPm),
                    new DateTimeFormatPartOptionTO(2, CompareTextValueToDateTimePart.IsTextAmPm, false, null, AssignManager.AssignAmPm),
                });

            TimeFormatPartOptions.Add("Z", TimeZones.Select(k =>
            {
                IDateTimeFormatPartOptionTO dateTimeFormatPartOptionTo = new DateTimeFormatPartOptionTO(k.Key.Length, CompareTextValueToDateTimePart.IsTextTimeZone, false, null, AssignTimeZone);
                return dateTimeFormatPartOptionTo;
            }).OrderByDescending(k => k.Length).ToList());

            TimeFormatPartOptions.Add("ZZ", TimeZones.Select(k =>
            {
                IDateTimeFormatPartOptionTO dateTimeFormatPartOptionTo = new DateTimeFormatPartOptionTO(k.Key.Length, CompareTextValueToDateTimePart.IsTextTimeZone, false, null, AssignTimeZone);
                return dateTimeFormatPartOptionTo;
            }).OrderByDescending(k => k.Length).ToList());

            TimeFormatPartOptions.Add("ZZZ", TimeZones.Select(k =>
            {
                IDateTimeFormatPartOptionTO dateTimeFormatPartOptionTo = new DateTimeFormatPartOptionTO(k.Key.Length, CompareTextValueToDateTimePart.IsTextTimeZone, false, null, AssignTimeZone);
                return dateTimeFormatPartOptionTo;
            }).OrderByDescending(k => k.Length).ToList());

            TimeFormatPartOptions.Add("era",
                new List<IDateTimeFormatPartOptionTO>
                {
                    new DateTimeFormatPartOptionTO(2, (data, treatAsTim) => data.ToLower().Equals("ad"), false, "AD",
                        AssignManager.AssignEra)
                });

            TimeFormatPartOptions.Add("Era",
                new List<IDateTimeFormatPartOptionTO>
                {
                    new DateTimeFormatPartOptionTO(3, (data, treatAsTim) => data.ToLower().Equals("a.d"), false, "A.D",
                        AssignManager.AssignEra)
                });

            TimeFormatPartOptions.Add("ERA",
                new List<IDateTimeFormatPartOptionTO>
                {
                    new DateTimeFormatPartOptionTO(4, (data, treatAsTim) => data.ToLower().Equals("a.d."), false, "A.D.",
                        AssignManager.AssignEra)
                });
        }

        /// <summary>
        ///     Creates a list of all valid time zones
        /// </summary>
        private static void CreateTimeZones()
        {
            //
            // Create UCT entries
            //
            const string UctShort = "UCT";
            const string UctLong = "Coordinated Universal Time";
            TimeZones.Add(UctShort.ToLower(), new TimeZoneTO(UctShort, UctShort, UctLong));
            TimeZones.Add(UctLong.ToLower(), new TimeZoneTO(UctShort, UctShort, UctLong));

            for (int hours = -12; hours < 13; hours++)
            {
                if (hours != 0)
                {
                    for (int minutes = 0; minutes < 2; minutes++)
                    {
                        string min = minutes == 0 ? "00" : "30";
                        string hrs = string.Concat(hours / Math.Abs(hours) < 0 ? "-" : "+",
                            Math.Abs(hours).ToString(CultureInfo.InvariantCulture).PadLeft(2, '0'));
                        string uct = string.Concat(UctShort, hrs, ":", min);
                        TimeZones.Add(uct.ToLower(), new TimeZoneTO(UctShort, uct, UctLong));
                    }
                }
                else
                {
                    TimeZones.Add(UctShort + "-00:30", new TimeZoneTO(UctShort, UctShort + "-00:30", UctLong));
                    TimeZones.Add(UctShort + "+00:30", new TimeZoneTO(UctShort, UctShort + "+00:30", UctLong));
                }
            }

            //
            // Create GMT entries
            //
            const string GmtShort = "GMT";
            const string GmtLong = "Greenwich Mean Time";
            TimeZones.Add(GmtShort.ToLower(), new TimeZoneTO(GmtShort, GmtShort, GmtLong));
            TimeZones.Add(GmtLong.ToLower(), new TimeZoneTO(GmtShort, GmtShort, GmtLong));

            for (int hours = -12; hours < 13; hours++)
            {
                if (hours != 0)
                {
                    for (int minutes = 0; minutes < 2; minutes++)
                    {
                        string min = minutes == 0 ? "00" : "30";
                        string hrs = string.Concat(hours / Math.Abs(hours) < 0 ? "-" : "+",
                            Math.Abs(hours).ToString(CultureInfo.InvariantCulture).PadLeft(2, '0'));
                        string gmt = string.Concat(GmtShort, hrs, ":", min);
                        TimeZones.Add(gmt.ToLower(), new TimeZoneTO(GmtShort, gmt, GmtLong));
                    }
                }
                else
                {
                    TimeZones.Add(GmtShort + "-00:30", new TimeZoneTO(GmtShort, GmtShort + "-00:30", GmtLong));
                    TimeZones.Add(GmtShort + "+00:30", new TimeZoneTO(GmtShort, GmtShort + "+00:30", GmtLong));
                }
            }

            //
            // Read in system time zones
            //
            foreach (TimeZoneInfo timeZoneInfo in TimeZoneInfo.GetSystemTimeZones())
            {
                ITimeZoneTO timeZoneTo;
                if (!TimeZones.TryGetValue(timeZoneInfo.DisplayName.ToLower(), out timeZoneTo))
                {
                    TimeZones.Add(timeZoneInfo.DisplayName.ToLower(),
                        new TimeZoneTO(timeZoneInfo.StandardName, timeZoneInfo.StandardName, timeZoneInfo.DisplayName));
                }

                if (!TimeZones.TryGetValue(timeZoneInfo.DaylightName.ToLower(), out timeZoneTo))
                {
                    TimeZones.Add(timeZoneInfo.DaylightName.ToLower(),
                        new TimeZoneTO(timeZoneInfo.DaylightName, timeZoneInfo.DaylightName, timeZoneInfo.DisplayName));
                }

                if (!TimeZones.TryGetValue(timeZoneInfo.StandardName.ToLower(), out timeZoneTo))
                {
                    TimeZones.Add(timeZoneInfo.StandardName.ToLower(),
                        new TimeZoneTO(timeZoneInfo.StandardName, timeZoneInfo.StandardName, timeZoneInfo.DisplayName));
                }
            }
        }

        //Ashley make a .net version of this method
        /// <summary>
        ///     Creates forward lookup information for date time parts
        /// </summary>
        private static void CreateDateTimeFormatForwardLookupsForDotNet()
        {
            //
            // Lookups for dddd, ddd, dd, d
            //
            DateTimeFormatForwardLookupsForDotNet.Add('d', new List<int> { 4, 3, 2, 1 });

            //
            // Lookups for fffffff, ffffff, fffff, ffff, fff, ff, f
            //
            DateTimeFormatForwardLookupsForDotNet.Add('f', new List<int> { 7, 6, 5, 4, 3, 2, 1 });

            //
            // Lookups for FFFFFFF, FFFFFF, FFFFF, FFFF, FFF, FF, F
            //
            DateTimeFormatForwardLookupsForDotNet.Add('F', new List<int> { 7, 6, 5, 4, 3, 2, 1 });

            //
            // Lookups for gg, g
            //
            DateTimeFormatForwardLookupsForDotNet.Add('g', new List<int> { 2, 1 });

            //
            // Lookups for hh, h
            //
            DateTimeFormatForwardLookupsForDotNet.Add('h', new List<int> { 2, 1 });

            //
            // Lookups for HH, H
            //
            DateTimeFormatForwardLookupsForDotNet.Add('H', new List<int> { 2, 1 });

            //
            // Lookups for K
            //
            DateTimeFormatForwardLookupsForDotNet.Add('K', new List<int> { 1 });

            //
            // Lookups for mm, m
            //
            DateTimeFormatForwardLookupsForDotNet.Add('m', new List<int> { 2, 1 });

            //
            // Lookups for MMMM, MMM, MM, M
            //
            DateTimeFormatForwardLookupsForDotNet.Add('M', new List<int> { 4, 3, 2, 1 });

            //
            // Lookups for ss, s
            //
            DateTimeFormatForwardLookupsForDotNet.Add('s', new List<int> { 2, 1 });

            //
            // Lookups for tt, t
            //
            DateTimeFormatForwardLookupsForDotNet.Add('t', new List<int> { 2, 1 });

            //
            // Lookups for yyyyy, yyyy, yyy, yy, y
            //
            DateTimeFormatForwardLookupsForDotNet.Add('y', new List<int> { 5, 4, 3, 2, 1 });

            //
            // Lookups for zzz, zz, z
            //
            DateTimeFormatForwardLookupsForDotNet.Add('z', new List<int> { 3, 2, 1 });
        }

        /// <summary>
        ///     Creates forward lookup information for date time parts
        /// </summary>
        private static void CreateDateTimeFormatForwardLookups()
        {
            //
            // Lookups for yyyyy, yyyy, yyy, yy and y
            //
            DateTimeFormatForwardLookups.Add('y', new List<int> { 5, 4, 3, 2, 1 });

            //
            // Lookups for min, mm and m
            //
            DateTimeFormatForwardLookups.Add('m', new List<int> { 3, 2, 1 });

            //
            // Lookups for MM and M
            //
            DateTimeFormatForwardLookups.Add('M', new List<int> { 2, 1 });

            //
            // Lookups for dd, dW, dw, dy and d
            //
            DateTimeFormatForwardLookups.Add('d', new List<int> { 2, 1 });

            //
            // Lookups for w
            //
            DateTimeFormatForwardLookups.Add('w', new List<int> { 2, 1 });

            //
            // Lookups for DW
            //
            DateTimeFormatForwardLookups.Add('D', new List<int> { 2 });

            //
            // Lookups for 24h
            //
            DateTimeFormatForwardLookups.Add('2', new List<int> { 3 });

            //
            // Lookups for 12h
            //
            DateTimeFormatForwardLookups.Add('1', new List<int> { 3 });

            //
            // Lookups for ss and sp
            //
            DateTimeFormatForwardLookups.Add('s', new List<int> { 2 });

            //
            // Lookups for am/pm
            //
            DateTimeFormatForwardLookups.Add('a', new List<int> { 5 });

            //
            // Lookups for ZZZ, ZZ and Z
            //
            DateTimeFormatForwardLookups.Add('Z', new List<int> { 3, 2, 1 });

            //
            // Lookups for Era
            //
            DateTimeFormatForwardLookups.Add('E', new List<int> { 3 });
            DateTimeFormatForwardLookups.Add('e', new List<int> { 3 });
        }


        private static void AssignTimeZone(IDateTimeResultTO dateTimeResultTo, bool assignAsTime, IConvertible value)
        {
            string lowerValue = value.ToString(CultureInfo.InvariantCulture).ToLower();

            ITimeZoneTO timeZoneTo;
            if (TimeZones.TryGetValue(lowerValue, out timeZoneTo))
            {
                dateTimeResultTo.TimeZone = timeZoneTo;
            }
        }
        public List<IDateTimeFormatPartTO> DateTimeFormatParts => DateTimeFormatsParts.Values.ToList();

    }
}



