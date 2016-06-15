/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2016 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Globalization;
using System.Windows.Data;
using Dev2.Studio.Core.Interfaces;

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.Core.AppResources.Converters
{
    public class ContextualResourceModelToImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
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

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}
