/*
*  Warewolf - The Easy Service Bus
*  Copyright 2015 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common.Interfaces.Core.Convertors.DateAndTime;

namespace Dev2.Converters.DateAndTime.TO
{
    public class TimeZoneTO : ITimeZoneTO
    {
        public TimeZoneTO(string shortName, string name, string longName)
        {
            ShortName = shortName;
            Name = name;
            LongName = longName;
        }

        public string ShortName { get; set; }
        public string Name { get; set; }
        public string LongName { get; set; }
    }
}