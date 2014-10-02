
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


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
