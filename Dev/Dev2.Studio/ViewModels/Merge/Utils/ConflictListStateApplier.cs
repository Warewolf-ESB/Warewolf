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
        readonly ConflictRowList conflicts;
        // TODO: this state applier should also be used to disable Connectors that are unavailable
        public ConflictListStateApplier(ConflictRowList conflicts)
        {
            this.conflicts = conflicts;
            RegisterEventHandlerForConflictItemChanges();
        }
        public void SetConnectorSelectionsToCurrentState()
        {
            foreach (var conflict in conflicts)
            {
                if (conflict is IConflictCheckable check)
                {
                    check.IsCurrentChecked = true;
                    // TODO: Verify UI updates based on HasConflicts - every 10 seconds?
                }
            }
        }

        // NEW
        public void RegisterEventHandlerForConflictItemChanges()
        {
            foreach (var conflict in conflicts)
            {
                var innerConflictRow = conflict;

                innerConflictRow.Current.NotifyIsCheckedChanged += (current, isChecked) =>
                {
                    Handler(current, innerConflictRow.Different);
                };
                innerConflictRow.Different.NotifyIsCheckedChanged += (diff, isChecked) =>
                {
                    Handler(innerConflictRow.Current, diff);
                };
            }
        }

        private void Handler(IConflictItem currentItem, IConflictItem diffItem)
        {
            // apply tool or connector state to list view
            // e.g. disable conflicting connectors

        }
    }
}
