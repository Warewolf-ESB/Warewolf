#pragma warning disable CC0091, S1226, S100, CC0044, CC0045, CC0021, S1449, S1541, S1067, S3235, CC0015, S107, S2292, S1450, S105, CC0074, S1135, S101, S3776, CS0168, S2339, CC0031, S3240, CC0020, CS0108, S1694, S1481, CC0008, AD0001, S2328, S2696, S1643, CS0659, CS0067, S104, CC0030, CA2202, S3376, S1185, CS0219, S3253, S1066, CC0075, S3459, S1871, S1125, CS0649, S2737, S1858, CC0082, CC0001, S3241, S2223, S1301, CC0013, S2955, S1944, CS4014, S3052, S2674, S2344, S1939, S1210, CC0033, CC0002, S3458, S3254, S3220, S2197, S1905, S1699, S1659, S1155, CS0105, CC0019, S3626, S3604, S3440, S3256, S2692, S2345, S1109, FS0058, CS1998, CS0661, CS0660, CS0162, CC0089, CC0032, CC0011, CA1001
﻿using Dev2;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core;
using Dev2.Common.Interfaces.RabbitMQ;
using Microsoft.Practices.Prism.Commands;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Dev2.Common.Interfaces.Threading;
using Dev2.Studio.Interfaces;







namespace Warewolf.Studio.ViewModels
{
    public class ManageRabbitMQSourceViewModel : SourceBaseImpl<IRabbitMQServiceSourceDefinition>, IManageRabbitMQSourceViewModel
    {
        string _hostName;
        int _port;
        string _userName;
        string _password;
        string _virtualHost;

        string _resourceName;
        string _testErrorMessage;
        bool _testPassed;
        bool _testFailed;
        bool _testing;
        string _headerText;

        IRabbitMQServiceSourceDefinition _rabbitMQServiceSource;
        readonly IRabbitMQSourceModel _rabbitMQSourceModel;

        public ManageRabbitMQSourceViewModel(IRabbitMQSourceModel rabbitMQSourceModel, Task<IRequestServiceNameViewModel> requestServiceNameViewModel)
            : this(rabbitMQSourceModel)
        {
            VerifyArgument.IsNotNull("requestServiceNameViewModel", requestServiceNameViewModel);

            _rabbitMQSourceModel = rabbitMQSourceModel;
            RequestServiceNameViewModel = requestServiceNameViewModel;

            HeaderText = Resources.Languages.Core.RabbitMQSourceNewHeaderLabel;
            Header = Resources.Languages.Core.RabbitMQSourceNewHeaderLabel;
            HostName = string.Empty;
            Port = 5672;
            UserName = string.Empty;
            Password = string.Empty;
            VirtualHost = "/";
        }

        public ManageRabbitMQSourceViewModel(IRabbitMQSourceModel rabbitMQSourceModel, IRabbitMQServiceSourceDefinition rabbitMQServiceSource,IAsyncWorker asyncWorker)
            : this(rabbitMQSourceModel)
        {
            VerifyArgument.IsNotNull("rabbitMQServiceSource", rabbitMQServiceSource);
            asyncWorker.Start(() => rabbitMQSourceModel.FetchSource(rabbitMQServiceSource.ResourceID), source =>
            {
                _rabbitMQServiceSource = source;
                _rabbitMQServiceSource.ResourcePath = rabbitMQServiceSource.ResourcePath;
                SetupHeaderTextFromExisting();
                ToItem();
                FromModel(source);
            });
        }

        void ToItem()
        {
            Item = new RabbitMQServiceSourceDefinition()
            {
                HostName = _rabbitMQServiceSource.HostName,
                Password = _rabbitMQServiceSource.Password,
                UserName = _rabbitMQServiceSource.UserName,
                Port = _rabbitMQServiceSource.Port,
                ResourceName = _rabbitMQServiceSource.ResourceName,
                ResourceID = _rabbitMQServiceSource.ResourceID,
                VirtualHost = _rabbitMQServiceSource.VirtualHost,
                ResourcePath = _rabbitMQServiceSource.ResourcePath,
            };
        }

