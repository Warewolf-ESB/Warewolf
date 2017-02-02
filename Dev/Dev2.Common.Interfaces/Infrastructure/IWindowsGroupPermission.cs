using System;
using Dev2.Common.Interfaces.Security;
using Newtonsoft.Json;

namespace Dev2.Common.Interfaces.Infrastructure
{
    public interface IWindowsGroupPermission
    {
        bool IsServer { get; set; }
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