/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace Dev2.Studio.AppResources.ExtensionMethods
{
    public static class StringExtensions
    {
        public static string ExceptChars(this string str, IEnumerable<char> toExclude)
        {
            var sb = new StringBuilder();
            foreach (char c in str)
            {
                                if(!toExclude.Contains(c))
                {
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }

        public static bool SpaceCaseInsenstiveComparision(this string stringa, string stringb)
        {
            return stringa == null && stringb == null || stringa != null && stringa.ToLower().ExceptChars(new[] { ' ', '\t', '\n', '\r' }).Equals(stringb.ToLower().ExceptChars(new[] { ' ', '\t', '\n', '\r' }));
        }
    }
}
