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

namespace Dev2.Common.Interfaces.Core.Convertors.DateAndTime
{
    public interface IDateTimeFormatPartOptionTO
    {
        int Length { get; set; }
        int ResultLength { get; set; }
        Func<string, bool, bool> Predicate { get; set; }
        bool IsNumeric { get; set; }
        IConvertible ActualValue { get; set; }
        Action<IDateTimeResultTO, bool, IConvertible> AssignAction { get; set; }
    }
}