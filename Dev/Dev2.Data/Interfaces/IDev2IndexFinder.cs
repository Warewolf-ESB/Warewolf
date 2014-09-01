using System.Collections.Generic;
using Dev2.Data.Enums;

namespace Dev2.Data.Interfaces
{
    public interface IDev2IndexFinder
    {
        /// <summary>
        /// Finds the index of a specified character set in a string.
        /// </summary>
        /// <param name="stringToSearchIn">The string to search in.</param>
        /// <param name="firstOccurrence">The first occurrence or the last occurrence.</param>
        /// <param name="charsToSearchFor">The chars to search for.</param>
        /// <param name="direction">The direction.</param>
        /// <param name="matchCase">if set to <c>true</c> [match case].</param>
        /// <param name="startIndex">The start index.</param>
        /// <returns></returns>
        IEnumerable<int> FindIndex(string stringToSearchIn, string firstOccurrence, string charsToSearchFor, string direction, bool matchCase, string startIndex);

        /// <summary>
        /// Finds the index of a specified character set in a string.
        /// </summary>
        /// <param name="stringToSearchIn">The string to search in.</param>
        /// <param name="occurrence">The first occurrence or the last occurrence.</param>
        /// <param name="charsToSearchFor">The chars to search for.</param>
        /// <param name="direction">The direction.</param>
        /// <param name="matchCase">if set to <c>true</c> [match case].</param>
        /// <param name="startIndex">The start index.</param>
        /// <returns></returns>
        IEnumerable<int> FindIndex(string stringToSearchIn, enIndexFinderOccurrence occurrence, string charsToSearchFor,
                         enIndexFinderDirection direction, bool matchCase, int startIndex);
    }
}