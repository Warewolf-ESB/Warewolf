using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace Dev2.Runtime.Configuration.Converters
{
    public enum LogicalOperator
    {
        None,
        And,
        Or
    }

    public class MultiBoolToBoolConverter : IMultiValueConverter
    {
        public LogicalOperator Operator { get; set; }

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if(Operator == LogicalOperator.None)
                throw new Exception("Operator need to be specified");

            var list = values.ToList().Cast<bool>();

            if(Operator == LogicalOperator.And)
            {
                return list.All(l => l);
            }

            return list.Any(l => l);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
