using System;
using System.Network;
using System.Text;
using System.Windows;
using System.Xml;
using System.Xml.Linq;
using Caliburn.Micro;
using Dev2.Providers.Logs;
using Dev2.Services.Events;
using Dev2.Studio.Core.AppResources.Repositories;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Messages;
using Dev2.Studio.Core.Network;
using Dev2.Workspaces;
using Action = System.Action;

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.Core.Models
{
    // BUG 9276 : TWR : 2013.04.19 - refactored so that we share environments

    public class EnvironmentModel : IEnvironmentModel
    {
        IEventAggregator _eventPublisher;
        bool _publishEventsOnDispatcherThread;

        // BUG 9940 - 2013.07.29 - TWR - added
        public event EventHandler<ConnectedEventArgs> IsConnectedChanged;

        #region CTOR
        //, IWizardEngine wizardEngine
        public EnvironmentModel(Guid id, IEnvironmentConnection environmentConnection, bool publishEventsOnDispatcherThread = true)
            : this(EventPublishers.Aggregator, id, environmentConnection, publishEventsOnDispatcherThread)
        {
        }

        public EnvironmentModel(Guid id, IEnvironmentConnection environmentConnection, IResourceRepository resourceRepository, bool publishEventsOnDispatcherThread = true)
            : this(EventPublishers.Aggregator, id, environmentConnection, resourceRepository, publishEventsOnDispatcherThread)
        {
        }
        //, IWizardEngine wizardEngine
        public EnvironmentModel(IEventAggregator eventPublisher, Guid id, IEnvironmentConnection environmentConnection, bool publishEventsOnDispatcherThread = true)
        {
            Initialize(eventPublisher, id, environmentConnection, null, publishEventsOnDispatcherThread);
        }

        public EnvironmentModel(IEventAggregator eventPublisher, Guid id, IEnvironmentConnection environmentConnection, IResourceRepository resourceRepository, bool publishEventsOnDispatcherThread = true)
        {
            VerifyArgument.IsNotNull("resourceRepository", resourceRepository);
            Initialize(eventPublisher, id, environmentConnection, resourceRepository, publishEventsOnDispatcherThread);
        }
        //, IWizardEngine wizardEngine
        void Initialize(IEventAggregator eventPublisher, Guid id, IEnvironmentConnection environmentConnection, IResourceRepository resourceRepository, bool publishEventsOnDispatcherThread)
        {
            VerifyArgument.IsNotNull("environmentConnection", environmentConnection);
            VerifyArgument.IsNotNull("eventPublisher", eventPublisher);
            _eventPublisher = eventPublisher;

            CanStudioExecute = true;

            ID = id; // The resource ID
            Connection = environmentConnection;

            // MUST set Connection before creating new ResourceRepository!!
            ResourceRepository = resourceRepository ?? new ResourceRepository(this);

            _publishEventsOnDispatcherThread = publishEventsOnDispatcherThread;

            // This is also triggered by a network state change
            Connection.LoginStateChanged += OnConnectionLoginStateChanged;

            // PBI 9228: TWR - 2013.04.17
            Connection.ServerStateChanged += OnServerStateChanged;

            // BUG 9940 - 2013.07.29 - TWR - added
            Connection.NetworkStateChanged += OnNetworkStateChanged;
        }

        #endregion

        #region Properties

        public bool CanStudioExecute { get; set; }

        public Guid ID { get; private set; }

        // BUG: 8786 - TWR - 2013.02.20 - Added category
        public string Category { get; set; }

        public IEnvironmentConnection Connection { get; private set; }

        public string Name { get { return Connection.DisplayName; } set { Connection.DisplayName = value; } }

        public bool IsConnected { get { return Connection.IsConnected; } }

        public IResourceRepository ResourceRepository { get; private set; }

        #endregion

        #region Connect

        public void Connect()
        {
            if(Connection.IsConnected)
            {
                return;
            }
            if(string.IsNullOrEmpty(Name))
            {
                throw new ArgumentException(string.Format(StringResources.Error_Connect_Failed, StringResources.Error_DSF_Name_Not_Provided));
            }

            this.TraceInfo("Attempting to connect to [ " + Connection.AppServerUri + " ] ");
            Connection.Connect();
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
                Connection.Disconnect();

            }
        }

        #endregion

        #region IsLocalHost
        public bool IsLocalHost()
        {
            return Connection.IsLocalHost;
        }
        #endregion

        #region ForceLoadResources

        public void ForceLoadResources()
        {
            if(Connection.IsConnected && CanStudioExecute)
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
                ResourceRepository.UpdateWorkspace(WorkspaceItemRepository.Instance.WorkspaceItems);
            }
        }

        #endregion

        #region ToSourceDefinition

        public StringBuilder ToSourceDefinition()
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


            var result = new StringBuilder();
            XmlWriterSettings xws = new XmlWriterSettings();
            xws.OmitXmlDeclaration = true;
            using(XmlWriter xw = XmlWriter.Create(result, xws))
            {
                xml.Save(xw);
            }


            return result;
        }

        #endregion

        #region Event Handlers

        void RaiseIsConnectedChanged(bool isOnline)
        {
            // BUG 9940 - 2013.07.29 - TWR - added
            if(IsConnectedChanged != null)
            {
                IsConnectedChanged(this, new ConnectedEventArgs { IsConnected = isOnline });
            }
        }

        void OnNetworkStateChanged(object sender, NetworkStateEventArgs e)
        {
            // BUG 9940 - 2013.07.29 - TWR - added
            RaiseIsConnectedChanged(e.ToState == NetworkState.Online);
        }

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
            RaiseIsConnectedChanged(isOnline);


            AbstractEnvironmentMessage message;
            if(isOnline)
            {
                message = new EnvironmentConnectedMessage(this);
            }
            else
            {
                message = new EnvironmentDisconnectedMessage(this);
            }

            if(_publishEventsOnDispatcherThread)
            {
                if(Application.Current != null)
                {
                    // application is not shutting down!!
                    this.TraceInfo("Publish message of type - " + typeof(AbstractEnvironmentMessage));
                    Application.Current.Dispatcher.BeginInvoke(new Action(() => _eventPublisher.Publish(message)), null);
                }
            }
            else
            {
                this.TraceInfo("Publish message of type - " + typeof(AbstractEnvironmentMessage));
                _eventPublisher.Publish(message);
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
