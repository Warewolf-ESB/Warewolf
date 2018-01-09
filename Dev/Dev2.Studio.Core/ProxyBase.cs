using System.Collections.Generic;
using Dev2.Controller;
using Dev2.Studio.Interfaces;

namespace Dev2.Studio.Core
{
    public abstract class ProxyBase
    {

        public ICommunicationControllerFactory CommunicationControllerFactory { get; private set; }
        
        protected readonly IEnvironmentConnection Connection;

        protected ProxyBase(ICommunicationControllerFactory communicationControllerFactory, IEnvironmentConnection connection)
        {
            VerifyArgument.AreNotNull(new Dictionary<string, object>{{"communicationControllerFactory",communicationControllerFactory},{"connection",connection}});
            CommunicationControllerFactory = communicationControllerFactory;
            Connection = connection;
        }

    }
}
