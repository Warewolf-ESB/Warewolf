using System.Threading.Tasks;
using Dev2.Common;
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

        protected override string ReadPermissions()
        {
            dynamic dataObj = new Unlimited.Framework.UnlimitedObject();
            dataObj.Service = "SecurityReadService";

            return _serverProxy.ExecuteCommand(dataObj.XmlString, _serverProxy.WorkspaceID, GlobalConstants.NullDataListID);
        }

        protected override void OnDisposed()
        {
        }

    }
}