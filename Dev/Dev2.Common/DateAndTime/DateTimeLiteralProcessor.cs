#pragma warning disable CC0091, S1226, S100, CC0044, CC0045, CC0021, S1449, S1541, S1067, S3235, CC0015, S107, S2292, S1450, S105, CC0074, S1135, S101, S3776, CS0168, S2339, CC0031, S3240, CC0020, CS0108, S1694, S1481, CC0008, AD0001, S2328, S2696, S1643, CS0659, CS0067, S104, CC0030, CA2202, S3376, S1185, CS0219, S3253, S1066, CC0075, S3459, S1871, S1125, CS0649, S2737, S1858, CC0082, CC0001, S3241, S2223, S1301, CC0013, S2955, S1944, CS4014, S3052, S2674, S2344, S1939, S1210, CC0033, CC0002, S3458, S3254, S3220, S2197, S1905, S1699, S1659, S1155, CS0105, CC0019, S3626, S3604, S3440, S3256, S2692, S2345, S1109, FS0058, CS1998, CS0661, CS0660, CS0162, CC0089, CC0032, CC0011, CA1001
using System.Collections.Generic;
using System.Linq;
using Dev2.Common.DateAndTime.TO;
using Dev2.Common.Interfaces.Core.Convertors.DateAndTime;
using Warewolf.Resource.Errors;

namespace Dev2.Common.DateAndTime
{
    public static class DateTimeLiteralProcessor
    {
        const char EscapeCharacter = '\\';

        public static DateTimeParser.LiteralRegionStates ProcessInsideEscapedLiteral(ref string error, char currentChar, DateTimeParser.LiteralRegionStates literalRegionState, ref string currentValue, ref bool nothingDied)
        {
            if(currentChar == DateTimeParser.DateLiteralCharacter || currentChar == EscapeCharacter)
            {
                literalRegionState = DateTimeParser.LiteralRegionStates.InsideLiteralRegion;
                currentValue += currentChar;
            }
            else
            {
                nothingDied = false;
                error = ErrorResource.BackSlashFormatError;
            }
            return literalRegionState;
        }

        public static int ProcessInsideLiteral(List<IDateTimeFormatPartTO> formatParts, ref string error, char currentChar, char[] formatArray, int count, int forwardLookupLength, ref string currentValue, ref DateTimeParser.LiteralRegionStates literalRegionState)
        {
            string tmpForwardLookupResult;
            if(currentChar == DateTimeParser.DateLiteralCharacter &&
               CheckForDoubleEscapedLiteralCharacter(formatArray, count, out tmpForwardLookupResult, out error))
            {
                forwardLookupLength = tmpForwardLookupResult.Length;
                currentValue += currentChar;
            }
            else if(currentChar == DateTimeParser.DateLiteralCharacter)
            {
                literalRegionState = DateTimeParser.LiteralRegionStates.OutsideLiteralRegion;
                formatParts.Add(new DateTimeFormatPartTO(currentValue, true, ""));
                currentValue = "";
            }
            else if(currentChar == EscapeCharacter)
            {
                literalRegionState = DateTimeParser.LiteralRegionStates.InsideLiteralRegionWithEscape;
            }
            else
            {
                currentValue += currentChar;
            }
            return forwardLookupLength;
        }

        public static DateTimeParser.LiteralRegionStates ProcessInsideInferredEscapedLiteral(ref string error, char currentChar, DateTimeParser.LiteralRegionStates literalRegionState, ref string currentValue, ref bool nothingDied)
        {
            if(currentChar == DateTimeParser.DateLiteralCharacter || currentChar == EscapeCharacter)
            {
                literalRegionState = DateTimeParser.LiteralRegionStates.InsideInferredLiteralRegion;
                currentValue += currentChar;
            }
            else
            {
                nothingDied = false;
                error = ErrorResource.BackSlashFormatError;
            }
            return literalRegionState;
        }

