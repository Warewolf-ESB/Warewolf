/*
*  Warewolf - The Easy Service Bus
*  Copyright 2015 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using Dev2.Common.Interfaces.Core.Convertors.DateAndTime;

namespace Dev2.Converters.DateAndTime.TO
{
    internal class DateTimeFormatPartOptionTO : IDateTimeFormatPartOptionTO
    {
        #region Constructor

        public DateTimeFormatPartOptionTO(int length, Func<string, bool, bool> predicate, bool isNumeric,
            IConvertible actualValue, Action<IDateTimeResultTO, bool, IConvertible> assignAction, int resultLength = -1)
        {
            Length = length;
            Predicate = predicate;
            IsNumeric = isNumeric;
            ActualValue = actualValue;
            AssignAction = assignAction;
            if (resultLength == -1)
            {
                ResultLength = Length;
            }
            else
            {
                ResultLength = resultLength;
            }
        }

        #endregion Constructor

        #region Properties

        public int Length { get; set; }
        public int ResultLength { get; set; }
        public Func<string, bool, bool> Predicate { get; set; }
        public IConvertible ActualValue { get; set; }
        public bool IsNumeric { get; set; }
        public Action<IDateTimeResultTO, bool, IConvertible> AssignAction { get; set; }

        #endregion Properties
    }
}