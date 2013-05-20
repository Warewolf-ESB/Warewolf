using System;
using System.Network;
using System.Windows;
using System.Xml.Linq;
using Dev2.Common;
using Dev2.DataList.Contract.Network;
using Dev2.Network;
using Dev2.Network.Execution;
using Dev2.Network.Messaging.Messages;
using Dev2.Studio.Core.AppResources.DependencyInjection.EqualityComparers;
using Dev2.Studio.Core.AppResources.Repositories;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Messages;
using Action = System.Action;
using Dev2.Studio.Core.Network;
using Dev2.Studio.Core.Wizards.Interfaces;

namespace Dev2.Studio.Core.Models
{
    // BUG 9276 : TWR : 2013.04.19 - refactored so that we share environments

    public class EnvironmentModel : IEnvironmentModel
    {
        bool _publishEventsOnDispatcherThread;
        Guid _updateWorkFlowFromServerSubToken;

        public bool CanStudioExecute { get; set; }

        #region CTOR

        public EnvironmentModel(Guid id, IEnvironmentConnection environmentConnection, IWizardEngine wizardEngine, bool publishEventsOnDispatcherThread = true)
        {
            if(wizardEngine == null)
            {
                throw new ArgumentNullException("wizardEngine");
            }
            Initialize(id, environmentConnection, publishEventsOnDispatcherThread);
            ResourceRepository = new ResourceRepository(this, wizardEngine);
        }

        public EnvironmentModel(Guid id, IEnvironmentConnection environmentConnection, IResourceRepository resourceRepository, bool publishEventsOnDispatcherThread = true)
        {
            if(resourceRepository == null)
            {
                throw new ArgumentNullException("resourceRepository");
            }
            Initialize(id, environmentConnection, publishEventsOnDispatcherThread);
            ResourceRepository = resourceRepository;
        }

        void Initialize(Guid id, IEnvironmentConnection environmentConnection, bool publishEventsOnDispatcherThread)
        {
            if (environmentConnection == null)
            {
                throw new ArgumentNullException("environmentConnection");
            }

            CanStudioExecute = true;

            ID = id; // The resource ID
            Connection = environmentConnection;
            _publishEventsOnDispatcherThread = publishEventsOnDispatcherThread;

            // This is also triggered by a network state change
            Connection.LoginStateChanged += OnConnectionLoginStateChanged;

            // PBI 9228: TWR - 2013.04.17
            Connection.ServerStateChanged += OnServerStateChanged;
        }

        #endregion

        #region Properties

        public Guid ID { get; private set; }

        // BUG: 8786 - TWR - 2013.02.20 - Added category
        public string Category { get; set; }

        public IEnvironmentConnection Connection { get; private set; }

        public string Name { get { return Connection.DisplayName; } set { Connection.DisplayName = value; } }

        public bool IsConnected { get { return Connection.IsConnected; } }

        public IResourceRepository ResourceRepository { get; private set; }

        public IStudioEsbChannel DsfChannel { get { return Connection.DataChannel; } }

        public INetworkExecutionChannel ExecutionChannel { get { return Connection.ExecutionChannel; } }

        public INetworkDataListChannel DataListChannel { get { return Connection.DataListChannel; } }

        public IWizardEngine WizardEngine { get { return ResourceRepository.WizardEngine; } }

        #endregion

        #region Connect

        public void Connect()
        {
            if(string.IsNullOrEmpty(Name))
            {
                throw new ArgumentException(string.Format(StringResources.Error_Connect_Failed, StringResources.Error_DSF_Name_Not_Provided));
            }

            StudioLogger.LogMessage("Attempting to connect to [ " + Connection.AppServerUri + " ] ");
            Connection.Connect();
            if(Connection.MessageAggregator != null)
            {
                _updateWorkFlowFromServerSubToken = Connection.MessageAggregator.Subscribe<UpdateWorkflowFromServerMessage>(UpdateCachedServicesBasedOnChangeFromServer);
            }
        }

