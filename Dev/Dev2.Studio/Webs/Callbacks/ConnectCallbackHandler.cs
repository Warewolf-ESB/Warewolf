using System;
using Caliburn.Micro;
using Dev2.Data.ServiceModel;
using Dev2.Network;
using Dev2.Providers.Logs;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Services.Events;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Messages;
using Dev2.Studio.Core.Models;
using Dev2.Webs.Callbacks;
using Newtonsoft.Json;

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.Webs.Callbacks
{
    public class ConnectCallbackHandler : WebsiteCallbackHandler
    {

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
        }

        #endregion

        protected override void Save(IEnvironmentModel environmentModel, dynamic jsonObj)
        {
            Save(jsonObj, environmentModel);
        }

        #region Save

        /// <summary>
        /// Saves the specified connection - method provided for testing.
        /// </summary>
        /// <param name="jsonObj"></param>
        /// <param name="defaultEnvironment">The environment where the connection will be saved - must ALWAYS be .</param>
        /// <exception cref="System.ArgumentNullException">connectionID</exception>
        public void Save(dynamic jsonObj, IEnvironmentModel defaultEnvironment)
        {
            if(jsonObj == null)
            {
                throw new ArgumentNullException();
            }
            Connection newConnection = JsonConvert.DeserializeObject<Connection>(jsonObj.ToString());

            var resourceID = newConnection.ResourceID;
            ServerProxy connection;
            if(newConnection.AuthenticationType == AuthenticationType.Windows || newConnection.AuthenticationType == AuthenticationType.Anonymous)
            {
                connection = new ServerProxy(new Uri(newConnection.WebAddress));
            }
            else
            {
                connection = new ServerProxy(newConnection.WebAddress, newConnection.UserName, newConnection.Password);
            }
            var newEnvironment = new EnvironmentModel(resourceID, connection) { Name = newConnection.ResourceName, Category = newConnection.ResourcePath };

            if(defaultEnvironment != null)
            {
                //
                // NOTE: This must ALWAYS save the environment to the server
                //
                defaultEnvironment.ResourceRepository.AddEnvironment(defaultEnvironment, newEnvironment);

                ReloadResource(defaultEnvironment, resourceID, Core.AppResources.Enums.ResourceType.Source);
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
