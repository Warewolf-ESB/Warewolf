/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using Dev2.SignalR.Wrappers;
using Dev2.SignalR.Wrappers.New;

namespace Warewolf.Client
{
    public class ProxyConnection
    {
        private IConnectedHubProxyWrapper _hubProxy;
        private readonly IHubConnectionWrapper _hubConnection;

        public ProxyConnection(string serverEndpoint, ICredentials credentials)
        {
            ServicePointManager.ServerCertificateValidationCallback = ValidateServerCertificate;

            _hubConnection = new HubConnectionWrapper(serverEndpoint) { Credentials = credentials};
        }
        private static bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) => true;

        public IConnectedHubProxyWrapper ConnectedProxy => _hubProxy ?? (_hubProxy = new ConnectedHubProxy { Connection = _hubConnection, Proxy = _hubConnection.CreateHubProxy("esb") });
    }
}