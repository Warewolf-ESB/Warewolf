using System.Windows.Input;
using Caliburn.Micro;
using Dev2.Common;
using Dev2.Communication;
using Dev2.Controller;
using Dev2.Data.ServiceModel;
using Dev2.Runtime.Configuration.ViewModels.Base;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Studio.Core.Interfaces;

namespace Dev2.Views.SharepointServerSource
{
    public class SharepointServerSourceViewModel:PropertyChangedBase
    {
        string _serverName;
        bool _isWindows;
        bool _isUser;
        string _userName;
        string _password;
        string _testResult;

        public SharepointServerSourceViewModel(SharepointServerSource serverSource, IEnvironmentModel environment)
        {
            ServerName = "";
            AuthenticationType = AuthenticationType.Windows;
            SaveCommand = new RelayCommand(o =>
            {
                if(IsWindows)
                {
                    AuthenticationType = AuthenticationType.Windows;                    
                }
                if(IsUser)
                {
                    AuthenticationType = AuthenticationType.User;
                }
                serverSource.DialogResult = true;
                serverSource.Close();
            });
            
            CancelCommand = new RelayCommand(o =>
            {
                serverSource.DialogResult = false;
                serverSource.Close();
            });
            TestCommand=new RelayCommand(o =>
            {
                Dev2JsonSerializer serializer = new Dev2JsonSerializer();
                var source = CreateSharepointServerSource();
                var comsController = new CommunicationController { ServiceName = "TestSharepointServerService" };
                comsController.AddPayloadArgument("SharepointServer", serializer.SerializeToBuilder(source));
                TestResult = comsController.ExecuteCommand<string>(environment.Connection, GlobalConstants.ServerWorkspaceID);
            });
        }

        public string TestResult
        {
            get
            {
                return _testResult;
            }
            set
            {
                _testResult = value;
                NotifyOfPropertyChange("TestResult");
            }
        }

        SharepointSource CreateSharepointServerSource()
        {
            var source = new SharepointSource();
            source.Server = ServerName;
            source.UserName = UserName;
            source.Password = Password;
            source.AuthenticationType = AuthenticationType;
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
                if(value.Equals(_isWindows))
                {
                    return;
                }
                _isWindows = value;                
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
                if(value.Equals(_isUser))
                {
                    return;
                }
                _isUser = value;
                NotifyOfPropertyChange("IsUser");
            }
        }
        public ICommand TestCommand { get; set; }
        public ICommand SaveCommand { get; set; }
        public ICommand CancelCommand { get; set; }
        public IContextualResourceModel Resource { get; set; }
        public string UserName
        {
            get
            {
                return _userName;
            }
            set
            {
                if(value == _userName)
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
                if(value == _password)
                {
                    return;
                }
                _password = value;
                NotifyOfPropertyChange("Password");
            }
        }
        public AuthenticationType AuthenticationType { get; set; }
    }
}
