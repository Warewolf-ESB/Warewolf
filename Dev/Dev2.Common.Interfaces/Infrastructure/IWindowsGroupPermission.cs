#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using Dev2.Common.Interfaces.Security;
using Newtonsoft.Json;

namespace Dev2.Common.Interfaces.Infrastructure
{
    public interface IWindowsGroupPermission
    {
        bool IsServer { get; set; }
        string ResourcePath { get; set; }
        Guid ResourceID { get; set; }
        string ResourceName { get; set; }
        string WindowsGroup { get; set; }
        bool IsDeleted { get; set; }
        bool EnableCellEditing { get; set; }
        bool CanRemove { get; }
        bool View { get; set; }
        bool Execute { get; set; }
        bool Contribute { get; set; }
        bool DeployTo { get; set; }
        bool DeployFrom { get; set; }
        bool Administrator { get; set; }
        bool IsNew { get; set; }
        [JsonIgnore]
        Permissions Permissions { get; set; }
        [JsonIgnore]
        bool IsBuiltInAdministrators { get; }
        [JsonIgnore]
        bool IsBuiltInGuests { get; }
        [JsonIgnore]
        bool IsBuiltInGuestsForExecution { get; }
        [JsonIgnore]
        bool IsValid { get; }
    }
}