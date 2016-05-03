/*
*  Warewolf - The Easy Service Bus
*  Copyright 2016 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

// ReSharper disable once CheckNamespace
using Dev2.Data.Binary_Objects;
using System.Collections.ObjectModel;

namespace Dev2.Studio.Core.Interfaces.DataList
{
    public interface IDataListItemModel
    {
        #region Properties

        string DisplayName { get; set; }

        string Description { get; set; }

        bool Input { get; set; }

        bool Output { get; set; }

        bool IsVisible { get; set; }

        bool IsExpanded { get; set; }

        bool IsUsed { get; set; }

        bool IsHeader { get; set; }

        bool IsCheckBoxVisible { get; set; }

        bool IsSelected { get; set; }

        bool HasError { get; set; }

        string ErrorMessage { get; set; }

        bool IsEditable { get; set; }

        enDev2ColumnArgumentDirection ColumnIODirection { get; set; }

        bool IsBlank { get; }

        #endregion Properties

        void SetError(string errorMessage);

        void RemoveError();

        string ValidateName(string name);

        // tobe removed
        //string LastIndexedName { get; set; }

        //string Name { get; set; }

        //bool IsRecordset { get; }

        //bool IsField { get; }

        // added to others
        //IDataListItemModel Parent { get; set; }

        //ObservableCollection<IDataListItemModel> Children { get; set; }

        //string FilterText { get; set; }

        //void Filter(string searchText);
    }
}