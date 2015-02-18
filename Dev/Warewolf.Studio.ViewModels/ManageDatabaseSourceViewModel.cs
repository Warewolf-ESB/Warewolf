using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using Dev2.Common.Interfaces.Core;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Explorer;
using Dev2.Common.Interfaces.Runtime.ServiceModel;
using Dev2.Common.Interfaces.ServerProxyLayer;
using Dev2.Common.Interfaces.Studio.ViewModels.Dialogues;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Mvvm;

namespace Warewolf.Studio.ViewModels
{
    public class ManageDatabaseSourceViewModel : BindableBase, IManageDatabaseSourceViewModel, IDockViewModel
    {
        private enSourceType _serverType;
        private AuthenticationType _authenticationType;
        private string _serverName;
        private string _databaseName;
        private string _userName;
        private string _password;
        private string _testMessage;
        private List<string> _databaseNames;
        private string _header;
        readonly IStudioUpdateManager _updateManager ;
        readonly IDbSource _dbSource;

        public ManageDatabaseSourceViewModel(IStudioUpdateManager updateManager)
        {
            _updateManager = updateManager;
       
            HeaderText = "New Database Connector Source Server";
            TestCommand = new DelegateCommand(TestConnection);
            OkCommand = new DelegateCommand(SaveConnection);
            Testing = true;
            TestPassed = true;
            TestMessage = "Test completed";
        }

        void SaveConnection()
        {
            if(_dbSource != null)
            {
                var res = RequestServiceNameViewModel.ShowSaveDialog();
                if(res==MessageBoxResult.OK)
                {
                    Save(ToDbSource());
                }
            }
            else
            {
                Save(ToDbSource());
            }
        }

        void Save(DbSourceDefinition toDbSource)
        {
            _updateManager.Save(toDbSource);
        }

 
        public ManageDatabaseSourceViewModel(IStudioUpdateManager updateManager, RequestServiceNameViewModel requestServiceNameViewModel):this(updateManager)
        {

            RequestServiceNameViewModel = requestServiceNameViewModel;

        }
        public ManageDatabaseSourceViewModel(IStudioUpdateManager updateManager, IDbSource dbSource)
            : this(updateManager)
        {
            _dbSource = dbSource;


        }
        void TestConnection()
        {
            TestMessage = _updateManager.TestDbConnection(ToDbSource());
        }

        DbSourceDefinition ToDbSource()
        {
            return new DbSourceDefinition
            {
                AuthenticationType = AuthenticationType,
                ServerName = ServerName ,
                Password = Password,
                UserName =  UserName ,
                Type = ServerType

            };
        }
        RequestServiceNameViewModel RequestServiceNameViewModel { get; set; }
        public bool Haschanged
        {
            get { return !ToDbSource().Equals(_dbSource); }
        }

        public enSourceType ServerType
        {
            get { return _serverType; }
            set
            {
                _serverType = value;
                OnPropertyChanged(() => ServerType);
                OnPropertyChanged(()=>Haschanged);
            }
        }

        public AuthenticationType AuthenticationType
        {
            get { return _authenticationType; }
            set
            {
                _authenticationType = value;
                OnPropertyChanged(() => AuthenticationType);
                OnPropertyChanged(() => Haschanged);
            }
        }

        public string ServerName
        {
            get { return _serverName; }
            set
            {
                _serverName = value;
                OnPropertyChanged(() => ServerName);
                OnPropertyChanged(() => Haschanged);
            }
        }

        public string DatabaseName
        {
            get { return _databaseName; }
            set
            {
                _databaseName = value;
                OnPropertyChanged(() => DatabaseName);
                OnPropertyChanged(() => Haschanged);
            }
        }

        public string UserName
        {
            get { return _userName; }
            set
            {
                _userName = value;
                OnPropertyChanged(() => UserName);
                OnPropertyChanged(() => Haschanged);
            }
        }

        public string Password
        {
            get { return _password; }
            set
            {
                _password = value;
                OnPropertyChanged(() => Password);
                OnPropertyChanged(() => Haschanged);
            }
        }

        public ICommand TestCommand { get; set; }

        public ICommand CancelTestCommand { get; set; }

        public string TestMessage
        {
            get { return _testMessage; }
            // ReSharper disable UnusedMember.Local
            private set
                // ReSharper restore UnusedMember.Local
            {
                _testMessage = value;
                OnPropertyChanged(() => TestMessage);
                OnPropertyChanged(()=>TestPassed);
            }
        }


        public bool TestPassed
        {
            get { return _testMessage=="Passed"; }
            // ReSharper disable UnusedMember.Local
 
        }
        public string ServerTypeLabel
        {
            get
            {
                return Resources.Languages.Core.DatabaseSourceTypeLabel;
            }
        }
       

        public string UserNameLabel
        {
            get
            {
                return Resources.Languages.Core.UserNameLabel;
            }
        }

        public string AuthenticationLabel
        {
            get
            {
                return Resources.Languages.Core.AuthenticationTypeLabel;
            }
        }

        public string PasswordLabel
        {
            get
            {
                return Resources.Languages.Core.PasswordLabel;

            }
        }

        public string TestLabel
        {
            get
            {
                return Resources.Languages.Core.TestConnectionLabel;
            }
        }
        
        public string CancelTestLabel
        {
            get
            {
                return Resources.Languages.Core.CancelTest;
            }
        }

        public bool TestPassed { get; private set; }

        public bool TestFailed { get; private set; }

        public bool Testing { get; private set; }


        public string ServerLabel
        {
            get
            {
                return Resources.Languages.Core.DatabaseSourceServerLabel;
            }
        }

        public string DatabaseLabel
        {
            get
            {
                return Resources.Languages.Core.DatabaseSourceDatabaseLabel;
            }
        }

        public ICommand OkCommand { get; set; }

        public string HeaderText
        {
            get { return _header; }
            set
            {
                _header = value;
                OnPropertyChanged(() => HeaderText);
                OnPropertyChanged(() => Header);
            }
        }

        public string WindowsAuthenticationToolTip
        {
            get
            {
                return Resources.Languages.Core.WindowsAuthenticationToolTip;
            }
        }
        public string UserAuthenticationToolTip
        {
            get
            {
                return Resources.Languages.Core.UserAuthenticationToolTip;
            }
        }

        public string ServerTypeTool
        {
            get
            {
                return Resources.Languages.Core.DatabaseSourceTypeToolTip;
            }
        }

        public List<string> DatabaseNames
        {
            get { return _databaseNames; }
            set
            {
                _databaseNames = value;
                OnPropertyChanged(() => DatabaseNames);
            }
        }

        public bool IsActive { get; set; }

        public event EventHandler IsActiveChanged;

        public string Header
        {
            get { return _header; }
            set
            {
                _header = value;
                OnPropertyChanged(() => Header);
            }
        }

        public ResourceType? Image
        {
            get { return ResourceType.DbSource; }
        }
    }
}