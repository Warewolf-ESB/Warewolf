/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Globalization;
using System.Windows.Data;
using Dev2.Common.Interfaces.Security;


namespace Dev2.Studio.AppResources.Converters

{
    public class NavigationViewModelPermissionToBooleanConverter : IMultiValueConverter
    {
        public NavigationViewModelPermissionToBooleanConverter()
        {
            DefaultValue = true;
        }

        public object DefaultValue { get; set; }

        #region Implementation of IMultiValueConverter

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var userPermissions = values[0] as Permissions?;
            var resourceType = values[1].ToString();
            if(!userPermissions.HasValue)
            {
                return DefaultValue;
            }

            Enum.TryParse(parameter as string, true, out Permissions requiredPermission);

            return userPermissions.Value.HasFlag(requiredPermission);
        }
        
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return new object[] { };
        }

        #endregion
    }
}
