#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Text;
using System.Xml;
using Dev2.Common;
using Dev2.Data;
using Dev2.Studio.Interfaces;
using Dev2.Studio.Interfaces.DataList;
using Dev2.Studio.ViewModels.DataList;

namespace Dev2.Studio.Core.DataList
{
    class DataListViewModelHelper : IDataListViewModelHelper
    {
        readonly DataListViewModel _dataListViewModel;

        public DataListViewModelHelper(DataListViewModel dataListViewModel)
        {
            _dataListViewModel = dataListViewModel;
        }

        #region Implementation of IDataListViewModelHelper

        public OptomizedObservableCollection<IDataListItemModel> CreateFullDataList()
        {
            var fullDataList = new OptomizedObservableCollection<IDataListItemModel>();
            foreach (var item in _dataListViewModel.ScalarCollection)
            {
                fullDataList.Add(item);
            }
            foreach (var item in _dataListViewModel.RecsetCollection)
            {
                fullDataList.Add(item);
            }
            foreach (var item in _dataListViewModel.ComplexObjectCollection)
            {
                fullDataList.Add(item);
            }
            return fullDataList;
        }

        public bool IsJsonAttribute(XmlNode child)
        {
            var jsonAttribute = false;
            if (child.Attributes == null)
            {
                return false;
            }

            var xmlAttribute = child.Attributes["IsJson"];
            if (xmlAttribute != null)
            {
                bool.TryParse(xmlAttribute.Value, out jsonAttribute);
            }

            return jsonAttribute;
        }
        const string Description = "Description";
        const string IsEditable = "IsEditable";
        public void AddItemToBuilder(StringBuilder result, IDataListItemModel item)
        {
            result.AppendFormat("<{0} {1}=\"{2}\" {3}=\"{4}\" {5}=\"{6}\" ",
                item.DisplayName
                , Description
                , item.Description
                , IsEditable
                , item.IsEditable
                , GlobalConstants.DataListIoColDirection
                , item.ColumnIODirection);
        }

        #endregion
    }
}