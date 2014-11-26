
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using Dev2.Services.Security;

// ReSharper disable CheckNamespace
namespace Dev2.Studio.Core.Interfaces
// ReSharper restore CheckNamespace
{
    public interface IEnvironmentModel : IEquatable<IEnvironmentModel>
    {
        // BUG 9940 - 2013.07.29 - TWR - added
        event EventHandler<ConnectedEventArgs> IsConnectedChanged;
        event EventHandler<ResourcesLoadedEventArgs> ResourcesLoaded;
        IAuthorizationService AuthorizationService { get; }
        Guid ID { get; }
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
        void Connect(IEnvironmentModel model);
        void ForceLoadResources();
        void LoadResources();
        bool IsLocalHostCheck();

        // BUG: 8786 - TWR - 2013.02.20 - Added category
        string Category { get; set; }
        string DisplayName { get; }
        void RaiseResourcesLoaded();

        event EventHandler AuthorizationServiceSet;
    }

    public class ConnectedEventArgs : EventArgs
    {
        public bool IsConnected { get; set; }
    }
    public class ResourcesLoadedEventArgs : EventArgs
    {
        public IEnvironmentModel Model { get; set; }

    }
}
