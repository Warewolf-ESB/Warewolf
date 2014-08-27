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
        IEnvironmentConnection Connection { get; }
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
