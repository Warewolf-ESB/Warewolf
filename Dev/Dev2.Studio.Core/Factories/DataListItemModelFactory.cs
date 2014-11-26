
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
using Dev2.Data.Binary_Objects;
using Dev2.Studio.Core.Interfaces.DataList;
using Dev2.Studio.Core.Models.DataList;

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.Core.Factories
{
    public static class DataListItemModelFactory
    {
        public static IDataListItemModel CreateDataListItemViewModel(IDataListViewModel dataListViewModel, IDataListItemModel parent)
        {
            return CreateDataListItemViewModel(dataListViewModel, string.Empty, string.Empty, parent, true);
        }

        public static IDataListItemModel CreateDataListItemViewModel(IDataListViewModel dataListViewModel, string name, string description, IDataListItemModel parent)
        {
            return CreateDataListItemViewModel(dataListViewModel, name, description, parent, true);
        }

        // ReSharper disable MethodOverloadWithOptionalParameter
        public static IDataListItemModel CreateDataListItemViewModel(IDataListViewModel dataListViewModel, string name, string description, IDataListItemModel parent, bool isEditable = true, enDev2ColumnArgumentDirection dev2ColumnArgumentDirection = enDev2ColumnArgumentDirection.None)
        // ReSharper restore MethodOverloadWithOptionalParameter
        {
            IDataListItemModel dataListModel = CreateDataListModel(name);
            dataListModel.Description = description;
            dataListModel.Parent = parent;
            dataListModel.IsExpanded = true;
            dataListModel.IsEditable = isEditable;
            return dataListModel;
        }

        public static IDataListItemModel CreateDataListModel(string displayname, string description, enDev2ColumnArgumentDirection dev2ColumnArgumentDirection = enDev2ColumnArgumentDirection.None, IDataListItemModel parent = null, OptomizedObservableCollection<IDataListItemModel> children = null, bool hasError = false, string errorMessage = "", bool isEditable = true, bool isVisable = true, bool isSelected = false)
        {
            IDataListItemModel dataListModel = new DataListItemModel(displayname, dev2ColumnArgumentDirection, description, parent, children, hasError, errorMessage, isEditable, isVisable, isSelected);
            return dataListModel;
        }

        public static IDataListItemModel CreateDataListModel(string displayname, string description = "", IDataListItemModel parent = null, OptomizedObservableCollection<IDataListItemModel> children = null, bool hasError = false, string errorMessage = "", bool isEditable = true, bool isVisable = true, bool isSelected = false, enDev2ColumnArgumentDirection dev2ColumnArgumentDirection = enDev2ColumnArgumentDirection.None)
        {
            IDataListItemModel dataListModel = new DataListItemModel(displayname, dev2ColumnArgumentDirection, description, parent, children, hasError, errorMessage, isEditable, isVisable, isSelected);
            if(parent != null && !String.IsNullOrEmpty(displayname))
            {
                dataListModel.Name = parent.DisplayName + "()." + displayname;
            }
            return dataListModel;
        }

        public static DataListHeaderItemModel CreateDataListHeaderItem(string displayname)
        {
            DataListHeaderItemModel dataListHeaderModel = new DataListHeaderItemModel(displayname);
            return dataListHeaderModel;
        }
    }

}
