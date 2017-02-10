/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Dev2;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core;
using Dev2.Common.Interfaces.SaveDialog;
using Dev2.Common.Interfaces.Threading;
using Dev2.Interfaces;
using Dev2.Runtime.ServiceModel.Data;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.PubSubEvents;

namespace Warewolf.Studio.ViewModels
{
    public sealed class ManageNewServerViewModel : SourceBaseImpl<IServerSource>, IManageNewServerViewModel
    {
        string _userName;
        string _password;
        string _testMessage;
        string _address;
        bool _testPassed;
        AuthenticationType _authenticationType;

        #region Implementation of IInnerDialogueTemplate

        public IAsyncWorker AsyncWorker { private get; set; }
        readonly IManageServerSourceModel _updateManager;
        CancellationTokenSource _token;

        string _resourceName;
        readonly string _warewolfserverName;
        string _headerText;
        IServerSource _serverSource;
        string _protocol;
        string _selectedPort;
        bool _testing;
        ComputerName _serverName;
        IList<ComputerName> _computerNames;
        string _emptyServerName;

        public ManageNewServerViewModel(IManageServerSourceModel updateManager, IEventAggregator aggregator, IAsyncWorker asyncWorker, IExternalProcessExecutor executor)
            : base("ServerSource")
        {
            VerifyArgument.IsNotNull("executor", executor);
            VerifyArgument.IsNotNull("asyncWorker", asyncWorker);
            VerifyArgument.IsNotNull("updateManager", updateManager);
            VerifyArgument.IsNotNull("aggregator", aggregator);

            AsyncWorker = asyncWorker;
            Protocols = new[] { "https", "http" };
            Protocol = Protocols[0];

            Ports = new ObservableCollection<string> { "3143", "3142" };
            SelectedPort = Ports[0];
            _updateManager = updateManager;

            _warewolfserverName = updateManager.ServerName;
            Header = Resources.Languages.Core.ServerSourceNewHeaderLabel;
            HeaderText = Resources.Languages.Core.ServerSourceNewHeaderLabel;

            TestCommand = new DelegateCommand(CheckVersionConflict, CanTest);
            OkCommand = new DelegateCommand(SaveConnection, CanSave);
            CancelTestCommand = new DelegateCommand(CancelTest, CanCancelTest);
        }
        public ManageNewServerViewModel(IManageServerSourceModel updateManager, Task<IRequestServiceNameViewModel> requestServiceNameViewModel, IEventAggregator aggregator, IAsyncWorker asyncWorker, IExternalProcessExecutor executor)
            : this(updateManager, aggregator, asyncWorker, executor)
        {
            VerifyArgument.IsNotNull("requestServiceNameViewModel", requestServiceNameViewModel);
            RequestServiceNameViewModel = requestServiceNameViewModel;
            GetLoadComputerNamesTask(null);
        }
        public ManageNewServerViewModel(IManageServerSourceModel updateManager, IEventAggregator aggregator, IServerSource serverSource, IAsyncWorker asyncWorker, IExternalProcessExecutor executor)
            : this(updateManager, aggregator, asyncWorker, executor)
        {
            VerifyArgument.IsNotNull("serverSource", serverSource);
            
            _warewolfserverName = updateManager.ServerName;
            AsyncWorker.Start(() => updateManager.FetchSource(serverSource.ID), source =>
            {

                _serverSource = source;
                _serverSource.ResourcePath = serverSource.ResourcePath;
                GetLoadComputerNamesTask(() =>
                    {
                        FromModel(_serverSource);
                        Item = ToModel();
                        SetupHeaderTextFromExisting();
                    }
                );
               
            });
        }

        void SetupHeaderTextFromExisting()
        {
            if (_warewolfserverName != null)
            {
            }
            HeaderText = (_serverSource == null ? ResourceName : _serverSource.Name).Trim();

            Header = _serverSource == null ? ResourceName : _serverSource.Name;
        }

