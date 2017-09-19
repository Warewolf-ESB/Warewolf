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
using System.Threading.Tasks;
using System.Xml.Linq;
using Dev2.Common;
using Dev2.Common.Common;
using Dev2.Communication;
using Dev2.Runtime.Diagnostics;
using Dev2.Runtime.Interfaces;
using Microsoft.AspNet.SignalR.Client;
using Dev2.DynamicServices;

namespace Dev2.Runtime.ServiceModel
{
    public class Connections : ExceptionManager, IConnections
    {

        private readonly Func<List<string>> _fetchComputers;
        private readonly IHubFactory _hubFactory;

        public Connections()
            : this(() => GetComputerNames.ComputerNames, new HubFactory())
        {
        }

        public Connections(Func<List<string>> fetchComputersFn, IHubFactory hubFactory)
        {
            _fetchComputers = fetchComputersFn;
            _hubFactory = hubFactory;
        }

        // POST: Service/Connections/Search

        public List<string> GetNames()
        {
            return _fetchComputers.Invoke();
        }

        // POST: Service/Connections/Test

        public ValidationResult CanConnectToServer(Dev2.Data.ServiceModel.Connection connection)
        {
            var result = new ValidationResult
            {
                ErrorFields = new ArrayList(new[] { "address" }),
            };

            try
            {
                // Validate URI, ports, etc...
                new Uri(connection.Address);


                var connectResult = ConnectToServer(connection);
                if (!string.IsNullOrEmpty(connectResult))
                {
                    if (connectResult.Contains("FatalError"))
                    {
                        var error = XElement.Parse(connectResult);
                        result.IsValid = false;
                        result.ErrorMessage = string.Join(" - ", error.Nodes().Cast<XElement>().Select(n => n.Value));
                    }
                }
            }
            catch (Exception ex)
            {
                var hex = ex.InnerException as HttpClientException;
                if (hex != null)
                {
                    result.IsValid = false;  // This we know how to handle this
                    result.ErrorMessage = Resources.ConnectionError + hex.Response.ReasonPhrase;
                    return result;
                }

                result.IsValid = false;
                // get something more relevant ;)
                if (ex.Message == "One or more errors occurred." && ex.InnerException != null)
                {
                    result.ErrorMessage = Resources.ConnectionError + GetLastExeptionMessage(ex);
                }
                else
                {
                    var msg = ex.Message;
                    if (msg.IndexOf(Resources.ConnectionError, StringComparison.Ordinal) >= 0 || msg.IndexOf("Invalid URI:", StringComparison.Ordinal) >= 0)
                    {
                        result.ErrorMessage = ex.Message;
                    }
                    else
                    {
                        result.ErrorMessage = Resources.ConnectionError + ex.Message;
                    }
                }
            }

            return result;
        }

        private string GetLastExeptionMessage(Exception ex)
        {
            if (ex.InnerException == null)
            {
                return ex.Message;
            }
            return GetLastExeptionMessage(ex.InnerException);
        }

        public IHubProxy CreateHubProxy(Dev2.Data.ServiceModel.Connection connection) => _hubFactory.CreateHubProxy(connection);

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
            if (!string.IsNullOrEmpty(serverVersion))
            {
                Version.TryParse(serverVersion, out Version sourceVersionNumber);
                Version.TryParse("0.0.0.6", out Version destVersionNumber);
                if (sourceVersionNumber != null && destVersionNumber != null)
                {
                    if (sourceVersionNumber < destVersionNumber)
                    {
                        throw new VersionConflictException(sourceVersionNumber, destVersionNumber);
                    }
                }
            }
        }
    }
}
