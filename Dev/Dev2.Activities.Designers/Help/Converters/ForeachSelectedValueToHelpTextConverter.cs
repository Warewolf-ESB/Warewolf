using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Dev2.Activities.Help.Converters
{
    [ValueConversion(typeof(string), typeof(string))]
    public class ForeachSelectedValueToHelpTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string result = string.Empty;
            string valueString = value.ToString();
            if (valueString == "* in Range")
            {
                result = HelpTextResources.ForEachRangeHelpText;
            }
            else if (valueString == "* in CSV")
            {
                result = HelpTextResources.ForEachCsvHelpText;
            }
            else if (valueString == "* in Recordset")
            {
                result = HelpTextResources.ForEachRecordHelpText;
            }
            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