        ManageRabbitMQSourceViewModel(IRabbitMQSourceModel rabbitMQSourceModel)
            : base("RabbitMQSource")
        {
            VerifyArgument.IsNotNull("rabbitMQSourceModel", rabbitMQSourceModel);

            _rabbitMQSourceModel = rabbitMQSourceModel;

            PublishCommand = new DelegateCommand(TestConnection, CanTest);
            OkCommand = new DelegateCommand(SaveConnection, CanSave);

            Testing = false;
            TestPassed = false;
            TestFailed = false;
            TestErrorMessage = "";
        }

        void SetupHeaderTextFromExisting()
        {
            if (_rabbitMQServiceSource != null)
            {
                var headerText = _rabbitMQServiceSource.ResourceName ?? ResourceName;
                HeaderText = Header = !string.IsNullOrWhiteSpace(headerText) ? headerText.Trim() : "";
            }
        }

        public override void FromModel(IRabbitMQServiceSourceDefinition source)
        {
            HostName = source.HostName;
            Port = source.Port;
            UserName = source.UserName;
            Password = source.Password;
            VirtualHost = source.VirtualHost;
            ResourceName = source.ResourceName;
        }

        public override IRabbitMQServiceSourceDefinition ToModel()
        {
            if (Item == null)
            {
                Item = ToSource();
                return Item;
            }
            return new RabbitMQServiceSourceDefinition
            {
                ResourceID = Item.ResourceID,
                ResourceName = Item.ResourceName,
                ResourcePath = Item.ResourcePath,
                HostName = HostName,
                Port = Port,
                UserName = UserName,
                Password = Password,
                VirtualHost = VirtualHost
            };
        }

        public override bool CanSave() => TestPassed;

        public bool CanTest()
        {
            if (Testing)
            {
                return false;
            }

            if (string.IsNullOrEmpty(HostName) ||
                (Port >= 49152 && Port <= 65535) ||
                string.IsNullOrEmpty(UserName) ||
                string.IsNullOrEmpty(Password) ||
                string.IsNullOrEmpty(VirtualHost))
            {
                return false;
            }
            return true;
        }

        public override void UpdateHelpDescriptor(string helpText)
        {
            var mainViewModel = CustomContainer.Get<IShellViewModel>();
            mainViewModel?.HelpViewModel.UpdateHelpText(helpText);
        }

        public override void Save()
        {
            SaveConnection();
        }

        void SaveConnection()
        {
            if (_rabbitMQServiceSource == null)
            {
                RequestServiceNameViewModel.Wait();
                if (RequestServiceNameViewModel.Exception == null)
                {
                    var requestServiceNameViewModel = RequestServiceNameViewModel.Result;
                    var res = requestServiceNameViewModel.ShowSaveDialog();

                    if (res == MessageBoxResult.OK)
                    {
                        ResourceName = requestServiceNameViewModel.ResourceName.Name;
                        var source = ToSource();
                        source.ResourcePath = requestServiceNameViewModel.ResourceName.Path ?? requestServiceNameViewModel.ResourceName.Name;
                        Save(source);
                        AfterSave(requestServiceNameViewModel, source);

                        Item = source;
                        _rabbitMQServiceSource = source;
                        SetupHeaderTextFromExisting();
                    }
                }
            }
            else
            {
                var source = ToSource();
                Save(source);
                Item = source;
                _rabbitMQServiceSource = source;
                SetupHeaderTextFromExisting();
            }
        }

        void AfterSave(IRequestServiceNameViewModel requestServiceNameViewModel, IRabbitMQServiceSourceDefinition source)
        {
            if (requestServiceNameViewModel.SingleEnvironmentExplorerViewModel != null)
            {
                AfterSave(requestServiceNameViewModel.SingleEnvironmentExplorerViewModel.Environments[0].ResourceId, source.ResourceID);
            }
        }

        void Save(IRabbitMQServiceSourceDefinition source)
        {
            _rabbitMQSourceModel.SaveSource(source);
        }

