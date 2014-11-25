
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/



using System;

namespace Warewolf.ComponentModel
{
    public static class Extensions
    {
        #region ToStringSafe

        public static string ToStringSafe(this string s)
        {
            return string.IsNullOrEmpty(s) ? string.Empty : s;

        }

        public static string ToStringSafe(this object obj)
        {
            return obj == null || Convert.IsDBNull(obj) ? string.Empty : obj.ToString();
        }

        #endregion
    }
}
