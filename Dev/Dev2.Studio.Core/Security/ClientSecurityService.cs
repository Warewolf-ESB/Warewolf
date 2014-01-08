using System.Collections.Generic;
using System.Threading.Tasks;
using Dev2.Controller;
using Dev2.Services.Security;

namespace Dev2.Security
{
    public class ClientSecurityService : SecurityServiceBase
    {
        readonly Network.ServerProxy _serverProxy;

        public ClientSecurityService(Network.ServerProxy serverProxy)
        {
            VerifyArgument.IsNotNull("serverProxy", serverProxy);
            _serverProxy = serverProxy;
        }

        public override void Read()
        {
            Task.Factory.StartNew(() => base.Read());
        }

        protected override List<WindowsGroupPermission> ReadPermissions()
        {

            CommunicationController communicationController = new CommunicationController
            {
                ServiceName = "SecurityReadService"
            };

            return communicationController.ExecuteCommand<List<WindowsGroupPermission>>(_serverProxy, _serverProxy.WorkspaceID);

        }

        protected override void OnDisposed()
        {
        }

    }
}