
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
using System.Windows;
using System.Windows.Data;

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.Core.AppResources.Converters
{
    public class TypeToVisibilityConverter : IValueConverter
    {
        public Type Type { get; set; }

        public Visibility MatchingVisibilityValue { get; set; }
        public Visibility MismatchVisibilityValue { get; set; }

        public TypeToVisibilityConverter()
        {
            MatchingVisibilityValue = Visibility.Visible;
            MismatchVisibilityValue = Visibility.Collapsed;
        }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if(value == null || value.GetType() != Type)
            {
                return MismatchVisibilityValue;
            }

            return MatchingVisibilityValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
