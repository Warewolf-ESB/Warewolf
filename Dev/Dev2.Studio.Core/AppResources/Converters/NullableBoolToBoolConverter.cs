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

namespace Dev2.Studio.Core.AppResources.Converters
{
    public class NullableBoolToBoolConverter
#if WINDOWS || NETFRAMEWORK
        : IValueConverter
#endif
    {
        public bool NullValueReplacement { get; set; }

        public NullableBoolToBoolConverter()
        {
            NullValueReplacement = true;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var b = value as bool?;
            return b ?? NullValueReplacement;
        }

        public
#if !(WINDOWS || NETFRAMEWORK)
            static
#endif
			object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
