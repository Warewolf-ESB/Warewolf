using System;
using System.Text;
using Dev2.Communication;
using Warewolf.Esb;

namespace Warewolf.Client
{
    public class ClusterJoinRequest : ICatalogRequest
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
    }

    public class ClusterJoinResponse
    {
        public Guid Token { get; set; }
    }
}