using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Network;
using System.Threading;

namespace Dev2.DynamicServices
{
    public class StudioNetworkSession : NetworkContext, IStudioNetworkSession
    {
        private StudioAccount _account;
        private object _auxiliaryLock;
        private HashSet<AuxiliaryConnectionRequest> _auxiliaryRequests;

        public StudioNetworkSession()
        {
            _auxiliaryLock = new object();
        }

        protected override void OnAttached(System.Network.InboundAuthenticationBroker broker, System.Network.NetworkAccount account)
        {
            lock (_auxiliaryLock)
            {
                _auxiliaryRequests = null;
            }

            _account = account as StudioAccount;
        }

        internal Guid NotifyAuxiliaryConnectionRequested()
        {
            Guid result;

            lock (_auxiliaryLock)
            {
                (_auxiliaryRequests ?? (_auxiliaryRequests = new HashSet<AuxiliaryConnectionRequest>(AuxiliaryConnectionRequestEqualityComparer.Singleton))).Add(new AuxiliaryConnectionRequest(result = Guid.NewGuid()));
            }

            return result;
        }

        protected override void OnDetached()
        {
            lock (_auxiliaryLock)
            {
                _auxiliaryRequests = null;
            }

            _account = null;
        }

        private sealed class AuxiliaryConnectionRequestEqualityComparer : IEqualityComparer<AuxiliaryConnectionRequest>
        {
            public static readonly AuxiliaryConnectionRequestEqualityComparer Singleton = new AuxiliaryConnectionRequestEqualityComparer();

            public bool Equals(AuxiliaryConnectionRequest x, AuxiliaryConnectionRequest y)
            {
                return x.Identifier == y.Identifier;
            }

            public int GetHashCode(AuxiliaryConnectionRequest obj)
            {
                return obj.Identifier.GetHashCode();
            }
        }

        private sealed class AuxiliaryConnectionRequest
        {
            private DateTime _expiration;
            private Guid _identifier;

            public DateTime Expiration { get { return _expiration; } }
            public Guid Identifier { get { return _identifier; } }

            public AuxiliaryConnectionRequest(Guid identifier)
            {
                _expiration = DateTime.Now.AddMinutes(2.0);
                _identifier = identifier;
            }
        }
    }
}
