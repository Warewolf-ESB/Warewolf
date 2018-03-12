/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common;
using Dev2.Common.Interfaces;
using Dev2.Studio.Interfaces.DataList;

namespace Dev2.Studio.Interfaces
{
    public interface IConflictModelFactory
    {
        string WorkflowName { get; set; }
        string ServerName { get; set; }
        string Header { get; set; }
        string HeaderVersion { get; set; }
        bool IsVariablesChecked { get; set; }
        bool IsWorkflowNameChecked { get; set; }
        IDataListViewModel DataListViewModel { get; set; }
        void GetDataList(IContextualResourceModel resourceModel);

        IToolConflictItem CreateModelItem(IToolConflictItem toolConflictItem, IConflictTreeNode node);

        event ConflictModelChanged SomethingConflictModelChanged;
    }
}
