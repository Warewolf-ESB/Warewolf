/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common.DateAndTime.TO;
using Dev2.Common.Interfaces.Core.Convertors.DateAndTime;
using Dev2.Common.TimeZoneBuilder;
using System;
using System.Globalization;

namespace Dev2.Common.DateAndTime
{
    public class StandardDateTimeParser : DateTimeParser
    {
        static StandardDateTimeParser()
        {
            var dateTimeFormatPart = new NewDateTimeFormatPart(TimeZones);
            dateTimeFormatPart.Build();
            _dateTimeFormatsParts = dateTimeFormatPart.DateTimeFormatsParts;
            _dateTimeFormatPartOptions = dateTimeFormatPart.DateTimeFormatPartOptions;
        }       
    }
}



