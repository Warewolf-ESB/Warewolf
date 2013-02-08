using System;
using System.Collections;
using System.DirectoryServices;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Network;
using System.Threading;
using System.Xml.Linq;
using Dev2.DynamicServices;
using Dev2.Runtime.Diagnostics;
using Dev2.Runtime.ServiceModel.Data;
using Newtonsoft.Json;
using Connection = Dev2.Runtime.ServiceModel.Data.Connection;

namespace Dev2.Runtime.ServiceModel
{
    public class Connections : ExceptionManager
    {
        #region Get

        // POST: Service/Connections/Get
        public Connection Get(string resourceID, Guid workspaceID, Guid dataListID)
        {
            var result = new Connection { ResourceID = Guid.Empty, ResourceType = enSourceType.Dev2Server, WebServerPort = Connection.DefaultWebServerPort };
            try
            {
                var xmlStr = Resources.ReadXml(workspaceID, enSourceType.Dev2Server, resourceID);
                if(!string.IsNullOrEmpty(xmlStr))
                {
                    var xml = XElement.Parse(xmlStr);
                    result = new Connection(xml);
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
                if(connection.ResourceID == Guid.Empty)
                {
                    connection.ResourceID = Guid.NewGuid();
                }
                connection.Save(workspaceID, dataListID);
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
            var root = new DirectoryEntry("WinNT:");

            // This search is case-sensitive!
            term = term.ToLower();
            var results = root.Children.Cast<DirectoryEntry>()
                              .SelectMany(dom => dom.Children.Cast<DirectoryEntry>()
                                                    .Where(entry => entry.SchemaClassName == "Computer" && entry.Name.ToLower().Contains(term)))
                              .Select(entry => entry.Name)
                              .ToList();

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
                    case enSourceType.SqlDatabase:
                        break;
                    case enSourceType.MySqlDatabase:
                        break;
                    case enSourceType.WebService:
                        break;
                    case enSourceType.DynamicService:
                        break;
                    case enSourceType.ManagementDynamicService:
                        break;
                    case enSourceType.Plugin:
                        break;
                    case enSourceType.Dev2Server:
                        result = CanConnectServer(connection);
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

        #region CanConnectServer

        ValidationResult CanConnectServer(Connection connection)
        {
            connection.Address = connection.Address.Replace("localhost", "127.0.0.1");
            var uri = new Uri(connection.Address);

            // EXPECTED FORMAT: http://{hostname}:{port}/dsf
            if(uri.Port > 0 && uri.PathAndQuery == "/dsf")
            {
                return CanConnectToTcpClient(connection);
            }

            return new ValidationResult
            {
                IsValid = false,
                ErrorFields = new ArrayList(new[] { "address" }),
                ErrorMessage = "Invalid URI: The format is not correct."
            };
        }

        #endregion

        #region CanConnectToTcpClient

        protected virtual ValidationResult CanConnectToTcpClient(Connection connection)
        {
            var result = new ValidationResult();
            try
            {
                var uriBuilder = new UriBuilder(connection.Address);
                var client = new TCPClient("TestClient");

                #region Try connect

                var connectCallBack = new AutoResetEvent(false);
                NetworkStateEventHandler networkStateHandler = delegate(NetworkHost host, NetworkStateEventArgs args)
                {
                    if(args.FromState == NetworkState.Connecting)
                    {
                        if(!String.IsNullOrEmpty(args.Message))
                        {
                            result.IsValid = false;
                            result.ErrorMessage = args.Message;
                        }
                        connectCallBack.Set();
                    }
                };
                try
                {
                    client.NetworkStateChanged += networkStateHandler;
                    client.Connect(uriBuilder.Host, uriBuilder.Port);
                    connectCallBack.WaitOne();
                }
                finally
                {
                    client.NetworkStateChanged -= networkStateHandler;
                }

                #endregion

                if(result.IsValid)
                {
                    if(connection.AuthenticationType == AuthenticationType.User)
                    {
                        #region Try login

                        var loginCallBack = new AutoResetEvent(false);
                        LoginStateEventHandler loginStateHandler = delegate(NetworkHost host, LoginStateEventArgs args)
                        {
                            if(args.Reply != AuthenticationResponse.Success)
                            {
                                result.IsValid = false;
                                result.ErrorMessage = args.Message;
                            }
                            loginCallBack.Set();
                        };
                        try
                        {
                            client.LoginStateChanged += loginStateHandler;
                            client.Login(new ServerAuthenticationBroker(connection.UserName, connection.Password));
                            loginCallBack.WaitOne();
                        }
                        finally
                        {
                            client.LoginStateChanged -= loginStateHandler;
                        }

                        #endregion
                    }

                    client.Disconnect();
                    result.ErrorFields = new ArrayList(new[] { "userName", "password" });
                }
                else
                {
                    result.ErrorFields = new ArrayList(new[] { "address" });
                }
            }
            catch(Exception ex)
            {
                result.IsValid = false;
                result.ErrorMessage = ex.Message;
            }
            return result;
        }

        #endregion

        #region ServerAuthenticationBroker

        class ServerAuthenticationBroker : OutboundSRPAuthenticationBroker
        {
            public ServerAuthenticationBroker(string username, string password)
                : base(username, password)
            {
                _remoteFirewall = true;
                _localIdentifier = new FourOctetUnion('D', 'E', 'V', '2').Int32;
                _localVersion = new Version(1, 0, 0, 0);
                _localPlatform = Environment.OSVersion.Platform;
                _localServicePack = Environment.OSVersion.ServicePack;
                _localFingerprint = WeaveUtility.GetVolumeSerial(Path.GetPathRoot(Environment.CurrentDirectory)[0].ToString(CultureInfo.InvariantCulture)
                                                                                                                  .ToUpper());
            }

            protected override void OnAuthenticated(ByteBuffer buffer)
            {
            }
        }

        #endregion

    }
}