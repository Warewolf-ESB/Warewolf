/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using Dev2.Data;
using Dev2.Data.Binary_Objects;
using Dev2.Studio.Core.Interfaces.DataList;
using Dev2.Studio.Core.Models.DataList;

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.Core.Factories
{
    public static class DataListItemModelFactory
    {
        // ReSharper disable MethodOverloadWithOptionalParameter

        public static IRecordSetFieldItemModel CreateDataListModel(string displayname, string description, enDev2ColumnArgumentDirection dev2ColumnArgumentDirection = enDev2ColumnArgumentDirection.None, IDataListItemModel parent = null, OptomizedObservableCollection<IDataListItemModel> children = null, bool hasError = false, string errorMessage = "", bool isEditable = true, bool isVisable = true, bool isSelected = false)
        {
            var dataListModel = new RecordSetFieldItemModel(displayname,parent as IRecordSetItemModel, dev2ColumnArgumentDirection, description, hasError, errorMessage, isEditable, isVisable, isSelected);
            if (parent != null && !String.IsNullOrEmpty(displayname))
            {
                dataListModel.DisplayName = parent.DisplayName + "()." + displayname;
            }
            return dataListModel;
        }

        public static IRecordSetFieldItemModel CreateDataListModel(string displayname, string description = "", IRecordSetItemModel parent = null, bool hasError = false, string errorMessage = "", bool isEditable = true, bool isVisable = true, bool isSelected = false, enDev2ColumnArgumentDirection dev2ColumnArgumentDirection = enDev2ColumnArgumentDirection.None)
        {
            var dataListModel = new RecordSetFieldItemModel(displayname,parent, dev2ColumnArgumentDirection, description, hasError, errorMessage, isEditable, isVisable, isSelected);
            if (parent != null && !String.IsNullOrEmpty(displayname))
            {
                dataListModel.DisplayName = parent.DisplayName + "()." + displayname;
            }
            return dataListModel;
        }

        public static DataListHeaderItemModel CreateDataListHeaderItem(string displayname)
        {
            DataListHeaderItemModel dataListHeaderModel = new DataListHeaderItemModel(displayname);
            return dataListHeaderModel;
        }

        public static IRecordSetItemModel CreateRecordSetItemModel(string displayname, string description = "", IDataListItemModel parent = null, OptomizedObservableCollection<IRecordSetFieldItemModel> children = null, bool hasError = false, string errorMessage = "", bool isEditable = true, bool isVisable = true, bool isSelected = false, enDev2ColumnArgumentDirection dev2ColumnArgumentDirection = enDev2ColumnArgumentDirection.None)
        {
            IRecordSetItemModel dataListModel = new RecordSetItemModel(displayname, dev2ColumnArgumentDirection, description, parent, children, hasError, errorMessage, isEditable, isVisable, isSelected);
            if(parent != null && !String.IsNullOrEmpty(displayname))
            {
                dataListModel.DisplayName = parent.DisplayName + "()." + displayname;
            }
            return dataListModel;
        }

        public static IRecordSetFieldItemModel CreateRecordSetFieldItemModel(string displayname, string description = "", IRecordSetItemModel parent = null, bool hasError = false, string errorMessage = "", bool isEditable = true, bool isVisable = true, bool isSelected = false, enDev2ColumnArgumentDirection dev2ColumnArgumentDirection = enDev2ColumnArgumentDirection.None)
        {
            IRecordSetFieldItemModel dataListModel = new RecordSetFieldItemModel(displayname, parent, dev2ColumnArgumentDirection, description, hasError, errorMessage, isEditable, isVisable, isSelected);
            if (parent != null && !String.IsNullOrEmpty(displayname))
            {
                dataListModel.DisplayName = parent.DisplayName + "()." + displayname;
            }
            return dataListModel;
        }
        
        public static IScalarItemModel CreateScalarItemModel(string displayname, string description = "", enDev2ColumnArgumentDirection dev2ColumnArgumentDirection = enDev2ColumnArgumentDirection.None, IDataListItemModel parent = null, OptomizedObservableCollection<IDataListItemModel> children = null, bool hasError = false, string errorMessage = "", bool isEditable = true, bool isVisable = true, bool isSelected = false)
        {
            IScalarItemModel dataListModel = new ScalarItemModel(displayname, dev2ColumnArgumentDirection, description, hasError, errorMessage, isEditable, isVisable, isSelected);
            return dataListModel;
        }
        
    }
}
