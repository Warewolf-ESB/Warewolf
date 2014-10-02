
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

namespace Microsoft.Win32.TaskScheduler
{
    internal static class TimeSpanExtension
    {
        public static TimeSpan Parse(string value)
        {
            return Parse(value, String.Empty);
        }

        public static TimeSpan Parse(string value, string formattedZero)
        {
            System.Globalization.TimeSpan2FormatInfo fi = System.Globalization.TimeSpan2FormatInfo.CurrentInfo;
            fi.TimeSpanZeroDisplay = formattedZero;
            return TimeSpan2.Parse(value, fi);
        }

        public static string ToString(this TimeSpan span, string format)
        {
            return System.Globalization.TimeSpan2FormatInfo.CurrentInfo.Format(format, span, null);
        }
    }
}
