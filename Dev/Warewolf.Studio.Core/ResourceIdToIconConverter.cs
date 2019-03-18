#pragma warning disable CC0091, S1226, S100, CC0044, CC0045, CC0021, S1449, S1541, S1067, S3235, CC0015, S107, S2292, S1450, S105, CC0074, S1135, S101, S3776, CS0168, S2339, CC0031, S3240, CC0020, CS0108, S1694, S1481, CC0008, AD0001, S2328, S2696, S1643, CS0659, CS0067, S104, CC0030, CA2202, S3376, S1185, CS0219, S3253, S1066, CC0075, S3459, S1871, S1125, CS0649, S2737, S1858, CC0082, CC0001, S3241, S2223, S1301, CC0013, S2955, S1944, CS4014, S3052, S2674, S2344, S1939, S1210, CC0033, CC0002, S3458, S3254, S3220, S2197, S1905, S1699, S1659, S1155, CS0105, CC0019, S3626, S3604, S3440, S3256, S2692, S2345, S1109, FS0058, CS1998, CS0661, CS0660, CS0162, CC0089, CC0032, CC0011, CA1001
﻿using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using Dev2;
using Dev2.Common;
using Dev2.Studio.Interfaces;
using FontAwesome.WPF;

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

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => null;

        #endregion
    }
}
