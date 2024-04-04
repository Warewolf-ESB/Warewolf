#pragma warning disable
﻿using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using Dev2;
using Dev2.Common;
using Dev2.Studio.Interfaces;
#if NETFRAMEWORK
using FontAwesome.WPF;
#else
using FontAwesome6;
using FontAwesome6.Fonts;
using Warewolf.Studio.Core.Extensions;
#endif

namespace Warewolf.Studio.Core
{
    public class ResourceIdToIconConverter : IValueConverter
    {
        readonly ResourceDictionary _dict;
        readonly SolidColorBrush _brush;

        #region Implementation of IValueConverter

        public ResourceIdToIconConverter()
        {
            const string Pathname = "/Warewolf.Studio.Themes.Luna;component/Images.xaml";
            _dict = Application.LoadComponent(new Uri(Pathname, UriKind.Relative)) as ResourceDictionary;

            _brush = new SolidColorBrush(Color.FromArgb(255, 51, 51, 51));
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var resourceId = Guid.Parse(value.ToString());

            var shellViewModel = CustomContainer.Get<IShellViewModel>();
            var environmentViewModel = shellViewModel?.ExplorerViewModel?.Environments?.FirstOrDefault(a => a.ResourceId == shellViewModel.ActiveServer.EnvironmentID);
            var resource = environmentViewModel?.UnfilteredChildren?.Flatten(model => model.UnfilteredChildren).FirstOrDefault(c => c.ResourceId == resourceId);

            if (resource != null)
            {
                return Convert(resource);
            }
            return _dict[CustomMenuIcons.WorkflowService] as DrawingImage;
        }

#pragma warning disable S1541 // Methods and properties should not be too complex
        private object Convert(IExplorerItemViewModel resource)
#pragma warning restore S1541 // Methods and properties should not be too complex
        {
            switch (resource.ResourceType)
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
                case "Scheduler":
#if NETFRAMEWORK
                    return ImageAwesome.CreateImageSource(FontAwesomeIcon.History, _brush);
#else
                    return ImageAwesomeExtensions.CreateImageSource(EFontAwesomeIcon.Solid_ClockRotateLeft, _brush);//ImageAwesomeExtensions.CreateImageSource(EFontAwesomeIcon.HISTORY, _brush);
#endif
                case "ServiceTestsViewer":
#if NETFRAMEWORK
                    return ImageAwesome.CreateImageSource(FontAwesomeIcon.Flask, _brush);
#else
                    return ImageAwesomeExtensions.CreateImageSource(EFontAwesomeIcon.Solid_Flask, _brush);//ImageAwesomeExtensions.CreateImageSource(EFontAwesomeIcon.Flask, _brush);
#endif
                case "MergeConflicts":
                    return _dict[CustomMenuIcons.MergeConflicts] as DrawingImage;
                case "Search":
#if NETFRAMEWORK
                    return ImageAwesome.CreateImageSource(FontAwesomeIcon.Search, _brush);
#else
                    return ImageAwesomeExtensions.CreateImageSource(EFontAwesomeIcon.Solid_MagnifyingGlass, _brush);//ImageAwesomeExtensions.CreateImageSource(EFontAwesomeIcon.search, _brush);
#endif
                case "Settings":
#if NETFRAMEWORK
                    return ImageAwesome.CreateImageSource(FontAwesomeIcon.Cogs, _brush);
#else
                    return ImageAwesomeExtensions.CreateImageSource(EFontAwesomeIcon.Solid_Gears, _brush);//ImageAwesomeExtensions.CreateImageSource(EFontAwesomeIcon.Cogs, _brush);
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

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => null;

        #endregion
    }
}
