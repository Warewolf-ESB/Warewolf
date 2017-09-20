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
using Dev2.Data.Interfaces.Enums;
using Dev2.Studio.Core.Models.DataList;
using Dev2.Studio.Interfaces.DataList;


namespace Dev2.Studio.Core.Factories
{
    public static class DataListItemModelFactory
    {
        public static DataListHeaderItemModel CreateDataListHeaderItem(string displayname)
        {
            DataListHeaderItemModel dataListHeaderModel = new DataListHeaderItemModel(displayname);
            return dataListHeaderModel;
        }

        public static IRecordSetFieldItemModel CreateDataListModel(ItemModel model, string displayname, string description, enDev2ColumnArgumentDirection dev2ColumnArgumentDirection, IDataListItemModel parent, OptomizedObservableCollection<IDataListItemModel> children)
        {
            var dataListModel = new RecordSetFieldItemModel(displayname, parent as IRecordSetItemModel, dev2ColumnArgumentDirection, description, model.HasError, model.ErrorMessage, model.IsEditable, model.IsVisable, model.IsSelected);
            if (parent != null && !String.IsNullOrEmpty(displayname))
            {
                dataListModel.DisplayName = parent.DisplayName + "()." + displayname;
            }
            return dataListModel;
        }

        public static IRecordSetFieldItemModel CreateDataListModel(string displayname) => CreateDataListModel(new ItemModel(), displayname, "", enDev2ColumnArgumentDirection.None, null, null);

        public static IRecordSetItemModel CreateRecordSetItemModel(ItemModel model, string displayname, string description, IDataListItemModel parent, OptomizedObservableCollection<IRecordSetFieldItemModel> children, enDev2ColumnArgumentDirection dev2ColumnArgumentDirection)
        {
            IRecordSetItemModel dataListModel = new RecordSetItemModel(displayname, dev2ColumnArgumentDirection, description, parent, children, model.HasError, model.ErrorMessage, model.IsEditable, model.IsVisable, model.IsSelected);
            if(parent != null && !String.IsNullOrEmpty(displayname))
            {
                dataListModel.DisplayName = parent.DisplayName + "()." + displayname;
            }
            return dataListModel;
        }

        public static IRecordSetItemModel CreateRecordSetItemModel(string displayname) => CreateRecordSetItemModel(new ItemModel(), displayname, "", null, null, enDev2ColumnArgumentDirection.None);
        public static IRecordSetItemModel CreateRecordSetItemModel(string displayname, string description) => CreateRecordSetItemModel(new ItemModel(), displayname, description, null, null, enDev2ColumnArgumentDirection.None);
        public static IRecordSetItemModel CreateRecordSetItemModel(string displayname, string description, enDev2ColumnArgumentDirection dev2ColumnArgumentDirection) => CreateRecordSetItemModel(new ItemModel(), displayname, description, null, null, dev2ColumnArgumentDirection);

        public static IRecordSetFieldItemModel CreateRecordSetFieldItemModel(ItemModel model, string displayname, string description, IRecordSetItemModel parent, enDev2ColumnArgumentDirection dev2ColumnArgumentDirection)
        {
            IRecordSetFieldItemModel dataListModel = new RecordSetFieldItemModel(displayname, parent, dev2ColumnArgumentDirection, description, model.HasError, model.ErrorMessage, model.IsEditable, model.IsVisable, model.IsSelected);
            if (parent != null && !String.IsNullOrEmpty(displayname))
            {
                dataListModel.DisplayName = parent.DisplayName + "()." + displayname;
            }
            return dataListModel;
        }

        public static IRecordSetFieldItemModel CreateRecordSetFieldItemModel(string displayname) => CreateRecordSetFieldItemModel(new ItemModel(), displayname, "", null, enDev2ColumnArgumentDirection.None);
        public static IRecordSetFieldItemModel CreateRecordSetFieldItemModel(string displayname, string description) => CreateRecordSetFieldItemModel(new ItemModel(), displayname, description, null, enDev2ColumnArgumentDirection.None);
        public static IRecordSetFieldItemModel CreateRecordSetFieldItemModel(IRecordSetItemModel parent) => CreateRecordSetFieldItemModel(new ItemModel(), "", "", parent, enDev2ColumnArgumentDirection.None);
        public static IRecordSetFieldItemModel CreateRecordSetFieldItemModel(string displayname, string description, IRecordSetItemModel parent) => CreateRecordSetFieldItemModel(new ItemModel(), displayname, description, parent, enDev2ColumnArgumentDirection.None);

        public static IScalarItemModel CreateScalarItemModel(ItemModel model, string displayname, string description, enDev2ColumnArgumentDirection dev2ColumnArgumentDirection, IDataListItemModel parent, OptomizedObservableCollection<IDataListItemModel> children)
        {
            IScalarItemModel dataListModel = new ScalarItemModel(displayname, dev2ColumnArgumentDirection, description, model.HasError, model.ErrorMessage, model.IsEditable, model.IsVisable, model.IsSelected);
            return dataListModel;
        }

        public static IScalarItemModel CreateScalarItemModel(string displayname) => CreateScalarItemModel(new ItemModel(), displayname, "", enDev2ColumnArgumentDirection.None, null, null);
        public static IScalarItemModel CreateScalarItemModel(string displayname, string description) => CreateScalarItemModel(new ItemModel(), displayname, description, enDev2ColumnArgumentDirection.None, null, null);
        public static IScalarItemModel CreateScalarItemModel(string displayname, string description, enDev2ColumnArgumentDirection dev2ColumnArgumentDirection) => CreateScalarItemModel(new ItemModel(), displayname, description, dev2ColumnArgumentDirection, null, null);
    }
}
