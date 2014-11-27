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

namespace Dev2.Converters.DateAndTime.TO
{
    public class DateTimeFormatPartTO : IDateTimeFormatPartTO
    {
        #region Constructor

        public DateTimeFormatPartTO(string value, bool isLiteral, string description)
        {
            Value = value;
            Isliteral = isLiteral;
            Description = description;
        }

        #endregion Constructor

        #region Properties

        public string Value { get; set; }
        public bool Isliteral { get; set; }
        public string Description { get; set; }

        #endregion Properties
    }
}