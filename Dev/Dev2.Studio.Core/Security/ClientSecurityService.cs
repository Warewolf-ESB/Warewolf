using System;
using System.Collections.Generic;
using Dev2.Common;
using Dev2.Services.Security;
using Newtonsoft.Json;

namespace Dev2.Security
{
    public class ClientSecurityService : DisposableObject, ISecurityService
    {
        readonly Network.ServerProxy _serverProxy;

        public ClientSecurityService(Network.ServerProxy serverProxy)
        {
            VerifyArgument.IsNotNull("serverProxy", serverProxy);
            _serverProxy = serverProxy;
            Permissions = new List<WindowsGroupPermission>();
        }

        public event EventHandler Changed;
        public IReadOnlyList<WindowsGroupPermission> Permissions { get; private set; }

        protected override void OnDisposed()
        {
        }

        public void Read()
        {
            dynamic dataObj = new Unlimited.Framework.UnlimitedObject();
            dataObj.Service = "SecurityReadService";

            var jsonPermissions = _serverProxy.ExecuteCommand(dataObj.XmlString, _serverProxy.WorkspaceID, GlobalConstants.NullDataListID);

            Permissions = !string.IsNullOrEmpty(jsonPermissions)
                ? JsonConvert.DeserializeObject<List<WindowsGroupPermission>>(jsonPermissions)
                : new List<WindowsGroupPermission>();

            if(Changed != null)
            {
                Changed(this, EventArgs.Empty);
            }
        }
    }
}