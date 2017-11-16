/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


namespace System.Windows.Controls
{
    internal static class AutoCompleteSearch
    {
        public static AutoCompleteFilterPredicate<string> GetFilter(AutoCompleteFilterMode FilterMode)
        {
            switch(FilterMode)
            {
                case AutoCompleteFilterMode.Contains:
                    return Contains;

                case AutoCompleteFilterMode.EqualsCaseSensitive:
                    return EqualsCaseSensitive;

                case AutoCompleteFilterMode.StartsWith:
                    return StartsWith;

                case AutoCompleteFilterMode.None:
                case AutoCompleteFilterMode.Custom:
                default:
                    return null;
            }
        }
        
        public static bool StartsWith(string text, string value)
        {
            return value.StartsWith(text, StringComparison.CurrentCultureIgnoreCase);
        }
        
        public static bool Contains(string text, string value)
        {
            return value.Contains(text, StringComparison.CurrentCultureIgnoreCase);
        }
        
        public static bool Equals(string text, string value)
        {
            return value.Equals(text, StringComparison.CurrentCultureIgnoreCase);
        }
        
        public static bool EqualsCaseSensitive(string text, string value)
        {
            return value.Equals(text, StringComparison.CurrentCulture);
        }
    }
}
