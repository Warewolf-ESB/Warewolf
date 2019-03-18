#pragma warning disable CC0091, S1226, S100, CC0044, CC0045, CC0021, S1449, S1541, S1067, S3235, CC0015, S107, S2292, S1450, S105, CC0074, S1135, S101, S3776, CS0168, S2339, CC0031, S3240, CC0020, CS0108, S1694, S1481, CC0008, AD0001, S2328, S2696, S1643, CS0659, CS0067, S104, CC0030, CA2202, S3376, S1185, CS0219, S3253, S1066, CC0075, S3459, S1871, S1125, CS0649, S2737, S1858, CC0082, CC0001, S3241, S2223, S1301, CC0013, S2955, S1944, CS4014, S3052, S2674, S2344, S1939, S1210, CC0033, CC0002, S3458, S3254, S3220, S2197, S1905, S1699, S1659, S1155, CS0105, CC0019, S3626, S3604, S3440, S3256, S2692, S2345, S1109, FS0058, CS1998, CS0661, CS0660, CS0162, CC0089, CC0032, CC0011, CA1001
﻿using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Dev2;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Common.Interfaces.Threading;
using Microsoft.Practices.Prism.PubSubEvents;
using Dev2.Common.Interfaces.Core;
using Dev2.Studio.Interfaces;




namespace Warewolf.Studio.ViewModels
{
    public class ManageWcfSourceViewModel : SourceBaseImpl<IWcfServerSource>, IManageWcfSourceViewModel
    {
        readonly IWcfSourceModel _updateManager;
        readonly IServer _environment;
        CancellationTokenSource _token;
        public ICommand TestCommand { get; set; }
        public ICommand CancelTestCommand { get; set; }
        public ICommand SaveCommand { get; set; }
        public string HeaderText { get; set; }

        string _endPointUrl;

        public ManageWcfSourceViewModel(IWcfSourceModel updateManager, Task<IRequestServiceNameViewModel> requestServiceNameViewModel, IEventAggregator aggregator, IAsyncWorker asyncWorker, IServer environment)
            : this(updateManager, aggregator, asyncWorker, environment)
        {
            VerifyArgument.IsNotNull("requestServiceNameViewModel", requestServiceNameViewModel);
            _requestServiceNameViewModel = requestServiceNameViewModel;
        }

        public ManageWcfSourceViewModel(IWcfSourceModel updateManager, IEventAggregator aggregator, IWcfServerSource wcfSource, IAsyncWorker asyncWorker, IServer environment)
            : this(updateManager, aggregator, asyncWorker, environment)
        {
            VerifyArgument.IsNotNull("source", wcfSource);
            asyncWorker.Start(() => updateManager.FetchSource(wcfSource.Id), source =>
            {
                _wcfServerSource = source;
                _wcfServerSource.Path = wcfSource.Path;
                SetupHeaderTextFromExisting();
                FromModel(source);
            });
        }

        public ManageWcfSourceViewModel(IWcfSourceModel updateManager, IEventAggregator aggregator, IAsyncWorker asyncWorker, IServer environment)
            : base("WcfSource")
        {
            VerifyArgument.IsNotNull("asyncWorker", asyncWorker);
            VerifyArgument.IsNotNull("updateManager", updateManager);
            VerifyArgument.IsNotNull("aggregator", aggregator);
            AsyncWorker = asyncWorker;
            _environment = environment;
            _updateManager = updateManager;
            _endPointUrl = string.Empty;

            HeaderText = Resources.Languages.Core.WcfServiceNewHeaderLabel;
            Header = Resources.Languages.Core.WcfServiceNewHeaderLabel;
            TestCommand = new Microsoft.Practices.Prism.Commands.DelegateCommand(TestConnection, CanTest);
            SaveCommand = new Microsoft.Practices.Prism.Commands.DelegateCommand(SaveConnection, CanSave);
            CancelTestCommand = new Microsoft.Practices.Prism.Commands.DelegateCommand(CancelTest, CanCancelTest);
        }

        bool _testPassed;
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

        bool _testFailed;
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

        public bool CanCancelTest() => Testing;

        public IAsyncWorker AsyncWorker { get; set; }

        public ICommand RefreshCommand { get; set; }

        string _testMessage;
        public string TestMessage
        {
            get { return _testMessage; }
            
            set
            
            {
                _testMessage = value;
                OnPropertyChanged(() => TestMessage);
                OnPropertyChanged(() => TestPassed);
            }
        }