        public override void FromModel(IServerSource source)
        {
            ResourceName = source.Name;
            AuthenticationType = source.AuthenticationType;
            UserName = source.UserName;
            if (ComputerNames != null)
            {
                ServerName = ComputerNames.FirstOrDefault(name => string.Equals(source.ServerName, name.Name, StringComparison.CurrentCultureIgnoreCase));
            }
            if (ServerName != null)
            {
                EmptyServerName = ServerName.Name ?? source.ServerName;
            }

            Protocol = source.Address.Contains("https") ? Protocols[0] : Protocols[1];
            int portIndex = GetSpecifiedIndexOf(source.Address, ':', 2);
            var ports = source.Address.Substring(portIndex + 1).Split('/');
            if (ports.Any())
                SelectedPort = ports[0];
            Address = source.Address;
            Password = source.Password;
            Header = ResourceName;
        }

        private static int GetSpecifiedIndexOf(string str, char ch, int index)
        {
            int i = 0, o = 1;
            while ((i = str.IndexOf(ch, i)) != -1)
            {
                if (o == index) return i;
                o++;
                i++;
            }
            return 0;
        }

        public override string Name
        {
            get
            {
                return ResourceName;
            }
            set
            {
                ResourceName = value;
            }
        }

        public ComputerName ServerName
        {
            get { return _serverName ?? new ComputerName(); }
            set
            {
                if (value != null && value != _serverName)
                {
                    _serverName = value;
                    OnPropertyChanged(() => ServerName);
                    OnPropertyChanged(() => Header);
                    Reset();
                    Address = GetAddressName();
                }
            }
        }
        /// <summary>
        /// The Server Name
        /// </summary>
        public string EmptyServerName
        {
            get { return _emptyServerName; }
            set
            {
                if (value != _emptyServerName)
                {
                    _emptyServerName = value;
                    if (_serverName == null || _serverName.Name != _emptyServerName)
                    {
                        _serverName = null;
                        _serverName = new ComputerName { Name = _emptyServerName };
                    }
                    if (string.IsNullOrWhiteSpace(_emptyServerName))
                    {
                        _serverName = null;
                    }
                    OnPropertyChanged(() => EmptyServerName);
                    OnPropertyChanged(() => Header);
                    Reset();
                    Address = GetAddressName();
                }
            }
        }

        void Reset()
        {
            TestPassed = false;
            TestMessage = "";
            TestFailed = false;
            Testing = false;
            ViewModelUtils.RaiseCanExecuteChanged(TestCommand);
            ViewModelUtils.RaiseCanExecuteChanged(OkCommand);
        }

        public override bool CanSave()
        {
            return TestPassed;
        }

        void GetLoadComputerNamesTask(Action additionalUiAction)
        {
            AsyncWorker.Start(() => _updateManager.GetComputerNames().Select(name => new ComputerName { Name = name }).ToList(), names =>
            {
                ComputerNames = names;
                additionalUiAction?.Invoke();
            }, exception =>
            {
                FailedTesting();
                if (exception.InnerException != null)
                {
                    exception = exception.InnerException;
                }
                TestMessage = exception.Message;
            });
        }

        void SaveConnection()
        {
            if (_serverSource == null)
            {
                RequestServiceNameViewModel.Wait();
                if (RequestServiceNameViewModel.Exception == null)
                {
                    var requestServiceNameViewModel = RequestServiceNameViewModel.Result;
                    var res = requestServiceNameViewModel.ShowSaveDialog();

                    if (res == MessageBoxResult.OK)
                    {
                        ResourceName = requestServiceNameViewModel.ResourceName.Name;
                        var src = ToSource();
                        src.ResourcePath = requestServiceNameViewModel.ResourceName.Path ?? requestServiceNameViewModel.ResourceName.Name;
                        Save(src);
                        if (requestServiceNameViewModel.SingleEnvironmentExplorerViewModel != null)
                            AfterSave(requestServiceNameViewModel.SingleEnvironmentExplorerViewModel.Environments[0].ResourceId, src.ID);
                        Item = src;
                        _serverSource = src;
                        SetupHeaderTextFromExisting();
                    }
                }
                else
                {
                    throw RequestServiceNameViewModel.Exception;
                }
            }
            else
            {
                var src = ToSource();
                Save(src);
                Item = src;
                _serverSource = src;
                SetupHeaderTextFromExisting();
            }
        }

        void Save(IServerSource source)
        {
            _updateManager.Save(source);
            
        }
        public override void Save()
        {
            SaveConnection();
            //ConnectControlSingleton.Instance.ReloadServer();
            
        }

