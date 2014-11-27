
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Collections.ObjectModel;
using Dev2.Data.Binary_Objects;

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.Core.Interfaces.DataList
{
    public interface IDataListItemModel
    {

        #region Properties

        bool IsUsed { get; set; }

        //IDataListValidator Validator { get; set; }

        string LastIndexedName { get; set; }

        bool IsSelected { get; set; }

        string DisplayName { get; set; }

        string Name { get; set; }

        string Description { get; set; }

        IDataListItemModel Parent { get; set; }

        ObservableCollection<IDataListItemModel> Children { get; set; }

        bool HasError { get; set; }

        string ErrorMessage { get; set; }

        bool IsEditable { get; set; }

        enDev2ColumnArgumentDirection ColumnIODirection { get; set; }

        bool IsVisable { get; set; }

        bool IsBlank { get; }

        bool IsRecordset { get; }

        bool IsField { get; }

        bool IsExpanded { get; set; }

        void SetError(string errorMessage);

        void RemoveError();

        #endregion Properties
    }
}
