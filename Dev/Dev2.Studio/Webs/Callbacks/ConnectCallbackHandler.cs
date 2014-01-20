using Caliburn.Micro;
using Dev2.Network;
using Dev2.Providers.Logs;
using Dev2.Services.Events;
using Dev2.Studio.Core;
using Dev2.Studio.Core.AppResources.Enums;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Messages;
using Dev2.Studio.Core.Models;
using Dev2.Webs.Callbacks;
using System;

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.Webs.Callbacks
{
    public class ConnectCallbackHandler : WebsiteCallbackHandler
    {
        // ReSharper disable once NotAccessedField.Local
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
            //Server = new ServerDTO();
            Uri defaultWebServerUri;
            _webServerPort = Uri.TryCreate(StringResources.Uri_WebServer, UriKind.Absolute, out defaultWebServerUri) ? defaultWebServerUri.Port : 80;
        }

        #endregion

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
        /// <param name="defaultEnvironment">The environment where the connection will be saved - must ALWAYS be .</param>
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

            Guid resourceID = Guid.Parse(connectionID);
            IEnvironmentModel activeEnvironment = CurrentEnvironmentRepository.ActiveEnvironment;
            var connection = new ServerProxy(new Uri(connectionUri));
            var newEnvironment = new EnvironmentModel(resourceID, connection, activeEnvironment.ResourceRepository) { Name = connectionName, Category = category };

            // BUG 9276 : TWR : 2013.04.19 - refactored so that we share environments

            if(defaultEnvironment != null)
            {
                //
                // NOTE: This must ALWAYS save the environment to the server
                //
                defaultEnvironment.ResourceRepository.AddEnvironment(defaultEnvironment, newEnvironment);

                ReloadResource(defaultEnvironment, resourceID, ResourceType.Source);
            }

            CurrentEnvironmentRepository.Save(newEnvironment);
            this.TraceInfo("Publish message of type - " + typeof(AddServerToExplorerMessage));
            EventPublisher.Publish(new AddServerToExplorerMessage(newEnvironment, Context, true));
            this.TraceInfo("Publish message of type - " + typeof(AddServerToDeployMessage));
            EventPublisher.Publish(new AddServerToDeployMessage(newEnvironment, Context));
        }

        #endregion
    }
}
