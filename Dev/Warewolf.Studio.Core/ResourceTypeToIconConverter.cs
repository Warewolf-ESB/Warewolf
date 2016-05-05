using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using Dev2.Common.Interfaces.Data;
using FontAwesome.WPF;

namespace Warewolf.Studio.Core
{
    public class ResourceTypeToIconConverter : IValueConverter
    {
        ResourceDictionary _dict;
        SolidColorBrush _brush;

        #region Implementation of IValueConverter

        public ResourceTypeToIconConverter()
        {
            const string pathname = "/Warewolf.Studio.Themes.Luna;component/Images.xaml";
            _dict = Application.LoadComponent(new Uri(pathname, System.UriKind.Relative)) as ResourceDictionary;
            
            _brush = new SolidColorBrush(Color.FromArgb(255, 51, 51, 51));
        }

        /// <summary>
        /// Converts a value. 
        /// </summary>
        /// <returns>
        /// A converted value. If the method returns null, the valid null value is used.
        /// </returns>
        /// <param name="value">The value produced by the binding source.</param><param name="targetType">The type of the binding target property.</param><param name="parameter">The converter parameter to use.</param><param name="culture">The culture to use in the converter.</param>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            ResourceType resourceType;
         
            if (value != null && Enum.TryParse(value.ToString(), out resourceType))
            {
                switch (resourceType)
                {
                    case ResourceType.WorkflowService:
                        return _dict[CustomMenuIcons.WorkflowService] as DrawingImage;
                    case ResourceType.DbService:
                        return _dict[CustomMenuIcons.DbService] as DrawingImage;
                    case ResourceType.PluginSource:
                        return _dict[CustomMenuIcons.PluginSource] as DrawingImage;
                    case ResourceType.EmailSource:
                        return _dict[CustomMenuIcons.EmailSource] as DrawingImage;
                    case ResourceType.ExchangeSource:
                        return _dict[CustomMenuIcons.ExchangeSource] as DrawingImage;
                    case ResourceType.WebService:
                        return _dict[CustomMenuIcons.WebService] as DrawingImage;
                    case ResourceType.DbSource:
                        return _dict[CustomMenuIcons.DbSource] as DrawingImage;
                    case ResourceType.PluginService:
                        return _dict[CustomMenuIcons.PluginService] as DrawingImage;
                    case ResourceType.WebSource:
                        return _dict[CustomMenuIcons.WebSource] as DrawingImage;
                    case ResourceType.SharepointServerSource:
                        return Application.Current.Resources["AddSharepointBlackLogo"];
                    case ResourceType.ServerSource:
                        return _dict[CustomMenuIcons.ServerSource] as DrawingImage;
                    case ResourceType.Server:
                        return _dict[CustomMenuIcons.Server] as DrawingImage;
                    case ResourceType.StartPage:
                        var imageSource = ImageAwesome.CreateImageSource(FontAwesomeIcon.Home, _brush);
                        return imageSource;
                    case ResourceType.OauthSource:
                        return Application.Current.Resources["DropboxSource"];
                    case ResourceType.Scheduler:
                        return ImageAwesome.CreateImageSource(FontAwesomeIcon.History, _brush);
                    case ResourceType.Settings:
                        return ImageAwesome.CreateImageSource(FontAwesomeIcon.Cogs, _brush);
                    case ResourceType.DependencyViewer:
                        return ImageAwesome.CreateImageSource(FontAwesomeIcon.Sitemap, _brush);
                    case ResourceType.DeployViewer:
                        return ImageAwesome.CreateImageSource(FontAwesomeIcon.PaperPlane, _brush);
                    default:
                        return _dict[CustomMenuIcons.Folder] as DrawingImage;
                }
            }
            return null;
        }


        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }

        #endregion
    }
}