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
        readonly ConflictRowList _conflictRowList;
        // TODO: this state applier should also be used to disable Connectors that are unavailable
        public ConflictListStateApplier(ConflictRowList conflicts)
        {
            this._conflictRowList = conflicts;
            RegisterEventHandlerForConflictItemChanges();
        }
        public void SetConnectorSelectionsToCurrentState()
        {
            foreach (var conflict in _conflictRowList)
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

        public void RegisterEventHandlerForConflictItemChanges()
        {
            foreach (var row in _conflictRowList)
            {
                row.Current.NotifyIsCheckedChanged += ConflictItemIsCheckedChangedHandler;
                row.Different.NotifyIsCheckedChanged += ConflictItemIsCheckedChangedHandler;
            }
        }

        static void ConflictItemIsCheckedChangedHandler(IConflictItem changedItem, bool isChecked)
        {
            if (changedItem is ConnectorConflictItem connectorConflictItem)
            {
                var destItem = connectorConflictItem.DestinationConflictItem();
                if (!(destItem is null))
                {
                    destItem.IsChecked = true;
                }
            }
        }
    }
}
