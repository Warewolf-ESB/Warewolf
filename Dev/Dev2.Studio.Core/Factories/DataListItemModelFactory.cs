#pragma warning disable CC0091, S1226, S100, CC0044, CC0045, CC0021, S1449, S1541, S1067, S3235, CC0015, S107, S2292, S1450, S105, CC0074, S1135, S101, S3776, CS0168, S2339, CC0031, S3240, CC0020, CS0108, S1694, S1481, CC0008, AD0001, S2328, S2696, S1643, CS0659, CS0067, S104, CC0030, CA2202, S3376, S1185, CS0219, S3253, S1066, CC0075, S3459, S1871, S1125, CS0649, S2737, S1858, CC0082, CC0001, S3241, S2223, S1301, CC0013, S2955, S1944, CS4014, S3052, S2674, S2344, S1939, S1210, CC0033, CC0002, S3458, S3254, S3220, S2197, S1905, S1699, S1659, S1155, CS0105, CC0019, S3626, S3604, S3440, S3256, S2692, S2345, S1109, FS0058, CS1998, CS0661, CS0660, CS0162, CC0089, CC0032, CC0011, CA1001
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
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
            var dataListHeaderModel = new DataListHeaderItemModel(displayname);
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

        public static IRecordSetFieldItemModel CreateDataListModel(ItemModel model, string displayname, string description, IRecordSetItemModel recordSetItemModel, enDev2ColumnArgumentDirection dev2ColumnArgumentDirection) => CreateDataListModel(model, displayname, description, dev2ColumnArgumentDirection, null, null);
        public static IRecordSetFieldItemModel CreateDataListModel(string displayname, string description, enDev2ColumnArgumentDirection dev2ColumnArgumentDirection, IRecordSetItemModel recordSetItemModel) => CreateDataListModel(new ItemModel(), displayname, description, dev2ColumnArgumentDirection, null, null);
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

        public static IRecordSetItemModel CreateRecordSetItemModel(string displayname, OptomizedObservableCollection<IRecordSetFieldItemModel> children) => CreateRecordSetItemModel(new ItemModel(), displayname, "", null, children, enDev2ColumnArgumentDirection.None);
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
