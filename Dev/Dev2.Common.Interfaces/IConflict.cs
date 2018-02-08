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
using System.ComponentModel;

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
        bool HasConflict { get; set; }
        bool IsChecked { get; set; }
        Guid UniqueId { get; set; }
    }

    public interface IConnectorConflictItem : IConflictItem, IEquatable<IConnectorConflictItem>
    {
        IArmConnectorConflict Container { get; set; }
        string ArmDescription { get; set; }
        Guid SourceUniqueId { get; set; }
        Guid DestinationUniqueId { get; set; }
        string Key { get; set; }
        event Action<IArmConnectorConflict, bool> OnChecked;
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
    }

    public interface IArmConnectorConflict : IConflictRow, IEquatable<IArmConnectorConflict>
    {
        IConnectorConflictItem CurrentArmConnector { get; set; }
        IConnectorConflictItem DifferentArmConnector { get; set; }
        string Key { get; set; }
    }
}
