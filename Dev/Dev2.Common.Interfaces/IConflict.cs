/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;

namespace Dev2.Common.Interfaces
{
    public interface IConflictCheckable
    {
        bool IsCurrentChecked { get; set; }
    }
    public interface ICheckable {
        bool IsChecked { get; set; }
        event Action<IConflictItem, bool> NotifyIsCheckedChanged;
    }

    public interface IConnectorConflictItem : IConflictItem, IEquatable<IConnectorConflictItem>
    {
        string ArmDescription { get; set; }
        Guid SourceUniqueId { get; set; }
        Guid DestinationUniqueId { get; set; }
        string Key { get; set; }
        // TODO: implement List<IConnectorConflictItem> Conflicts(conflictsList) which returns the connectors that conflict with this connector
    }

    public interface IConflictItem
    {
        void SetAutoChecked();
        bool IsChecked { get; set; }
        event Action<IConflictItem, bool> NotifyIsCheckedChanged;
    }
    public interface IConflictRow
    {
        IConflictItem Current { get; }
        IConflictItem Different { get; }
        bool IsCurrentChecked { get; set; }
        bool IsStartNode { get; set; }
        bool IsEmptyItemSelected { get; set; }
        bool HasConflict { get; }
        bool IsChecked { get; set; }
        Guid UniqueId { get; }
    }

    public interface IConnectorConflictRow : IConflictRow, IEquatable<IConnectorConflictRow>
    {
        IConnectorConflictItem CurrentArmConnector { get; set; }
        IConnectorConflictItem DifferentArmConnector { get; set; }
        string Key { get; set; }
    }
}
