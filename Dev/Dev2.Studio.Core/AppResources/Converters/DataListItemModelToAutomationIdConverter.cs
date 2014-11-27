
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
using System.Windows.Data;
using Dev2.Studio.Core.Models.DataList;

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.Core.AppResources.Converters
{
    public class DataListItemModelToAutomationIdConverter : IValueConverter
    {
        const string AutoIdPrefix = "UI_";
        const string AutoIdSufix = "_AutoID";

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            DataListItemModel itemModel = value as DataListItemModel;


            if(itemModel != null)
            {
                if(itemModel.IsRecordset)
                {
                    return string.IsNullOrEmpty(itemModel.DisplayName)
                        ? string.Concat(AutoIdPrefix, "NewRecordSet", AutoIdSufix)
                        : string.Concat(AutoIdPrefix, "RecordSet_", itemModel.DisplayName, AutoIdSufix);
                }
                if(itemModel.IsField)
                {
                    return string.IsNullOrEmpty(itemModel.DisplayName)
                        ? string.Concat(AutoIdPrefix, "NewField", AutoIdSufix)
                        : string.Concat(AutoIdPrefix, "Field_", itemModel.DisplayName, AutoIdSufix);
                }
                return string.IsNullOrEmpty(itemModel.DisplayName)
                    ? string.Concat(AutoIdPrefix, "NewVariable", AutoIdSufix)
                    : string.Concat(AutoIdPrefix, "Variable_", itemModel.DisplayName, AutoIdSufix);
            }
            return itemModel;

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}
