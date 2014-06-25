using System;
using Caliburn.Micro;
using Dev2.AppResources.Enums;
using Dev2.Data.ServiceModel;
using Dev2.Messages;
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
        #region Fields

        readonly ConnectControlInstanceType _connectControlInstanceType;

        #endregion


        #region CTOR

        public ConnectCallbackHandler(ConnectControlInstanceType connectControlInstanceType = ConnectControlInstanceType.Explorer)
            : this(EnvironmentRepository.Instance, connectControlInstanceType)
        {
        }

        public ConnectCallbackHandler(IEnvironmentRepository currentEnvironmentRepository, ConnectControlInstanceType connectControlInstanceType = ConnectControlInstanceType.Explorer)
            : this(EventPublishers.Aggregator, currentEnvironmentRepository, connectControlInstanceType)
        {
        }

        public ConnectCallbackHandler(IEventAggregator eventPublisher, IEnvironmentRepository currentEnvironmentRepository, ConnectControlInstanceType connectControlInstanceType = ConnectControlInstanceType.Explorer)
            : base(eventPublisher, currentEnvironmentRepository)
        {
            _connectControlInstanceType = connectControlInstanceType;
        }

        #endregion

        protected override void Save(IEnvironmentModel environmentModel, dynamic jsonObj)
        {
            Save(jsonObj, environmentModel);
        }

        #region Overrides of WebsiteCallbackHandler

        public override void Cancel()
        {
            EventPublisher.Publish(new SetConnectControlSelectedServerMessage(EnvironmentRepository.Instance.ActiveEnvironment, _connectControlInstanceType));
            base.Cancel();
        }

        #endregion

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

            var resourceId = newConnection.ResourceID;
            ServerProxy connection;
            if(newConnection.AuthenticationType == AuthenticationType.Windows || newConnection.AuthenticationType == AuthenticationType.Anonymous)
            {
                connection = new ServerProxy(new Uri(newConnection.WebAddress));
            }
            else
            {
                connection = new ServerProxy(newConnection.WebAddress, newConnection.UserName, newConnection.Password);
            }
            var newEnvironment = new EnvironmentModel(resourceId, connection) { Name = newConnection.ResourceName, Category = newConnection.ResourcePath };

            if(defaultEnvironment != null)
            {
                //
                // NOTE: This must ALWAYS save the environment to the server
                //
                defaultEnvironment.ResourceRepository.AddEnvironment(defaultEnvironment, newEnvironment);

                ReloadResource(defaultEnvironment, resourceId, Core.AppResources.Enums.ResourceType.Source);
            }

            CurrentEnvironmentRepository.Save(newEnvironment);
            this.TraceInfo("Publish message of type - " + typeof(AddServerToExplorerMessage));
            EventPublisher.Publish(new AddServerToExplorerMessage(newEnvironment, true));
            this.TraceInfo("Publish message of type - " + typeof(AddServerToDeployMessage));
            EventPublisher.Publish(new AddServerToDeployMessage(newEnvironment, _connectControlInstanceType));
        }

        #endregion
    }
}
