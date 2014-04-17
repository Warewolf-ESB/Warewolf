using System;
using System.Network;
using System.Text;
using System.Windows;
using System.Xml;
using System.Xml.Linq;
using Caliburn.Micro;
using Dev2.Messages;
using Dev2.Providers.Logs;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Security;
using Dev2.Services.Events;
using Dev2.Services.Security;
using Dev2.Studio.Core.AppResources.Repositories;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Messages;
using Dev2.Workspaces;
using Action = System.Action;

// ReSharper disable CheckNamespace
namespace Dev2.Studio.Core.Models
{
    // BUG 9276 : TWR : 2013.04.19 - refactored so that we share environments

    public class EnvironmentModel : ObservableObject, IEnvironmentModel
    {
        IEventAggregator _eventPublisher;
        bool _publishEventsOnDispatcherThread;

        // BUG 9940 - 2013.07.29 - TWR - added
        public event EventHandler<ConnectedEventArgs> IsConnectedChanged;
        public event EventHandler<ResourcesLoadedEventArgs> ResourcesLoaded;

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

            // BUG 9940 - 2013.07.29 - TWR - added
            Connection.NetworkStateChanged += OnNetworkStateChanged;

            AuthorizationService = CreateAuthorizationService(environmentConnection);
            AuthorizationService.PermissionsChanged += OnAuthorizationServicePermissionsChanged;
            OnAuthorizationServicePermissionsChanged(this, EventArgs.Empty);
        }

        #endregion

        #region Properties

        public IAuthorizationService AuthorizationService { get; private set; }

        public bool CanStudioExecute { get; set; }

        public Guid ID { get; private set; }

        // BUG: 8786 - TWR - 2013.02.20 - Added category
        public bool IsLocalHostCheck()
        {
            return Connection.IsLocalHost;
        }

        public string Category { get; set; }

        public bool IsLocalHost { get { return IsLocalHostCheck(); } }
        public bool HasLoadedResources { get; private set; }

        public IEnvironmentConnection Connection { get; private set; }

        public string Name { get { return Connection.DisplayName; } set { Connection.DisplayName = value; } }

        public bool IsConnected { get { return Connection.IsConnected; } }

        public bool IsAuthorized { get { return Connection.IsAuthorized; } }
        public bool IsAuthorizedDeployFrom
        {
            get
            {
                return AuthorizationService.IsAuthorized(AuthorizationContext.DeployFrom, null);
            }
        }
        public bool IsAuthorizedDeployTo
        {
            get
            {
                return AuthorizationService.IsAuthorized(AuthorizationContext.DeployTo, null);
            }
        }

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
            _eventPublisher.Publish(new SelectedServerConnectedMessage(this));
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
                _eventPublisher.Publish(new SelectedServerConnectedMessage(this));
            }
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

        public void RaiseResourcesLoaded()
        {
            RaiseLoadedResources();
        }

        public void LoadResources()
        {
            if(Connection.IsConnected && CanStudioExecute)
            {
                ResourceRepository.UpdateWorkspace(WorkspaceItemRepository.Instance.WorkspaceItems);
                HasLoadedResources = true;


            }
        }

        #endregion

        #region ToSourceDefinition

        public StringBuilder ToSourceDefinition()
        {
            var connectionString = string.Join(";",
                string.Format("AppServerUri={0}", Connection.AppServerUri),
                string.Format("WebServerPort={0}", Connection.WebServerUri.Port),
                string.Format("AuthenticationType={0}", Connection.AuthenticationType)
                );
            if(Connection.AuthenticationType == AuthenticationType.User)
            {
                connectionString = string.Join(";",
                    connectionString,
                    string.Format("UserName={0}", Connection.UserName),
                    string.Format("Password={0}", Connection.Password)
                    );
            }

            var xml = new XElement("Source",
                new XAttribute("ID", ID),
                new XAttribute("Name", Name ?? ""),
                new XAttribute("Type", "Dev2Server"),
                new XAttribute("ConnectionString", connectionString),
                new XElement("TypeOf", "Dev2Server"),
                new XElement("DisplayName", Name),
                new XElement("Category", Category ?? "") // BUG: 8786 - TWR - 2013.02.20 - Changed to use category
                );


            var result = new StringBuilder();
            XmlWriterSettings xws = new XmlWriterSettings { OmitXmlDeclaration = true };
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
            // ReSharper disable ExplicitCallerInfoArgument
            OnPropertyChanged("IsConnected");
            // ReSharper restore ExplicitCallerInfoArgument
        }
        void RaiseLoadedResources()
        {
            if(ResourcesLoaded != null)
            {
                ResourcesLoaded(this, new ResourcesLoadedEventArgs { Model = this });
            }
        }

        void OnNetworkStateChanged(object sender, NetworkStateEventArgs e)
        {
            // BUG 9940 - 2013.07.29 - TWR - added
            RaiseNetworkStateChanged(e.ToState == NetworkState.Online);
        }

        void RaiseNetworkStateChanged(bool isOnline)
        {
            RaiseIsConnectedChanged(isOnline);
            if(!isOnline)
                HasLoadedResources = false;

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
                    this.TraceInfo("Publish message of type - " + message.GetType());
                    Application.Current.Dispatcher.BeginInvoke(new Action(() => _eventPublisher.Publish(message)), null);
                }
            }
            else
            {
                this.TraceInfo("Publish message of type - " + message.GetType());
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
            //Dont ever EVER check any other property here or the connect control will die and you will be beaten;)
            return ID == other.ID;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as IEnvironmentModel);
        }

        public override int GetHashCode()
        {
            return ID.GetHashCode();
        }

        #endregion

        protected virtual IAuthorizationService CreateAuthorizationService(IEnvironmentConnection environmentConnection)
        {
            var isLocalConnection = environmentConnection.WebServerUri != null && !string.IsNullOrEmpty(environmentConnection.WebServerUri.AbsoluteUri) && environmentConnection.WebServerUri.AbsoluteUri.Contains("localhost");
            return new ClientAuthorizationService(new ClientSecurityService(environmentConnection), isLocalConnection);
        }

        void OnAuthorizationServicePermissionsChanged(object sender, EventArgs eventArgs)
        {
            // ReSharper disable ExplicitCallerInfoArgument
            OnPropertyChanged("IsAuthorizedDeployTo");
            OnPropertyChanged("IsAuthorizedDeployFrom");

        }

        #region Overrides of Object

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>
        /// A string that represents the current object.
        /// </returns>
        public override string ToString()
        {
            return Name;
        }

        #endregion
    }
}
