#pragma warning disable CC0091, S1226, S100, CC0044, CC0045, CC0021, S1449, S1541, S1067, S3235, CC0015, S107, S2292, S1450, S105, CC0074, S1135, S101, S3776, CS0168, S2339, CC0031, S3240, CC0020, CS0108, S1694, S1481, CC0008, AD0001, S2328, S2696, S1643, CS0659, CS0067, S104, CC0030, CA2202, S3376, S1185, CS0219, S3253, S1066, CC0075, S3459, S1871, S1125, CS0649, S2737, S1858, CC0082, CC0001, S3241, S2223, S1301, CC0013, S2955, S1944, CS4014, S3052, S2674, S2344, S1939, S1210, CC0033, CC0002, S3458, S3254, S3220, S2197, S1905, S1699, S1659, S1155, CS0105, CC0019, S3626, S3604, S3440, S3256, S2692, S2345, S1109, FS0058, CS1998, CS0661, CS0660, CS0162, CC0089, CC0032, CC0011, CA1001
﻿/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
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
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core;
using Dev2.Common.Interfaces.ServerProxyLayer;
using Dev2.Common.Interfaces.Threading;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Studio.Interfaces;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.PubSubEvents;


namespace Warewolf.Studio.ViewModels
{
    public class ManageWebserviceSourceViewModel : SourceBaseImpl<IWebServiceSource>, IManageWebserviceSourceViewModel
    {
        public IAsyncWorker AsyncWorker { get; set; }
        public IExternalProcessExecutor Executor { get; set; }
        AuthenticationType _authenticationType;
        string _hostName;
        string _userName;
        string _password;
        string _defaultQuery;
        string _testMessage;
        string _testDefault;
        readonly IManageWebServiceSourceModel _updateManager;
        IWebServiceSource _webServiceSource;
        bool _testPassed;
        bool _testFailed;
        bool _testing;
        bool _isHyperLinkEnabled;
        string _resourceName;
        CancellationTokenSource _token;
        readonly string _warewolfserverName;
        string _headerText;
        bool _isDisposed;
        Task<IRequestServiceNameViewModel> _requestServiceNameViewModel;
        public ManageWebserviceSourceViewModel(IManageWebServiceSourceModel updateManager, IEventAggregator aggregator,IAsyncWorker asyncWorker,IExternalProcessExecutor executor)
            : base("WebSource")
        {
            VerifyArgument.IsNotNull("executor", executor);
            VerifyArgument.IsNotNull("asyncWorker", asyncWorker);
            VerifyArgument.IsNotNull("updateManager", updateManager);
            VerifyArgument.IsNotNull("aggregator", aggregator);
            AsyncWorker = asyncWorker;
            Executor = executor;
            _updateManager = updateManager;
            _warewolfserverName = updateManager.ServerName;
            _authenticationType = AuthenticationType.Anonymous;
            _hostName = string.Empty;
            _defaultQuery = string.Empty;
            _userName = string.Empty;
            _password = string.Empty;
            HeaderText = Resources.Languages.Core.WebserviceNewHeaderLabel;
            Header = Resources.Languages.Core.WebserviceNewHeaderLabel;
            TestCommand = new DelegateCommand(TestConnection, CanTest);
            OkCommand = new DelegateCommand(SaveConnection, CanSave);
            CancelTestCommand = new DelegateCommand(CancelTest, CanCancelTest);
            ViewInBrowserCommand = new DelegateCommand(ViewInBrowser, CanViewInBrowser);

        }

        static bool CanViewInBrowser() => true;

        void ViewInBrowser()
        {
            try
            {
                Executor.OpenInBrowser(new Uri(TestDefault));
            }
            catch(Exception exception)
            {
                TestFailed = true;
                TestPassed = false;
                Testing = false;
                TestMessage = exception.Message;
            }
        }

        public ManageWebserviceSourceViewModel(IManageWebServiceSourceModel updateManager, Task<IRequestServiceNameViewModel> requestServiceNameViewModel, IEventAggregator aggregator, IAsyncWorker asyncWorker, IExternalProcessExecutor executor)
            : this(updateManager, aggregator, asyncWorker,executor)
        {
            VerifyArgument.IsNotNull("requestServiceNameViewModel", requestServiceNameViewModel);
            _requestServiceNameViewModel = requestServiceNameViewModel;

        }
        public ManageWebserviceSourceViewModel(IManageWebServiceSourceModel updateManager, IEventAggregator aggregator, IWebServiceSource webServiceSource, IAsyncWorker asyncWorker, IExternalProcessExecutor executor)
            : this(updateManager, aggregator, asyncWorker, executor)
        {
            VerifyArgument.IsNotNull("webServiceSource", webServiceSource);
            _warewolfserverName = updateManager.ServerName;
            AsyncWorker.Start(() => updateManager.FetchSource(webServiceSource.Id), source =>
            {
                _webServiceSource = source;
                _webServiceSource.Path = webServiceSource.Path;
                
                FromModel(_webServiceSource);
                Item = ToSource();
                SetupHeaderTextFromExisting();
            });

        }

