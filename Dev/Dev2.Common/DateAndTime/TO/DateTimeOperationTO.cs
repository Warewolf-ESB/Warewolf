/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common.Interfaces.Core.Convertors.DateAndTime;

namespace Dev2.Common.DateAndTime.TO
{
    public class DateTimeOperationTO : IDateTimeOperationTO
    {
        /// <summary>
        ///     Constructor for creating a DateTimeTO
        /// </summary>
        public DateTimeOperationTO()
        {
        }

        /// <summary>
        ///     Constructor for creating a DateTimeTO
        /// </summary>
        public DateTimeOperationTO(string dateTime, string inputFormat, string outputFormat, string timeModifierType,
            int timeModifierAmount, string result)
        {
            //All the properties will be set in the constructor.DONE
            DateTime = dateTime;
            InputFormat = inputFormat;
            OutputFormat = outputFormat;
            TimeModifierType = timeModifierType;
            TimeModifierAmount = timeModifierAmount;
            Result = result;
        }

        /// <summary>
        ///     The property that holds the date time string the user enters into the "Input" box
        /// </summary>
        public string DateTime { get; set; }

        /// <summary>
        ///     The property that holds the input format string the user enters into the "Input Format" box
        /// </summary>
        public string InputFormat { get; set; }

        /// <summary>
        ///     The property that holds the output format string the user enters into the "Output Format" box
        /// </summary>
        public string OutputFormat { get; set; }

        //2012.09.27: massimo.guerrera - Added for the new functionality for the time modification
        /// <summary>
        ///     The property that holds the type the user selects in the "AddType" combobox
        /// </summary>
        public string TimeModifierType { get; set; }

        //2012.09.27: massimo.guerrera - Added for the new functionality for the time modification
        /// <summary>
        ///     The property that holds the amount that the user enters into the "Amount" box
        /// </summary>
        public int TimeModifierAmount { get; set; }

        /// <summary>
        ///     The property that holds the result string the user enters into the "Result" box
        /// </summary>
        public string Result { get; set; }
    }
}