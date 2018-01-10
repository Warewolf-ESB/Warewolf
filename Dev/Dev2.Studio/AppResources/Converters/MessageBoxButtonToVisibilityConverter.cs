/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
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

namespace Dev2.Studio.AppResources.Converters
{
    public class MessageBoxButtonToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {

            if (value == null || parameter == null)
            {
                
                return Binding.DoNothing;
            }

            if (!Enum.TryParse(parameter.ToString(), true, out MessageBoxResult buttonType))
            {
                return Binding.DoNothing;
            }

            if (!Enum.TryParse(value.ToString(), true, out MessageBoxButton messageBoxButton))
            {
                return Binding.DoNothing;
            }

            if (buttonType == MessageBoxResult.Cancel && (messageBoxButton == MessageBoxButton.OKCancel || messageBoxButton == MessageBoxButton.YesNoCancel))
            {
                return Visibility.Visible;
            }
            
            if (buttonType == MessageBoxResult.No && (messageBoxButton == MessageBoxButton.YesNo || messageBoxButton == MessageBoxButton.YesNoCancel))
            {
                return Visibility.Visible;
            }

            if (buttonType == MessageBoxResult.OK && (messageBoxButton == MessageBoxButton.OK || messageBoxButton == MessageBoxButton.OKCancel))
            {
                return Visibility.Visible;
            }

            if (buttonType == MessageBoxResult.Yes && (messageBoxButton == MessageBoxButton.YesNo || messageBoxButton == MessageBoxButton.YesNoCancel))
            {
                return Visibility.Visible;
            }

            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
