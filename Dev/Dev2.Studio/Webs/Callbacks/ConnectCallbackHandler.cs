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
using Newtonsoft.Json;

namespace Dev2.Webs.Callbacks
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
                //
                // NOTE: Public needs to drop through to User for the rest of the framework to pick up properly behind the scenes ;)
                //
                connection = new ServerProxy(newConnection.WebAddress, newConnection.UserName, newConnection.Password);
            }
            var newEnvironment = new EnvironmentModel(resourceId, connection) { Name = newConnection.ResourceName, Category = newConnection.ResourcePath };

            if(defaultEnvironment != null)
            {
                // NOTE : NEVER EVER CALL defaultEnvironment.ResourceRepository.AddEnvironment(defaultEnvironment, newEnvironment);
                // THERE IS NO REASON TO RE-SAVE A SAVED RESOURCE!!!!

                ReloadResource(defaultEnvironment, resourceId, Studio.Core.AppResources.Enums.ResourceType.Source);
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
