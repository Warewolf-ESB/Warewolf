/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using Warewolf.Resource.Errors;

namespace Dev2.Converters
{
    public class TypeToIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            object iconValue = Application.Current.Resources["NoIcon"];
            if (value != null)
            {
                if (value.ToString() == "WorkflowService")
                {
                    iconValue = Application.Current.Resources["WorkflowIcon"];
                }
            }
            return iconValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new Exception(ErrorResource.MethodNotImplemented);
        }
    }
}