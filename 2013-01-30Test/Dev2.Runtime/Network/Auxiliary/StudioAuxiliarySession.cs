using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Network;

namespace Dev2.DynamicServices.Network.Auxiliary
{
    public class StudioAuxiliarySession : NetworkContext
    {
        private StudioAuxiliaryAccount _account;

        public StudioAuxiliarySession()
        {
        }

        protected override void OnAttached(System.Network.InboundAuthenticationBroker broker, System.Network.NetworkAccount account)
        {
            _account = account as StudioAuxiliaryAccount;
        }

        protected override void OnDetached()
        {
            _account = null;
        }
    }
}