        bool _testing;
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
                ViewModelUtils.RaiseCanExecuteChanged(TestCommand);
                ViewModelUtils.RaiseCanExecuteChanged(CancelTestCommand);
            }
        }

        void TestConnection()
        {
            _token = new CancellationTokenSource();

            var t = new Task(SetupProgressSpinner, _token.Token);

            t.ContinueWith(a => Dispatcher.CurrentDispatcher.Invoke(() =>
            {
                if (!_token.IsCancellationRequested)
                {
                    switch (t.Status)
                    {
                        case TaskStatus.Faulted:
                            {
                                TestFailed = true;
                                TestPassed = false;
                                Testing = false;

                                TestMessage = GetExceptionMessage(t.Exception);
                                break;
                            }
                        case TaskStatus.RanToCompletion:
                            {
                                TestMessage = "";
                                TestFailed = false;
                                TestPassed = true;
                                Testing = false;
                                break;
                            }

                        case TaskStatus.Created:
                            break;
                        case TaskStatus.WaitingForActivation:
                            break;
                        case TaskStatus.WaitingToRun:
                            break;
                        case TaskStatus.Running:
                            break;
                        case TaskStatus.WaitingForChildrenToComplete:
                            break;
                        case TaskStatus.Canceled:
                            break;
                        default:
                            break;
                    }
                }
            }));
            t.Start();
        }

        public bool CanTest()
        {
            if (Testing)
            {
                return false;
            }

            if (string.IsNullOrEmpty(EndpointUrl))
            {
                return false;
            }
            return true;
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

        string _resourceName;
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

        public string EndpointUrl
        {
            get
            {
                return _endPointUrl;
            }
            set
            {
                if (_endPointUrl != value)
                {
                    TestPassed = false;
                }
                _endPointUrl = value;
                OnPropertyChanged(() => EndpointUrl);
                ViewModelUtils.RaiseCanExecuteChanged(TestCommand);
                ViewModelUtils.RaiseCanExecuteChanged(SaveCommand);
            }
        }

        public Action<Action> DispatcherAction { get; set; }

        void SetupProgressSpinner()
        {
            Dispatcher.CurrentDispatcher.Invoke(() =>
            {
                Testing = true;
                TestFailed = false;
                TestPassed = false;
            });

            var wcfServerSource = ToNewSource();
            _updateManager.TestConnection(wcfServerSource);
        }

        public IWcfServerSource ToNewSource() => new WcfServiceSourceDefinition
        {
            EndpointUrl = EndpointUrl,
            ResourceType = "WcfSource",
            Type = enSourceType.WcfSource,
            Id = _wcfServerSource?.Id ?? Guid.NewGuid()
        };

        public string Path { get; set; }
        public override IWcfServerSource ToModel()
        {
            if (Item == null)
            {
                Item = ToSource();
                return Item;
            }

            return new WcfServiceSourceDefinition()
            {
                EndpointUrl = EndpointUrl,
                Id = Item.Id,
                Path = Path,
            };
        }

        readonly Task<IRequestServiceNameViewModel> _requestServiceNameViewModel;

        public IRequestServiceNameViewModel GetRequestServiceNameViewModel()
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
        void SaveConnection()
        {
            if (_wcfServerSource == null)
            {
                var res = GetRequestServiceNameViewModel().ShowSaveDialog();

                if (res == MessageBoxResult.OK)
                {
                    var src = ToSource();
                    src.Name = GetRequestServiceNameViewModel().ResourceName.Name;
                    src.Path = GetRequestServiceNameViewModel().ResourceName.Path ?? GetRequestServiceNameViewModel().ResourceName.Name;
                    Save(src);
                    if (GetRequestServiceNameViewModel().SingleEnvironmentExplorerViewModel != null)
                    {
                        AfterSave(GetRequestServiceNameViewModel().SingleEnvironmentExplorerViewModel.Environments[0].ResourceId, src.Id);
                    }

                    Item = src;
                    _wcfServerSource = src;
                    ResourceName = _wcfServerSource.Name;
                    SetupHeaderTextFromExisting();
                }
            }
            else
            {
                var src = ToSource();
                src.Path = Item.Path ?? "";
                src.Name = Item.Name;
                Save(src);
                Item = src;
                _wcfServerSource = src;
                SetupHeaderTextFromExisting();
            }
            TestPassed = false;
        }

        void SetupHeaderTextFromExisting()
        {
            HeaderText = (_wcfServerSource == null ? ResourceName : _wcfServerSource.Name).Trim();
            Header = (_wcfServerSource == null ? ResourceName : _wcfServerSource.Name).Trim();
        }
        IWcfServerSource _wcfServerSource;
        public IWcfServerSource ToSource()
        {
            if (_wcfServerSource == null)
            {
                return new WcfServiceSourceDefinition()
                {
                    EndpointUrl = EndpointUrl,
                    ResourceName = Name,
                    Name = Name,
                    ResourceType = "WcfSource",
                    ResourceID = _wcfServerSource?.Id ?? Guid.NewGuid(),
                    Id = _wcfServerSource?.Id ?? Guid.NewGuid()
                };
            }
            else
            {
                _wcfServerSource.EndpointUrl = EndpointUrl;
                _wcfServerSource.ResourceType = "WcfSource";
                _wcfServerSource.Name = Name;
                return _wcfServerSource;
            }
        }

        public override void FromModel(IWcfServerSource source)
        {
            ResourceName = source.Name;
            EndpointUrl = source.EndpointUrl;            
        }

        public override bool CanSave() => TestPassed;

        public override void UpdateHelpDescriptor(string helpText)
        {
            var mainViewModel = CustomContainer.Get<IShellViewModel>();
            mainViewModel?.HelpViewModel.UpdateHelpText(helpText);
        }

        public void Save(IWcfServerSource source)
        {
            _updateManager.Save(source);
        }
        public override void Save()
        {
            SaveConnection();
        }
    }
}
