
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
                ImagePath = @"\Images\crossIcon.png";
            }
            else if (errorType == enErrorType.Warning)
            {
                ImagePath = @"\Images\warningIcon.png";
            }
            else if (errorType == enErrorType.Correct)
            {
                ImagePath = @"\Images\tickIcon.png";
            }
            return ImagePath;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}
