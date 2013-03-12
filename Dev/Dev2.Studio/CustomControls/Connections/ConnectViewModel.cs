// TWR: Moved here as ConnectView/Model are related to the ConnectControl and will become a user control later
using Dev2.Studio.Core;
using Dev2.Studio.Core.AppResources.Repositories;
using Dev2.Studio.Core.Factories;
using Dev2.Studio.Core.InterfaceImplementors;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.ViewModels.Base;
using System;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Windows.Input;

namespace Dev2.Studio.ViewModels.Explorer
{
    public class ConnectViewModel : ValidationController, IDataErrorInfo
    {
        #region Class Members

        private RelayCommand _okayCommand;
        private RelayCommand _cancelCommand;

        private int _webServerPort;

        #endregion Class Members

        #region Constructor

        public ConnectViewModel()
        {
            Server = new ServerDTO();

            Uri defaultWebServerUri;
            if(Uri.TryCreate(StringResources.Uri_WebServer, UriKind.Absolute, out defaultWebServerUri))
            {
                _webServerPort = defaultWebServerUri.Port;
            }
            else
            {
                _webServerPort = 80;
            }
        }

        #endregion Constructor

        #region Commands

        public ICommand OkayCommand
        {
            get
            {
                if(_okayCommand == null)
                {
                    _okayCommand = new RelayCommand(c => Okay(), c => validationErrors.Count == 0);
                }
                return _okayCommand;
            }
        }

        public ICommand CancelCommand
        {
            get
            {
                if(_cancelCommand == null)
                {
                    _cancelCommand = new RelayCommand(p => RequestClose(), p => true);
                }
                return _cancelCommand;
            }

        }

        #endregion

        #region Private Methods

        private void Okay()
        {
            //
            // Create the web server address
            //
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

            // PBI 6597: TWR - moved connection save here to promote reusability
            SaveConnection();

            RequestClose(ViewModelDialogResults.Okay);
        }

        #endregion Private Methods

        #region Properties

        [Import]
        public IFrameworkRepository<IEnvironmentModel> EnvironmentRepository { get; set; }

        public IServer Server { get; private set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Server Name Required")]
        public string Name
        {
            get
            {
                return Server.Alias;
            }
            set
            {
                Server.Alias = value;
            }
        }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Web Server Port Required")]
        public string WebServerPort
        {
            get
            {
                return _webServerPort.ToString(); //Environment.WebServerPort.ToString(CultureInfo.InvariantCulture);
            }
            set
            {
                int portNumber;
                if(int.TryParse(value, out portNumber))
                {
                    _webServerPort = portNumber;
                }

                base.OnPropertyChanged("WebServerAddress");
            }
        }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Server Address Required")]
        public string DsfAddress
        {
            get
            {
                return Server.AppAddress;
            }
            set
            {
                Server.AppAddress = value;
                base.OnPropertyChanged("WebServerAddress");
            }
        }

        public string Error
        {
            get { throw new NotImplementedException(); }
        }

        public string this[string columnName]
        {
            get
            {
                string error = null;

                switch(columnName)
                {
                    case "WebServerPort":
                        if(string.IsNullOrEmpty(WebServerPort))
                        {
                            error = "A valid web server port must be entered";
                            AddError("InvalidWebServerPort", error);
                        }
                        else
                        {
                            RemoveError("InvalidWebServerPort");
                        }

                        int port = int.Parse(StringResources.Default_WebServer_Port);
                        if(!int.TryParse(WebServerPort, out port))
                        {
                            error = "A valid web server port must be entered";
                            AddError("InvalidWebServerPortFormat", error);
                        }
                        else
                        {
                            if(port.ToString().Length > 4 || port == 0)
                            {
                                error = "A valid web server port must be entered";
                                AddError("InvalidWebServerPortLength", error);
                            }
                            else
                            {
                                RemoveError("InvalidWebServerPortLength");
                                RemoveError("InvalidWebServerPortFormat");
                            }
                        }

                        break;

                    case "Name":

                        error = ValidateStringCannotBeNullOrEmpty(columnName, Name);

                        if(!string.IsNullOrEmpty(Name))
                        {
                            if(Name.IndexOfAny(Path.GetInvalidFileNameChars()) != -1)
                            {
                                error = "The environment name is not valid";
                                AddError("InvalidEnvironmentFileName", error);
                            }
                            else
                            {
                                RemoveError("InvalidEnvironmentFileName");
                            }
                        }

                        if(EnvironmentRepository.All().Any(c => c.Name.Equals(Name, StringComparison.InvariantCultureIgnoreCase)))
                        {
                            error = "Environment Name Already Exists";
                            AddError("EnvironmentNameAlreadyExists", error);
                        }
                        else
                        {
                            RemoveError("EnvironmentNameAlreadyExists");
                        }

                        break;

                    case "DsfAddress":
                        error = ValidateStringCannotBeNullOrEmpty(columnName, DsfAddress);

                        if(!Uri.IsWellFormedUriString(DsfAddress, UriKind.Absolute))
                        {
                            error = "Server address  is not valid";
                            AddError("ServerAddressIsNotValid", error);
                        }
                        else
                        {
                            Uri serverAddress = new Uri(DsfAddress);

                            RemoveError("ServerAddressIsNotValid");

                            if(serverAddress.Scheme.ToLower() != "http")
                            {

                                error = "Incorrect server address scheme was provided - http expected";
                                AddError("IncorrectServerAddressScheme", error);
                            }
                            else
                            {
                                RemoveError("IncorrectServerAddressScheme");
                            }

                            if(serverAddress.IsLoopback)
                            {
                                if(EnvironmentRepository.All().Any(c => c.DsfAddress.IsLoopback))
                                {
                                    error = "Loopback Environment Already Exists";
                                    AddError("LoopbackEnvironmentAlreadyExists", error);
                                }
                                else
                                {
                                    RemoveError("LoopbackEnvironmentAlreadyExists");
                                }
                            }

                            if(WebServerPort == new Uri(DsfAddress).Port.ToString())
                            {
                                error = "Application Server and Web Server cannot be on the same port. Please change one of them to the correct port";
                                AddError("DuplicatePort", error);
                            }
                            else
                            {
                                RemoveError("DuplicatePort");
                            }

                            if(EnvironmentRepository.All().Any(c => c.DsfAddress == new Uri(DsfAddress)))
                            {

                                error = "Environment Already Exists";
                                AddError("EnvironmentAlreadyExists", error);
                            }
                            else
                            {

                                RemoveError("EnvironmentAlreadyExists");
                            }
                        }
                        break;

                    default:
                        throw new ArgumentException("Unexpected property name encountered", columnName);
                }

                return error;
            }
        }

        #endregion Properties

        #region SaveConnection

        void SaveConnection()
        {
            if(EnvironmentRepository != null)
            {
                Server.Environment = EnvironmentModelFactory.CreateEnvironmentModel(Server);
                //
                // NOTE: This must ALWAYS save the environment to the server
                //
                ResourceRepository.AddEnvironment(Core.EnvironmentRepository.DefaultEnvironment, Server.Environment);
                //
                // Now add it to the repository
                //
                EnvironmentRepository.Save(Server.Environment);
            }
        }

        #endregion



    }
}
