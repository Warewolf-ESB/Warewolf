using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using Dev2.Common.Interfaces.Data;

namespace Dev2.Converters
{
    public class TypeToIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var iconValue = Application.Current.Resources["NoIcon"];
            if(value != null)
            {
                ResourceType resourceType;
                Enum.TryParse(value.ToString(), true, out resourceType);
                if(resourceType == ResourceType.WorkflowService)
                {
                    iconValue = Application.Current.Resources["WorkflowIcon"];
                }
            }
            return iconValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new Exception("The method or operation is not implemented.");
        }
    }

}
