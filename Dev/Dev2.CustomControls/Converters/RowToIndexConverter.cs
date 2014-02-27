using System;
using System.Activities.Presentation.Model;
using System.Windows.Data;
using System.Windows.Markup;

namespace Dev2.CustomControls.Converters
{
    public class RowToIndexConverter : MarkupExtension, IValueConverter
    {
        static RowToIndexConverter _converter;

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            ModelItem row = value as ModelItem;
            if(row != null)
            {
                ModelItemCollection collection = row.Parent as ModelItemCollection;
                if(row != null)
                {
                    if(collection != null)
                    {
                        return collection.IndexOf(row) + 1;
                    }
                }
            }
            return -1;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if(_converter == null) _converter = new RowToIndexConverter();
            return _converter;
        }
    }
}
