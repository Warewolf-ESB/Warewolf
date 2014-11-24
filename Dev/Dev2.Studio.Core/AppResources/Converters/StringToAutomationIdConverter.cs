
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
using System.Globalization;
using System.Windows.Data;

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.Core.AppResources.Converters
{
    public class StringToAutomationIdConverter : IValueConverter
    {
        const string AutoIdPrefix = "UI_";
        const string AutoIdSufix = "_AutoID";

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string s = value as string;


            if(!string.IsNullOrEmpty(s))
            {
                //Ashley: Remove server address part of localhost strings
                s = s.Contains(@"localhost (http://") ? "localhost" : s;
                return string.Concat(AutoIdPrefix, s, AutoIdSufix);
            }
            return s;

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}
