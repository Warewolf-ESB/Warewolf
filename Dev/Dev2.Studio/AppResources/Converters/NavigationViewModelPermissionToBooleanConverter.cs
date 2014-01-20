using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using Dev2.Services.Security;

namespace Dev2.Studio.AppResources.Converters
{
    public class NavigationViewModelPermissionToBooleanConverter : IValueConverter
    {
        public NavigationViewModelPermissionToBooleanConverter()
        {
            DefaultValue = true;
        }

        public object DefaultValue { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var userPermissions = value as Permissions?;
            if(!userPermissions.HasValue)
            {
                return DefaultValue;
            }

            Permissions requiredPermission;
            Enum.TryParse(parameter as string, true, out requiredPermission);

            return userPermissions.Value.HasFlag(requiredPermission);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }      
    }
}
