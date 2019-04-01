#pragma warning disable
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
using static Dev2.Common.DateAndTime.DateTimeParser;
using System.Text;

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

        protected static Dictionary<char, List<int>> _dateTimeFormatForwardLookups = new Dictionary<char, List<int>>();
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

        // TODO: move logic into GetDateTimeFormatPartsProcess
        public string TranslateDotNetToDev2Format(string originalFormat, out string error)
        {
            var getDateTimeFormatParts = new GetDateTimeFormatPartsProcess(originalFormat, _dateTimeFormatForwardLookupsForDotNet, _dateTimeFormatPartOptionsForDotNet);
            getDateTimeFormatParts.Execute();
            getDateTimeFormatParts.Result = ReplaceToken(getDateTimeFormatParts.Result, "m", "Minutes");
            getDateTimeFormatParts.Result = ReplaceToken(getDateTimeFormatParts.Result, "mm", "Minutes");
            getDateTimeFormatParts.Result = ReplaceToken(getDateTimeFormatParts.Result, "M", "Month in single digit");
            getDateTimeFormatParts.Result = ReplaceToken(getDateTimeFormatParts.Result, "MM", "Month in 2 digits");
            getDateTimeFormatParts.Result = ReplaceToken(getDateTimeFormatParts.Result, "MMM", "Month text abbreviated");
            getDateTimeFormatParts.Result = ReplaceToken(getDateTimeFormatParts.Result, "MMMM", "Month text in full");
            getDateTimeFormatParts.Result = ReplaceToken(getDateTimeFormatParts.Result, "h", "Hours in 12 hour format");
            getDateTimeFormatParts.Result = ReplaceToken(getDateTimeFormatParts.Result, "hh", "Hours in 12 hour format");
            getDateTimeFormatParts.Result = ReplaceToken(getDateTimeFormatParts.Result, "H", "Hours in 24 hour format");
            getDateTimeFormatParts.Result = ReplaceToken(getDateTimeFormatParts.Result, "HH", "Hours in 24 hour format");
            getDateTimeFormatParts.Result = ReplaceToken(getDateTimeFormatParts.Result, "tt", "am or pm");
            getDateTimeFormatParts.Result = ReplaceToken(getDateTimeFormatParts.Result, "ddd", "Day of Week text abbreviated");
            getDateTimeFormatParts.Result = ReplaceToken(getDateTimeFormatParts.Result, "dddd", "Day of Week in full");
            getDateTimeFormatParts.Result = ReplaceToken(getDateTimeFormatParts.Result, "fff", "Split Seconds: 987");
            //
            // Get input format string for the dotnet parts
            //
            var dev2Format = new StringBuilder("");
             
            foreach (IDateTimeFormatPartTO part in getDateTimeFormatParts.Result)
            {
                dev2Format.Append(part.Isliteral ? "'" + part.Value + "'" : part.Value);
            }
            error = getDateTimeFormatParts.Error;
            return dev2Format.ToString();
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
            out string error)
        {
           var getDateTimeFormatParts = new GetDateTimeFormatPartsProcess(format, _dateTimeFormatForwardLookups, _dateTimeFormatPartOptions);
            var result = getDateTimeFormatParts.Execute();
            formatParts = getDateTimeFormatParts.Result;
            error = getDateTimeFormatParts.Error;
            return result;
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
                // TODO: remove this allocation from the while loop
                var getDateTimeFormatParts = new GetDateTimeFormatPartsProcess(originalInputFormat, _dateTimeFormatForwardLookups, _dateTimeFormatPartOptions);
                nothingDied = getDateTimeFormatParts.Execute();
                error = getDateTimeFormatParts.Error;
                if (!string.IsNullOrEmpty(error))
                {
                    return false;
                }
                if (nothingDied)
                {
                    TryCulture(parseAsTime, ref result, ref error, ref nothingDied, ref originalInputFormat, ref culturesTried, MaxAttempts, dateTimeArray, ref position, getDateTimeFormatParts.Result);
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

        static bool IsBlankResult(IDateTimeResultTO result)
        {
            var equal = result.AmPm == DateTimeAmPm.am;
            equal &= result.Days == 0;
            equal &= result.DaysOfWeek == 0 || result.DaysOfWeek == 1;
            equal &= result.DaysOfYear == 0;
            equal &= result.Era == null;
            equal &= result.Hours == 0;
            equal &= !result.Is24H;
            equal &= result.Milliseconds == 0;
            equal &= result.Minutes == 0;
            equal &= result.Months == 0;
            equal &= result.Seconds == 0;
            equal &= result.Weeks == 0;
            equal &= result.Years == 0;

            return equal;
        }

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
                
                var (predicateRun, forwardLookupResult) = GetPredicateRunForwardLookupResult(dateTimeArray, startPosition, partOption, passAsTime);

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

                    partOption.AssignAction?.Invoke(result, passAsTime, GetCorrectTypeValue(partOption, forwardLookupResult));
                }
                
                partOptionsCount++;
            }
        }

        private static IConvertible GetCorrectTypeValue(IDateTimeFormatPartOptionTO partOption, string forwardLookupResult)
        {
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
            return value;
        }

        private static (bool predicateRun, string forwardLookupResult) GetPredicateRunForwardLookupResult(char[] dateTimeArray, int startPosition, IDateTimeFormatPartOptionTO partOption, bool passAsTime)
        {
            var forwardLookupResult = string.Empty;
            var predicateRun = false;

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
            return (predicateRun, forwardLookupResult);
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

    class GetDateTimeFormatPartsProcess
    {
        private bool _nothingDied = true;
        private readonly string _format;
        private readonly Dictionary<char, List<int>> _dateTimeFormatForwardLookups1;
        private readonly Dictionary<string, List<IDateTimeFormatPartOptionTO>> _dateTimeFormatPartOptions;

        private List<IDateTimeFormatPartTO> _formatParts;
        public List<IDateTimeFormatPartTO> Result
        {
            get => _formatParts;
            set => _formatParts = value;
        }

        private string _error;
        public string Error
        {
            get => _error;
            set => _error = value;
        }

        public GetDateTimeFormatPartsProcess(string format, Dictionary<char, List<int>> dateTimeFormatForwardLookups, Dictionary<string, List<IDateTimeFormatPartOptionTO>> dateTimeFormatPartOptions)
        {
            _format = format;
            _dateTimeFormatForwardLookups1 = dateTimeFormatForwardLookups;
            _dateTimeFormatPartOptions = dateTimeFormatPartOptions;
        }

        public bool Execute()
        {
            var formatArray = _format.ToArray();

            _formatParts = new List<IDateTimeFormatPartTO>();
            _error = "";

            var literalRegionState = LiteralRegionStates.OutsideLiteralRegion;
            var count = 0;

            var currentValue = "";
            while (count < formatArray.Length && _nothingDied)
            {
                var forwardLookupLength = 0;

                GetLiteralRegionStateOrLength(_formatParts, formatArray, ref literalRegionState, count, ref currentValue, ref forwardLookupLength);

                count++;
                if (forwardLookupLength > 0)
                {
                    count += forwardLookupLength - 1;
                }
            }

            if (currentValue.Length > 0 && literalRegionState != LiteralRegionStates.InsideLiteralRegion &&
                literalRegionState != LiteralRegionStates.InsideLiteralRegionWithEscape)
            {
                _formatParts.Add(new DateTimeFormatPartTO(currentValue, true, ""));
            }
            else if (currentValue.Length > 0)
            {
                _nothingDied = false;
                _error = "A \' character defines a start or end of a non date time region, there appears to be a extra \' character.";
            }
            else
            {
                //valid
            }

            return _nothingDied;
        }
        
        private void GetLiteralRegionStateOrLength(List<IDateTimeFormatPartTO> formatParts, char[] formatArray, ref LiteralRegionStates literalRegionState, int count, ref string currentValue, ref int forwardLookupLength)
        {
            var currentChar = formatArray[count];
            switch (literalRegionState)
            {
                case LiteralRegionStates.OutsideLiteralRegion:
                    forwardLookupLength = DateTimeLiteralProcessor.ProcessOutsideLiteral(_dateTimeFormatForwardLookups1, _dateTimeFormatPartOptions, formatParts, ref _error, currentChar, formatArray, count, forwardLookupLength, ref literalRegionState, ref currentValue);
                    break;
                case LiteralRegionStates.InsideInferredLiteralRegion:
                    forwardLookupLength = DateTimeLiteralProcessor.ProcessInsideInferredLiteral(_dateTimeFormatForwardLookups1, _dateTimeFormatPartOptions, formatParts, ref _error, currentChar, formatArray, count, forwardLookupLength, ref currentValue, ref literalRegionState);
                    break;
                case LiteralRegionStates.InsideInferredLiteralRegionWithEscape:
                    literalRegionState = DateTimeLiteralProcessor.ProcessInsideInferredEscapedLiteral(ref _error, currentChar, literalRegionState, ref currentValue, ref _nothingDied);
                    break;
                case LiteralRegionStates.InsideLiteralRegion:
                    forwardLookupLength = DateTimeLiteralProcessor.ProcessInsideLiteral(formatParts, ref _error, currentChar, formatArray, count, forwardLookupLength, ref currentValue, ref literalRegionState);
                    break;
                case LiteralRegionStates.InsideLiteralRegionWithEscape:
                    literalRegionState = DateTimeLiteralProcessor.ProcessInsideEscapedLiteral(ref _error, currentChar, literalRegionState, ref currentValue, ref _nothingDied);
                    break;
                default:
                    throw new ArgumentOutOfRangeException("Unrecognized literal region state: " + literalRegionState);
            }
        }
    }
}



