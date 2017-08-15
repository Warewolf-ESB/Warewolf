/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Explorer;
using Dev2.Common.Interfaces.Infrastructure;
using Dev2.Common.Interfaces.Security;
using Dev2.Common.Interfaces.Toolbox;
using Dev2.Common.Interfaces.Versioning;
using Dev2.Services.Security;
using Newtonsoft.Json;

namespace Dev2.Studio.Interfaces

{
    public interface IServer : IEquatable<IServer>
    {
        event EventHandler<ConnectedEventArgs> IsConnectedChanged;
        event EventHandler<ResourcesLoadedEventArgs> ResourcesLoaded;
        IAuthorizationService AuthorizationService { get; }
        string Name { get; set; }
        bool IsConnected { get; }
        bool CanStudioExecute { get; set; }
        bool IsAuthorized { get; }
        bool IsAuthorizedDeployFrom { get; }
        bool IsAuthorizedDeployTo { get; }
        bool IsLocalHost { get; }
        bool HasLoadedResources { get; }
        IEnvironmentConnection Connection { get; set; }
        IResourceRepository ResourceRepository { get; }
        void Connect();
        void Disconnect();
        void ForceLoadResources();
        void LoadResources();
        bool IsLocalHostCheck();
        string DisplayName { get;  }
        event EventHandler AuthorizationServiceSet;
        Task<IExplorerItem> LoadExplorer(bool reloadCatalogue = false);
        IList<IToolDescriptor> LoadTools();
        [JsonIgnore]
        IExplorerRepository ExplorerRepository { get; }
        [JsonIgnore]
        IStudioUpdateManager UpdateRepository { get; }
        [JsonIgnore]
        IQueryManager QueryProxy { get; }
        bool AllowEdit { get; }
        List<IWindowsGroupPermission> Permissions { get; set; }
        Guid EnvironmentID { get; }
        Guid? ServerID { get; }
        event PermissionsChanged PermissionsChanged;
        event NetworkStateChanged NetworkStateChanged;
        event ItemAddedEvent ItemAddedEvent;
        string GetServerVersion();
        Task<bool> ConnectAsync();
        bool HasLoaded { get; }
        bool CanDeployTo { get; }
        bool CanDeployFrom { get; }
        IExplorerRepository ProxyLayer { get; }
        Permissions UserPermissions { get; set; }
        IVersionInfo VersionInfo { get; set; }

        string GetMinSupportedVersion();
        Task<List<string>> LoadExplorerDuplicates();
        Permissions GetPermissions(Guid resourceID);
        Dictionary<string, string> GetServerInformation();

    }

    public class ConnectedEventArgs : EventArgs
    {
        public bool IsConnected { get; set; }
    }
    public class ResourcesLoadedEventArgs : EventArgs
    {
        public IServer Model { get; set; }

    }
}
