using System.Collections.Generic;
using Dev2.Common.Interfaces;

namespace Dev2.Common.TimeZoneBuilder
{
    internal class DateTimeFormatForwardLookupsForDotNet : IDateTimeFormatForwardLookupsForDotNet
    {
        internal DateTimeFormatForwardLookupsForDotNet()
        {
            DateTimeFormatForwardLookupsForDotNetLu = new Dictionary<char, List<int>>();
        }

        public Dictionary<char, List<int>> DateTimeFormatForwardLookupsForDotNetLu { get; set; }
        #region Implementation of IDateTimeParserBuilder

        public void Build()
        {

            //
            // Lookups for dddd, ddd, dd, d
            //
            DateTimeFormatForwardLookupsForDotNetLu.Add('d', new List<int> { 4, 3, 2, 1 });

            //
            // Lookups for fffffff, ffffff, fffff, ffff, fff, ff, f
            //
            DateTimeFormatForwardLookupsForDotNetLu.Add('f', new List<int> { 7, 6, 5, 4, 3, 2, 1 });

            //
            // Lookups for FFFFFFF, FFFFFF, FFFFF, FFFF, FFF, FF, F
            //
            DateTimeFormatForwardLookupsForDotNetLu.Add('F', new List<int> { 7, 6, 5, 4, 3, 2, 1 });

            //
            // Lookups for gg, g
            //
            DateTimeFormatForwardLookupsForDotNetLu.Add('g', new List<int> { 2, 1 });

            //
            // Lookups for hh, h
            //
            DateTimeFormatForwardLookupsForDotNetLu.Add('h', new List<int> { 2, 1 });

            //
            // Lookups for HH, H
            //
            DateTimeFormatForwardLookupsForDotNetLu.Add('H', new List<int> { 2, 1 });

            //
            // Lookups for K
            //
            DateTimeFormatForwardLookupsForDotNetLu.Add('K', new List<int> { 1 });

            //
            // Lookups for mm, m
            //
            DateTimeFormatForwardLookupsForDotNetLu.Add('m', new List<int> { 2, 1 });

            //
            // Lookups for MMMM, MMM, MM, M
            //
            DateTimeFormatForwardLookupsForDotNetLu.Add('M', new List<int> { 4, 3, 2, 1 });

            //
            // Lookups for ss, s
            //
            DateTimeFormatForwardLookupsForDotNetLu.Add('s', new List<int> { 2, 1 });

            //
            // Lookups for tt, t
            //
            DateTimeFormatForwardLookupsForDotNetLu.Add('t', new List<int> { 2, 1 });

            //
            // Lookups for yyyyy, yyyy, yyy, yy, y
            //
            DateTimeFormatForwardLookupsForDotNetLu.Add('y', new List<int> { 5, 4, 3, 2, 1 });

            //
            // Lookups for zzz, zz, z
            //
            DateTimeFormatForwardLookupsForDotNetLu.Add('z', new List<int> { 3, 2, 1 });

        }

        #endregion
    }
}