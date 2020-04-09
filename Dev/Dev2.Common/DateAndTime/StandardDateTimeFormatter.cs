#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common.Interfaces.Core.Convertors.DateAndTime;
using System;
using System.Globalization;

namespace Dev2.Common.DateAndTime
{
    public class StandardDateTimeFormatter : DateTimeFormatter
    {
        public override bool TryFormat(IDateTimeOperationTO dateTimeTO, out string result, out string error)
        {
            if (string.IsNullOrEmpty(dateTimeTO.DateTime))
            {
                dateTimeTO.DateTime = DateTime.Now.ToString(GlobalConstants.Dev2DotNetDefaultDateTimeFormat, CultureInfo.InvariantCulture);
            }

            var internallyParsedValue = DateTime.TryParseExact(dateTimeTO.DateTime.Trim(), dateTimeTO.InputFormat, CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces, out var dateResult);
            if (internallyParsedValue)
            {
                var tmpDateTime = PerformDateTimeModification(dateTimeTO, dateResult);
                result = tmpDateTime.ToString(dateTimeTO.OutputFormat, CultureInfo.InvariantCulture);
                error = "";
            }
            else
            {
                var secondResult = DateTime.Parse(dateTimeTO.DateTime.Trim(), CultureInfo.InvariantCulture);
                var tmpDateTime = PerformDateTimeModification(dateTimeTO, secondResult);
                result = tmpDateTime.ToString(dateTimeTO.OutputFormat, CultureInfo.InvariantCulture);
                error = "";
            }

            return true;
        }

        DateTime PerformDateTimeModification(IDateTimeOperationTO dateTimeTO, DateTime tmpDateTime)
        {
            var dateTime = tmpDateTime;

            if (string.IsNullOrWhiteSpace(dateTimeTO.TimeModifierType))
            {
                return dateTime;
            }

            if (TimeModifiers.TryGetValue(dateTimeTO.TimeModifierType, out var funcToExecute))
            {
                dateTime = funcToExecute(dateTime, dateTimeTO.TimeModifierAmount);
            }

            return dateTime;
        }
    }
}