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
using System.Windows.Data;
using Dev2.Common.Interfaces.Infrastructure.Providers.Errors;

namespace Dev2.Studio.Core.AppResources.Converters
{
    public class ErrorTypeToImageConverter : IValueConverter
    {
        public string ImagePath { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var errorType = value as ErrorType?;

            switch (errorType)
            {
                case ErrorType.Critical:
                    ImagePath = @"\Images\ServiceStatusError-32.png";
                    break;
                case ErrorType.Warning:
                    ImagePath = @"\Images\ServiceStatusWarning-16.png";
                    break;
                case ErrorType.None:
                    ImagePath = @"\Images\ServiceStatusOK-32.png";
                    break;
                default:
                    ImagePath = @"\Images\ServiceStatusOK-32.png";
                    break;
            }
            return ImagePath;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Binding.DoNothing;
    }

    public class ErrorTypeToTooltipConverter : IValueConverter
    {
        public string Tooltip { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var errorType = value as ErrorType?;

            switch (errorType)
            {
                case ErrorType.Critical:
                    Tooltip = "Critical";
                    break;
                case ErrorType.Warning:
                    Tooltip = "Warning";
                    break;
                case ErrorType.None:
                    Tooltip = "Passed";
                    break;
                default:
                    Tooltip = "Passed";
                    break;
            }
            return Tooltip;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Binding.DoNothing;
    }
}
