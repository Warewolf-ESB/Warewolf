/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Dev2.Common;
using Dev2.Common.Common;
using Dev2.Communication;
using Dev2.Runtime.Diagnostics;
using Dev2.Runtime.ServiceModel.Data;
using Microsoft.AspNet.SignalR.Client;
// ReSharper disable ClassWithVirtualMembersNeverInherited.Global
// ReSharper disable MemberCanBePrivate.Global

// ReSharper disable InconsistentNaming
namespace Dev2.Runtime.ServiceModel
{
    public class Connections : ExceptionManager, IConnections
    {

        #region Fields

        // ReSharper disable FieldCanBeMadeReadOnly.Local
        Func<List<string>> _fetchComputers;
        // ReSharper restore FieldCanBeMadeReadOnly.Local

        #endregion

        #region Constructor

        public Connections()
            : this(() => GetComputerNames.ComputerNames)
        {
        }

        public Connections(Func<List<string>> fetchComputersFn)
        {
            _fetchComputers = fetchComputersFn;
        }

        #endregion

        #region Search

        // POST: Service/Connections/Search

        public List<string> GetNames()
        {
            return _fetchComputers.Invoke();
        } 
        #endregion

        #region Test

        // POST: Service/Connections/Test

        #endregion

        public ValidationResult CanConnectToServer(Dev2.Data.ServiceModel.Connection connection)
        {
            var result = new ValidationResult
            {
                ErrorFields = new ArrayList(new[] { "address" }),
            };

            try
            {
                // Validate URI, ports, etc...
                // ReSharper disable ObjectCreationAsStatement
                new Uri(connection.Address);
                // ReSharper restore ObjectCreationAsStatement

                var connectResult = ConnectToServer(connection);
                if(!string.IsNullOrEmpty(connectResult))
                {
                    if(connectResult.Contains("FatalError"))
                    {
                        var error = XElement.Parse(connectResult);
                        result.IsValid = false;
                        result.ErrorMessage = string.Join(" - ", error.Nodes().Cast<XElement>().Select(n => n.Value));
                    }
                }
            }
            catch(Exception ex)
            {
                var hex = ex.InnerException as HttpClientException;
                if(hex != null)
                {
                    result.IsValid = false;  // This we know how to handle this
                    result.ErrorMessage = "Connection Error : " + hex.Response.ReasonPhrase;
                    return result;
                }

                result.IsValid = false;
                // get something more relevant ;)
                if(ex.Message == "One or more errors occurred." && ex.InnerException != null)
                {
                    result.ErrorMessage = "Connection Error : " + ex.InnerException.Message;
                }
                else
                {
                    var msg = ex.Message;
                    if(msg.IndexOf("Connection Error : ", StringComparison.Ordinal) >= 0 || msg.IndexOf("Invalid URI:", StringComparison.Ordinal) >= 0)
                    {
                        result.ErrorMessage = ex.Message;
                    }
                    else
                    {
                        result.ErrorMessage = "Connection Error : " + ex.Message;
                    }

                }
            }

            return result;
        }
        
        public IHubProxy CreateHubProxy(Dev2.Data.ServiceModel.Connection connection)
        {
            var principle = Thread.CurrentPrincipal;
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

                    var hub = new HubConnection(connection.FetchTestConnectionAddress()) { Credentials = client.Credentials };
                    hub.Error += exception => { };
                    ServicePointManager.ServerCertificateValidationCallback = ValidateServerCertificate;
                    var proxy = hub.CreateHubProxy("esb");
                    if (!hub.Start().Wait(GlobalConstants.NetworkTimeOut))
                    {
                        throw new HttpClientException(new HttpResponseMessage(HttpStatusCode.GatewayTimeout));
                    }
                    return proxy;
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
        protected virtual string ConnectToServer(Dev2.Data.ServiceModel.Connection connection)
        {
            // we need to grab the principle and impersonate to properly execute in context of the requesting user ;)
            var proxy = CreateHubProxy(connection);
            CheckServerVersion(proxy);
            return "Success";
        }

        private static void CheckServerVersion(IHubProxy proxy)
        {
            Dev2JsonSerializer serializer = new Dev2JsonSerializer();
            var esbExecuteRequest = new EsbExecuteRequest { ServiceName = "GetServerVersion" };
            Envelope envelope = new Envelope
            {
                Content = serializer.Serialize(esbExecuteRequest),
                Type = typeof(Envelope)
            };
            var messageId = Guid.NewGuid();
            proxy.Invoke<Receipt>("ExecuteCommand", envelope, true, Guid.Empty, Guid.Empty, messageId).Wait();
            Task<string> fragmentInvoke = proxy.Invoke<string>("FetchExecutePayloadFragment", new FutureReceipt { PartID = 0, RequestID = messageId });
            var serverVersion = fragmentInvoke.Result;
            if(!string.IsNullOrEmpty(serverVersion))
            {
                Version sourceVersionNumber;
                Version.TryParse(serverVersion, out sourceVersionNumber);
                Version destVersionNumber;
                Version.TryParse("0.0.0.6", out destVersionNumber);
                if(sourceVersionNumber != null && destVersionNumber != null)
                {
                    if(sourceVersionNumber < destVersionNumber)
                    {
                        throw new VersionConflictException(sourceVersionNumber, destVersionNumber);
                    }
                }
            }
        }

        /// <summary>
        /// Validates the server certificate.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="certificate">The certificate.</param>
        /// <param name="chain">The chain.</param>
        /// <param name="sslpolicyerrors">The policyholders.</param>
        /// <returns></returns>
        static bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslpolicyerrors)
        {
            return true;
        }
    }
}
