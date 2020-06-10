#pragma warning disable
﻿/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using Dev2.Common.Interfaces.Core.Convertors.DateAndTime;



namespace Dev2.Common.DateAndTime
{
    public abstract class DateTimeFormatterBase: IDateTimeFormatter
    {
        public abstract bool TryFormat(IDateTimeOperationTO dateTimeTO, out string result, out string error);
        public abstract bool TryFormat(DateTime dateTimeTO, out string result, out string error);
    }
}