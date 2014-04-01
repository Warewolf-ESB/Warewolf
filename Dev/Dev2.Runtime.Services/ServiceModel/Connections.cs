using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Xml.Linq;
using Dev2.Common;
using Dev2.Common.Common;
using Dev2.Data.ServiceModel;
using Dev2.Runtime.Diagnostics;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.ServiceModel.Data;
using Newtonsoft.Json;
using Connection = Dev2.Data.ServiceModel.Connection;

namespace Dev2.Runtime.ServiceModel
{
    public class Connections : ExceptionManager
    {

        #region Fields

        // ReSharper disable FieldCanBeMadeReadOnly.Local
        Func<List<string>> FetchComputers;
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
            FetchComputers = fetchComputersFn;
        }

        #endregion

        #region Get

        // POST: Service/Connections/Get
        public Connection Get(string resourceID, Guid workspaceID, Guid dataListID)
        {
            var result = new Connection { ResourceID = Guid.Empty, ResourceType = ResourceType.Server, WebServerPort = Connection.DefaultWebServerPort };
            try
            {

                var contents = ResourceCatalog.Instance.GetResourceContents(workspaceID, Guid.Parse(resourceID));
                if(contents != null && contents.Length > 0)
                {
                    var xml = contents.ToXElement();
                    result = new Connection(xml);

                    // now we need to remove \ for display to the user ;)
                    if(result.UserName == GlobalConstants.PublicUsername && string.IsNullOrEmpty(result.Password))
                    {
                        result.UserName = string.Empty;
                    }
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
                var connection = JsonConvert.DeserializeObject<Connection>(args);

                // convert public user and pass to proper ntlm user and pass ;)
                if(string.IsNullOrEmpty(connection.UserName) && string.IsNullOrEmpty(connection.Password))
                {
                    connection.UserName = GlobalConstants.PublicUsername;
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

            var tmp = FetchComputers.Invoke();
            var results = tmp.FindAll(s => s.ToLower().Contains(term));
            return JsonConvert.SerializeObject(results);
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
                var connection = JsonConvert.DeserializeObject<Connection>(args);
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

        ValidationResult CanConnectToServer(Connection connection)
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
            catch(WebException wex)
            {
                result.IsValid = false;
                result.ErrorMessage = string.Format("{0} - {1}", wex.Status, wex.Message);
            }
            catch(Exception ex)
            {
                result.IsValid = false;
                result.ErrorMessage = ex.Message;
            }
            return result;
        }

        protected virtual string ConnectToServer(Connection connection)
        {
            string result;
            using(var client = new WebClient())
            {
                if(connection.AuthenticationType == AuthenticationType.Windows)
                {
                    client.UseDefaultCredentials = true;
                }
                else
                {
                    client.UseDefaultCredentials = false;

                    // we to default to the hidden public user name of \, silly know but that is how to get around ntlm auth ;)
                    if(string.IsNullOrEmpty(connection.UserName) && string.IsNullOrEmpty(connection.Password))
                    {
                        connection.UserName = GlobalConstants.PublicUsername;
                    }

                    client.Credentials = new NetworkCredential(connection.UserName, connection.Password);
                }

                var testAddress = GetTestAddress(connection);
                result = client.DownloadString(testAddress + "/services/ping");
            }
            return result;
        }

        public string GetTestAddress(Connection connection)
        {
            if(connection != null && !string.IsNullOrEmpty(connection.Address))
            {
                var testAddress = connection.Address.Replace("/dsf", "");
                return testAddress;
            }
            return "";
        }
    }
}