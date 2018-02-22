/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common.Interfaces;

namespace Dev2.ViewModels.Merge.Utils
{
    /// <summary>
    /// Manage the selection of items in a
    /// list of IConflictRow entries.
    /// </summary>
    public class ConflictListStateApplier
    {
        readonly ConflictRowList conflictRowList;
        // TODO: this state applier should also be used to disable Connectors that are unavailable
        public ConflictListStateApplier(ConflictRowList conflicts)
        {
            this.conflictRowList = conflicts;
            //RegisterEventHandlerForConflictItemChanges()
        }
        public void SetConnectorSelectionsToCurrentState()
        {
            foreach (var row in conflictRowList)
            {
                if (row.Current is IToolConflictItem toolConflictItem && !(row.Current is ToolConflictItem.Empty))
                {
                    toolConflictItem.IsChecked = true;
                    foreach (var connector in toolConflictItem.OutboundConnectors)
                    {
                        connector.IsChecked = true;
                    }
                }
            }
        }

        public void RegisterEventHandlerForConflictItemChanges()
        {
            foreach (var row in conflictRowList)
            {
                row.Current.NotifyIsCheckedChanged += ConflictItemIsCheckedChangedHandler;
                row.Different.NotifyIsCheckedChanged += ConflictItemIsCheckedChangedHandler;
            }
        }

        static void ConflictItemIsCheckedChangedHandler(IConflictItem changedItem, bool isChecked)
        {
            if (changedItem is ConnectorConflictItem connectorConflictItem)
            {
                connectorConflictItem.DestinationConflictItem().IsChecked = true;
            }
        }
    }
}
