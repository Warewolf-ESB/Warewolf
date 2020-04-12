using System;
using System.Text;
using System.Threading.Tasks;
using Dev2.Communication;
using Dev2.SignalR.Wrappers;
using Warewolf.Esb;

namespace Warewolf.Client
{
    public class ClusterJoinRequest : IExecutableRequest<ClusterJoinResponse>, ICatalogRequest
    {
        private readonly string _key;

        public ClusterJoinRequest(string key)
        {
            _key = key;
        }

        public IEsbRequest Build()
        {
            var payload = new EsbExecuteRequest { ServiceName = nameof(Service.Cluster.ClusterJoinRequest) };
            payload.AddArgument(Service.Cluster.ClusterJoinRequest.Key, new StringBuilder(_key));
            return payload;
        }

        public Task<ClusterJoinResponse> Execute(IHubProxyWrapper hubProxy)
        {
            return hubProxy.ExecReq2<ClusterJoinResponse>(this);
        }

        public Task<ClusterJoinResponse> Execute(IConnectedHubProxyWrapper hubProxy, int maxRetries)
        {
            return hubProxy.ExecReq3<ClusterJoinResponse>(this, maxRetries);
        }
    }

    public class ClusterJoinResponse
    {
        public Guid Token { get; set; }
    }
}