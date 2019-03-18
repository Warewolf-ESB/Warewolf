#pragma warning disable CC0091, S1226, S100, CC0044, CC0045, CC0021, S1449, S1541, S1067, S3235, CC0015, S107, S2292, S1450, S105, CC0074, S1135, S101, S3776, CS0168, S2339, CC0031, S3240, CC0020, CS0108, S1694, S1481, CC0008, AD0001, S2328, S2696, S1643, CS0659, CS0067, S104, CC0030, CA2202, S3376, S1185, CS0219, S3253, S1066, CC0075, S3459, S1871, S1125, CS0649, S2737, S1858, CC0082, CC0001, S3241, S2223, S1301, CC0013, S2955, S1944, CS4014, S3052, S2674, S2344, S1939, S1210, CC0033, CC0002, S3458, S3254, S3220, S2197, S1905, S1699, S1659, S1155, CS0105, CC0019, S3626, S3604, S3440, S3256, S2692, S2345, S1109, FS0058, CS1998, CS0661, CS0660, CS0162, CC0089, CC0032, CC0011, CA1001
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
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

        readonly Func<List<string>> _fetchComputers;
        readonly IHubFactory _hubFactory;

        public Connections()
            : this(() => GetComputerNames.ComputerNames, new HubFactory())
        {
        }

        public Connections(Func<List<string>> fetchComputersFn, IHubFactory hubFactory)
        {
            _fetchComputers = fetchComputersFn;
            _hubFactory = hubFactory;
        }

        public List<string> GetNames() => _fetchComputers.Invoke();

        public ValidationResult CanConnectToServer(Dev2.Data.ServiceModel.Connection connection)
        {
            var result = new ValidationResult
            {
                ErrorFields = new ArrayList(new[] { "address" }),
            };

            try
            {
                // Validate URI, ports, etc...
#pragma warning disable S1848 // Objects should not be created to be dropped immediately without being used
                new Uri(connection.Address);
#pragma warning restore S1848 // Objects should not be created to be dropped immediately without being used

                var connectResult = ConnectToServer(connection);
                if (!string.IsNullOrEmpty(connectResult) && connectResult.Contains("FatalError"))
                {
                    var error = XElement.Parse(connectResult);
                    result.IsValid = false;
                    result.ErrorMessage = string.Join(" - ", error.Nodes().Cast<XElement>().Select(n => n.Value));
                }

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

        string GetLastExeptionMessage(Exception ex)
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
            return "Success";
        }
    }
}
