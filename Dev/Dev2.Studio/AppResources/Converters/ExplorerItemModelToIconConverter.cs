using Dev2.Common.Interfaces.Data;
using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace Dev2.AppResources.Converters
{
    public class ExplorerItemModelToIconConverter : IMultiValueConverter
    {
        #region Implementation of IMultiValueConverter

        /// <summary>
        /// Converts source values to a value for the binding target. The data binding engine calls this method when it propagates the values from source bindings to the binding target.
        /// </summary>
        /// <returns>
        /// A converted value.If the method returns null, the valid null value is used.A return value of <see cref="T:System.Windows.DependencyProperty"/>.<see cref="F:System.Windows.DependencyProperty.UnsetValue"/> indicates that the converter did not produce a value, and that the binding will use the <see cref="P:System.Windows.Data.BindingBase.FallbackValue"/> if it is available, or else will use the default value.A return value of <see cref="T:System.Windows.Data.Binding"/>.<see cref="F:System.Windows.Data.Binding.DoNothing"/> indicates that the binding does not transfer the value or use the <see cref="P:System.Windows.Data.BindingBase.FallbackValue"/> or the default value.
        /// </returns>
        /// <param name="values">The array of values that the source bindings in the <see cref="T:System.Windows.Data.MultiBinding"/> produces. The value <see cref="F:System.Windows.DependencyProperty.UnsetValue"/> indicates that the source binding has no value to provide for conversion.</param><param name="targetType">The type of the binding target property.</param><param name="parameter">The converter parameter to use.</param><param name="culture">The culture to use in the converter.</param>
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            ResourceType resourceType = values[0] is ResourceType ? (ResourceType)values[0] : ResourceType.Unknown;
            bool isExpanded = values[1] is bool && (bool)values[1];
            Uri uri;
            switch(resourceType)
            {
                case ResourceType.WorkflowService:
                    uri = new Uri("pack://application:,,,/Warewolf Studio;component/Images/Workflow-32.png");
                    return new BitmapImage(uri);
                case ResourceType.DbService:
                    uri = new Uri("pack://application:,,,/Warewolf Studio;component/Images/DatabaseService-32.png");
                    return new BitmapImage(uri);
                case ResourceType.PluginService:
                    uri = new Uri("pack://application:,,,/Warewolf Studio;component/Images/PluginService-32.png");
                    return new BitmapImage(uri);
                case ResourceType.WebService:
                    uri = new Uri("pack://application:,,,/Warewolf Studio;component/Images/WebService-32.png");
                    return new BitmapImage(uri);
                case ResourceType.DbSource:
                    uri = new Uri("pack://application:,,,/Warewolf Studio;component/Images/DatabaseSource-32.png");
                    return new BitmapImage(uri);
                case ResourceType.PluginSource:
                    uri = new Uri("pack://application:,,,/Warewolf Studio;component/Images/PluginSource-32.png");
                    return new BitmapImage(uri);
                case ResourceType.WebSource:
                    uri = new Uri("pack://application:,,,/Warewolf Studio;component/Images/WebSource-32.png");
                    return new BitmapImage(uri);
                case ResourceType.EmailSource:
                    uri = new Uri("pack://application:,,,/Warewolf Studio;component/Images/EmailSource-32.png");
                    return new BitmapImage(uri);
                case ResourceType.ServerSource:
                    uri = new Uri("pack://application:,,,/Warewolf Studio;component/Images/ExplorerWarewolfConnection-32.png");
                    return new BitmapImage(uri);
                case ResourceType.Server:
                    uri = new Uri("pack://application:,,,/Warewolf Studio;component/Images/ExplorerWarewolfConnection-32.png");
                    return new BitmapImage(uri);
                case ResourceType.Version:
                case ResourceType.Message:
                    return null;
                case ResourceType.Folder:
                    uri = isExpanded ? new Uri("pack://application:,,,/Warewolf Studio;component/Images/ExplorerFolderOpen-32.png") :
                                       new Uri("pack://application:,,,/Warewolf Studio;component/Images/ExplorerFolder-32.png");

                    return new BitmapImage(uri);
                default:
                    uri = new Uri("pack://application:,,,/Warewolf Studio;component/Images/Workflow-32.png");
                    return new BitmapImage(uri);
            }
        }

        /// <summary>
        /// Converts a binding target value to the source binding values.
        /// </summary>
        /// <returns>
        /// An array of values that have been converted from the target value back to the source values.
        /// </returns>
        /// <param name="value">The value that the binding target produces.</param><param name="targetTypes">The array of types to convert to. The array length indicates the number and types of values that are suggested for the method to return.</param><param name="parameter">The converter parameter to use.</param><param name="culture">The culture to use in the converter.</param>
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return new object[] { };
        }

        #endregion
    }
}
