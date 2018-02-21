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
            RegisterEventHandlerForConflictItemChanges();
            RegisterConnectorEventHandlerForTool();
        }
        public void SetConnectorSelectionsToCurrentState()
        {
            foreach (var conflict in conflictRowList)
            {
                if (conflict is IConflictCheckable check)
                {
                    check.IsCurrentChecked = true;
                }

                if (conflict.Current is ConnectorConflictItem.Empty)
                {
                    conflict.Different.IsChecked = true;
                }
                if (conflict.Different is ConnectorConflictItem.Empty)
                {
                    conflict.Current.IsChecked = true;
                }
            }
        }

        // NEW
        public void RegisterEventHandlerForConflictItemChanges()
        {
            foreach (var row in conflictRowList)
            {
                row.Current.NotifyIsCheckedChanged += (current, isChecked) =>
                {
                    Handler(current, row.Different);
                };
                row.Different.NotifyIsCheckedChanged += (diff, isChecked) =>
                {
                    Handler(row.Current, diff);
                };
            }
        }

        private void RegisterConnectorEventHandlerForTool()
        {
            ToolConflictRow lastToolRow = null;
            foreach (var row in conflictRowList)
            {
                if (row is ToolConflictRow toolRow)
                {
                    lastToolRow = toolRow;
                }
                if (row is ConnectorConflictRow connectorRow)
                {
                    if (lastToolRow == null)
                    {
                        throw new System.Exception("Invalid connector sequence detected");
                    }
                    lastToolRow.Current.NotifyIsCheckedChanged += (item, isChecked) => {
                        connectorRow.CurrentArmConnector.AllowSelection = item.IsChecked;
                    };
                    lastToolRow.Different.NotifyIsCheckedChanged += (item, isChecked) => {
                        connectorRow.DifferentArmConnector.AllowSelection = item.IsChecked;
                    };
                }
            }
        }

        private void Handler(IConflictItem currentItem, IConflictItem diffItem)
        {
            if (currentItem is ConnectorConflictItem connectorConflictItemCurrent)
            {
                SetDependentConnectorsToDisabled(connectorConflictItemCurrent);
            }
            if (diffItem is ConnectorConflictItem connectorConflictItemDiff)
            {
                SetDependentConnectorsToDisabled(connectorConflictItemDiff);
            }
        }

        private void SetDependentConnectorsToDisabled(IConnectorConflictItem conflictItem)
        {
            if (conflictItem.IsChecked)
            {
                return;
            }

            var found = false;
            foreach (var row in conflictRowList)
            {
                // get as connector

                // test if same as event item
                if (row.Current.Equals(conflictItem))
                {
                    found = true;
                }
                // set all connectors after we find the disabled connector
                // unless we find a connector that was manually set by the user (then we should break)
                if (found)
                {
                    row.Current.AllowSelection = false;
                }
            }
        }
    }
}