        void TestConnection()
        {
            try
            {
                Testing = true;
                TestErrorMessage = "";
                _rabbitMQSourceModel.TestSource(ToNewSource());
                TestPassed = true;
                TestFailed = false;
                Testing = false;
            }
            catch (Exception exception)
            {
                TestPassed = false;
                TestFailed = true;
                Testing = false;
                TestErrorMessage = GetExceptionMessage(exception);
            }
        }

        IRabbitMQServiceSourceDefinition ToSource()
        {
            if (_rabbitMQServiceSource == null)
            {
                return ToNewSource();
            }

            else
            {
                _rabbitMQServiceSource.HostName = HostName;
                _rabbitMQServiceSource.Port = Port;
                _rabbitMQServiceSource.UserName = UserName;
                _rabbitMQServiceSource.Password = Password;
                _rabbitMQServiceSource.VirtualHost = VirtualHost;
                return _rabbitMQServiceSource;
            }
        }

        IRabbitMQServiceSourceDefinition ToNewSource() => new RabbitMQServiceSourceDefinition
        {
            ResourceName = ResourceName,
            HostName = HostName,
            Port = Port,
            UserName = UserName,
            Password = Password,
            VirtualHost = VirtualHost,
            ResourceID = _rabbitMQServiceSource?.ResourceID ?? Guid.NewGuid()
        };

        public Task<IRequestServiceNameViewModel> RequestServiceNameViewModel { get; set; }

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

        public string HostName
        {
            get { return _hostName; }
            set
            {
                if (value != _hostName)
                {
                    _hostName = value;
                    OnPropertyChanged(() => HostName);
                    OnPropertyChanged(() => Header);
                    ViewModelUtils.RaiseCanExecuteChanged(PublishCommand);
                    ViewModelUtils.RaiseCanExecuteChanged(OkCommand);
                }
            }
        }

        public int Port
        {
            get { return _port; }
            set
            {
                if (value != _port)
                {
                    _port = value;
                    OnPropertyChanged(() => Port);
                    OnPropertyChanged(() => Header);
                    ViewModelUtils.RaiseCanExecuteChanged(PublishCommand);
                    ViewModelUtils.RaiseCanExecuteChanged(OkCommand);
                }
            }
        }

        public string UserName
        {
            get { return _userName; }
            set
            {
                if (value != _userName)
                {
                    _userName = value;
                    OnPropertyChanged(() => UserName);
                    OnPropertyChanged(() => Header);
                    ViewModelUtils.RaiseCanExecuteChanged(PublishCommand);
                    ViewModelUtils.RaiseCanExecuteChanged(OkCommand);
                }
            }
        }

        public string Password
        {
            get { return _password; }
            set
            {
                if (value != _password)
                {
                    _password = value;
                    OnPropertyChanged(() => Password);
                    OnPropertyChanged(() => Header);
                    ViewModelUtils.RaiseCanExecuteChanged(PublishCommand);
                    ViewModelUtils.RaiseCanExecuteChanged(OkCommand);
                }
            }
        }

        public string VirtualHost
        {
            get { return _virtualHost; }
            set
            {
                if (value != _virtualHost)
                {
                    _virtualHost = value;
                    OnPropertyChanged(() => VirtualHost);
                    OnPropertyChanged(() => Header);
                    ViewModelUtils.RaiseCanExecuteChanged(PublishCommand);
                    ViewModelUtils.RaiseCanExecuteChanged(OkCommand);
                }
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
                ViewModelUtils.RaiseCanExecuteChanged(PublishCommand);
            }
        }

        public string TestErrorMessage
        {
            get { return _testErrorMessage; }

            set
            {
                _testErrorMessage = value;
                OnPropertyChanged(() => TestErrorMessage);
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
                ViewModelUtils.RaiseCanExecuteChanged(OkCommand);
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

        public ICommand PublishCommand { get; set; }
        public ICommand OkCommand { get; set; }

        public string HostNameLabel => Resources.Languages.Core.HostNameLabel;

        public string PortLabel => Resources.Languages.Core.PortLabel;

        public string UserNameLabel => Resources.Languages.Core.UserNameLabel;

        public string PasswordLabel => Resources.Languages.Core.PasswordLabel;

        public string VirtualHostLabel => Resources.Languages.Core.VirtualHostLabel;
    }
}