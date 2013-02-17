using System;
using Dev2.Studio.Core;
using Dev2.Studio.Core.AppResources.Enums;
using Dev2.Studio.Core.AppResources.Repositories;
using Dev2.Studio.Core.Factories;
using Dev2.Studio.Core.InterfaceImplementors;
using Dev2.Studio.Core.Interfaces;

namespace Dev2.Studio.Webs.Callbacks
{
    public class ConnectCallbackHandler : WebsiteCallbackHandler
    {
        readonly int _webServerPort;

        #region CTOR

        public ConnectCallbackHandler()
        {
            Server = new ServerDTO();
            Uri defaultWebServerUri;
            _webServerPort = Uri.TryCreate(StringResources.Uri_WebServer, UriKind.Absolute, out defaultWebServerUri) ? defaultWebServerUri.Port : 80;
        }

        #endregion

        public IServer Server { get; private set; }

        protected override void Save(IEnvironmentModel environmentModel, dynamic jsonObj)
        {
            Save(jsonObj.ResourceID.Value, jsonObj.Address.Value, jsonObj.ResourceName.Value, (int)jsonObj.WebServerPort.Value, environmentModel);
        }

        #region Save

        /// <summary>
        /// Saves the specified connection - method provided for testing.
        /// </summary>
        /// <param name="connectionID">The connection ID.</param>
        /// <param name="connectionUri">The connection URI.</param>
        /// <param name="connectionName">The name of the connection.</param>
        /// <param name="webServerPort">The web server port.</param>
        /// <param name="defaultEnvironment">The environment where the connection will be saved - must ALWAYS be <see cref="EnvironmentRepository.DefaultEnvironment"/>.</param>
        public void Save(string connectionID, string connectionUri, string connectionName, int webServerPort, IEnvironmentModel defaultEnvironment)
        {
            if(string.IsNullOrEmpty(connectionID))
            {
                throw new ArgumentNullException("connectionID");
            }
            if(string.IsNullOrEmpty(connectionUri))
            {
                throw new ArgumentNullException("connectionUri");
            }
            if(string.IsNullOrEmpty(connectionName))
            {
                throw new ArgumentNullException("connectionName");
            }

            Server.ID = Guid.Parse(connectionID).ToString();
            Server.AppAddress = new Uri(connectionUri).ToString();  // validate uri format before assigning
            Server.Alias = connectionName;

            #region Server.WebAddress

            Uri tmp;
            if(Server.AppUri.Host.ToUpper() == "LOCALHOST")
            {
                Server.AppAddress = string.Format("{0}://{1}{2}", Server.AppUri.Scheme, Server.AppUri.Authority.Replace(Server.AppUri.Host, "127.0.0.1"), Server.AppUri.AbsolutePath);
            }
            if(!Uri.TryCreate(string.Format("{0}://{1}:{2}", Server.AppUri.Scheme, Server.AppUri.Host, _webServerPort), UriKind.Absolute, out tmp))
            {
                Uri.TryCreate(StringResources.Uri_WebServer, UriKind.Absolute, out tmp);
            }
            Server.WebAddress = tmp.AbsoluteUri;

            #endregion

            Server.Environment = EnvironmentModelFactory.CreateEnvironmentModel(Server);

            if(defaultEnvironment != null)
            {
                //
                // NOTE: This must ALWAYS save the environment to the server
                //
                ResourceRepository.AddEnvironment(defaultEnvironment, Server.Environment);

                ReloadResource(defaultEnvironment, connectionName, ResourceType.Source);
            }

            if(CurrentEnvironmentRepository != null)
            {
                CurrentEnvironmentRepository.Save(Server.Environment);
            }

            Mediator.SendMessage(MediatorMessages.AddServerToExplorer, Server.Environment);
        }

        #endregion
    }
}
