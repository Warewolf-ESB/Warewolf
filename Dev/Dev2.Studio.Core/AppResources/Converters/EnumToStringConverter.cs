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

using System;
using System.Globalization;
#if WINDOWS || NETFRAMEWORK
using System.Windows.Data;
#endif
using Dev2.Common.ExtMethods;
using Dev2.Common.Interfaces.Enums.Enums;


namespace Dev2.Studio.Core.AppResources.Converters
{
    public class EnumToStringConverter
#if WINDOWS || NETFRAMEWORK
        : IValueConverter
#endif
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => (value as Enum).GetDescription();
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Dev2EnumConverter.GetEnumFromStringDiscription(value?.ToString(), targetType);
    }
}
