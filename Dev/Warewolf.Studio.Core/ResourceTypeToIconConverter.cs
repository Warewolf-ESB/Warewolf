#pragma warning disable
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Controls;
#if NETFRAMEWORK
using FontAwesome.WPF;
#else
using FontAwesome6.Fonts;
using FontAwesome6;
using Warewolf.Studio.Core.Extensions;
#endif

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

#pragma warning disable S1541 // Methods and properties should not be too complex
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
#pragma warning restore S1541 // Methods and properties should not be too complex
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
                    case "ElasticsearchSource":
                        return _dict["ElasticsearchSource"] as DrawingImage;
                    case "ODBC":
                        return _dict["OdbcSource"] as DrawingImage;
                    case "RedisSource":
                        return _dict[CustomMenuIcons.RedisSource] as DrawingImage;
                    case "WebSource":
                        return _dict[CustomMenuIcons.WebSource] as DrawingImage;
                    case "SharepointServerSource":
                        return Application.Current.Resources["SharepointSource"];
                    case "ServerSource":
                    case "Server":
                    case "Dev2Server":
                        return _dict[CustomMenuIcons.ServerSource] as DrawingImage;
                    case "StartPage":
#if NETFRAMEWORK
                        var imageSource = ImageAwesome.CreateImageSource(FontAwesomeIcon.Home, _brush);
#else
                        var imageSource = ImageAwesomeExtensions.CreateImageSource(EFontAwesomeIcon.Solid_House, _brush);
#endif
                        return imageSource;
                    case "OAuth":
                    case "OauthSource":
                    case "DropBoxSource":
                        return Application.Current.Resources["DropboxSource"];
                        //TODO: Remove once Triggers is the only entry point.
                    case "Scheduler":
#if NETFRAMEWORK
                        return ImageAwesome.CreateImageSource(FontAwesomeIcon.History, _brush);
#else
                        return ImageAwesomeExtensions.CreateImageSource(EFontAwesomeIcon.Solid_ClockRotateLeft, _brush);
#endif
                    case "Triggers":
#if NETFRAMEWORK
                        return ImageAwesome.CreateImageSource(FontAwesomeIcon.Play, _brush);
#else
                        return ImageAwesomeExtensions.CreateImageSource(EFontAwesomeIcon.Solid_Play, _brush);
#endif
                    case "ServiceTestsViewer":
#if NETFRAMEWORK
                        return ImageAwesome.CreateImageSource(FontAwesomeIcon.Flask, _brush);
#else
                        return ImageAwesomeExtensions.CreateImageSource(EFontAwesomeIcon.Solid_Flask, _brush);
#endif
                    case "MergeConflicts":
                        return _dict[CustomMenuIcons.MergeConflicts] as DrawingImage;
                    case "Search":
#if NETFRAMEWORK
                        return ImageAwesome.CreateImageSource(FontAwesomeIcon.Search, _brush);
#else
                        return ImageAwesomeExtensions.CreateImageSource(EFontAwesomeIcon.Solid_MagnifyingGlass, _brush);
#endif
                    case "Settings":
#if NETFRAMEWORK
                        return ImageAwesome.CreateImageSource(FontAwesomeIcon.Cogs, _brush);
#else
                        return ImageAwesomeExtensions.CreateImageSource(EFontAwesomeIcon.Solid_Gears, _brush);
#endif
                    case "DependencyViewer":
#if NETFRAMEWORK
                        return ImageAwesome.CreateImageSource(FontAwesomeIcon.Sitemap, _brush);
#else
                        return ImageAwesomeExtensions.CreateImageSource(EFontAwesomeIcon.Solid_Sitemap, _brush);
#endif
                    case "DeployViewer":
#if NETFRAMEWORK
                        return ImageAwesome.CreateImageSource(FontAwesomeIcon.PaperPlane, _brush);
#else
                        return ImageAwesomeExtensions.CreateImageSource(EFontAwesomeIcon.Solid_PaperPlane, _brush);
#endif
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