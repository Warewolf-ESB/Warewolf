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

using System;
using System.Collections.Generic;
using Dev2.Common.ExtMethods;
using Dev2.Data.Interfaces;
using Dev2.Data.Interfaces.Enums;
using Warewolf.Resource.Errors;

namespace Dev2.Data.Operations
{
    public class Dev2IndexFinder : IDev2IndexFinder
    {
        #region Methods

        public IEnumerable<int> FindIndex(string stringToSearchIn, string firstOccurrence, string charsToSearchFor, string direction, bool matchCase, string startIndex)
        {
            var occurrence = enIndexFinderOccurrence.FirstOccurrence;
            var dir = enIndexFinderDirection.LeftToRight;
            #region Set the enums according to the strings

            switch (firstOccurrence)
            {
                case "First Occurrence":
                    occurrence = enIndexFinderOccurrence.FirstOccurrence;
                    break;

                case "Last Occurrence":
                    occurrence = enIndexFinderOccurrence.LastOccurrence;
                    break;

                case "All Occurrences":
                    occurrence = enIndexFinderOccurrence.AllOccurrences;
                    break;
                default:
                    break;
            }

            switch (direction)
            {
                case "Left to Right":
                    dir = enIndexFinderDirection.LeftToRight;
                    break;

                case "Right to Left":
                    dir = enIndexFinderDirection.RightToLeft;
                    break;
                default:
                    break;
            }

            startIndex = !string.IsNullOrWhiteSpace(startIndex) ? startIndex : "0";
            if(!int.TryParse(startIndex, out int startIdx))
            {
                throw new Exception(ErrorResource.StartIndexNotANumber);
            }

            #endregion

            return FindIndex(stringToSearchIn, occurrence, charsToSearchFor, dir, matchCase, startIdx);
        }

        public IEnumerable<int> FindIndex(string stringToSearchIn, enIndexFinderOccurrence occurrence, string charsToSearchFor, enIndexFinderDirection direction, bool matchCase, int startIndex)
        {
            IEnumerable<int> result = new[] { -1 };


            if(!string.IsNullOrEmpty(stringToSearchIn) && !string.IsNullOrEmpty(charsToSearchFor))
            {
                #region Calculate the index according to what the user enterd

                var comparisonType = matchCase ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
                var firstIndex = stringToSearchIn.IndexOf(charsToSearchFor, startIndex, comparisonType);
                var lastIndex = stringToSearchIn.LastIndexOf(charsToSearchFor, stringToSearchIn.Length - 1, comparisonType);

                result = direction == enIndexFinderDirection.RightToLeft ? RightToLeftIndexSearch(occurrence, firstIndex, lastIndex, stringToSearchIn, charsToSearchFor,
                                                    comparisonType) : LeftToRightIndexSearch(occurrence, firstIndex, lastIndex, stringToSearchIn, charsToSearchFor,
                                                    comparisonType);

                #endregion
            }
            return result;
        }

        #endregion

        #region Private Methods

        IEnumerable<int> RightToLeftIndexSearch(enIndexFinderOccurrence occurrence, int firstIndex, int lastIndex, string stringToSearchIn, string charsToSearchFor, StringComparison comparisonType)
        {
            var index = -1;
            IEnumerable<int> result;
            switch (occurrence)
            {
                case enIndexFinderOccurrence.FirstOccurrence:
                    if (lastIndex != -1)
                    {
                        index = stringToSearchIn.Length - lastIndex - (charsToSearchFor.Length - 1);
                    }
                    result = new[] { index };
                    break;

                case enIndexFinderOccurrence.LastOccurrence:
                    if (firstIndex != -1)
                    {
                        index = stringToSearchIn.Length - firstIndex - (charsToSearchFor.Length - 1);
                    }
                    result = new[] { index };
                    break;

                case enIndexFinderOccurrence.AllOccurrences:
                    var foundIndexes = new List<int>();
                    stringToSearchIn = stringToSearchIn.ReverseString();
                    var currentIndex = firstIndex;
                    while (currentIndex != -1 && currentIndex != stringToSearchIn.Length)
                    {
                        currentIndex = stringToSearchIn.IndexOf(charsToSearchFor, currentIndex, comparisonType);
                        if (currentIndex != -1)
                        {
                            currentIndex++;
                            foundIndexes.Add(currentIndex);
                        }
                    }
                    result = foundIndexes.ToArray();
                    break;

                default:
                    throw new Exception(ErrorResource.ErrorInDev2IndexFinder);

            }
            return result;
        }

        IEnumerable<int> LeftToRightIndexSearch(enIndexFinderOccurrence occurrence, int firstIndex, int lastIndex, string stringToSearchIn, string charsToSearchFor, StringComparison comparisonType)
        {
            var index = -1;
            IEnumerable<int> result;
            switch (occurrence)
            {
                case enIndexFinderOccurrence.FirstOccurrence:
                    if (firstIndex != -1)
                    {
                        index = firstIndex + 1;
                    }
                    result = new[] { index };
                    break;

                case enIndexFinderOccurrence.LastOccurrence:
                    if (lastIndex != -1)
                    {
                        index = lastIndex + 1;
                    }
                    result = new[] { index };
                    break;

                case enIndexFinderOccurrence.AllOccurrences:
                    List<int> foundIndexes;
                    foundIndexes = firstIndex != -1 ? FindAll(firstIndex, stringToSearchIn, charsToSearchFor, comparisonType) : new List<int> { firstIndex };
                    result = foundIndexes.ToArray();
                    break;

                default:
                    throw new Exception(ErrorResource.ErrorInDev2IndexFinder);

            }
            return result;
        }

        private static List<int> FindAll(int firstIndex, string stringToSearchIn, string charsToSearchFor, StringComparison comparisonType)
        {
            var foundIndexes = new List<int> { firstIndex + 1 };
            var currentIndex = firstIndex;
            while (currentIndex != -1)
            {
                currentIndex = stringToSearchIn.IndexOf(charsToSearchFor, currentIndex + 1, comparisonType);
                if (currentIndex != -1)
                {
                    foundIndexes.Add(currentIndex + 1);
                }
            }

            return foundIndexes;
        }

        #endregion
    }
}
