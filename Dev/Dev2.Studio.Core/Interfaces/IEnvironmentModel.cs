using System;
using Dev2.DataList.Contract.Network;
using Dev2.Network.Execution;
using Dev2.Studio.Core.Wizards.Interfaces;

namespace Dev2.Studio.Core.Interfaces
{
    public interface IEnvironmentModel : IEquatable<IEnvironmentModel>
    {
        Guid ID { get; }
        string Name { get; set; }
        bool IsConnected { get; }

        IStudioEsbChannel DsfChannel { get; }
        INetworkExecutionChannel ExecutionChannel { get; }
        INetworkDataListChannel DataListChannel { get; }
        IEnvironmentConnection Connection { get; }
        IResourceRepository ResourceRepository { get; }
        IWizardEngine WizardEngine { get; }

        void Connect();
        void Disconnect();
        void Connect(IEnvironmentModel model);
        void LoadResources();

        // BUG: 8786 - TWR - 2013.02.20 - Added category
        string Category { get; set; }

        string ToSourceDefinition();
    }
}
