using System;
using System.Windows.Data;
using Dev2.Studio.Core.Interfaces;

namespace Dev2.Studio.Core.AppResources.Converters
{
    public class ContextualResourceModelToImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            IContextualResourceModel resource = value as IContextualResourceModel;
            Uri uri;
            if(resource != null)
            {
                if(!Uri.TryCreate(resource.IconPath, UriKind.Absolute, out uri))
                {
                    uri = new Uri(new Uri(resource.Environment.Connection.WebServerUri, "icons/"), resource.IconPath);
                }
            }
            else
            {
                uri = new Uri("");
            }

            return uri;
            //resource.IconPath = string.Concat(MainViewModel.ActiveEnvironment.WebServerAddress,"icons/",data.IconPath);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}
