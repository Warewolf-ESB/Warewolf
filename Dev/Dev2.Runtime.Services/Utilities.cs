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
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Dev2.Runtime
{
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public static class Utilities
    {
        #region GenerateString

        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        public static string GenerateString(this Random random, int length, string prefix = "")
        {
            var modulo = length / 3;
            var builder = new StringBuilder(prefix);
            for(var k = 0; k < length; k++)
            {
                var ch = Convert.ToChar(Convert.ToInt32(Math.Floor(26 * random.NextDouble() + 65)));
                if(k % modulo != 0)
                {
                    ch = char.ToLower(ch);
                }
                builder.Append(ch);
            }
            return builder.ToString();
        }

        #endregion

    }
}
