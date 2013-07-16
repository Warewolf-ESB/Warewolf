using System;
using System.Collections.Generic;
using System.Network;
using Dev2.Communication;
using Dev2.Data.ServiceModel.Messages;
using Dev2.DynamicServices;
using Dev2.Services;

namespace Dev2.Tests.Runtime
{
    public class MockStudioNetworkServer : StudioNetworkServer
    {
        public MockStudioNetworkServer(string serverName, StudioFileSystem fileSystem, EsbServicesEndpoint channel, Guid serverID)
            : base(serverName, fileSystem, channel, serverID)
        {
        }

        public MockStudioNetworkServer(string serverName, StudioFileSystem fileSystem, EsbServicesEndpoint channel, Guid serverID, bool autoAccountCreation)
            : base(serverName, fileSystem, channel, serverID, autoAccountCreation)
        {
        }

        public MockStudioNetworkServer(string serverName, StudioFileSystem fileSystem, EsbServicesEndpoint channel, Guid serverID, bool autoAccountCreation, IPushService pushService)
            : base(serverName, fileSystem, channel, serverID, autoAccountCreation, pushService)
        {
        }

        public void TestOnCompilerMessageReceived(IList<CompileMessageTO> messages)
        {
            OnCompilerMessageReceived(messages);
        }

        public List<IMemo> WriteEventProviderMemos { get; private set; }
        protected override void WriteEventProviderClientMessage(IMemo memo, IEnumerable<INetworkOperator> operators)
        {
            if(WriteEventProviderMemos == null)
            {
                WriteEventProviderMemos = new List<IMemo>();
            }
            WriteEventProviderMemos.Add(memo);
        }
    }
}