        public static int ProcessInsideInferredLiteral(Dictionary<char, List<int>> dateTimeFormatForwardLookups, Dictionary<string, List<IDateTimeFormatPartOptionTO>> dateTimeFormatPartOptions, List<IDateTimeFormatPartTO> formatParts, ref string error, char currentChar, char[] formatArray, int count, int forwardLookupLength, ref string currentValue, ref DateTimeParser.LiteralRegionStates literalRegionState)
        {
            string tmpCurrentValue;
            string tmpForwardLookupResult;
            if(currentChar == DateTimeParser.DateLiteralCharacter &&
               CheckForDoubleEscapedLiteralCharacter(formatArray, count, out tmpForwardLookupResult, out error))
            {
                forwardLookupLength = tmpForwardLookupResult.Length;
                currentValue += currentChar;
            }
            else if(currentChar == DateTimeParser.DateLiteralCharacter)
            {
                literalRegionState = DateTimeParser.LiteralRegionStates.InsideLiteralRegion;
                formatParts.Add(new DateTimeFormatPartTO(currentValue, true, ""));
                currentValue = "";
            }
            else if(currentChar == EscapeCharacter)
            {
                literalRegionState = DateTimeParser.LiteralRegionStates.InsideInferredLiteralRegionWithEscape;
            }
            else if(TryGetDateTimeFormatPart(formatArray, count, currentChar, dateTimeFormatForwardLookups,
                dateTimeFormatPartOptions, out tmpCurrentValue, out error))
            {
                literalRegionState = DateTimeParser.LiteralRegionStates.OutsideLiteralRegion;
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
            return forwardLookupLength;
        }

        public static int ProcessOutsideLiteral(Dictionary<char, List<int>> dateTimeFormatForwardLookups, Dictionary<string, List<IDateTimeFormatPartOptionTO>> dateTimeFormatPartOptions, List<IDateTimeFormatPartTO> formatParts, ref string error, char currentChar, char[] formatArray, int count, int forwardLookupLength, ref DateTimeParser.LiteralRegionStates literalRegionState, ref string currentValue)
        {
            string tmpForwardLookupResult;
            if(currentChar == DateTimeParser.DateLiteralCharacter && CheckForDoubleEscapedLiteralCharacter(formatArray, count, out tmpForwardLookupResult, out error))
            {
                forwardLookupLength = tmpForwardLookupResult.Length;
                literalRegionState = DateTimeParser.LiteralRegionStates.InsideInferredLiteralRegion;
                currentValue += currentChar;
            }
            else if(currentChar == DateTimeParser.DateLiteralCharacter)
            {
                literalRegionState = DateTimeParser.LiteralRegionStates.InsideLiteralRegion;
            }
            else if(TryGetDateTimeFormatPart(formatArray, count, currentChar, dateTimeFormatForwardLookups,
                dateTimeFormatPartOptions, out currentValue, out error))
            {
                forwardLookupLength = currentValue.Length;
                formatParts.Add(new DateTimeFormatPartTO(currentValue, false, ""));
                currentValue = "";
            }
            else
            {
                error = "";
                literalRegionState = DateTimeParser.LiteralRegionStates.InsideInferredLiteralRegion;
                currentValue += currentChar;
            }
            return forwardLookupLength;
        }

        static bool CheckForDoubleEscapedLiteralCharacter(char[] formatArray, int startPosition,
            out string result, out string error)
        {
            error = "";
            result = DateTimeParser.ForwardLookup(formatArray, startPosition, 2);
            return result == "''";
        }
        
        static bool TryGetDateTimeFormatPart(char[] formatArray, int startPosition, char forwardLookupIndex,
            Dictionary<char, List<int>> dateTimeFormatForwardLookups,
            Dictionary<string, List<IDateTimeFormatPartOptionTO>> dateTimeFormatPartOptions, out string result,
            out string error)
        {
            var nothingDied = true;

            error = "";
            result = "";

            List<int> lookupLengths;
            if (dateTimeFormatForwardLookups.TryGetValue(forwardLookupIndex, out lookupLengths))
            {
                var lookupResults =
                    lookupLengths.Select(i => DateTimeParser.ForwardLookup(formatArray, startPosition, i)).ToList();

                var count = 0;
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
                    else
                    {
                        CheckLookupLengths(startPosition, forwardLookupIndex, lookupLengths, count, ref error, ref nothingDied);
                    }

                    count++;
                }
            }
            else
            {
                nothingDied = false;
                error = string.Format(ErrorResource.UnexpectedCharacterAtIndex, startPosition);
            }

            return nothingDied;
        }

        private static void CheckLookupLengths(int startPosition, char forwardLookupIndex, List<int> lookupLengths, int count, ref string error, ref bool nothingDied)
        {
            if (count == lookupLengths.Count - 1)
            {
                nothingDied = false;
                error =
                    string.Concat("Failed to find any format part matches in forward lookups from character '",
                        forwardLookupIndex, "' at index ", startPosition, " of format.");
            }
        }
    }
}