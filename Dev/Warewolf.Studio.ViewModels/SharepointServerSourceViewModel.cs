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
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Dev2;
using Dev2.Common;
using Dev2.Common.Common;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core;
using Dev2.Common.Interfaces.SaveDialog;
using Dev2.Common.Interfaces.Threading;
using Dev2.Data.ServiceModel;
using Dev2.Interfaces;
using Dev2.Runtime.Configuration.ViewModels.Base;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Studio.Core.Interfaces;
using Microsoft.Practices.Prism.PubSubEvents;
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable VirtualMemberCallInContructor
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Warewolf.Studio.ViewModels
{
    public class SharepointServerSourceViewModel : SourceBaseImpl<ISharepointServerSource>, IManageSharePointSourceViewModel, IDisposable
    {
        public IAsyncWorker AsyncWorker { get; set; }
        ISharepointServerSource _sharePointServiceSource;
        private readonly IEnvironmentModel _environment;
        private readonly ISharePointSourceModel _updateManager;
        private string _serverName;
        private bool _isWindows;
        private bool _isUser;
        private string _userName;
        private string _password;
        private string _testResult;
        private IContextualResourceModel _resource;
        private AuthenticationType _authenticationType;
        private CancellationTokenSource _token;
        private bool _testComplete;
        private bool _isLoading;
        private bool _testPassed;
        private string _resourceName;
        private bool _testing;
        private string _headerText;
        private string _testMessage;
        private bool _testFailed;
        private string _path;
        private bool _isDisposed;
        readonly Task<IRequestServiceNameViewModel> _requestServiceNameViewModel;

        public SharepointServerSourceViewModel(ISharePointSourceModel updateManager, IEventAggregator aggregator, IAsyncWorker asyncWorker, IEnvironmentModel environment)
            : base("SharepointServerSource")
        {
            VerifyArgument.IsNotNull("asyncWorker", asyncWorker);
            VerifyArgument.IsNotNull("updateManager", updateManager);
            VerifyArgument.IsNotNull("aggregator", aggregator);
            AsyncWorker = asyncWorker;
            _environment = environment;
            _updateManager = updateManager;
            _authenticationType = AuthenticationType.Windows;
            _serverName = string.Empty;
            _userName = string.Empty;
            _password = string.Empty;
            IsWindows = true;
            HeaderText = Resources.Languages.Core.SharePointServiceNewHeaderLabel;
            Header = Resources.Languages.Core.SharePointServiceNewHeaderLabel;
            TestCommand = new Microsoft.Practices.Prism.Commands.DelegateCommand(TestConnection, CanTest);
            SaveCommand = new Microsoft.Practices.Prism.Commands.DelegateCommand(SaveConnection, CanSave);
            CancelTestCommand = new Microsoft.Practices.Prism.Commands.DelegateCommand(CancelTest, CanCancelTest);
        }

        public SharepointServerSourceViewModel(ISharePointSourceModel updateManager, Task<IRequestServiceNameViewModel> requestServiceNameViewModel, IEventAggregator aggregator, IAsyncWorker asyncWorker, IEnvironmentModel environment)
            : this(updateManager, aggregator, asyncWorker, environment)
        {
            VerifyArgument.IsNotNull("requestServiceNameViewModel", requestServiceNameViewModel);
            _requestServiceNameViewModel = requestServiceNameViewModel;
        }
        public SharepointServerSourceViewModel(ISharePointSourceModel updateManager, IEventAggregator aggregator, ISharepointServerSource sharePointServiceSource, IAsyncWorker asyncWorker, IEnvironmentModel environment)
            : this(updateManager, aggregator, asyncWorker, environment)
        {
            VerifyArgument.IsNotNull("sharePointServiceSource", sharePointServiceSource);

            asyncWorker.Start(() => updateManager.FetchSource(sharePointServiceSource.Id), source =>
            {
                _sharePointServiceSource = source;
                _sharePointServiceSource.Path = sharePointServiceSource.Path;
                SetupHeaderTextFromExisting();
                FromModel(source);
            });

        }

        void SetupHeaderTextFromExisting()
        {
            HeaderText = (_sharePointServiceSource == null ? ResourceName : _sharePointServiceSource.Name).Trim();
            Header = (_sharePointServiceSource == null ? ResourceName : _sharePointServiceSource.Name).Trim();
        }

        public override bool CanSave()
        {
            return TestPassed;
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
                        Testing = false;
                        TestFailed = true;
                        TestPassed = false;
                        TestMessage = "Test Cancelled";
                    });
                }
            }
        }

        public bool CanTest()
        {
            if (Testing)
                return false;
            if (string.IsNullOrEmpty(ServerName))
            {
                return false;
            }
            if (AuthenticationType == AuthenticationType.User)
            {
                return !string.IsNullOrEmpty(UserName) && !string.IsNullOrEmpty(Password);
            }
            return true;
        }

        public override void UpdateHelpDescriptor(string helpText)
        {
            var mainViewModel = CustomContainer.Get<IMainViewModel>();
            mainViewModel?.HelpViewModel.UpdateHelpText(helpText);
        }

        public override void FromModel(ISharepointServerSource sharepointServerSource)
        {
            ResourceName = sharepointServerSource.Name;
            AuthenticationType = sharepointServerSource.AuthenticationType;
            UserName = sharepointServerSource.UserName;
            ServerName = sharepointServerSource.Server;
            Password = sharepointServerSource.Password;
            IsSharepointOnline = sharepointServerSource.IsSharepointOnline;
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

        public string ResourceName
        {
            get
            {
                return _resourceName;
            }
            set
            {
                _resourceName = value;
                OnPropertyChanged(_resourceName);
            }
        }

        public bool UserAuthenticationSelected => AuthenticationType == AuthenticationType.User;

        void SaveConnection()
        {
            if (_sharePointServiceSource == null)
            {
                var res = RequestServiceNameViewModel.ShowSaveDialog();

                if (res == MessageBoxResult.OK)
                {
                    ResourceName = RequestServiceNameViewModel.ResourceName.Name;
                    var src = ToSource();
                    src.Path = RequestServiceNameViewModel.ResourceName.Path ?? RequestServiceNameViewModel.ResourceName.Name;
                    Save(src);
                    if (RequestServiceNameViewModel.SingleEnvironmentExplorerViewModel != null)
                        AfterSave(RequestServiceNameViewModel.SingleEnvironmentExplorerViewModel.Environments[0].ResourceId, src.Id);
                    Item = src;
                    _sharePointServiceSource = src;
                    SetupHeaderTextFromExisting();
                }
            }
            else
            {
                var src = ToSource();
                Save(src);
                Item = src;
                _sharePointServiceSource = src;
                SetupHeaderTextFromExisting();
            }
        }

        public void Save(ISharepointServerSource source)
        {
            _updateManager.Save(source);
        }
        public override void Save()
        {
            SaveConnection();
        }
        
        void TestConnection()
        {
            _token = new CancellationTokenSource();
            AsyncWorker.Start(SetupProgressSpinner, () =>
            {
                TestMessage = "";
                TestFailed = false;
                TestPassed = true;
                Testing = false;
            },
            _token, exception =>
            {
                TestFailed = true;
                TestPassed = false;
                Testing = false;
                TestMessage = GetExceptionMessage(exception);
            });
        }

        void SetupProgressSpinner()
        {
            Dispatcher.CurrentDispatcher.Invoke(() =>
            {
                Testing = true;
                TestFailed = false;
                TestPassed = false;
            });
            var sharepointServerSource = ToNewSource();
            _updateManager.TestConnection(sharepointServerSource);
            IsSharepointOnline = sharepointServerSource.IsSharepointOnline;
        }

        ISharepointServerSource ToNewSource()
        {
            return new SharePointServiceSourceDefinition
            {
                AuthenticationType = AuthenticationType,
                Server = ServerName,
                Password = Password,
                UserName = UserName,
                Name = ResourceName,
                Id = _sharePointServiceSource?.Id ?? Guid.NewGuid()
            };
        }

        ISharepointServerSource ToSource()
        {
            if (_sharePointServiceSource == null)
                return new SharePointServiceSourceDefinition
                {
                    AuthenticationType = AuthenticationType,
                    Server = ServerName,
                    Password = Password,
                    UserName = UserName,
                    Name = ResourceName,
                    IsSharepointOnline = IsSharepointOnline,
                    Id = _sharePointServiceSource?.Id ?? Guid.NewGuid()
                };
            // ReSharper disable once RedundantIfElseBlock
            else
            {
                _sharePointServiceSource.AuthenticationType = AuthenticationType;
                _sharePointServiceSource.Server = ServerName;
                _sharePointServiceSource.Password = Password;
                _sharePointServiceSource.UserName = UserName;
                _sharePointServiceSource.IsSharepointOnline = IsSharepointOnline;
                return _sharePointServiceSource;
            }
        }

        public override ISharepointServerSource ToModel()
        {
            if (Item == null)
            {
                Item = ToSource();
                return Item;
            }

            return new SharePointServiceSourceDefinition
            {
                Name = ResourceName,
                Server = ServerName,
                AuthenticationType = AuthenticationType,
                UserName = UserName,
                Password = Password,
                Id = Item.Id,
                Path = Path,
                IsSharepointOnline = IsSharepointOnline
            };
        }

        IRequestServiceNameViewModel RequestServiceNameViewModel
        {
            get
            {
                _requestServiceNameViewModel.Wait();
                if (_requestServiceNameViewModel.Exception == null)
                {
                    return _requestServiceNameViewModel.Result;
                }
                // ReSharper disable once RedundantIfElseBlock
                else
                {
                    throw _requestServiceNameViewModel.Exception;
                }
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
                if (_authenticationType != value)
                {
                    _authenticationType = value;
                    if (_authenticationType == AuthenticationType.Windows)
                    {
                        IsWindows = true;
                        OnPropertyChanged(() => IsWindows);
                    }
                    else
                    {
                        IsUser = true;
                        OnPropertyChanged(() => IsUser);
                    }
                    OnPropertyChanged(() => AuthenticationType);
                    OnPropertyChanged(() => Header);
                    OnPropertyChanged(() => UserAuthenticationSelected);
                    TestPassed = false;
                    ViewModelUtils.RaiseCanExecuteChanged(TestCommand);
                    ViewModelUtils.RaiseCanExecuteChanged(SaveCommand);
                }
            }
        }

        public string ServerName
        {
            get
            {
                return _serverName;
            }
            set
            {
                if (_serverName != value)
                {
                    TestPassed = false;
                }
                _serverName = value;
                OnPropertyChanged(() => ServerName);
                ViewModelUtils.RaiseCanExecuteChanged(TestCommand);
                ViewModelUtils.RaiseCanExecuteChanged(SaveCommand);
            }
        }

        public string UserName
        {
            get
            {
                return _userName;
            }
            set
            {
                _userName = value;
                OnPropertyChanged(() => UserName);
                OnPropertyChanged(() => Header);
                TestPassed = false;
                ViewModelUtils.RaiseCanExecuteChanged(TestCommand);
                ViewModelUtils.RaiseCanExecuteChanged(SaveCommand);
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
                _password = value;
                OnPropertyChanged(() => Password);
                OnPropertyChanged(() => Header);
                TestPassed = false;
                ViewModelUtils.RaiseCanExecuteChanged(TestCommand);
                ViewModelUtils.RaiseCanExecuteChanged(SaveCommand);
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

                OnPropertyChanged(() => IsWindows);
                ViewModelUtils.RaiseCanExecuteChanged(TestCommand);
                ViewModelUtils.RaiseCanExecuteChanged(SaveCommand);
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
                OnPropertyChanged(() => IsUser);
            }
        }
        public bool TestFailed
        {
            get
            {
                return _testFailed;
            }
            set
            {
                _testFailed = value;
                OnPropertyChanged(() => TestFailed);
            }
        }
        public bool TestPassed
        {
            get { return _testPassed; }
            set
            {
                _testPassed = value;
                OnPropertyChanged(() => TestPassed);
                ViewModelUtils.RaiseCanExecuteChanged(SaveCommand);
            }
        }
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

        public ICommand TestCommand { get; set; }
        public ICommand SaveCommand { get; set; }
        public ICommand CancelTestCommand { get; set; }

        public bool TestComplete
        {
            get { return _testComplete; }
            set
            {
                _testComplete = value;
                OnPropertyChanged("TestComplete");
                var command = SaveCommand as RelayCommand;
                command?.RaiseCanExecuteChanged();
            }
        }
        public bool Testing
        {
            get
            {
                return _testing;
            }
            private set
            {
                _testing = value;

                OnPropertyChanged(() => Testing);
                ViewModelUtils.RaiseCanExecuteChanged(TestCommand);
                ViewModelUtils.RaiseCanExecuteChanged(CancelTestCommand);
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
                OnPropertyChanged(() => IsLoading);
            }
        }

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
                OnPropertyChanged("TestResult");
            }
        }

        public string Path
        {
            get
            {
                return _path;
            }
            set
            {
                _path = value;
                OnPropertyChanged(() => Path);
            }
        }

        public bool IsSharepointOnline { get; set; }

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
        void UpdateBasedOnResource(SharepointSource sharepointSource)
        {
            ServerName = sharepointSource.Server;
            UserName = sharepointSource.UserName;
            Password = sharepointSource.Password;
            AuthenticationType = sharepointSource.AuthenticationType;

        }

        #region Implementation of IDisposable

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public new void Dispose()
        {
            Dispose(true);

            // This object will be cleaned up by the Dispose method.
            // Therefore, you should call GC.SupressFinalize to
            // take this object off the finalization queue
            // and prevent finalization code for this object
            // from executing a second time.
            GC.SuppressFinalize(this);
        }

        // Dispose(bool disposing) executes in two distinct scenarios.
        // If disposing equals true, the method has been called directly
        // or indirectly by a user's code. Managed and unmanaged resources
        // can be disposed.
        // If disposing equals false, the method has been called by the
        // runtime from inside the finalizer and you should not reference
        // other objects. Only unmanaged resources can be disposed.
        void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (!_isDisposed)
            {
                // If disposing equals true, dispose all managed
                // and unmanaged resources.
                if (disposing)
                {
                    // Dispose managed resources.
                    _token.Dispose();
                }

                // Dispose unmanaged resources.
                _isDisposed = true;
            }
        }
        #endregion
    }
}
