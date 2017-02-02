using System;
using System.Windows.Input;
using Caliburn.Micro;
using Dev2.Common;
using Dev2.Common.Common;
using Dev2.Communication;
using Dev2.Controller;
using Dev2.Data.ServiceModel;
using Dev2.Runtime.Configuration.ViewModels.Base;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Studio.Core.Interfaces;

namespace Dev2.Views.SharepointServerSource
{
    public class SharepointServerSourceViewModel : PropertyChangedBase
    {
        readonly IEnvironmentModel _environment;
        string _serverName;
        bool _isWindows;
        bool _isUser;
        string _userName;
        string _password;
        string _testResult;
        
        IContextualResourceModel _resource;
        AuthenticationType _authenticationType;
        bool _testComplete;
        bool _isLoading;

        public SharepointServerSourceViewModel(SharepointServerSource serverSource, IEnvironmentModel environment)
        {
            IsLoading = false;
            TestComplete = false;
            _environment = environment;
            ServerName = "";
            AuthenticationType = AuthenticationType.Windows;
            IsWindows = true;
            SaveCommand = new RelayCommand(o =>
            {
                serverSource.DialogResult = true;
                serverSource.Close();
            }, o => TestComplete);

            CancelCommand = new RelayCommand(o =>
            {
                serverSource.DialogResult = false;
                serverSource.Close();
            });
            TestCommand = new RelayCommand(o =>
            {
                IsLoading = true;
                Dev2JsonSerializer serializer = new Dev2JsonSerializer();
                var source = CreateSharepointServerSource();
                var comsController = new CommunicationController { ServiceName = "TestSharepointServerService" };
                comsController.AddPayloadArgument("SharepointServer", serializer.SerializeToBuilder(source));
                var sharepointSourceTo = comsController.ExecuteCommand<SharepointSourceTo>(environment.Connection, GlobalConstants.ServerWorkspaceID);
                TestResult = sharepointSourceTo.TestMessage;
                IsSharepointOnline = sharepointSourceTo.IsSharepointOnline;
                IsLoading = false;
            }, o => !TestComplete);
        }

        public bool IsSharepointOnline { get; set; }

        public string TestResult
        {
            get
            {
                return _testResult;
            }
            set
            {
                _testResult = value;
                if (!_testResult.Contains("Failed"))
                {
                    TestComplete = true;
                }
                NotifyOfPropertyChange("TestResult");
            }
        }

        SharepointSource CreateSharepointServerSource()
        {
            var source = new SharepointSource { Server = ServerName, UserName = UserName, Password = Password, AuthenticationType = AuthenticationType };
            return source;
        }

        public string ServerName
        {
            get
            {
                return _serverName;
            }
            set
            {
                _serverName = value;
                NotifyOfPropertyChange("ServerName");
            }
        }
        public bool IsWindows
        {
            get
            {
                return _isWindows;
            }
            set
            {
                if (value.Equals(_isWindows))
                {
                    return;
                }
                _isWindows = value;
                if (_isWindows)
                {
                    AuthenticationType = AuthenticationType.Windows;
                }

                NotifyOfPropertyChange("IsWindows");
            }
        }
        public bool IsUser
        {
            get
            {
                return _isUser;
            }
            set
            {
                if (value.Equals(_isUser))
                {
                    return;
                }
                _isUser = value;
                if (_isUser)
                {
                    AuthenticationType = AuthenticationType.User;
                }
                NotifyOfPropertyChange("IsUser");
            }
        }
        public ICommand TestCommand { get; set; }
        public ICommand SaveCommand { get; set; }
        public ICommand CancelCommand { get; set; }
        public IContextualResourceModel Resource
        {
            get
            {
                return _resource;
            }
            set
            {
                _resource = value;
                var xaml = _resource.WorkflowXaml;
                if (xaml.IsNullOrEmpty() && _resource.ID != Guid.Empty)
                {
                    var message = _environment.ResourceRepository.FetchResourceDefinition(_environment, GlobalConstants.ServerWorkspaceID, _resource.ID, false);
                    xaml = message.Message;
                }
                if (!xaml.IsNullOrEmpty())
                {
                    UpdateBasedOnResource(new SharepointSource(xaml.ToXElement()));
                }
            }
        }

        public bool TestComplete
        {
            get { return _testComplete; }
            set
            {
                _testComplete = value;
                NotifyOfPropertyChange("TestComplete");
                var command = SaveCommand as RelayCommand;
                if (command != null)
                {
                    command.RaiseCanExecuteChanged();
                }
            }
        }

        public bool IsLoading
        {
            get
            {
                return _isLoading;
            }
            set
            {
                _isLoading = value;
                NotifyOfPropertyChange(() => IsLoading);
            }
        }

        void UpdateBasedOnResource(SharepointSource sharepointSource)
        {
            ServerName = sharepointSource.Server;
            UserName = sharepointSource.UserName;
            Password = sharepointSource.Password;
            AuthenticationType = sharepointSource.AuthenticationType;
            IsSharepointOnline = sharepointSource.IsSharepointOnline;
        }

        public string UserName
        {
            get
            {
                return _userName;
            }
            set
            {
                if (value == _userName)
                {
                    return;
                }
                _userName = value;
                NotifyOfPropertyChange("UserName");
            }
        }
        public string Password
        {
            get
            {
                return _password;
            }
            set
            {
                if (value == _password)
                {
                    return;
                }
                _password = value;
                NotifyOfPropertyChange("Password");
            }
        }
        public AuthenticationType AuthenticationType
        {
            get
            {
                return _authenticationType;
            }
            set
            {
                _authenticationType = value;
                if (_authenticationType == AuthenticationType.Windows)
                {
                    IsWindows = true;
                }
                else if (_authenticationType == AuthenticationType.User)
                {
                    IsUser = true;
                }
            }
        }
    }
}
