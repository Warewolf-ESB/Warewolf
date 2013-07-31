using System;
using Dev2.DataList.Contract.Network;
using Dev2.Network.Execution;
using Dev2.Studio.Core.Wizards.Interfaces;

namespace Dev2.Studio.Core.Interfaces
{
    public interface IEnvironmentModel : IEquatable<IEnvironmentModel>
    {
        // BUG 9940 - 2013.07.29 - TWR - added
        event EventHandler<ConnectedEventArgs> IsConnectedChanged;

        Guid ID { get; }
        string Name { get; set; }
        bool IsConnected { get; }
        bool CanStudioExecute { get; set; }

        IStudioEsbChannel DsfChannel { get; }
        INetworkExecutionChannel ExecutionChannel { get; }
        INetworkDataListChannel DataListChannel { get; }
        IEnvironmentConnection Connection { get; }
        IResourceRepository ResourceRepository { get; }
        IWizardEngine WizardEngine { get; }

        void Connect();
        void Disconnect();
        void Connect(IEnvironmentModel model);
        void ForceLoadResources();
        void LoadResources();
        bool IsLocalHost();

        // BUG: 8786 - TWR - 2013.02.20 - Added category
        string Category { get; set; }

        string ToSourceDefinition();
    }

    public class ConnectedEventArgs : EventArgs
    {
        public bool IsConnected { get; set; }
    }
}
