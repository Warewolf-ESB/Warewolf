using System;
using System.Collections.Generic;
using Dev2.Common.ExtMethods;
using Dev2.Data.Enums;
using Dev2.Data.Interfaces;

namespace Dev2.Data.Operations
{
    public class Dev2IndexFinder : IDev2IndexFinder
    {
        #region Methods

        public IEnumerable<int> FindIndex(string stringToSearchIn, string firstOccurrence, string charsToSearchFor, string direction, bool matchCase, string startIndex)
        {
            enIndexFinderOccurrence occurrence = enIndexFinderOccurrence.FirstOccurrence;
            enIndexFinderDirection dir = enIndexFinderDirection.LeftToRight;
            int startIdx;
            #region Set the enums according to the strings

            switch(firstOccurrence)
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
            }

            switch(direction)
            {
                case "Left to Right":
                    dir = enIndexFinderDirection.LeftToRight;
                    break;

                case "Right to Left":
                    dir = enIndexFinderDirection.RightToLeft;
                    break;
            }

            startIndex = !string.IsNullOrWhiteSpace(startIndex) ? startIndex : "0";
            if(!int.TryParse(startIndex, out startIdx))
            {
                throw new Exception("The start index specified was not a number.");
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
                int firstIndex = stringToSearchIn.IndexOf(charsToSearchFor, startIndex, comparisonType);
                int lastIndex = stringToSearchIn.LastIndexOf(charsToSearchFor, stringToSearchIn.Length - 1, comparisonType);

                if(direction == enIndexFinderDirection.RightToLeft)
                {
                    result = RightToLeftIndexSearch(occurrence, firstIndex, lastIndex, stringToSearchIn, charsToSearchFor,
                                                    comparisonType);
                }
                else
                {
                    result = LeftToRightIndexSearch(occurrence, firstIndex, lastIndex, stringToSearchIn, charsToSearchFor,
                                                    comparisonType);
                }

                #endregion
            }
            return result;
        }

        #endregion

        #region Private Methods

        private IEnumerable<int> RightToLeftIndexSearch(enIndexFinderOccurrence occurrence, int firstIndex, int lastIndex, string stringToSearchIn, string charsToSearchFor, StringComparison comparisonType)
        {
            int index = -1;
            IEnumerable<int> result;
            switch(occurrence)
            {
                case enIndexFinderOccurrence.FirstOccurrence:
                    if(lastIndex != -1)
                    {
                        index = stringToSearchIn.Length - lastIndex - (charsToSearchFor.Length - 1);
                    }
                    result = new[] { index };
                    break;

                case enIndexFinderOccurrence.LastOccurrence:
                    if(firstIndex != -1)
                    {
                        index = stringToSearchIn.Length - firstIndex - (charsToSearchFor.Length - 1);
                    }
                    result = new[] { index };
                    break;

                case enIndexFinderOccurrence.AllOccurrences:
                    List<int> foundIndexes = new List<int>();
                    stringToSearchIn = stringToSearchIn.ReverseString();
                    int currentIndex = firstIndex;
                    while(currentIndex != -1 && currentIndex != stringToSearchIn.Length)
                    {
                        currentIndex = stringToSearchIn.IndexOf(charsToSearchFor, currentIndex, comparisonType);
                        if(currentIndex != -1)
                        {
                            currentIndex++;
                            foundIndexes.Add(currentIndex);
                        }
                    }
                    result = foundIndexes.ToArray();
                    break;

                default:
                    throw new Exception("Error In Dev2IndexFinder");

            }
            return result;
        }

        private IEnumerable<int> LeftToRightIndexSearch(enIndexFinderOccurrence occurrence, int firstIndex, int lastIndex, string stringToSearchIn, string charsToSearchFor, StringComparison comparisonType)
        {
            int index = -1;
            IEnumerable<int> result;
            switch(occurrence)
            {
                case enIndexFinderOccurrence.FirstOccurrence:
                    if(firstIndex != -1)
                    {
                        index = firstIndex + 1;
                    }
                    result = new[] { index };
                    break;

                case enIndexFinderOccurrence.LastOccurrence:
                    if(lastIndex != -1)
                    {
                        index = lastIndex + 1;
                    }
                    result = new[] { index };
                    break;

                case enIndexFinderOccurrence.AllOccurrences:
                    List<int> foundIndexes;
                    if(firstIndex != -1)
                    {
                        foundIndexes = new List<int> { firstIndex + 1 };
                        int currentIndex = firstIndex;
                        while(currentIndex != -1)
                        {

                            currentIndex = stringToSearchIn.IndexOf(charsToSearchFor, currentIndex + 1, comparisonType);
                            if(currentIndex != -1)
                            {
                                foundIndexes.Add(currentIndex + 1);
                            }
                        }
                    }
                    else
                    {
                        foundIndexes = new List<int> { firstIndex };
                    }

                    result = foundIndexes.ToArray();
                    break;

                default:
                    throw new Exception("Error In Dev2IndexFinder");

            }
            return result;
        }

        #endregion
    }
}
