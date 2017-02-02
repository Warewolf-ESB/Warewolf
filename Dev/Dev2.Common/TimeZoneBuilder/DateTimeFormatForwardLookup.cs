using System.Collections.Generic;
using Dev2.Common.Interfaces;

namespace Dev2.Common.TimeZoneBuilder
{
    internal class DateTimeFormatForwardLookup : IDateTimeFormatForwardLookup
    {
        public DateTimeFormatForwardLookup()
        {
            DateTimeFormatForwardLookups = new Dictionary<char, List<int>>();
        }
        public Dictionary<char, List<int>> DateTimeFormatForwardLookups { get; set; }
        #region Implementation of IDateTimeParserBuilder

        public void Build()
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

        #endregion
    }
}