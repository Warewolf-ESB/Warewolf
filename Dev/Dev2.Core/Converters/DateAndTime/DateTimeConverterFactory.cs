/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using Dev2.Common.Interfaces.Core.Convertors.DateAndTime;

namespace Dev2.Converters.DateAndTime
{
    public class DateTimeConverterFactory
    {
        /// <summary>
        ///     Instantiates a concreate implementation of the IDateTimeFormatter
        /// </summary>
        public static IDateTimeFormatter CreateFormatter()
        {
            return new DateTimeFormatter();
        }

        /// <summary>
        ///     Instantiates a concreate implementation of the IDateTimeParser
        /// </summary>
        public static IDateTimeParser CreateParser()
        {
            return new DateTimeParser();
        }

        /// <summary>
        ///     Instantiates a concreate implementation of the IDateTimeComparer
        /// </summary>
        public static IDateTimeComparer CreateComparer()
        {
            return new DateTimeComparer();
        }

        /// <summary>
        ///     Instantiates a concreate implementation of the DateTimeDiffTO
        /// </summary>
        public static IDateTimeDiffTO CreateDateTimeDiffTO(string input1, string input2, string inputFormat,
            string outputType)
        {
            return new DateTimeDiffTO(input1, input2, inputFormat, outputType);
        }

        /// <summary>
        ///     Instantiates a concreate implementation of the DateTimeTO
        /// </summary>
        public static IDateTimeOperationTO CreateDateTimeTO(string dateTime, string inputFormat, string outputFormat,
            string timeModifierType, int timeModifierAmount, string result)
        {
            return new DateTimeOperationTO(dateTime, inputFormat, outputFormat, timeModifierType, timeModifierAmount,
                result);
        }
    }
}