
using Dev2.Studio.Core.ErrorHandling;
using System;
using System.Windows.Data;

namespace Dev2.Studio.Core.AppResources.Converters
{
    public class Dev2ErrorTypeToImageConverter : IValueConverter
    {
        public string ImagePath { get; set; }


        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            enErrorType? errorType = value as enErrorType?;

            if (errorType == enErrorType.Critical)
            {
                ImagePath = @"\Images\ServiceStatusError-16.png";
            }
            else if (errorType == enErrorType.Warning)
            {
                ImagePath = @"\Images\ServiceStatusWarning-16.png";
            }
            else if (errorType == enErrorType.Correct)
            {
                ImagePath = @"\Images\ServiceStatusOK-16.png";
            }
            return ImagePath;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}
