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
using Dev2.Data.Security;
using Dev2.Runtime.Interfaces;
using Dev2.Runtime.ServiceModel.Data;
using Microsoft.AspNetCore.SignalR.Client;
using Connection = Dev2.Data.ServiceModel.Connection;

namespace Dev2.Runtime.ServiceModel
{
    public class HubFactory : IHubFactory
    {
        //public IHubProxy CreateHubProxy(Connection connection)
        public HubConnection GetHubConnection(Connection connection)
        {
            var serverUser = Common.Utilities.OrginalExecutingUser;
            var principle = serverUser;

            var identity = principle.Identity as WindowsIdentity;
            WindowsIdentity context = null;

            if (identity != null && connection.AuthenticationType == AuthenticationType.Windows)
            {
                context = identity.Impersonate();
                try
                {

                    if (context != null)
                    {
                        return context.RunImpersonated<HubConnection>(() =>
                        {
                            return CreateHubProxyInternal(connection, false);
                        });
                    }
                }
                finally
                {
                    if (context != null && connection.AuthenticationType == AuthenticationType.Windows)
                    {
                        //context.Dispose(); Should not dispose identity passed.
                    }
                }
            }
            else
            {
                return CreateHubProxyInternal(connection, false);
            }
            return null;
        }

        public HubConnection GetTestHubConnection(Connection connection)
        {
            var serverUser = Common.Utilities.OrginalExecutingUser;
            var principle = serverUser;

            var identity = principle.Identity as WindowsIdentity;
            WindowsIdentity context = null;

            if (identity != null && connection.AuthenticationType == AuthenticationType.Windows)
            {
                context = identity.Impersonate();
                try
                {

                    if (context != null)
                    {
                        return context.RunImpersonated<HubConnection>(() =>
                        {
                            return CreateHubProxyInternal(connection, true);
                        });
                    }
                }
                finally
                {
                    if (context != null && connection.AuthenticationType == AuthenticationType.Windows)
                    {
                        //context.Dispose(); Should not dispose identity passed.
                    }
                }
            }
            else
            {
                return CreateHubProxyInternal(connection, true);
            }
            return null;
        }

        private HubConnection CreateHubProxyInternal(Connection connection, bool isCalledForTestConnectionService)
        {
            try
            {
                using (var client = new WebClient())
                {
                    if (connection.AuthenticationType == AuthenticationType.Windows)
                    {
                        client.UseDefaultCredentials = true;
                    }
                    else
                    {
                        //// we to default to the hidden public user name of \, silly know but that is how to get around ntlm auth ;)
                        if (connection.AuthenticationType == AuthenticationType.Public)
                        {
                            if (isCalledForTestConnectionService)
                                client.UseDefaultCredentials = true;
                            else
                            {
                                client.UseDefaultCredentials = false;

                                connection.UserName = GlobalConstants.PublicUsername;
                                connection.Password = string.Empty;
                                
                                client.Credentials = new NetworkCredential(connection.UserName, connection.Password);
                            }
                        }

                    }


                    var credentials = new NetworkCredential(connection.UserName, connection.Password);
                    var connectionAddress = connection.FetchTestConnectionAddress();
                    var hubConnection = new HubConnectionBuilder().WithUrl(connectionAddress, options =>
                    {
                        options.Transports = Microsoft.AspNetCore.Http.Connections.HttpTransportType.ServerSentEvents;
                        options.Credentials = client.Credentials;
                    }).Build();


                    ServicePointManager.ServerCertificateValidationCallback = (sender, certificate, chain, errors) => true;


                    if (!hubConnection.StartAsync().Wait(GlobalConstants.NetworkTimeOut))
                    {
                        throw new Net6.Compatibility.HttpClientException(new HttpResponseMessage(HttpStatusCode.GatewayTimeout));
                    }

                    return hubConnection;
                }
            }
            finally
            {
                // if (context != null && connection.AuthenticationType == AuthenticationType.Windows)
                // {
                //context.Undo();
                // }
            }
        }
    }
}
