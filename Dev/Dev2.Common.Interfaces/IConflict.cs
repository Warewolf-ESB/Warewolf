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
    public delegate void ToggledEventHandler(IConflictItem item, bool isChecked);

    public interface IConflictCheckable
    {
        bool IsCurrentChecked { get; set; }
    }
    public interface ICheckable {
        bool IsChecked { get; set; }
        event ToggledEventHandler NotifyIsCheckedChanged;
    }
    public interface IConflict
    {
        bool IsEmptyItemSelected { get; set; }
        bool HasConflict { get; }
        bool IsChecked { get; set; }
        Guid UniqueId { get; set; }
    }

    public interface IConnectorConflictItem : IConflictItem, IEquatable<IConnectorConflictItem>
    {
        string ArmDescription { get; set; }
        Guid SourceUniqueId { get; set; }
        Guid DestinationUniqueId { get; set; }
        string Key { get; set; }
        event Action<IConnectorConflictRow, bool> OnChecked;
        // TODO: implement List<IConnectorConflictItem> Conflicts(conflictsList) which returns the connectors that conflict with this connector
    }

    public interface IConflictItem
    {
        bool IsChecked { get; set; }
        event ToggledEventHandler NotifyIsCheckedChanged;
    }
    public interface IConflictRow : IConflict
    {
        IConflictItem Current { get; }
        IConflictItem Different { get; }
        bool IsCurrentChecked { get; set; }
        bool IsStartNode { get; set; }
    }

    public interface IConnectorConflictRow : IConflictRow, IEquatable<IConnectorConflictRow>
    {
        IConnectorConflictItem CurrentArmConnector { get; set; }
        IConnectorConflictItem DifferentArmConnector { get; set; }
        string Key { get; set; }
    }
}
