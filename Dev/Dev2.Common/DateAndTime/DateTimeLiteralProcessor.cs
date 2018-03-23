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
                        if (count == lookupLengths.Count - 1)
                        {
                            nothingDied = false;
                            error =
                                string.Concat("Failed to find any format part matches in forward lookups from character '",
                                    forwardLookupIndex, "' at index ", startPosition, " of format.");
                        }
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
    }
}