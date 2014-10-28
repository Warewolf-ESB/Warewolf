/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Dev2.Common;
using Dev2.Common.Interfaces.Core.Convertors.DateAndTime;
using Dev2.Converters.DateAndTime.TO;

namespace Dev2.Converters.DateAndTime
{
    public class DateTimeParser : IDateTimeParser
    {
        #region Private Enums

        /// <summary>
        ///     used to describe the position of the parser relative to escaped regions
        /// </summary>
        private enum LiteralRegionStates
        {
            OutsideLiteralRegion,
            InsideLiteralRegion,
            InsideLiteralRegionWithEscape,
            InsideInferredLiteralRegion,
            InsideInferredLiteralRegionWithEscape,
        }

        #endregion Private Enums

        #region Class Members

        private const char DateLiteralCharacter = '\'';
        private const char TimeLiteralCharacter = ':';
        private const char EscapeCharacter = '\\';

        private static readonly Dictionary<char, List<int>> DateTimeFormatForwardLookups =
            new Dictionary<char, List<int>>();

        private static readonly Dictionary<string, IDateTimeFormatPartTO> DateTimeFormatsParts =
            new Dictionary<string, IDateTimeFormatPartTO>();

        private static readonly Dictionary<string, List<IDateTimeFormatPartOptionTO>> DateTimeFormatPartOptions =
            new Dictionary<string, List<IDateTimeFormatPartOptionTO>>();

        private static readonly Dictionary<string, List<IDateTimeFormatPartOptionTO>> TimeFormatPartOptions =
            new Dictionary<string, List<IDateTimeFormatPartOptionTO>>();

        private static readonly Dictionary<string, ITimeZoneTO> TimeZones = new Dictionary<string, ITimeZoneTO>();

        private static readonly Dictionary<string, IDateTimeFormatPartTO> DateTimeFormatPartsForDotNet =
            new Dictionary<string, IDateTimeFormatPartTO>();

        private static readonly Dictionary<string, List<IDateTimeFormatPartOptionTO>> DateTimeFormatPartOptionsForDotNet
            = new Dictionary<string, List<IDateTimeFormatPartOptionTO>>();

        private static readonly Dictionary<char, List<int>> DateTimeFormatForwardLookupsForDotNet =
            new Dictionary<char, List<int>>();

        #endregion Class Members

        #region Constructors

        static DateTimeParser()
        {
            CreateTimeZones();
            CreateDateTimeFormatForwardLookups();
            CreateDateTimeFormatParts();
            CreateTimeFormatParts();
            CreateDateTimeFormatForwardLookupsForDotNet();
            CreateDateTimeFormatPartsForDotNet();
        }

        #endregion Constructors

        #region Methods

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
            //
            // Get input format parts for the dotnet format
            //
            List<IDateTimeFormatPartTO> dotNetFormatParts;
            TryGetDateTimeFormatParts(originalFormat, DateTimeFormatForwardLookupsForDotNet,
                DateTimeFormatPartOptionsForDotNet, out dotNetFormatParts, out error);

            //
            // Replace input format parts with Dev2 equivilents
            //
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

        #endregion Methods

        #region Internal Methods

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
        internal static bool TryGetDateTimeFormatParts(string format,
            Dictionary<char, List<int>> dateTimeFormatForwardLookups,
            Dictionary<string, List<IDateTimeFormatPartOptionTO>> dateTimeFormatPartOptions,
            out List<IDateTimeFormatPartTO> formatParts, out string error)
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
                    string tmpForwardLookupResult;
                    if (currentChar == DateLiteralCharacter &&
                        CheckForDoubleEscapedLiteralCharacter(formatArray, count, out tmpForwardLookupResult, out error))
                    {
                        forwardLookupLength = tmpForwardLookupResult.Length;
                        literalRegionState = LiteralRegionStates.InsideInferredLiteralRegion;
                        currentValue += currentChar;
                    }
                    else if (currentChar == DateLiteralCharacter)
                    {
                        literalRegionState = LiteralRegionStates.InsideLiteralRegion;
                    }
                    else if (TryGetDateTimeFormatPart(formatArray, count, currentChar, dateTimeFormatForwardLookups,
                        dateTimeFormatPartOptions, out currentValue, out error))
                    {
                        forwardLookupLength = currentValue.Length;
                        formatParts.Add(new DateTimeFormatPartTO(currentValue, false, ""));
                        currentValue = "";
                    }
                    else
                    {
                        error = "";
                        literalRegionState = LiteralRegionStates.InsideInferredLiteralRegion;
                        currentValue += currentChar;
                    }
                }
                else if (literalRegionState == LiteralRegionStates.InsideInferredLiteralRegion)
                {
                    string tmpCurrentValue;
                    string tmpForwardLookupResult;
                    if (currentChar == DateLiteralCharacter &&
                        CheckForDoubleEscapedLiteralCharacter(formatArray, count, out tmpForwardLookupResult, out error))
                    {
                        forwardLookupLength = tmpForwardLookupResult.Length;
                        currentValue += currentChar;
                    }
                    else if (currentChar == DateLiteralCharacter)
                    {
                        literalRegionState = LiteralRegionStates.InsideLiteralRegion;
                        formatParts.Add(new DateTimeFormatPartTO(currentValue, true, ""));
                        currentValue = "";
                    }
                    else if (currentChar == EscapeCharacter)
                    {
                        literalRegionState = LiteralRegionStates.InsideInferredLiteralRegionWithEscape;
                    }
                    else if (TryGetDateTimeFormatPart(formatArray, count, currentChar, dateTimeFormatForwardLookups,
                        dateTimeFormatPartOptions, out tmpCurrentValue, out error))
                    {
                        literalRegionState = LiteralRegionStates.OutsideLiteralRegion;
                        forwardLookupLength = tmpCurrentValue.Length;
                        formatParts.Add(new DateTimeFormatPartTO(currentValue, true, ""));
                        formatParts.Add(new DateTimeFormatPartTO(tmpCurrentValue, false, ""));
                        currentValue = "";
                    }
                    else
                    {
                        error = "";
                        currentValue += currentChar;
                    }
                }
                else if (literalRegionState == LiteralRegionStates.InsideInferredLiteralRegionWithEscape)
                {
                    if (currentChar == DateLiteralCharacter || currentChar == EscapeCharacter)
                    {
                        literalRegionState = LiteralRegionStates.InsideInferredLiteralRegion;
                        currentValue += currentChar;
                    }
                    else
                    {
                        nothingDied = false;
                        error = "A \'\\\' character must be followed by a \' or preceeded by a \\.";
                    }
                }
                else if (literalRegionState == LiteralRegionStates.InsideLiteralRegion)
                {
                    string tmpForwardLookupResult;
                    if (currentChar == DateLiteralCharacter &&
                        CheckForDoubleEscapedLiteralCharacter(formatArray, count, out tmpForwardLookupResult, out error))
                    {
                        forwardLookupLength = tmpForwardLookupResult.Length;
                        currentValue += currentChar;
                    }
                    else if (currentChar == DateLiteralCharacter)
                    {
                        literalRegionState = LiteralRegionStates.OutsideLiteralRegion;
                        formatParts.Add(new DateTimeFormatPartTO(currentValue, true, ""));
                        currentValue = "";
                    }
                    else if (currentChar == EscapeCharacter)
                    {
                        literalRegionState = LiteralRegionStates.InsideLiteralRegionWithEscape;
                    }
                    else
                    {
                        currentValue += currentChar;
                    }
                }
                else if (literalRegionState == LiteralRegionStates.InsideLiteralRegionWithEscape)
                {
                    if (currentChar == DateLiteralCharacter || currentChar == EscapeCharacter)
                    {
                        literalRegionState = LiteralRegionStates.InsideLiteralRegion;
                        currentValue += currentChar;
                    }
                    else
                    {
                        nothingDied = false;
                        error = "A \'\\\' character must be followed by a \' or preceeded by a \\.";
                    }
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
                error =
                    "A \' character defines a start or end of a non date time region, there apears to be a extra \' character.";
            }

