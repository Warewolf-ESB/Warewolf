using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using Dev2.Studio.Core.Interfaces;
using Unlimited.Framework;

namespace Dev2.Studio.Core.AppResources.Converters
{
    public class LayoutObjectViewModelToImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            LayoutObjectViewModel layoutObjectViewModel = value as LayoutObjectViewModel;
            Uri uri;
            if (layoutObjectViewModel != null && layoutObjectViewModel.LayoutObjectGrid != null && 
                layoutObjectViewModel.LayoutObjectGrid.ResourceModel != null && layoutObjectViewModel.LayoutObjectGrid.ResourceModel.Environment != null)
            {
                if (!Uri.TryCreate(layoutObjectViewModel.IconPath, UriKind.Absolute, out uri))
                {
                    if(!Uri.TryCreate(layoutObjectViewModel.LayoutObjectGrid.ResourceModel.Environment.Connection.AppServerUri, "icons/" + layoutObjectViewModel.IconPath, out uri))
                    {
                        uri = new Uri(new Uri(StringResources.Uri_WebServer), "icons/" + layoutObjectViewModel.IconPath);
                    }
                }
            }
            else
            {
                uri = new Uri(StringResources.Uri_WebServer);
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
