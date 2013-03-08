using System;
using Dev2.DataList.Contract.Network;
using Dev2.Network.Execution;

namespace Dev2.Studio.Core.Interfaces 
{
    public interface IEnvironmentModel : IEquatable<IEnvironmentModel>
    {
        Guid ID { get; set; }
        string Name { get; set; }
        bool IsConnected { get; }
        Uri DsfAddress { get; set; }
        IStudioEsbChannel DsfChannel { get;  }
        INetworkExecutionChannel ExecutionChannel { get; }
        INetworkDataListChannel DataListChannel { get; }
        IEnvironmentConnection EnvironmentConnection { get; set; }
        IResourceRepository Resources { get; set; }
        Uri WebServerAddress { get; }
        int WebServerPort { get; set; }

        void Connect();
        void Disconnect();
        void Connect(IEnvironmentModel model);
        void LoadResources();

        // BUG: 8786 - TWR - 2013.02.20 - Added category
        string Category { get; set; }

        string ToSourceDefinition();
    }
}