        public ManageWebserviceSourceViewModel() : base("WebSource")
        {
          
        }

        void SetupHeaderTextFromExisting()
        {
            var serverName = _warewolfserverName;
            if(serverName.Equals("localhost", StringComparison.OrdinalIgnoreCase))
            {
                HeaderText = (_webServiceSource == null ? ResourceName : _webServiceSource.Name).Trim();
                Header = (_webServiceSource == null ? ResourceName : _webServiceSource.Name).Trim();
            }
            else
            {
                HeaderText = (_webServiceSource == null ? ResourceName : _webServiceSource.Name).Trim();
                Header = (_webServiceSource == null ? ResourceName : _webServiceSource.Name).Trim();
            }
        }

        public override bool CanSave() => TestPassed || CanTest();

        bool CanCancelTest() => Testing;

        void CancelTest()
        {
            if (_token != null && !_token.IsCancellationRequested && _token.Token.CanBeCanceled)
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

        public bool CanTest()
        {
            if (Testing)
            {
                return false;
            }

            if (string.IsNullOrEmpty(HostName))
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
            var mainViewModel = CustomContainer.Get<IShellViewModel>();
            mainViewModel?.HelpViewModel.UpdateHelpText(helpText);
        }

        public override void FromModel(IWebServiceSource source)
        {
            ResourceName = source.Name;
            AuthenticationType = source.AuthenticationType;
            UserName = source.UserName;
            DefaultQuery = source.DefaultQuery;
            HostName = source.HostName;
            Password = source.Password;
            SelectedGuid = source.Id;            
        }

        public override string Name
        {
            get => ResourceName;
            set => ResourceName = value;
        }

        public string ResourceName
        {
            get => _resourceName;
            set
            {
                _resourceName = value;
                OnPropertyChanged(_resourceName);
            }
        }

        public bool UserAuthenticationSelected => AuthenticationType == AuthenticationType.User;

        void SaveConnection()
        {
            if (_webServiceSource == null)
            {
                var res = GetRequestServiceNameViewModel().ShowSaveDialog();

                if (res == MessageBoxResult.OK)
                {
                    ResourceName = GetRequestServiceNameViewModel().ResourceName.Name;
                    var src = ToSource();
                    src.Path = GetRequestServiceNameViewModel().ResourceName.Path ?? GetRequestServiceNameViewModel().ResourceName.Name;
                    Save(src);
                    if (GetRequestServiceNameViewModel().SingleEnvironmentExplorerViewModel != null)
                    {
                        AfterSave(GetRequestServiceNameViewModel().SingleEnvironmentExplorerViewModel.Environments[0].ResourceId, src.Id);
                    }

                    Item = src;
                    _webServiceSource = src;
                    SetupHeaderTextFromExisting();
                }
            }
            else
            {
                var src = ToSource();
                Save(src);
                Item = src;
                _webServiceSource = src;
                SetupHeaderTextFromExisting();
            }
        }

        void Save(IWebServiceSource source) => _updateManager.Save(source);

        public override void Save() => SaveConnection();

        void TestConnection()
        {
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
                TestFailed = true;
                TestPassed = false;
                Testing = false;
                TestMessage = GetExceptionMessage(exception);
            });
        }

