using Dev2.Studio.Core;
using Dev2.Studio.Core.AppResources.Repositories;
using Dev2.Studio.Core.Factories;
using Dev2.Studio.Core.InterfaceImplementors;
using Dev2.Studio.Core.Interfaces;
using Newtonsoft.Json.Linq;
using System;
using System.ComponentModel.Composition;
using System.Windows;

namespace Dev2.Studio.Custom_Dev2_Controls.Connections
{
    public class ConnectCallbackHandler : IPropertyEditorWizard
    {
        public Window Owner { get; set; }
        public IServer Server { get; private set; }

        [Import]
        public IFrameworkRepository<IEnvironmentModel> CurrentEnvironmentRepository { get; set; }

        readonly int _webServerPort;

        #region CTOR

        public ConnectCallbackHandler()
        {
            Server = new ServerDTO();
            Uri defaultWebServerUri;
            _webServerPort = Uri.TryCreate(StringResources.Uri_WebServer, UriKind.Absolute, out defaultWebServerUri) ? defaultWebServerUri.Port : 80;
        }

        #endregion

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
            }

            if(CurrentEnvironmentRepository != null)
            {
                CurrentEnvironmentRepository.Save(Server.Environment);
            }

            Mediator.SendMessage(MediatorMessages.AddServerToExplorer, Server.Environment);
        }

        #endregion

        #region Implementation of IPropertyEditorWizard

        public ILayoutObjectViewModel SelectedLayoutObject
        {
            get
            {
                return null;
            }
        }

        public void OpenPropertyEditor()
        {
        }

        public void Dev2Set(string data, string uri)
        {
        }

        public void Dev2SetValue(string value)
        {
            Dev2SetValue(value, EnvironmentRepository.DefaultEnvironment);
        }

        public void Dev2SetValue(string value, IEnvironmentModel defaultEnvironment)
        {
            Close();

            if(string.IsNullOrEmpty(value))
            {
                throw new ArgumentNullException("value");
            }
            dynamic jsonObj = JObject.Parse(value);
            Save(jsonObj.ResourceID.Value, jsonObj.Address.Value, jsonObj.ResourceName.Value, (int)jsonObj.WebServerPort.Value, defaultEnvironment);
        }

        public void Dev2Done()
        {
        }

        public void Dev2ReloadResource(string resourceName, string resourceType)
        {
        }

        public void Close()
        {
            if(Owner != null)
            {
                Owner.Close();
            }
        }

        public void Cancel()
        {
            Close();
        }

        public event NavigateRequestedEventHandler NavigateRequested;

        protected void OnNavigateRequested(string uri)
        {
            if(NavigateRequested != null)
            {
                NavigateRequested(uri);
            }
        }

        #endregion
    }
}
