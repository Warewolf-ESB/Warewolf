
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
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using Dev2.Common.Interfaces.Security;

namespace Dev2.Studio.AppResources.Converters
{
    public class NavigationViewModelPermissionVisibilityConverter : IValueConverter
    {
        public NavigationViewModelPermissionVisibilityConverter()
        {
            DefaultValue = Visibility.Collapsed;
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

            return Convert(requiredPermission, userPermissions.Value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }

        object Convert(Permissions requiredPermission, Permissions userPermissions)
        {
            // unauthorized icon is visible if user does not have View and Execute permissions
            var isUnauthorizedIconVisible = userPermissions == Permissions.None
                                            || !(
                                                userPermissions.HasFlag(Permissions.View) ||
                                                userPermissions.HasFlag(Permissions.Execute) ||
                                                userPermissions.HasFlag(Permissions.Contribute) ||
                                                userPermissions.HasFlag(Permissions.Administrator)
                                                );

            var visible = isUnauthorizedIconVisible;
            if(requiredPermission != Permissions.None)
            {
                // View or Execute
                visible = !isUnauthorizedIconVisible
                          && (
                              userPermissions.HasFlag(requiredPermission) ||
                              userPermissions.HasFlag(Permissions.Contribute) ||
                              userPermissions.HasFlag(Permissions.Administrator)
                              );
            }
            return visible ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}