        void SetupProgressSpinner()
        {
            if (Application.Current != null && Application.Current.Dispatcher != null)
            {
                Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    Testing = true;
                    TestFailed = false;
                    TestPassed = false;
                }), DispatcherPriority.Background);
            }
            _updateManager.TestConnection(ToNewSource());
        }

        IWebServiceSource ToNewSource() => new WebServiceSourceDefinition
        {
            AuthenticationType = AuthenticationType,
            HostName = HostName,
            Password = Password,
            UserName = UserName,
            Name = ResourceName,
            DefaultQuery = DefaultQuery,
            Id = _webServiceSource?.Id ?? Guid.NewGuid()
        };

        IWebServiceSource ToSource()
        {
            if (_webServiceSource == null)
            {
                return new WebServiceSourceDefinition
                {
                    AuthenticationType = AuthenticationType,
                    HostName = HostName,
                    Password = Password,
                    UserName = UserName,
                    DefaultQuery = DefaultQuery,
                    Name = ResourceName,
                    Id = _webServiceSource?.Id ?? SelectedGuid
                }
            ;
            }
            else
            {
                _webServiceSource.AuthenticationType = AuthenticationType;
                _webServiceSource.DefaultQuery = DefaultQuery;
                _webServiceSource.Password = Password;
                _webServiceSource.HostName = HostName;
                _webServiceSource.UserName = UserName;
                return _webServiceSource;
            }
        }

        public override IWebServiceSource ToModel()
        {
            if (Item == null)
            {
                Item = ToSource();
                return Item;
            }

            return new WebServiceSourceDefinition
            {
                Name = Item.Name,
                HostName = HostName,
                AuthenticationType = AuthenticationType,
                DefaultQuery = DefaultQuery,
                Password = Password,
                UserName = UserName,
                Id = Item.Id,
                Path = Item.Path
            };
        }

        public IRequestServiceNameViewModel GetRequestServiceNameViewModel()
        {
            if (_requestServiceNameViewModel != null)
            {
                _requestServiceNameViewModel.Wait();
                if (_requestServiceNameViewModel.Exception == null)
                {
                    return _requestServiceNameViewModel.Result;
                }

                else
                {
                    throw _requestServiceNameViewModel.Exception;
                }
            }
            return null;
        }

        public void SetRequestServiceNameViewModel(IRequestServiceNameViewModel value)
        {
            _requestServiceNameViewModel = new Task<IRequestServiceNameViewModel>(() => value);
            _requestServiceNameViewModel.Start();
        }

        public AuthenticationType AuthenticationType
        {
            get => _authenticationType;
            set
            {
                if (_authenticationType != value)
                {
                    _authenticationType = value;
                    OnPropertyChanged(() => AuthenticationType);
                    OnPropertyChanged(() => Header);
                    OnPropertyChanged(() => UserAuthenticationSelected);
                    ResetTestValue();
                }
            }
        }

        public string HostName
        {
            get => _hostName;
            set
            {
                if (value != _hostName)
                {
                    _hostName = value;
                    TestDefault = _hostName + "" + DefaultQuery;
                    OnPropertyChanged(() => HostName);
                    ResetTestValue();
                }
            }
        }

        public string DefaultQuery
        {
            get => _defaultQuery;
            set
            {
                if (value != _defaultQuery)
                {
                    _defaultQuery = value;
                    TestDefault = _hostName + "" + DefaultQuery;
                    OnPropertyChanged(() => DefaultQuery);
                    ResetTestValue();
                }
            }
        }

        public string UserName
        {
            get => _userName;
            set
            {
                if (value != _userName)
                {
                    _userName = value;
                    OnPropertyChanged(() => UserName);
                    ResetTestValue();
                }
            }
        }

        void ResetTestValue()
        {
            TestMessage = "";
            TestFailed = false;
            Testing = false;
            OnPropertyChanged(() => Header);
            ViewModelUtils.RaiseCanExecuteChanged(TestCommand);
            ViewModelUtils.RaiseCanExecuteChanged(OkCommand);
        }

        public string Password
        {
            get => _password;
            set
            {
                if (value != _userName)
                {
                    _password = value;
                    OnPropertyChanged(() => Password);
                    ResetTestValue();
                }
            }
        }

        public ICommand CancelTestCommand { get; set; }

        public ICommand TestCommand { get; set; }

        public ICommand ViewInBrowserCommand { get; set; }

        public string TestDefault
        {
            get => _testDefault;
            set
            {
                _testDefault = value;
                OnPropertyChanged(() => TestDefault);
                ViewModelUtils.RaiseCanExecuteChanged(TestCommand);
            }
        }

        public string TestMessage
        {
            get => _testMessage;
            set
            {
                _testMessage = value;
                OnPropertyChanged(() => TestMessage);
                OnPropertyChanged(() => TestPassed);
            }
        }

        public bool TestPassed
        {
            get => _testPassed;
            set
            {
                _testPassed = value;
                if (_testPassed)
                {
                    IsHyperLinkEnabled = true;
                }
                OnPropertyChanged(() => TestPassed);
                ViewModelUtils.RaiseCanExecuteChanged(OkCommand);
                ViewModelUtils.RaiseCanExecuteChanged(ViewInBrowserCommand);
            }
        }

        public bool IsHyperLinkEnabled
        {
            get => _isHyperLinkEnabled;
            set
            {
                _isHyperLinkEnabled = value;
                OnPropertyChanged(() => IsHyperLinkEnabled);
            }
        }

        public bool TestFailed
        {
            get => _testFailed;
            set
            {
                _testFailed = value;
                OnPropertyChanged(() => TestFailed);
            }
        }

        public bool Testing
        {
            get => _testing;
            private set
            {
                _testing = value;

                OnPropertyChanged(() => Testing);
                ViewModelUtils.RaiseCanExecuteChanged(ViewInBrowserCommand);
                ViewModelUtils.RaiseCanExecuteChanged(TestCommand);
                ViewModelUtils.RaiseCanExecuteChanged(CancelTestCommand);
            }
        }

        public ICommand OkCommand { get; set; }

        public string HeaderText
        {
            get => _headerText;
            set
            {
                _headerText = value;
                OnPropertyChanged(() => HeaderText);
                OnPropertyChanged(() => Header);
            }
        }

        public bool IsEmpty => string.IsNullOrEmpty(HostName) && AuthenticationType == AuthenticationType.Anonymous && string.IsNullOrEmpty(UserName) && string.IsNullOrEmpty(Password);

        protected override void OnDispose()
        {
            GetRequestServiceNameViewModel()?.Dispose();
            DisposeManageWebserviceSourceViewModel(true);
        }

        void DisposeManageWebserviceSourceViewModel(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    _token?.Dispose();
                }
                _isDisposed = true;
            }
        }
    }
}
