using System;
using Caliburn.Micro;
using Dev2.Providers.Logs;
using Dev2.Services.Events;
using Dev2.Studio.Core;
using Dev2.Studio.Core.AppResources.Enums;
using Dev2.Studio.Core.AppResources.Repositories;
using Dev2.Studio.Core.InterfaceImplementors;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Messages;

namespace Dev2.Studio.Webs.Callbacks
{
    public class ConnectCallbackHandler : WebsiteCallbackHandler
    {
        readonly int _webServerPort;

        #region CTOR

        public ConnectCallbackHandler(Guid? context = null)
            : this(EnvironmentRepository.Instance, context)
        {
        }

        public ConnectCallbackHandler(IEnvironmentRepository currentEnvironmentRepository, Guid? context = null)
            : this(EventPublishers.Aggregator, currentEnvironmentRepository, context)
        {
        }

        public ConnectCallbackHandler(IEventAggregator eventPublisher, IEnvironmentRepository currentEnvironmentRepository, Guid? context = null)
            : base(eventPublisher, currentEnvironmentRepository, context)
        {
            Server = new ServerDTO();
            Uri defaultWebServerUri;
            _webServerPort = Uri.TryCreate(StringResources.Uri_WebServer, UriKind.Absolute, out defaultWebServerUri) ? defaultWebServerUri.Port : 80;
        }

        #endregion

        public IServer Server { get; private set; }

        protected override void Save(IEnvironmentModel environmentModel, dynamic jsonObj)
        {
            Save(jsonObj.ResourceID.Value, jsonObj.ResourcePath.Value, jsonObj.Address.Value, jsonObj.ResourceName.Value, (int)jsonObj.WebServerPort.Value, environmentModel);
        }

        #region Save

        /// <summary>
        /// Saves the specified connection - method provided for testing.
        /// </summary>
        /// <param name="connectionID">The connection ID.</param>
        /// <param name="category">The category.</param>
        /// <param name="connectionUri">The connection URI.</param>
        /// <param name="connectionName">The name of the connection.</param>
        /// <param name="webServerPort">The web server port.</param>
        /// <param name="defaultEnvironment">The environment where the connection will be saved - must ALWAYS be <see cref="EnvironmentRepository.Instance.Source" />.</param>
        /// <exception cref="System.ArgumentNullException">connectionID</exception>
        public void Save(string connectionID, string category, string connectionUri, string connectionName, int webServerPort, IEnvironmentModel defaultEnvironment)
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
            Server.AppAddress = new Uri(connectionUri).ToString(); // validate uri format before assigning
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

            // BUG 9276 : TWR : 2013.04.19 - refactored so that we share environments
            Server.Environment = CurrentEnvironmentRepository.Fetch(Server);
            Server.Environment.Category = category;

            if(defaultEnvironment != null)
            {
                //
                // NOTE: This must ALWAYS save the environment to the server
                //
                ResourceRepository.AddEnvironment(defaultEnvironment, Server.Environment);

                ReloadResource(defaultEnvironment, Guid.Parse(connectionID), ResourceType.Source);
            }

            CurrentEnvironmentRepository.Save(Server.Environment);
            Logger.TraceInfo("Publish message of type - " + typeof(AddServerToExplorerMessage));
            _eventPublisher.Publish(new AddServerToExplorerMessage(Server.Environment, Context,true));
            Logger.TraceInfo("Publish message of type - " + typeof(AddServerToDeployMessage));
            _eventPublisher.Publish(new AddServerToDeployMessage(Server, Context));
        }

        #endregion
    }
}
