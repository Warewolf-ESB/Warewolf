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
        
        public List<string> GetNames()
        {
            return _fetchComputers.Invoke();
        }
        
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
            }
            catch (Exception ex)
            {
                if (ex.InnerException is HttpClientException hex)
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
                    result.ErrorMessage = msg.IndexOf(Resources.ConnectionError, StringComparison.Ordinal) >= 0 || msg.IndexOf("Invalid URI:", StringComparison.Ordinal) >= 0 ? ex.Message : Resources.ConnectionError + ex.Message;
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
    }
}
