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
    public class DateTimeDiffTO : IDateTimeDiffTO
    {
        #region Ctor

        public DateTimeDiffTO(string input1, string input2, string inputFormat, string outputType)
        {
            Input1 = input1;
            Input2 = input2;
            InputFormat = inputFormat;
            OutputType = outputType;
        }

        #endregion Ctor

        #region Properties

        public string Input1 { get; set; }

        public string Input2 { get; set; }

        public string InputFormat { get; set; }

        public string OutputType { get; set; }

        #endregion Properties
    }
}