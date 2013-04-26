using System;
using System.Network;
using System.Windows;
using System.Xml.Linq;
using Dev2.DataList.Contract.Network;
using Dev2.Network.Execution;
using Dev2.Studio.Core.Factories;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Messages;
using Dev2.Studio.Core.Network;
using Action = System.Action;

namespace Dev2.Studio.Core.Models
{
    public class EnvironmentModel : IEnvironmentModel
    {
        private IResourceRepository _resources;
        bool _publishEventsOnDispatcherThread;

        #region Constructor

        public EnvironmentModel(IEnvironmentConnection environmentConnection, bool publishEventsOnDispatcherThread = true)
        {
            if(environmentConnection == null)
            {
                throw new ArgumentNullException("environmentConnection");
            }
            Connection = environmentConnection;
            _publishEventsOnDispatcherThread = publishEventsOnDispatcherThread;

            // This is also triggered by a network state change
            Connection.LoginStateChanged += OnConnectionLoginStateChanged;

            // PBI 9228: TWR - 2013.04.17
            Connection.ServerStateChanged += OnServerStateChanged;
        }

        #endregion Constructor

        #region Properties

        public Guid ID { get; set; }

        // BUG: 8786 - TWR - 2013.02.20 - Added category
        public string Category { get; set; }

        public IEnvironmentConnection Connection { get; private set; }

        public string Name
        {
            get
            {
                if(Connection != null)
                {
                    return Connection.DisplayName;
                }
                return null;
            }
            set
            {
                if(Connection != null)
                {
                    Connection.DisplayName = value;
                }
            }

        }

        public bool IsConnected
        {
            get
            {
                if(Connection != null)
                {
                    return Connection.IsConnected;
                }
                return false;
            }
        }

        public IResourceRepository ResourceRepository
        {
            get
            {
                return _resources;
            }
            set
            {
                _resources = value;
            }
        }

        public IStudioEsbChannel DsfChannel
        {
            get
            {
                if(Connection != null)
                {
                    return Connection.DataChannel;
                }
                return null;
            }
        }

        public INetworkExecutionChannel ExecutionChannel
        {
            get
            {
                if(Connection != null)
                {
                    return Connection.ExecutionChannel;
                }
                return null;
            }
        }

        public INetworkDataListChannel DataListChannel
        {
            get
            {
                if(Connection != null)
                {
                    return Connection.DataListChannel;
                }
                return null;
            }
        }

        #endregion Properties

        #region Methods

        public void Connect()
        {
            if(string.IsNullOrEmpty(Name))
            {
                throw new ArgumentException(string.Format(StringResources.Error_Connect_Failed, StringResources.Error_DSF_Name_Not_Provided));
            }

            Connection.Connect();
        }

        public void Disconnect()
        {
            if(Connection != null && Connection.IsConnected)
            {
                Connection.Disconnect();
            }
        }

        public void Connect(IEnvironmentModel model)
        {
            // Connect using auxilliary connections

            if(!model.IsConnected)
            {
                model.Connection.Connect(true);

                if(!model.IsConnected) throw new InvalidOperationException("Model failed to connect.");
            }
            Connect();
        }

        public void LoadResources()
        {
            if(Connection.IsConnected)
            {
                _resources = ResourceRepositoryFactory.CreateResourceRepository(this);
                _resources.Load();
            }
        }

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

        #endregion Methods

        #region Connection Event Handlers

        // PBI 9228: TWR - 2013.04.17

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
            if(Connection.IsAuxiliary)
            {
                return;
            }

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
                    Application.Current.Dispatcher.BeginInvoke(new Action(() => Connection.EventAggregator.Publish(message)), null);
                }
            }
            else
            {
                Connection.EventAggregator.Publish(message);
            }
        }

        #endregion Event Handlers


        #region IEquatable

        public bool Equals(IEnvironmentModel other)
        {
            if(other == null)
            {
                return false;
            }
            return ID == other.ID;
        }

        public override bool Equals(object obj)
        {
            if(obj == null)
            {
                return false;
            }

            var item = obj as IEnvironmentModel;
            return item != null && Equals(item);
        }

        public override int GetHashCode()
        {
            return ID.GetHashCode();
        }

        #endregion
    }
}