        void UpdateCachedServicesBasedOnChangeFromServer(UpdateWorkflowFromServerMessage updateWorkflowFromServerMessage, IStudioNetworkChannelContext studioNetworkChannelContext)
        {
            var resourceID = updateWorkflowFromServerMessage.ResourceID;
            if (resourceID != Guid.Empty)
            {
                var resourceModel = ResourceRepository.FindSingle(model => model.ID == resourceID);
                ResourceRepository.ReloadResource(resourceModel.ResourceName, resourceModel.ResourceType, ResourceModelEqualityComparer.Current);
//                if (!resourceModel.Environment.ResourceRepository.IsInCache(resourceModel.ID))
//                {
//                   
//                }
//                ResourceRepository.RefreshResource(resourceID);
            }
        }

        public void Connect(IEnvironmentModel other)
        {
            if(other == null)
            {
                throw new ArgumentNullException("other");
            }

            if(!other.IsConnected)
            {
                other.Connection.Connect();

                if(!other.IsConnected)
                {
                    throw new InvalidOperationException("Environment failed to connect.");
                }
            }
            Connect();
        }

        #endregion

        #region Disconnect

        public void Disconnect()
        {
            if(Connection.IsConnected)
            {
                if(_updateWorkFlowFromServerSubToken != Guid.Empty)
                {
                    Connection.MessageAggregator.Unsubscibe(_updateWorkFlowFromServerSubToken);
                }
                Connection.Disconnect();
                
            }
        }

        #endregion

        #region IsLocalHost
        public bool IsLocalHost()
        {
            return Connection.DisplayName == "localhost";
        }
        #endregion

        #region ForceLoadResources

        public void ForceLoadResources()
        {
            if (Connection.IsConnected && CanStudioExecute)
            {
                ResourceRepository.ForceLoad();
            }
        }

        #endregion

        #region LoadResources

        public void LoadResources()
        {
            if(Connection.IsConnected && CanStudioExecute)
            {
                ResourceRepository.Load();
            }
        }

        #endregion

        #region ToSourceDefinition

        public string ToSourceDefinition()
        {
            var xml = new XElement("Source",
                new XAttribute("ID", ID),
                new XAttribute("Name", Name ?? ""),
                new XAttribute("Type", "Dev2Server"),
                new XAttribute("ConnectionString", string.Join(";",
                    string.Format("AppServerUri={0}", Connection.AppServerUri),
                    string.Format("WebServerPort={0}", Connection.WebServerUri.Port)
                    )),
                new XElement("TypeOf", "Dev2Server"),
                new XElement("DisplayName", Name),
                new XElement("Category", Category ?? "") // BUG: 8786 - TWR - 2013.02.20 - Changed to use category
                );

            return xml.ToString();
        }

        #endregion

        #region Event Handlers


        void OnServerStateChanged(object sender, ServerStateEventArgs e)
        {
            RaiseNetworkStateChanged(e.State == ServerState.Online);
        }

        private void OnConnectionLoginStateChanged(object sender, LoginStateEventArgs e)
        {
            RaiseNetworkStateChanged(e.LoggedIn);
        }

        void RaiseNetworkStateChanged(bool isOnline)
        {
            // If auxilliry connection then do nothing
            if (Connection.IsAuxiliary)
            {
                return;
            }

            AbstractEnvironmentMessage message;
            if (isOnline)
            {
                message = new EnvironmentConnectedMessage(this);
            }
            else
            {
                message = new EnvironmentDisconnectedMessage(this);
            }

            if (_publishEventsOnDispatcherThread)
            {
                if (Application.Current != null)
                {
                    // application is not shutting down!!
                    Application.Current.Dispatcher.BeginInvoke(new Action(() => Connection.EventAggregator.Publish(message)), null);
                }
            }
            else
            {
                Connection.EventAggregator.Publish(message);
            }
        }
        #endregion

        #region IEquatable

        public bool Equals(IEnvironmentModel other)
        {
            if(other == null)
            {
                return false;
            }

            // BUG 9276 : TWR : 2013.04.19 - refactored to use deleted EnvironmentModelEqualityComparer logic instead!           
            return ID == other.ID
                   && Name == other.Name;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as IEnvironmentModel);
        }

        public override int GetHashCode()
        {
            return ID.GetHashCode() ^ Connection.ServerID.GetHashCode() ^ Connection.AppServerUri.AbsoluteUri.GetHashCode();
        }

        #endregion
    }
}
