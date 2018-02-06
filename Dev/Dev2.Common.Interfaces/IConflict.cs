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
    public interface ICheckable
    {
        bool IsCurrentChecked { get; set; }
    }
    public interface IConflict
    {
        bool IsEmptyItemSelected { get; set; }
        bool HasConflict { get; set; }
        bool IsChecked { get; set; }
        Guid UniqueId { get; set; }
    }

    public interface IMergeArmConnectorConflict : IEquatable<IMergeArmConnectorConflict>
    {
        IArmConnectorConflict Container { get; set; }
        string ArmDescription { get; set; }
        string SourceUniqueId { get; set; }
        string DestinationUniqueId { get; set; }
        bool IsChecked { get; set; }
        string Key { get; set; }
        event Action<IArmConnectorConflict, bool> OnChecked;
    }

    public interface IArmConnectorConflict : IConflict, IEquatable<IArmConnectorConflict>
    {
        IMergeArmConnectorConflict CurrentArmConnector { get; set; }
        IMergeArmConnectorConflict DifferentArmConnector { get; set; }
        string Key { get; set; }
    }
}