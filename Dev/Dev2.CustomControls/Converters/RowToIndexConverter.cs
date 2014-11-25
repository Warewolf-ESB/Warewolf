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
using System.Activities.Presentation.Model;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace Dev2.CustomControls.Converters
{
    public class RowToIndexConverter : MarkupExtension, IValueConverter
    {
        private static RowToIndexConverter _converter;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var row = value as ModelItem;
            if (row != null)
            {
                var collection = row.Parent as ModelItemCollection;
                if (row != null)
                {
                    if (collection != null)
                    {
                        return collection.IndexOf(row) + 1;
                    }
                }
            }
            return -1;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (_converter == null) _converter = new RowToIndexConverter();
            return _converter;
        }
    }
}