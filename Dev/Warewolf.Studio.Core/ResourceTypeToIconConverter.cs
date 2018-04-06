using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using FontAwesome.WPF;
using System.Windows.Controls;

namespace Warewolf.Studio.Core
{
    public class ResourceTypeToIconConverter : IValueConverter
    {
        readonly ResourceDictionary _dict;
        readonly SolidColorBrush _brush;

        #region Implementation of IValueConverter

        public ResourceTypeToIconConverter()
        {
            const string Pathname = "/Warewolf.Studio.Themes.Luna;component/Images.xaml";
            _dict = Application.LoadComponent(new Uri(Pathname, UriKind.Relative)) as ResourceDictionary;
            
            _brush = new SolidColorBrush(Color.FromArgb(255, 51, 51, 51));
        }
        
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
            {
                switch (value.ToString())
                {
                    case "WorkflowService":
                        return _dict[CustomMenuIcons.WorkflowService] as DrawingImage;
                    case "PluginSource":
                    case "ComPluginSource":
                        return _dict[CustomMenuIcons.PluginSource] as DrawingImage;
                    case "WcfSource":
                        return _dict["WcfEndPointSource"] as DrawingImage;
                    case "EmailSource":
                        return _dict[CustomMenuIcons.EmailSource] as DrawingImage;
                    case "RabbitMQSource":
                        return _dict[CustomMenuIcons.RabbitMqSource] as DrawingImage;
                    case "ExchangeSource":    
                        return _dict[CustomMenuIcons.ExchangeSource] as DrawingImage;
                    case "SqlDatabase":
                        return _dict[CustomMenuIcons.SqlServerSource] as DrawingImage;
                    case "Oracle":
                        return _dict[CustomMenuIcons.OracleSource] as DrawingImage;
                    case "MySqlDatabase":
                        return _dict[CustomMenuIcons.MySqlSource] as DrawingImage;
                    case "PostgreSQL":
                        return _dict["PostgreSource"] as DrawingImage;
                    case "ODBC":
                        return _dict["OdbcSource"] as DrawingImage;
                    case "WebSource":
                        return _dict[CustomMenuIcons.WebSource] as DrawingImage;
                    case "SharepointServerSource":
                        return Application.Current.Resources["SharepointSource"];
                    case "ServerSource":
                    case "Server":
                    case "Dev2Server":
                        return _dict[CustomMenuIcons.ServerSource] as DrawingImage;
                    case "StartPage":
                        var imageSource = ImageAwesome.CreateImageSource(FontAwesomeIcon.Home, _brush);
                        return imageSource;
                    case "OAuth":
                    case "OauthSource":
                    case "DropBoxSource":
                        return Application.Current.Resources["DropboxSource"];
                    case "Scheduler":
                        return ImageAwesome.CreateImageSource(FontAwesomeIcon.History, _brush);
                    case "ServiceTestsViewer":
                        return ImageAwesome.CreateImageSource(FontAwesomeIcon.Flask, _brush);
                    case "MergeConflicts":
                        return _dict[CustomMenuIcons.MergeConflicts] as DrawingImage;
                    case "Search":
                        return ImageAwesome.CreateImageSource(FontAwesomeIcon.Search, _brush);
                    case "Settings":
                        return ImageAwesome.CreateImageSource(FontAwesomeIcon.Cogs, _brush);
                    case "DependencyViewer":
                        return ImageAwesome.CreateImageSource(FontAwesomeIcon.Sitemap, _brush);
                    case "DeployViewer":
                        return ImageAwesome.CreateImageSource(FontAwesomeIcon.PaperPlane, _brush);
                    default:
                        return _dict[CustomMenuIcons.Folder] as DrawingImage;
                }
            }
            return null;
        }


        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => null;

        #endregion
    }
}