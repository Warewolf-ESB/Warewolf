﻿/*
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
    }

    public interface IConnectorConflictItem : IConflictItem
    {
        string ArmDescription { get; set; }
        Guid SourceUniqueId { get; set; }
        Guid DestinationUniqueId { get; set; }
        string Key { get; set; }

        IToolConflictItem SourceConflictItem();
        IToolConflictItem DestinationConflictItem();

        IConnectorConflictItem Clone();
    }

    public interface IConflictItem
    {
        void SetAutoChecked();
        bool IsChecked { get; set; }
        bool AllowSelection { get; set; }
        event Action<IConflictItem, bool> NotifyIsCheckedChanged;
    }
    public interface IConflictRow
    {
        IConflictItem Current { get; }
        IConflictItem Different { get; }
        bool IsCurrentChecked { get; set; }
        bool ContainsStart { get; set; }
        bool IsEmptyItemSelected { get; set; }
        bool HasConflict { get; set; }
        bool IsChecked { get; set; }
        Guid UniqueId { get; }
    }

    public interface IConnectorConflictRow : IConflictRow
    {
        IConnectorConflictItem CurrentArmConnector { get; set; }
        IConnectorConflictItem DifferentArmConnector { get; set; }
        string Key { get; set; }
    }
}
