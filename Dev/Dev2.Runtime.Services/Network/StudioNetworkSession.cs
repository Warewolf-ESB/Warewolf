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
using System.Collections.Generic;
using System.Network;

namespace Dev2.DynamicServices
{
    public class StudioNetworkSession : NetworkContext, IStudioNetworkSession
    {
        private readonly object _auxiliaryLock;
        private StudioAccount _account;
        private HashSet<AuxiliaryConnectionRequest> _auxiliaryRequests;

        public StudioNetworkSession()
        {
            _auxiliaryLock = new object();
        }

        public StudioAccount Account
        {
            get { return _account; }
        }

        protected override void OnAttached(InboundAuthenticationBroker broker, NetworkAccount account)
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
                (_auxiliaryRequests ??
                 (_auxiliaryRequests =
                     new HashSet<AuxiliaryConnectionRequest>(AuxiliaryConnectionRequestEqualityComparer.Singleton))).Add
                    (new AuxiliaryConnectionRequest(result = Guid.NewGuid()));
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

        private sealed class AuxiliaryConnectionRequest
        {
            private readonly DateTime _expiration;
            private readonly Guid _identifier;

            public AuxiliaryConnectionRequest(Guid identifier)
            {
                _expiration = DateTime.Now.AddMinutes(2.0);
                _identifier = identifier;
            }

            public DateTime Expiration
            {
                get { return _expiration; }
            }

            public Guid Identifier
            {
                get { return _identifier; }
            }
        }

        private sealed class AuxiliaryConnectionRequestEqualityComparer : IEqualityComparer<AuxiliaryConnectionRequest>
        {
            public static readonly AuxiliaryConnectionRequestEqualityComparer Singleton =
                new AuxiliaryConnectionRequestEqualityComparer();

            public bool Equals(AuxiliaryConnectionRequest x, AuxiliaryConnectionRequest y)
            {
                return x.Identifier == y.Identifier;
            }

            public int GetHashCode(AuxiliaryConnectionRequest obj)
            {
                return obj.Identifier.GetHashCode();
            }
        }
    }
}