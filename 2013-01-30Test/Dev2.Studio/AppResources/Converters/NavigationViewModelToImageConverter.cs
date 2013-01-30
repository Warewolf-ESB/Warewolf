using System;
using System.Windows.Data;
using Dev2.Studio.ViewModels.Navigation;

namespace Dev2.Studio.AppResources.Converters
{
    public class NavigationViewModelToImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var navigationItemViewModel = value as AbstractTreeViewModel;
            if (navigationItemViewModel == null) return value;

            Uri uri;
            if (!Uri.TryCreate(navigationItemViewModel.IconPath, UriKind.Absolute, out uri))
            {
                uri = new Uri(new Uri(navigationItemViewModel.EnvironmentModel.WebServerAddress, "icons/"), navigationItemViewModel.IconPath);
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