            return nothingDied;
        }

        internal static int GetDayOfWeekInt(DayOfWeek dayOfWeek)
        {
            int val;
            if (dayOfWeek == DayOfWeek.Sunday)
            {
                val = 7;
            }
            else
            {
                val = (int) dayOfWeek;
            }

            return val;
        }

        #endregion Internal Methods

        #region Private Methods

        /// <summary>
        ///     Parses the given data using the specified format
        /// </summary>
        private bool TryParse(string data, string inputFormat, bool parseAsTime, out IDateTimeResultTO result,
            out string error)
        {
            bool nothingDied = true;

            result = new DateTimeResultTO();
            error = "";

            //2013.05.03: Ashley Lewis - Bug 9300 try invariant culture
            int culturesTried = 0;
            const int MaxAttempts = 8;
            if (string.IsNullOrWhiteSpace(data))
            {
                //2013.07.24: Ashley Lewis for PBI 10028 - null to default
                data = DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToLongTimeString();
            }

            if (string.IsNullOrWhiteSpace(inputFormat))
            {
                //07.03.2013: Ashley Lewis for Bug 9167 - null to default
                inputFormat =
                    TranslateDotNetToDev2Format(
                        GlobalConstants.Dev2DotNetDefaultDateTimeFormat.Replace("ss", "ss.fff"), out error);
            }
            else
            {
                //never try invariant culture if the user set the input format
                culturesTried = MaxAttempts;
            }
            //2013.05.03: Ashley Lewis - Bug 9300 try invariant culture
            while (culturesTried <= MaxAttempts)
            {
                char[] dateTimeArray = data.ToArray();
                List<IDateTimeFormatPartTO> formatParts;
                int position = 0;

                //
                // Get input format parts
                //
                nothingDied = TryGetDateTimeFormatParts(inputFormat, DateTimeFormatForwardLookups,
                    DateTimeFormatPartOptions, out formatParts, out error);
                if (!string.IsNullOrEmpty(error))
                {
                    return false;
                }
                if (nothingDied)
                {
                    //
                    // Extract information from the dateTime string for every part
                    //
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
                    //2013.05.03: Ashley Lewis - Bug 9300 try other cultures and patterns (be very lenient if the user left input format blank)
                    if (!nothingDied)
                    {
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
                                error =
                                    "Could not parse input datetime with given input format (even after trying default datetime formats from other cultures)";
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

        public static bool IsBlankResult(IDateTimeResultTO result)
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
        private static bool TryGetDataFromDateTime(char[] dateTimeArray, int startPosition, IDateTimeFormatPartTO part,
            IDateTimeResultTO result, bool passAsTime, out int resultLength, out string error)
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
                        error = string.Concat("Unrecognised format part '", part.Value, "'.");
                    }
                }
                else
                {
                    if (!DateTimeFormatPartOptions.TryGetValue(part.Value, out partOptions))
                    {
                        nothingDied = false;
                        error = string.Concat("Unrecognised format part '", part.Value, "'.");
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

                        //Added by Massimo.Guerrera - For Bug 9494 to let the input format to be more forgiving
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

                            //
                            // Assign the value to the result
                            //
                            if (partOption.AssignAction != null)
                            {
                                partOption.AssignAction(result, passAsTime, value);
                            }
                        }

                        partOptionsCount++;
                    }

                    //
                    // If no viable data was found set error
                    //
                    if (!dataFound)
                    {
                        nothingDied = false;
                        error = string.Concat("Unexpected value at index ", startPosition, ".");
                    }
                }
            }

            return nothingDied;
        }

        /// <summary>
        ///     Performs a forward lookup for the given forwardLookupIndex and checks is the result is a double escaped literal
        ///     character.
        /// </summary>
        private static bool CheckForDoubleEscapedLiteralCharacter(char[] formatArray, int startPosition,
            out string result, out string error)
        {
            error = "";
            //2013.06.03: Ashley Lewis for bug 9601 - reduced forward look up from 3 to 2
            result = ForwardLookup(formatArray, startPosition, 2);
            return (result == "''");
        }

        /// <summary>
        ///     Performs a forward lookup for the given forwardLookupIndex and checks the results against known
        ///     date time format parts. Returns true if part is found otherwise false.
        /// </summary>
        private static bool TryGetDateTimeFormatPart(char[] formatArray, int startPosition, char forwardLookupIndex,
            Dictionary<char, List<int>> dateTimeFormatForwardLookups,
            Dictionary<string, List<IDateTimeFormatPartOptionTO>> dateTimeFormatPartOptions, out string result,
            out string error)
        {
            bool nothingDied = true;

            error = "";
            result = "";

            List<int> lookupLengths;
            if (dateTimeFormatForwardLookups.TryGetValue(forwardLookupIndex, out lookupLengths))
            {
                //
                // Perform all forward lookups
                //
                List<string> lookupResults =
                    lookupLengths.Select(i => ForwardLookup(formatArray, startPosition, i)).ToList();

                int count = 0;
                while (count < lookupResults.Count && nothingDied)
                {
                    //
                    // Check if forward lookup result is a known date time format part
                    //
                    List<IDateTimeFormatPartOptionTO> tmp;
                    if (dateTimeFormatPartOptions.TryGetValue(lookupResults[count], out tmp))
                    {
                        result = lookupResults[count];
                        count = lookupResults.Count;
                    }
                    else if (count == lookupLengths.Count - 1)
                    {
                        nothingDied = false;
                        error =
                            string.Concat("Failed to find any format part matches in forward lookups from character '",
                                forwardLookupIndex, "' at index ", startPosition, " of format.");
                    }

                    count++;
                }
            }
            else
            {
                nothingDied = false;
                error = string.Concat("Unexpected character at index ", startPosition, " of format.");
            }

            return nothingDied;
        }

        /// <summary>
        ///     Does a forward lookup on the given array and returns the resulting string
        /// </summary>
        private static string ForwardLookup(char[] formatArray, int startPosition, int lookupLength)
        {
            string result = "";
            int position = startPosition;

            while (position >= 0 && position < formatArray.Length && position < (startPosition + lookupLength))
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
            DateTimeFormatsParts.Add("yy", new DateTimeFormatPartTO("yy", false, "Year in 2 digits: 08"));
            DateTimeFormatPartOptions.Add("yy",
                new List<IDateTimeFormatPartOptionTO>
                {
                    new DateTimeFormatPartOptionTO(2, IsTextNumeric, true, null, AssignYears)
                });

            DateTimeFormatsParts.Add("yyyy", new DateTimeFormatPartTO("yyyy", false, "Year in 4 digits: 2008"));
            DateTimeFormatPartOptions.Add("yyyy",
                new List<IDateTimeFormatPartOptionTO>
                {
                    new DateTimeFormatPartOptionTO(4, IsTextNumeric, true, null, AssignYears)
                });

            DateTimeFormatsParts.Add("mm", new DateTimeFormatPartTO("mm", false, "Month in 2 digits: 03"));
            DateTimeFormatPartOptions.Add("mm",
                new List<IDateTimeFormatPartOptionTO>
                {
                    new DateTimeFormatPartOptionTO(2, IsNumberMonth, true, null, AssignMonths)
                });

            DateTimeFormatsParts.Add("m", new DateTimeFormatPartTO("m", false, "Month in single digit: 3"));
            DateTimeFormatPartOptions.Add("m",
                new List<IDateTimeFormatPartOptionTO>
                {
                    new DateTimeFormatPartOptionTO(2, IsNumberMonth, true, null, AssignMonths),
                    new DateTimeFormatPartOptionTO(1, IsNumberMonth, true, null, AssignMonths),
                });

            DateTimeFormatsParts.Add("M", new DateTimeFormatPartTO("M", false, "Month text abbreviated: Mar"));
            DateTimeFormatPartOptions.Add("M",
                new List<IDateTimeFormatPartOptionTO>
                {
                    new DateTimeFormatPartOptionTO(
                        CultureInfo.CurrentCulture.DateTimeFormat.AbbreviatedMonthNames[0].Length, IsTextJanuary, false,
                        1, AssignMonths),
                    new DateTimeFormatPartOptionTO(
                        CultureInfo.CurrentCulture.DateTimeFormat.AbbreviatedMonthNames[1].Length, IsTextFebuary, false,
                        2, AssignMonths),
                    new DateTimeFormatPartOptionTO(
                        CultureInfo.CurrentCulture.DateTimeFormat.AbbreviatedMonthNames[2].Length, IsTextMarch, false, 3,
                        AssignMonths),
                    new DateTimeFormatPartOptionTO(
                        CultureInfo.CurrentCulture.DateTimeFormat.AbbreviatedMonthNames[3].Length, IsTextApril, false, 4,
                        AssignMonths),
                    new DateTimeFormatPartOptionTO(
                        CultureInfo.CurrentCulture.DateTimeFormat.AbbreviatedMonthNames[4].Length, IsTextMay, false, 5,
                        AssignMonths),
                    new DateTimeFormatPartOptionTO(
                        CultureInfo.CurrentCulture.DateTimeFormat.AbbreviatedMonthNames[5].Length, IsTextJune, false, 6,
                        AssignMonths),
                    new DateTimeFormatPartOptionTO(
                        CultureInfo.CurrentCulture.DateTimeFormat.AbbreviatedMonthNames[6].Length, IsTextJuly, false, 7,
                        AssignMonths),
                    new DateTimeFormatPartOptionTO(
                        CultureInfo.CurrentCulture.DateTimeFormat.AbbreviatedMonthNames[7].Length, IsTextAugust, false,
                        8, AssignMonths),
                    new DateTimeFormatPartOptionTO(
                        CultureInfo.CurrentCulture.DateTimeFormat.AbbreviatedMonthNames[8].Length, IsTextSeptember,
                        false, 9, AssignMonths),
                    new DateTimeFormatPartOptionTO(
                        CultureInfo.CurrentCulture.DateTimeFormat.AbbreviatedMonthNames[9].Length, IsTextOctober, false,
                        10, AssignMonths),
                    new DateTimeFormatPartOptionTO(
                        CultureInfo.CurrentCulture.DateTimeFormat.AbbreviatedMonthNames[10].Length, IsTextNovember,
                        false, 11, AssignMonths),
                    new DateTimeFormatPartOptionTO(
                        CultureInfo.CurrentCulture.DateTimeFormat.AbbreviatedMonthNames[11].Length, IsTextDecember,
                        false, 12, AssignMonths),
                });

            DateTimeFormatsParts.Add("MM", new DateTimeFormatPartTO("MM", false, "Month text in full: March"));
            DateTimeFormatPartOptions.Add("MM",
                new List<IDateTimeFormatPartOptionTO>
                {
                    new DateTimeFormatPartOptionTO(CultureInfo.CurrentCulture.DateTimeFormat.MonthNames[0].Length,
                        IsTextJanuary, false, 1, AssignMonths),
                    new DateTimeFormatPartOptionTO(CultureInfo.CurrentCulture.DateTimeFormat.MonthNames[1].Length,
                        IsTextFebuary, false, 2, AssignMonths),
                    new DateTimeFormatPartOptionTO(CultureInfo.CurrentCulture.DateTimeFormat.MonthNames[2].Length,
                        IsTextMarch, false, 3, AssignMonths),
                    new DateTimeFormatPartOptionTO(CultureInfo.CurrentCulture.DateTimeFormat.MonthNames[3].Length,
                        IsTextApril, false, 4, AssignMonths),
                    new DateTimeFormatPartOptionTO(CultureInfo.CurrentCulture.DateTimeFormat.MonthNames[4].Length,
                        IsTextMay, false, 5, AssignMonths),
                    new DateTimeFormatPartOptionTO(CultureInfo.CurrentCulture.DateTimeFormat.MonthNames[5].Length,
                        IsTextJune, false, 6, AssignMonths),
                    new DateTimeFormatPartOptionTO(CultureInfo.CurrentCulture.DateTimeFormat.MonthNames[6].Length,
                        IsTextJuly, false, 7, AssignMonths),
                    new DateTimeFormatPartOptionTO(CultureInfo.CurrentCulture.DateTimeFormat.MonthNames[7].Length,
                        IsTextAugust, false, 8, AssignMonths),
                    new DateTimeFormatPartOptionTO(CultureInfo.CurrentCulture.DateTimeFormat.MonthNames[8].Length,
                        IsTextSeptember, false, 9, AssignMonths),
                    new DateTimeFormatPartOptionTO(CultureInfo.CurrentCulture.DateTimeFormat.MonthNames[9].Length,
                        IsTextOctober, false, 10, AssignMonths),
                    new DateTimeFormatPartOptionTO(CultureInfo.CurrentCulture.DateTimeFormat.MonthNames[10].Length,
                        IsTextNovember, false, 11, AssignMonths),
                    new DateTimeFormatPartOptionTO(CultureInfo.CurrentCulture.DateTimeFormat.MonthNames[11].Length,
                        IsTextDecember, false, 12, AssignMonths),
                });

            DateTimeFormatsParts.Add("d", new DateTimeFormatPartTO("d", false, "Day of month in single digit: 6"));
            DateTimeFormatPartOptions.Add("d",
                new List<IDateTimeFormatPartOptionTO>
                {
                    new DateTimeFormatPartOptionTO(2, IsNumberDay, true, null, AssignDays),
                    new DateTimeFormatPartOptionTO(1, IsNumberDay, true, null, AssignDays)
                });

            DateTimeFormatsParts.Add("dd", new DateTimeFormatPartTO("dd", false, "Day of month in 2 digits: 06"));
            DateTimeFormatPartOptions.Add("dd",
                new List<IDateTimeFormatPartOptionTO>
                {
                    new DateTimeFormatPartOptionTO(2, IsNumberDay, true, null, AssignDays)
                });

            DateTimeFormatsParts.Add("DW", new DateTimeFormatPartTO("DW", false, "Day of Week in full: Thursday"));
            DateTimeFormatPartOptions.Add("DW",
                new List<IDateTimeFormatPartOptionTO>
                {
                    new DateTimeFormatPartOptionTO(CultureInfo.CurrentCulture.DateTimeFormat.DayNames[0].Length,
                        IsTextSunday, false, 7, AssignDaysOfWeek),
                    new DateTimeFormatPartOptionTO(CultureInfo.CurrentCulture.DateTimeFormat.DayNames[1].Length,
                        IsTextMonday, false, 1, AssignDaysOfWeek),
                    new DateTimeFormatPartOptionTO(CultureInfo.CurrentCulture.DateTimeFormat.DayNames[2].Length,
                        IsTextTuesday, false, 2, AssignDaysOfWeek),
                    new DateTimeFormatPartOptionTO(CultureInfo.CurrentCulture.DateTimeFormat.DayNames[3].Length,
                        IsTextWednesday, false, 3, AssignDaysOfWeek),
                    new DateTimeFormatPartOptionTO(CultureInfo.CurrentCulture.DateTimeFormat.DayNames[4].Length,
                        IsTextThursday, false, 4, AssignDaysOfWeek),
                    new DateTimeFormatPartOptionTO(CultureInfo.CurrentCulture.DateTimeFormat.DayNames[5].Length,
                        IsTextFriday, false, 5, AssignDaysOfWeek),
                    new DateTimeFormatPartOptionTO(CultureInfo.CurrentCulture.DateTimeFormat.DayNames[6].Length,
                        IsTextSaturday, false, 6, AssignDaysOfWeek),
                });

            DateTimeFormatsParts.Add("dW", new DateTimeFormatPartTO("dW", false, "Day of Week text abbreviated: Thu"));
            DateTimeFormatPartOptions.Add("dW",
                new List<IDateTimeFormatPartOptionTO>
                {
                    new DateTimeFormatPartOptionTO(
                        CultureInfo.CurrentCulture.DateTimeFormat.AbbreviatedDayNames[0].Length, IsTextSunday, false, 7,
                        AssignDaysOfWeek, 6),
                    new DateTimeFormatPartOptionTO(
                        CultureInfo.CurrentCulture.DateTimeFormat.AbbreviatedDayNames[1].Length, IsTextMonday, false, 1,
                        AssignDaysOfWeek, 6),
                    new DateTimeFormatPartOptionTO(
                        CultureInfo.CurrentCulture.DateTimeFormat.AbbreviatedDayNames[2].Length, IsTextTuesday, false, 2,
                        AssignDaysOfWeek, 7),
                    new DateTimeFormatPartOptionTO(
                        CultureInfo.CurrentCulture.DateTimeFormat.AbbreviatedDayNames[3].Length, IsTextWednesday, false,
                        3, AssignDaysOfWeek, 9),
                    new DateTimeFormatPartOptionTO(
                        CultureInfo.CurrentCulture.DateTimeFormat.AbbreviatedDayNames[4].Length, IsTextThursday, false,
                        4, AssignDaysOfWeek, 8),
                    new DateTimeFormatPartOptionTO(
                        CultureInfo.CurrentCulture.DateTimeFormat.AbbreviatedDayNames[5].Length, IsTextFriday, false, 5,
                        AssignDaysOfWeek, 6),
                    new DateTimeFormatPartOptionTO(
                        CultureInfo.CurrentCulture.DateTimeFormat.AbbreviatedDayNames[6].Length, IsTextSaturday, false,
                        6, AssignDaysOfWeek, 7),
                });

            DateTimeFormatsParts.Add("dw", new DateTimeFormatPartTO("dw", false, "Day of Week number: 4"));
            DateTimeFormatPartOptions.Add("dw",
                new List<IDateTimeFormatPartOptionTO>
                {
                    new DateTimeFormatPartOptionTO(1, IsNumberDayOfWeek, true, null, AssignDaysOfWeek),
                });

            DateTimeFormatsParts.Add("dy", new DateTimeFormatPartTO("dy", false, "Day of year: 66"));
            DateTimeFormatPartOptions.Add("dy",
                new List<IDateTimeFormatPartOptionTO>
                {
                    new DateTimeFormatPartOptionTO(3, IsNumberDayOfYear, true, null, AssignDaysOfYear),
                    new DateTimeFormatPartOptionTO(2, IsNumberDayOfYear, true, null, AssignDaysOfYear),
                    new DateTimeFormatPartOptionTO(1, IsNumberDayOfYear, true, null, AssignDaysOfYear)
                });

            DateTimeFormatsParts.Add("ww", new DateTimeFormatPartTO("ww", false, "Week of year: 09"));
            DateTimeFormatPartOptions.Add("ww",
                new List<IDateTimeFormatPartOptionTO>
                {
                    new DateTimeFormatPartOptionTO(2, IsNumberWeekOfYear, true, null, AssignWeeks),
                });

            DateTimeFormatsParts.Add("w", new DateTimeFormatPartTO("w", false, "Week of year: 9"));
            DateTimeFormatPartOptions.Add("w",
                new List<IDateTimeFormatPartOptionTO>
                {
                    new DateTimeFormatPartOptionTO(2, IsNumberWeekOfYear, true, null, AssignWeeks),
                    new DateTimeFormatPartOptionTO(1, IsNumberWeekOfYear, true, null, AssignWeeks),
                });

            DateTimeFormatsParts.Add("24h", new DateTimeFormatPartTO("24h", false, "Hours in 24 hour format: 15"));
            DateTimeFormatPartOptions.Add("24h",
                new List<IDateTimeFormatPartOptionTO>
                {
                    new DateTimeFormatPartOptionTO(2, IsNumber24H, true, null, Assign24Hours)
                });

            DateTimeFormatsParts.Add("12h", new DateTimeFormatPartTO("12h", false, "Hours in 12 hour format: 3"));
            DateTimeFormatPartOptions.Add("12h",
                new List<IDateTimeFormatPartOptionTO>
                {
                    new DateTimeFormatPartOptionTO(2, IsNumber12H, true, null, Assign12Hours),
                    new DateTimeFormatPartOptionTO(1, IsNumber12H, true, null, Assign12Hours),
                });

            DateTimeFormatsParts.Add("min", new DateTimeFormatPartTO("min", false, "Minutes: 30"));
            DateTimeFormatPartOptions.Add("min",
                new List<IDateTimeFormatPartOptionTO>
                {
                    new DateTimeFormatPartOptionTO(2, IsNumberMinutes, true, null, AssignMinutes),
                    new DateTimeFormatPartOptionTO(1, IsNumberMinutes, true, null, AssignMinutes),
                });

            DateTimeFormatsParts.Add("ss", new DateTimeFormatPartTO("ss", false, "Seconds: 29"));
            DateTimeFormatPartOptions.Add("ss",
                new List<IDateTimeFormatPartOptionTO>
                {
                    new DateTimeFormatPartOptionTO(2, IsNumberSeconds, true, null, AssignSeconds),
                    new DateTimeFormatPartOptionTO(1, IsNumberSeconds, true, null, AssignSeconds),
                });

            DateTimeFormatsParts.Add("sp", new DateTimeFormatPartTO("sp", false, "Split Seconds: 987"));
            DateTimeFormatPartOptions.Add("sp",
                new List<IDateTimeFormatPartOptionTO>
                {
                    new DateTimeFormatPartOptionTO(3, IsNumberMilliseconds, true, null, AssignMilliseconds),
                    new DateTimeFormatPartOptionTO(2, IsNumberMilliseconds, true, null, AssignMilliseconds),
                    new DateTimeFormatPartOptionTO(1, IsNumberMilliseconds, true, null, AssignMilliseconds),
                });

            DateTimeFormatsParts.Add("am/pm", new DateTimeFormatPartTO("am/pm", false, "am or pm"));
            DateTimeFormatPartOptions.Add("am/pm",
                new List<IDateTimeFormatPartOptionTO>
                {
                    new DateTimeFormatPartOptionTO(4, IsTextAmPm, false, null, AssignAmPm),
                    new DateTimeFormatPartOptionTO(3, IsTextAmPm, false, null, AssignAmPm),
                    new DateTimeFormatPartOptionTO(2, IsTextAmPm, false, null, AssignAmPm),
                });

            DateTimeFormatsParts.Add("Z",
                new DateTimeFormatPartTO("Z", false, "Time zone in short format: GMT (if available on the system)"));
            DateTimeFormatPartOptions.Add("Z", TimeZones.Select(k =>
            {
                IDateTimeFormatPartOptionTO dateTimeFormatPartOptionTo = new DateTimeFormatPartOptionTO(k.Key.Length,
                    IsTextTimeZone, false, null, AssignTimeZone);
                return dateTimeFormatPartOptionTo;
            }).OrderByDescending(k => k.Length).ToList());

            DateTimeFormatsParts.Add("ZZ", new DateTimeFormatPartTO("ZZ", false, "Time zone: Grenwich mean time"));
            DateTimeFormatPartOptions.Add("ZZ", TimeZones.Select(k =>
            {
                IDateTimeFormatPartOptionTO dateTimeFormatPartOptionTo = new DateTimeFormatPartOptionTO(k.Key.Length,
                    IsTextTimeZone, false, null, AssignTimeZone);
                return dateTimeFormatPartOptionTo;
            }).OrderByDescending(k => k.Length).ToList());

            DateTimeFormatsParts.Add("ZZZ", new DateTimeFormatPartTO("ZZZ", false, "Time zone offset: GMT + 02:00"));
            DateTimeFormatPartOptions.Add("ZZZ", TimeZones.Select(k =>
            {
                IDateTimeFormatPartOptionTO dateTimeFormatPartOptionTo = new DateTimeFormatPartOptionTO(k.Key.Length,
                    IsTextTimeZone, false, null, AssignTimeZone);
                return dateTimeFormatPartOptionTo;
            }).OrderByDescending(k => k.Length).ToList());

            DateTimeFormatsParts.Add("Era", new DateTimeFormatPartTO("Era", false, "A.D."));

            DateTimeFormatPartOptions.Add("era",
                new List<IDateTimeFormatPartOptionTO>
                {
                    new DateTimeFormatPartOptionTO(2, (data, treatAsTim) => data.ToLower().Equals("ad"), false, "AD",
                        AssignEra)
                });

            DateTimeFormatPartOptions.Add("Era",
                new List<IDateTimeFormatPartOptionTO>
                {
                    new DateTimeFormatPartOptionTO(3, (data, treatAsTim) => data.ToLower().Equals("a.d"), false, "A.D",
                        AssignEra)
                });

            DateTimeFormatPartOptions.Add("ERA",
                new List<IDateTimeFormatPartOptionTO>
                {
                    new DateTimeFormatPartOptionTO(4, (data, treatAsTim) => data.ToLower().Equals("a.d."), false, "A.D.",
                        AssignEra)
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
                    new DateTimeFormatPartOptionTO(2, IsTextNumeric, true, null, AssignYears)
                });

            TimeFormatPartOptions.Add("yyyy",
                new List<IDateTimeFormatPartOptionTO>
                {
                    new DateTimeFormatPartOptionTO(4, IsTextNumeric, true, null, AssignYears)
                });

            TimeFormatPartOptions.Add("mm",
                new List<IDateTimeFormatPartOptionTO>
                {
                    new DateTimeFormatPartOptionTO(2, IsNumberMonth, true, null, AssignMonths)
                });

            TimeFormatPartOptions.Add("m",
                new List<IDateTimeFormatPartOptionTO>
                {
                    new DateTimeFormatPartOptionTO(2, IsNumberMonth, true, null, AssignMonths),
                    new DateTimeFormatPartOptionTO(1, IsNumberMonth, true, null, AssignMonths),
                });

            TimeFormatPartOptions.Add("MM",
                new List<IDateTimeFormatPartOptionTO>
                {
                    new DateTimeFormatPartOptionTO(2, IsNumberMonth, true, null, AssignMonths)
                });

            TimeFormatPartOptions.Add("M",
                new List<IDateTimeFormatPartOptionTO>
                {
                    new DateTimeFormatPartOptionTO(2, IsNumberMonth, true, null, AssignMonths),
                    new DateTimeFormatPartOptionTO(1, IsNumberMonth, true, null, AssignMonths),
                });

            TimeFormatPartOptions.Add("d",
                new List<IDateTimeFormatPartOptionTO>
                {
                    new DateTimeFormatPartOptionTO(2, IsNumberDay, true, null, AssignDays),
                    new DateTimeFormatPartOptionTO(1, IsNumberDay, true, null, AssignDays)
                });

            TimeFormatPartOptions.Add("dd",
                new List<IDateTimeFormatPartOptionTO>
                {
                    new DateTimeFormatPartOptionTO(2, IsNumberDay, true, null, AssignDays)
                });

            TimeFormatPartOptions.Add("DW",
                new List<IDateTimeFormatPartOptionTO>
                {
                    new DateTimeFormatPartOptionTO(1, IsNumberDayOfWeek, true, null, AssignDaysOfWeek),
                });

            TimeFormatPartOptions.Add("dW",
                new List<IDateTimeFormatPartOptionTO>
                {
                    new DateTimeFormatPartOptionTO(1, IsNumberDayOfWeek, true, null, AssignDaysOfWeek),
                });

            TimeFormatPartOptions.Add("dw",
                new List<IDateTimeFormatPartOptionTO>
                {
                    new DateTimeFormatPartOptionTO(1, IsNumberDayOfWeek, true, null, AssignDaysOfWeek),
                });

            TimeFormatPartOptions.Add("dy",
                new List<IDateTimeFormatPartOptionTO>
                {
                    new DateTimeFormatPartOptionTO(3, IsNumberDayOfYear, true, null, AssignDaysOfYear),
                    new DateTimeFormatPartOptionTO(2, IsNumberDayOfYear, true, null, AssignDaysOfYear),
                    new DateTimeFormatPartOptionTO(1, IsNumberDayOfYear, true, null, AssignDaysOfYear)
                });

            TimeFormatPartOptions.Add("w",
                new List<IDateTimeFormatPartOptionTO>
                {
                    new DateTimeFormatPartOptionTO(2, IsNumberWeekOfYear, true, null, AssignWeeks),
                    new DateTimeFormatPartOptionTO(1, IsNumberWeekOfYear, true, null, AssignWeeks),
                });

            TimeFormatPartOptions.Add("24h",
                new List<IDateTimeFormatPartOptionTO>
                {
                    new DateTimeFormatPartOptionTO(2, IsNumber24H, true, null, Assign24Hours)
                });

            TimeFormatPartOptions.Add("12h",
                new List<IDateTimeFormatPartOptionTO>
                {
                    new DateTimeFormatPartOptionTO(2, IsNumber12H, true, null, Assign12Hours),
                    new DateTimeFormatPartOptionTO(1, IsNumber12H, true, null, Assign12Hours),
                });

            TimeFormatPartOptions.Add("min",
                new List<IDateTimeFormatPartOptionTO>
                {
                    new DateTimeFormatPartOptionTO(2, IsNumberMinutes, true, null, AssignMinutes),
                    new DateTimeFormatPartOptionTO(1, IsNumberMinutes, true, null, AssignMinutes),
                });

            TimeFormatPartOptions.Add("ss",
                new List<IDateTimeFormatPartOptionTO>
                {
                    new DateTimeFormatPartOptionTO(2, IsNumberSeconds, true, null, AssignSeconds),
                    new DateTimeFormatPartOptionTO(1, IsNumberSeconds, true, null, AssignSeconds),
                });

            TimeFormatPartOptions.Add("sp",
                new List<IDateTimeFormatPartOptionTO>
                {
                    new DateTimeFormatPartOptionTO(3, IsNumberMilliseconds, true, null, AssignMilliseconds),
                    new DateTimeFormatPartOptionTO(2, IsNumberMilliseconds, true, null, AssignMilliseconds),
                    new DateTimeFormatPartOptionTO(1, IsNumberMilliseconds, true, null, AssignMilliseconds),
                });

            TimeFormatPartOptions.Add("am/pm",
                new List<IDateTimeFormatPartOptionTO>
                {
                    new DateTimeFormatPartOptionTO(4, IsTextAmPm, false, null, AssignAmPm),
                    new DateTimeFormatPartOptionTO(3, IsTextAmPm, false, null, AssignAmPm),
                    new DateTimeFormatPartOptionTO(2, IsTextAmPm, false, null, AssignAmPm),
                });

            TimeFormatPartOptions.Add("Z", TimeZones.Select(k =>
            {
                IDateTimeFormatPartOptionTO dateTimeFormatPartOptionTo = new DateTimeFormatPartOptionTO(k.Key.Length,
                    IsTextTimeZone, false, null, AssignTimeZone);
                return dateTimeFormatPartOptionTo;
            }).OrderByDescending(k => k.Length).ToList());

            TimeFormatPartOptions.Add("ZZ", TimeZones.Select(k =>
            {
                IDateTimeFormatPartOptionTO dateTimeFormatPartOptionTo = new DateTimeFormatPartOptionTO(k.Key.Length,
                    IsTextTimeZone, false, null, AssignTimeZone);
                return dateTimeFormatPartOptionTo;
            }).OrderByDescending(k => k.Length).ToList());

            TimeFormatPartOptions.Add("ZZZ", TimeZones.Select(k =>
            {
                IDateTimeFormatPartOptionTO dateTimeFormatPartOptionTo = new DateTimeFormatPartOptionTO(k.Key.Length,
                    IsTextTimeZone, false, null, AssignTimeZone);
                return dateTimeFormatPartOptionTo;
            }).OrderByDescending(k => k.Length).ToList());

            TimeFormatPartOptions.Add("era",
                new List<IDateTimeFormatPartOptionTO>
                {
                    new DateTimeFormatPartOptionTO(2, (data, treatAsTim) => data.ToLower().Equals("ad"), false, "AD",
                        AssignEra)
                });

            TimeFormatPartOptions.Add("Era",
                new List<IDateTimeFormatPartOptionTO>
                {
                    new DateTimeFormatPartOptionTO(3, (data, treatAsTim) => data.ToLower().Equals("a.d"), false, "A.D",
                        AssignEra)
                });

            TimeFormatPartOptions.Add("ERA",
                new List<IDateTimeFormatPartOptionTO>
                {
                    new DateTimeFormatPartOptionTO(4, (data, treatAsTim) => data.ToLower().Equals("a.d."), false, "A.D.",
                        AssignEra)
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
                        string min = (minutes == 0) ? "00" : "30";
                        string hrs = string.Concat(((hours/Math.Abs(hours) < 0) ? "-" : "+"),
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
                        string min = (minutes == 0) ? "00" : "30";
                        string hrs = string.Concat(((hours/Math.Abs(hours) < 0) ? "-" : "+"),
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
            DateTimeFormatForwardLookupsForDotNet.Add('d', new List<int> {4, 3, 2, 1});

            //
            // Lookups for fffffff, ffffff, fffff, ffff, fff, ff, f
            //
            DateTimeFormatForwardLookupsForDotNet.Add('f', new List<int> {7, 6, 5, 4, 3, 2, 1});

            //
            // Lookups for FFFFFFF, FFFFFF, FFFFF, FFFF, FFF, FF, F
            //
            DateTimeFormatForwardLookupsForDotNet.Add('F', new List<int> {7, 6, 5, 4, 3, 2, 1});

            //
            // Lookups for gg, g
            //
            DateTimeFormatForwardLookupsForDotNet.Add('g', new List<int> {2, 1});

            //
            // Lookups for hh, h
            //
            DateTimeFormatForwardLookupsForDotNet.Add('h', new List<int> {2, 1});

            //
            // Lookups for HH, H
            //
            DateTimeFormatForwardLookupsForDotNet.Add('H', new List<int> {2, 1});

            //
            // Lookups for K
            //
            DateTimeFormatForwardLookupsForDotNet.Add('K', new List<int> {1});

            //
            // Lookups for mm, m
            //
            DateTimeFormatForwardLookupsForDotNet.Add('m', new List<int> {2, 1});

            //
            // Lookups for MMMM, MMM, MM, M
            //
            DateTimeFormatForwardLookupsForDotNet.Add('M', new List<int> {4, 3, 2, 1});

            //
            // Lookups for ss, s
            //
            DateTimeFormatForwardLookupsForDotNet.Add('s', new List<int> {2, 1});

            //
            // Lookups for tt, t
            //
            DateTimeFormatForwardLookupsForDotNet.Add('t', new List<int> {2, 1});

            //
            // Lookups for yyyyy, yyyy, yyy, yy, y
            //
            DateTimeFormatForwardLookupsForDotNet.Add('y', new List<int> {5, 4, 3, 2, 1});

            //
            // Lookups for zzz, zz, z
            //
            DateTimeFormatForwardLookupsForDotNet.Add('z', new List<int> {3, 2, 1});
        }

        /// <summary>
        ///     Creates forward lookup information for date time parts
        /// </summary>
        private static void CreateDateTimeFormatForwardLookups()
        {
            //
            // Lookups for yyyyy, yyyy, yyy, yy and y
            //
            DateTimeFormatForwardLookups.Add('y', new List<int> {5, 4, 3, 2, 1});

            //
            // Lookups for min, mm and m
            //
            DateTimeFormatForwardLookups.Add('m', new List<int> {3, 2, 1});

            //
            // Lookups for MM and M
            //
            DateTimeFormatForwardLookups.Add('M', new List<int> {2, 1});

            //
            // Lookups for dd, dW, dw, dy and d
            //
            DateTimeFormatForwardLookups.Add('d', new List<int> {2, 1});

            //
            // Lookups for w
            //
            DateTimeFormatForwardLookups.Add('w', new List<int> {2, 1});

            //
            // Lookups for DW
            //
            DateTimeFormatForwardLookups.Add('D', new List<int> {2});

            //
            // Lookups for 24h
            //
            DateTimeFormatForwardLookups.Add('2', new List<int> {3});

            //
            // Lookups for 12h
            //
            DateTimeFormatForwardLookups.Add('1', new List<int> {3});

            //
            // Lookups for ss and sp
            //
            DateTimeFormatForwardLookups.Add('s', new List<int> {2});

            //
            // Lookups for am/pm
            //
            DateTimeFormatForwardLookups.Add('a', new List<int> {5});

            //
            // Lookups for ZZZ, ZZ and Z
            //
            DateTimeFormatForwardLookups.Add('Z', new List<int> {3, 2, 1});

            //
            // Lookups for Era
            //
            DateTimeFormatForwardLookups.Add('E', new List<int> {3});
            DateTimeFormatForwardLookups.Add('e', new List<int> {3});
        }

        #region DateTime Assign Actions

        private static void AssignEra(IDateTimeResultTO dateTimeResultTo, bool assignAsTime, IConvertible value)
        {
            dateTimeResultTo.Era = value.ToString(CultureInfo.InvariantCulture);
        }

        private static void AssignYears(IDateTimeResultTO dateTimeResultTo, bool assignAsTime, IConvertible value)
        {
            int years = Convert.ToInt32(value);

            if (!assignAsTime && years < 100)
            {
                years = CultureInfo.InvariantCulture.Calendar.ToFourDigitYear(years);
            }

            dateTimeResultTo.Years = Convert.ToInt32(years);
        }

        private static void AssignMonths(IDateTimeResultTO dateTimeResultTo, bool assignAsTime, IConvertible value)
        {
            dateTimeResultTo.Months = Convert.ToInt32(value);
        }

        private static void AssignDays(IDateTimeResultTO dateTimeResultTo, bool assignAsTime, IConvertible value)
        {
            dateTimeResultTo.Days = Convert.ToInt32(value);
        }

        private static void AssignDaysOfWeek(IDateTimeResultTO dateTimeResultTo, bool assignAsTime, IConvertible value)
        {
            dateTimeResultTo.DaysOfWeek = Convert.ToInt32(value);
        }

        private static void AssignDaysOfYear(IDateTimeResultTO dateTimeResultTo, bool assignAsTime, IConvertible value)
        {
            dateTimeResultTo.DaysOfYear = Convert.ToInt32(value);
        }

        private static void AssignWeeks(IDateTimeResultTO dateTimeResultTo, bool assignAsTime, IConvertible value)
        {
            dateTimeResultTo.Weeks = Convert.ToInt32(value);
        }

        private static void Assign12Hours(IDateTimeResultTO dateTimeResultTo, bool assignAsTime, IConvertible value)
        {
            dateTimeResultTo.Hours = Convert.ToInt32(value);
        }

        private static void Assign24Hours(IDateTimeResultTO dateTimeResultTo, bool assignAsTime, IConvertible value)
        {
            dateTimeResultTo.Hours = Convert.ToInt32(value);
            dateTimeResultTo.Is24H = true;
        }

        private static void AssignMinutes(IDateTimeResultTO dateTimeResultTo, bool assignAsTime, IConvertible value)
        {
            dateTimeResultTo.Minutes = Convert.ToInt32(value);
        }

        private static void AssignSeconds(IDateTimeResultTO dateTimeResultTo, bool assignAsTime, IConvertible value)
        {
            dateTimeResultTo.Seconds = Convert.ToInt32(value);
        }

        private static void AssignMilliseconds(IDateTimeResultTO dateTimeResultTo, bool assignAsTime, IConvertible value)
        {
            dateTimeResultTo.Milliseconds = Convert.ToInt32(value);
        }

        private static void AssignAmPm(IDateTimeResultTO dateTimeResultTo, bool assignAsTime, IConvertible value)
        {
            string lowerValue = value.ToString(CultureInfo.InvariantCulture).ToLower();

            if (lowerValue == "pm" || lowerValue == "p.m" || lowerValue == "p.m.")
            {
                dateTimeResultTo.AmPm = DateTimeAmPm.pm;
            }
            else
            {
                dateTimeResultTo.AmPm = DateTimeAmPm.am;
            }
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

        #endregion DateTime Assign Actions

        #region Predicates

        /// <summary>
        ///     Determines if a given string contains a number which is a vaild seconds number
        /// </summary>
        private static bool IsNumberSeconds(string data, bool treatAsTime)
        {
            bool nothingDied = true;

            int numericData;
            if (int.TryParse(data, NumberStyles.None, null, out numericData))
            {
                if (!treatAsTime)
                {
                    nothingDied = (numericData >= 0 && numericData <= 59);
                }
            }
            else
            {
                nothingDied = false;
            }

            return nothingDied;
        }

        /// <summary>
        ///     Determines if a given string contains a number which is a vaild minute number
        /// </summary>
        private static bool IsNumberMilliseconds(string data, bool treatAsTime)
        {
            bool nothingDied = true;

            int numericData;
            if (int.TryParse(data, NumberStyles.None, null, out numericData))
            {
                if (!treatAsTime)
                {
                    nothingDied = (numericData >= 0 && numericData <= 999);
                }
            }
            else
            {
                nothingDied = false;
            }

            return nothingDied;
        }

        /// <summary>
        ///     Determines if a given string contains a number which is a vaild minute number
        /// </summary>
        private static bool IsNumberMinutes(string data, bool treatAsTime)
        {
            bool nothingDied = true;

            int numericData;
            if (int.TryParse(data, NumberStyles.None, null, out numericData))
            {
                if (!treatAsTime)
                {
                    nothingDied = (numericData >= 0 && numericData <= 59);
                }
            }
            else
            {
                nothingDied = false;
            }

            return nothingDied;
        }

        /// <summary>
        ///     Determines if a given string contains a number which is a vaild 24 hours number
        /// </summary>
        private static bool IsNumber24H(string data, bool treatAsTime)
        {
            bool nothingDied = true;

            int numericData;
            if (data.Length == 2 && int.TryParse(data, out numericData))
            {
                if (!treatAsTime)
                {
                    nothingDied = (numericData >= 0 && numericData <= 24);
                }
            }
            else
            {
                nothingDied = false;
            }

            return nothingDied;
        }

        /// <summary>
        ///     Determines if a given string contains a number which is a vaild 12 hours number
        /// </summary>
        private static bool IsNumber12H(string data, bool treatAsTime)
        {
            bool nothingDied = true;

            int numericData;
            if (int.TryParse(data, NumberStyles.None, null, out numericData))
            {
                if (!treatAsTime)
                {
                    nothingDied = (numericData >= 1 && numericData <= 12);
                }
            }
            else
            {
                nothingDied = false;
            }

            return nothingDied;
        }

        /// <summary>
        ///     Determines if a given string contains a number which is a vaild month
        /// </summary>
        private static bool IsNumberMonth(string data, bool treatAsTime)
        {
            bool nothingDied = true;

            int numericData;
            if (int.TryParse(data, NumberStyles.None, null, out numericData))
            {
                if (!treatAsTime)
                {
                    nothingDied = (numericData >= 1 && numericData <= 12);
                }
            }
            else
            {
                nothingDied = false;
            }

            return nothingDied;
        }

        /// <summary>
        ///     Determines if a given string contains a number which is a vaild month
        /// </summary>
        private static bool IsNumberDay(string data, bool treatAsTime)
        {
            bool nothingDied = true;

            int numericData;
            if (int.TryParse(data, NumberStyles.None, null, out numericData))
            {
                if (!treatAsTime)
                {
                    nothingDied = (numericData >= 1 && numericData <= 31);
                }
            }
            else
            {
                nothingDied = false;
            }

            return nothingDied;
        }

        /// <summary>
        ///     Determines if a given string contains a number which is a vaild day of the week
        /// </summary>
        private static bool IsNumberDayOfWeek(string data, bool treatAsTime)
        {
            bool nothingDied = true;

            int numericData;
            if (int.TryParse(data, NumberStyles.None, null, out numericData))
            {
                if (!treatAsTime)
                {
                    nothingDied = (numericData >= 1 && numericData <= 7);
                }
            }
            else
            {
                nothingDied = false;
            }

            return nothingDied;
        }

        /// <summary>
        ///     Determines if a given string contains a number which is a vaild day of the year
        /// </summary>
        private static bool IsNumberDayOfYear(string data, bool treatAsTime)
        {
            bool nothingDied = true;

            int numericData;
            if (int.TryParse(data, NumberStyles.None, null, out numericData) && (numericData >= 1 && numericData <= 365))
            {
                //nothing to do since nothignDied is already true
            }
            else
            {
                nothingDied = false;
            }

            return nothingDied;
        }

        /// <summary>
        ///     Determines if a given string contains a number which is a vaild day of the year
        /// </summary>
        private static bool IsNumberWeekOfYear(string data, bool treatAsTime)
        {
            bool nothingDied = true;

            int numericData;
            if (int.TryParse(data, NumberStyles.None, null, out numericData))
            {
                if (!treatAsTime)
                {
                    nothingDied = (numericData >= 1 && numericData <= 52);
                }
            }
            else
            {
                nothingDied = false;
            }

            return nothingDied;
        }

        /// <summary>
        ///     Determines if a given string is a number
        /// </summary>
        private static bool IsTextNumeric(string data, bool treatAsTime)
        {
            int numericData;
            return int.TryParse(data, NumberStyles.None, null, out numericData);
        }

        /// <summary>
        ///     Determines if a given string represents am
        /// </summary>
        private static bool IsTextAmPm(string data, bool treatAsTime)
        {
            string lowerData = data.ToLower();
            return lowerData == "am" || lowerData == "pm" || lowerData == "a.m" || lowerData == "p.m" ||
                   lowerData == "a.m." || lowerData == "p.m.";
        }

        /// <summary>
        ///     Determines if a given string represents a time zone
        /// </summary>
        private static bool IsTextTimeZone(string data, bool treatAsTime)
        {
            string lowerData = data.ToLower();
            ITimeZoneTO timeZoneTo;
            return TimeZones.TryGetValue(lowerData, out timeZoneTo);
        }

        /// <summary>
        ///     Determines if a given string represents the month of January
        /// </summary>
        private static bool IsTextJanuary(string data, bool treatAsTime)
        {
            string lowerData = data.ToLower();
            return lowerData == CultureInfo.CurrentCulture.DateTimeFormat.AbbreviatedMonthNames[0].ToLower() ||
                   lowerData == CultureInfo.CurrentCulture.DateTimeFormat.MonthNames[0].ToLower();
        }

        /// <summary>
        ///     Determines if a given string represents the month of Febuary
        /// </summary>
        private static bool IsTextFebuary(string data, bool treatAsTime)
        {
            string lowerData = data.ToLower();
            return lowerData == CultureInfo.CurrentCulture.DateTimeFormat.AbbreviatedMonthNames[1].ToLower() ||
                   lowerData == CultureInfo.CurrentCulture.DateTimeFormat.MonthNames[1].ToLower();
        }

        /// <summary>
        ///     Determines if a given string represents the month of March
        /// </summary>
        private static bool IsTextMarch(string data, bool treatAsTime)
        {
            string lowerData = data.ToLower();
            return lowerData == CultureInfo.CurrentCulture.DateTimeFormat.AbbreviatedMonthNames[2].ToLower() ||
                   lowerData == CultureInfo.CurrentCulture.DateTimeFormat.MonthNames[2].ToLower();
        }

        /// <summary>
        ///     Determines if a given string represents the month of April
        /// </summary>
        private static bool IsTextApril(string data, bool treatAsTime)
        {
            string lowerData = data.ToLower();
            return lowerData == CultureInfo.CurrentCulture.DateTimeFormat.AbbreviatedMonthNames[3].ToLower() ||
                   lowerData == CultureInfo.CurrentCulture.DateTimeFormat.MonthNames[3].ToLower();
        }

        /// <summary>
        ///     Determines if a given string represents the month of May
        /// </summary>
        private static bool IsTextMay(string data, bool treatAsTime)
        {
            string lowerData = data.ToLower();
            return lowerData == CultureInfo.CurrentCulture.DateTimeFormat.AbbreviatedMonthNames[4].ToLower() ||
                   lowerData == CultureInfo.CurrentCulture.DateTimeFormat.MonthNames[4].ToLower();
        }

        /// <summary>
        ///     Determines if a given string represents the month of June
        /// </summary>
        private static bool IsTextJune(string data, bool treatAsTime)
        {
            string lowerData = data.ToLower();
            return lowerData == CultureInfo.CurrentCulture.DateTimeFormat.AbbreviatedMonthNames[5].ToLower() ||
                   lowerData == CultureInfo.CurrentCulture.DateTimeFormat.MonthNames[5].ToLower();
        }

        /// <summary>
        ///     Determines if a given string represents the month of July
        /// </summary>
        private static bool IsTextJuly(string data, bool treatAsTime)
        {
            string lowerData = data.ToLower();
            return lowerData == CultureInfo.CurrentCulture.DateTimeFormat.AbbreviatedMonthNames[6].ToLower() ||
                   lowerData == CultureInfo.CurrentCulture.DateTimeFormat.MonthNames[6].ToLower();
        }

        /// <summary>
        ///     Determines if a given string represents the month of August
        /// </summary>
        private static bool IsTextAugust(string data, bool treatAsTime)
        {
            string lowerData = data.ToLower();
            return lowerData == CultureInfo.CurrentCulture.DateTimeFormat.AbbreviatedMonthNames[7].ToLower() ||
                   lowerData == CultureInfo.CurrentCulture.DateTimeFormat.MonthNames[7].ToLower();
        }

        /// <summary>
        ///     Determines if a given string represents the month of September
        /// </summary>
        private static bool IsTextSeptember(string data, bool treatAsTime)
        {
            string lowerData = data.ToLower();
            return lowerData == CultureInfo.CurrentCulture.DateTimeFormat.AbbreviatedMonthNames[8].ToLower() ||
                   lowerData == CultureInfo.CurrentCulture.DateTimeFormat.MonthNames[8].ToLower();
        }

        /// <summary>
        ///     Determines if a given string represents the month of October
        /// </summary>
        private static bool IsTextOctober(string data, bool treatAsTime)
        {
            string lowerData = data.ToLower();
            return lowerData == CultureInfo.CurrentCulture.DateTimeFormat.AbbreviatedMonthNames[9].ToLower() ||
                   lowerData == CultureInfo.CurrentCulture.DateTimeFormat.MonthNames[9].ToLower();
        }

        /// <summary>
        ///     Determines if a given string represents the month of November
        /// </summary>
        private static bool IsTextNovember(string data, bool treatAsTime)
        {
            string lowerData = data.ToLower();
            return lowerData == CultureInfo.CurrentCulture.DateTimeFormat.AbbreviatedMonthNames[10].ToLower() ||
                   lowerData == CultureInfo.CurrentCulture.DateTimeFormat.MonthNames[10].ToLower();
        }

        /// <summary>
        ///     Determines if a given string represents the month of December
        /// </summary>
        private static bool IsTextDecember(string data, bool treatAsTime)
        {
            string lowerData = data.ToLower();
            return lowerData == CultureInfo.CurrentCulture.DateTimeFormat.AbbreviatedMonthNames[11].ToLower() ||
                   lowerData == CultureInfo.CurrentCulture.DateTimeFormat.MonthNames[11].ToLower();
        }

        /// <summary>
        ///     Determines if a given string represents the day Monday
        /// </summary>
        private static bool IsTextMonday(string data, bool treatAsTime)
        {
            string lowerData = data.ToLower();
            return lowerData == CultureInfo.CurrentCulture.DateTimeFormat.AbbreviatedDayNames[1].ToLower() ||
                   lowerData == CultureInfo.CurrentCulture.DateTimeFormat.DayNames[1].ToLower();
        }

        /// <summary>
        ///     Determines if a given string represents the day Tuesday
        /// </summary>
        private static bool IsTextTuesday(string data, bool treatAsTime)
        {
            string lowerData = data.ToLower();
            return lowerData == CultureInfo.CurrentCulture.DateTimeFormat.AbbreviatedDayNames[2].ToLower() ||
                   lowerData == CultureInfo.CurrentCulture.DateTimeFormat.DayNames[2].ToLower();
        }

        /// <summary>
        ///     Determines if a given string represents the day Wednesday
        /// </summary>
        private static bool IsTextWednesday(string data, bool treatAsTime)
        {
            string lowerData = data.ToLower();
            return lowerData == CultureInfo.CurrentCulture.DateTimeFormat.AbbreviatedDayNames[3].ToLower() ||
                   lowerData == CultureInfo.CurrentCulture.DateTimeFormat.DayNames[3].ToLower();
        }

        /// <summary>
        ///     Determines if a given string represents the day Thursday
        /// </summary>
        private static bool IsTextThursday(string data, bool treatAsTime)
        {
            string lowerData = data.ToLower();
            return lowerData == CultureInfo.CurrentCulture.DateTimeFormat.AbbreviatedDayNames[4].ToLower() ||
                   lowerData == CultureInfo.CurrentCulture.DateTimeFormat.DayNames[4].ToLower();
        }

        /// <summary>
        ///     Determines if a given string represents the day Friday
        /// </summary>
        private static bool IsTextFriday(string data, bool treatAsTime)
        {
            string lowerData = data.ToLower();
            return lowerData == CultureInfo.CurrentCulture.DateTimeFormat.AbbreviatedDayNames[5].ToLower() ||
                   lowerData == CultureInfo.CurrentCulture.DateTimeFormat.DayNames[5].ToLower();
        }

        /// <summary>
        ///     Determines if a given string represents the day Saturday
        /// </summary>
        private static bool IsTextSaturday(string data, bool treatAsTime)
        {
            string lowerData = data.ToLower();
            return lowerData == CultureInfo.CurrentCulture.DateTimeFormat.AbbreviatedDayNames[6].ToLower() ||
                   lowerData == CultureInfo.CurrentCulture.DateTimeFormat.DayNames[6].ToLower();
        }

        /// <summary>
        ///     Determines if a given string represents the day Sunday
        /// </summary>
        private static bool IsTextSunday(string data, bool treatAsTime)
        {
            string lowerData = data.ToLower();
            return lowerData == CultureInfo.CurrentCulture.DateTimeFormat.AbbreviatedDayNames[0].ToLower() ||
                   lowerData == CultureInfo.CurrentCulture.DateTimeFormat.DayNames[0].ToLower();
        }

        #endregion Predicates

        #endregion Private Methods

        #region Properties

        public List<IDateTimeFormatPartTO> DateTimeFormatParts
        {
            get { return DateTimeFormatsParts.Values.ToList(); }
        }

        #endregion Properties
    }
}