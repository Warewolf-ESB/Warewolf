
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2016 by Warewolf Ltd <alpha@warewolf.io>
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
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Dev2.Common;
using Dev2.Common.Common;
using Dev2.Common.Interfaces.Data;
using Dev2.Communication;
using Dev2.Runtime.Diagnostics;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.ServiceModel.Data;
using Microsoft.AspNet.SignalR.Client;
using Newtonsoft.Json;

// ReSharper disable InconsistentNaming
namespace Dev2.Runtime.ServiceModel
{
    public class Connections : ExceptionManager
    {

        #region Fields

        // ReSharper disable FieldCanBeMadeReadOnly.Local
        Func<List<string>> _fetchComputers;
        // ReSharper restore FieldCanBeMadeReadOnly.Local

        #endregion

        #region Constructor

        // default constructor to be used in prod.
        public Connections()
            : this(() => GetComputerNames.ComputerNames)
        {
        }

        // here for testing
        public Connections(Func<List<string>> fetchComputersFn)
        {
            _fetchComputers = fetchComputersFn;
        }

        #endregion

        #region Get

        // POST: Service/Connections/Get
        public Dev2.Data.ServiceModel.Connection Get(string resourceID, Guid workspaceID, Guid dataListID)
        {
            var result = new Dev2.Data.ServiceModel.Connection { ResourceID = Guid.Empty, ResourceType = ResourceType.Server, WebServerPort = Dev2.Data.ServiceModel.Connection.DefaultWebServerPort };
            try
            {

                var contents = ResourceCatalog.Instance.GetResourceContents(workspaceID, Guid.Parse(resourceID));
                if(contents != null && contents.Length > 0)
                {
                    var xml = contents.ToXElement();
                    result = new Dev2.Data.ServiceModel.Connection(xml);
                }
            }
            catch(Exception ex)
            {
                RaiseError(ex);
            }
            return result;
        }

        #endregion

        #region Save

        // POST: Service/Connections/Save
        public string Save(string args, Guid workspaceID, Guid dataListID)
        {
            try
            {
                var connection = JsonConvert.DeserializeObject<Dev2.Data.ServiceModel.Connection>(args);

                Uri actualUri;

                if(Uri.TryCreate(connection.Address, UriKind.RelativeOrAbsolute, out actualUri))
                {
                    var port = actualUri.Port;
                    connection.WebServerPort = port;
                }

                // convert public user and pass to proper ntlm user and pass ;)
                if(connection.AuthenticationType == AuthenticationType.Public)
                {
                    connection.UserName = GlobalConstants.PublicUsername;
                    connection.Password = string.Empty;
                }

                ResourceCatalog.Instance.SaveResource(workspaceID, connection);
                return connection.ToString();
            }
            catch(Exception ex)
            {
                RaiseError(ex);
                return new ValidationResult { IsValid = false, ErrorMessage = ex.Message }.ToString();
            }
        }

        #endregion

        #region Search

        // POST: Service/Connections/Search
        public string Search(string term, Guid workspaceID, Guid dataListID)
        {
            if(term == null)
            {
                term = "";
            }
            // This search is case-sensitive!
            term = term.ToLower();

            var tmp = _fetchComputers.Invoke();
            var results = tmp.FindAll(s => s.ToLower().Contains(term));
            return JsonConvert.SerializeObject(results);
        }

        public List<string> GetNames()
        {
            return _fetchComputers.Invoke();
        } 
        #endregion

        #region Test

        // POST: Service/Connections/Test
        public ValidationResult Test(string args, Guid workspaceID, Guid dataListID)
        {
            var result = new ValidationResult
            {
                IsValid = false,
                ErrorMessage = "Unknown connection type."
            };

            try
            {
                var connection = JsonConvert.DeserializeObject<Dev2.Data.ServiceModel.Connection>(args);
                switch(connection.ResourceType)
                {
                    case ResourceType.Server:
                        result = CanConnectToServer(connection);
                        break;
                }
            }
            catch(Exception ex)
            {
                RaiseError(ex);
                result.ErrorMessage = ex.Message;
            }
            return result;
        }

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

        protected virtual string ConnectToServer(Dev2.Data.ServiceModel.Connection connection)
        {
            // we need to grab the principle and impersonate to properly execute in context of the requesting user ;)
            var principle = Thread.CurrentPrincipal;
            var identity = principle.Identity as WindowsIdentity;
            WindowsImpersonationContext context = null;

            try
            {
                if(identity != null && connection.AuthenticationType == AuthenticationType.Windows)
                {
                    context = identity.Impersonate();
                }

                using(var client = new WebClient())
                {
                    if(connection.AuthenticationType == AuthenticationType.Windows)
                    {
                        client.UseDefaultCredentials = true;
                    }
                    else
                    {
                        client.UseDefaultCredentials = false;

                        //// we to default to the hidden public user name of \, silly know but that is how to get around ntlm auth ;)
                        if(connection.AuthenticationType == AuthenticationType.Public)
                        {
                            connection.UserName = GlobalConstants.PublicUsername;
                            connection.Password = string.Empty;
                        }

                        client.Credentials = new NetworkCredential(connection.UserName, connection.Password);
                    }

                    // Need to do hub connect here to get true permissions ;)
                    HubConnection hub = null;
                    try
                    {
                        // Credentials = client.Credentials 
                        hub = new HubConnection(connection.FetchTestConnectionAddress()) { Credentials = client.Credentials };
                        hub.Error +=exception => {};
                        ServicePointManager.ServerCertificateValidationCallback = ValidateServerCertificate;
#pragma warning disable 168
                        var proxy = hub.CreateHubProxy("esb"); // this is the magic line that causes proper validation
#pragma warning restore 168
                        hub.Start().Wait();
                        CheckServerVersion(proxy);
                        Dev2Logger.Debug("Hub State : " + hub.State);
                        return "Success";
                    }
                    finally
                    {
                        if(hub != null)
                        {
                            hub.Stop();
                            hub.Dispose();
                        }
                    }
                }
            }
            finally
            {
                if(context != null && connection.AuthenticationType == AuthenticationType.Windows)
                {
                    context.Undo();
                }
            }
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
