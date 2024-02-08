#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Net;
using System.Net.Http;
using System.Security.Principal;
using Dev2.Common;
using Dev2.Runtime.Interfaces;
using Dev2.Runtime.ServiceModel.Data;
using Microsoft.AspNetCore.SignalR.Client;
using Connection = Dev2.Data.ServiceModel.Connection;

namespace Dev2.Runtime.ServiceModel
{
    public class HubFactory: IHubFactory
    {
        public HubConnection GetHubConnection(Connection connection)
        {
            var serverUser = Common.Utilities.OrginalExecutingUser;
            var principle = serverUser;

            var identity = principle.Identity as WindowsIdentity;
            WindowsImpersonationContext context = null;

            try
            {
                if (identity != null && connection.AuthenticationType == AuthenticationType.Windows)
                {
                    context = identity.Impersonate();
                }

                using (var client = new WebClient())
                {
                    if (connection.AuthenticationType == AuthenticationType.Windows)
                    {
                        client.UseDefaultCredentials = true;
                    }
                    else
                    {
                        client.UseDefaultCredentials = false;

                        //// we to default to the hidden public user name of \, silly know but that is how to get around ntlm auth ;)
                        if (connection.AuthenticationType == AuthenticationType.Public)
                        {
                            connection.UserName = GlobalConstants.PublicUsername;
                            connection.Password = string.Empty;
                        }

                        client.Credentials = new NetworkCredential(connection.UserName, connection.Password);
                    }

                    var connectionAddress = connection.FetchTestConnectionAddress();
                    var hubConnection = new HubConnectionBuilder().WithUrl(connectionAddress, options =>
                    {
                        options.UseDefaultCredentials = client.UseDefaultCredentials;
                        options.Transports = Microsoft.AspNetCore.Http.Connections.HttpTransportType.ServerSentEvents;
                        options.Credentials = client.Credentials;
                    }).Build();


                    ServicePointManager.ServerCertificateValidationCallback = (sender, certificate, chain, errors) => true;


                    if (!hubConnection.StartAsync().Wait(GlobalConstants.NetworkTimeOut))
                    {
                        throw new HttpClientException(new HttpResponseMessage(HttpStatusCode.GatewayTimeout));
                    }

                    return hubConnection;
                }
            }
            finally
            {
                if (context != null && connection.AuthenticationType == AuthenticationType.Windows)
                {
                    context.Undo();
                }
            }
        }
    }
}