        public override IServerSource ToModel()
        {
            if (Item == null)
            {
                Item = ToSource();
                return Item;
            }
            return new ServerSource
            {
                Name = Item.Name,
                Address = GetAddressName(),
                AuthenticationType = AuthenticationType,
                Password = Password,
                UserName = UserName,
                ID = Item.ID,
                ResourcePath = Item.ResourcePath
            };
        }

        IServerSource ToNewSource()
        {

            return new ServerSource
            {
                AuthenticationType = AuthenticationType,
                Address = GetAddressName(),
                Password = Password,
                UserName = UserName,
                Name = ResourceName,
                ID = _serverSource?.ID ?? Guid.NewGuid()
            };
        }

        IServerSource ToSource()
        {
            if (_serverSource == null)
                return new ServerSource
                {
                    Address = GetAddressName(),
                    AuthenticationType = AuthenticationType,
                    Name = ResourceName,
                    UserName = UserName,
                    Password = Password,
                    ID = _serverSource?.ID ?? SelectedGuid
                }
            ;
            // ReSharper disable once RedundantIfElseBlock
            else
            {
                _serverSource.AuthenticationType = AuthenticationType;
                _serverSource.Address = GetAddressName();
                _serverSource.Password = Password;
                _serverSource.UserName = UserName;
                return _serverSource;
            }
        }

        public bool CanTest()
        {
            if (Testing)
                return false;
            if (string.IsNullOrEmpty(Address))
            {
                return false;
            }
            if (AuthenticationType == AuthenticationType.User)
            {
                return !string.IsNullOrEmpty(UserName) && !string.IsNullOrEmpty(Password);
            }
            return true;
        }

        bool CanCancelTest()
        {
            return Testing;
        }

        void CancelTest()
        {
            if (_token != null)
            {
                if (!_token.IsCancellationRequested && _token.Token.CanBeCanceled)
                {
                    _token.Cancel();
                    Dispatcher.CurrentDispatcher.Invoke(() =>
                    {
                        FailedTesting();
                        TestMessage = "Test Cancelled";
                    });
                }
            }
        }

        void TestConnection()
        {
            if (string.IsNullOrEmpty(GetAddressName()))
            {
                return;
            }
            _token = new CancellationTokenSource();
            AsyncWorker.Start(SetupProgressSpinner, () =>
            {
                TestMessage = "Passed";
                TestFailed = false;
                TestPassed = true;
                Testing = false;
            },
            _token, exception =>
            {
                FailedTesting();
                TestMessage = GetExceptionMessage(exception);
            });
        }

        void SetupProgressSpinner()
        {
            Dispatcher.CurrentDispatcher.Invoke(StartTesting);
            _updateManager.TestConnection(ToNewSource());
        }

        /// <summary>
        /// Command for save/ok
        /// </summary>
        public ICommand OkCommand { get; set; }
        public ICommand CancelTestCommand { get; private set; }

        public string HeaderText
        {
            get { return _headerText; }
            set
            {
                _headerText = value;
                OnPropertyChanged(() => HeaderText);
                OnPropertyChanged(() => Header);
            }
        }
        public IList<ComputerName> ComputerNames
        {
            get
            {
                return _computerNames;
            }
            set
            {
                _computerNames = value;
                OnPropertyChanged(() => ComputerNames);
            }
        }

        #endregion

        #region Implementation of INewServerDialogue

        /// <summary>
        /// The server address that we are trying to connect to
        /// </summary>
        public string Address
        {
            get
            {
                return _address;
            }
            set
            {
                if (_address != value)
                {
                    _address = value;
                    OnPropertyChanged(() => Address);
                    OnPropertyChanged(() => Header);
                    Reset();
                }
            }
        }
        /// <summary>
        ///  Windows or user or public
        /// </summary>
        public AuthenticationType AuthenticationType
        {
            get { return _authenticationType; }
            set
            {
                if (_authenticationType != value)
                {
                    _authenticationType = value;
                    OnPropertyChanged(() => AuthenticationType);
                    OnPropertyChanged(() => Header);
                    OnPropertyChanged(() => UserAuthenticationSelected);
                    Reset();
                }
            }
        }

        /// <summary>
        /// User Name
        /// </summary>
        public string UserName
        {
            get
            {
                return _userName;
            }
            set
            {
                if (_userName != value)
                {
                    _userName = value;
                    OnPropertyChanged(() => UserName);
                    OnPropertyChanged(() => Header);
                    Reset();
                }
            }
        }
        /// <summary>
        /// Password
        /// </summary>
        public string Password
        {
            get
            {
                return _password;
            }
            set
            {
                if (_password != value)
                {
                    _password = value;
                    OnPropertyChanged(() => Password);
                    OnPropertyChanged(() => Header);
                    Reset();
                }
            }
        }

        /// <summary>
        /// The message that will be set if the test is either successful or not
        /// </summary>
        public string TestMessage
        {
            get { return _testMessage; }
            // ReSharper disable UnusedMember.Local
            private set
            // ReSharper restore UnusedMember.Local
            {
                _testMessage = value;
                OnPropertyChanged(() => TestMessage);
                OnPropertyChanged(() => TestPassed);
            }
        }

        #endregion

        private string GetAddressName()
        {
            string addressName = null;
            if (!string.IsNullOrEmpty(ServerName?.Name))
            {
                addressName = GetAddressName(Protocol, ServerName.Name, SelectedPort);
            }
            return addressName;
        }

        string GetAddressName(string protocol, string serverName, string port)
        {
            return protocol + "://" + serverName + ":" + port;
        }

        public string ResourceName
        {
            get
            {
                return _resourceName;
            }
            set
            {
                _resourceName = value;
                OnPropertyChanged(ResourceName);
            }
        }
        public bool TestPassed
        {
            get { return _testPassed; }
            set
            {
                _testPassed = value;
                OnPropertyChanged(() => TestPassed);
                ViewModelUtils.RaiseCanExecuteChanged(OkCommand);
            }
        }
        public bool TestFailed
        {
            get
            {
                return _testPassed;
            }
            set
            {
                _testPassed = value;
                OnPropertyChanged(() => TestFailed);
            }
        }
        public bool Testing
        {
            get
            {
                return _testing;
            }
            set
            {
                _testing = value;
                OnPropertyChanged(() => Testing);
                ViewModelUtils.RaiseCanExecuteChanged(OkCommand);
                ViewModelUtils.RaiseCanExecuteChanged(CancelTestCommand);
            }
        }

        public bool UserAuthenticationSelected => AuthenticationType == AuthenticationType.User;

        /// <summary>
        /// Test if connection is successful
        /// </summary>
        public ICommand TestCommand { get; set; }

        Task<IRequestServiceNameViewModel> RequestServiceNameViewModel { get; }
        public string Protocol
        {
            get
            {
                return _protocol;
            }
            set
            {
                if (_protocol != value)
                {
                    _protocol = value;

                    Reset();
                    if (Protocol == "https" && SelectedPort == "3142")
                    {
                        SelectedPort = "3143";
                    }
                    else if (Protocol == "http" && SelectedPort == "3143")
                    {
                        SelectedPort = "3142";
                    }
                    OnPropertyChanged(Protocol);
                }
            }
        }

        public string[] Protocols { get; set; }
        public ObservableCollection<string> Ports { get; set; }
        public string SelectedPort
        {
            get
            {
                return _selectedPort;
            }
            set
            {
                if (_selectedPort != value)
                {
                    _selectedPort = value;
                    OnPropertyChanged(() => SelectedPort);
                    Reset();
                }
            }
        }

        public override void UpdateHelpDescriptor(string helpText)
        {
            var mainViewModel = CustomContainer.Get<IMainViewModel>();
            mainViewModel?.HelpViewModel.UpdateHelpText(helpText);
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>
        /// A string that represents the current object.
        /// </returns>
        public override string ToString()
        {
            return _headerText;
        }

        void CheckVersionConflict()
        {
            try
            {
                StartTesting();
                TestConnection();
            }
            catch (Exception ex)
            {
                FailedTesting();
                TestMessage = ex.Message;
            }
        }

        void StartTesting()
        {
            TestMessage = "";
            Testing = true;
            TestFailed = false;
            TestPassed = false;
        }
        void FailedTesting()
        {
            TestFailed = true;
            TestPassed = false;
            Testing = false;
        }
    }

    public interface IManageServerSourceModel
    {
        IList<string> GetComputerNames();
        void TestConnection(IServerSource resource);

        void Save(IServerSource toDbSource);


        string ServerName { get; set; }

        IServerSource FetchSource(Guid resourceID);
    }
}