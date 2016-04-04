﻿using Dev2;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.RabbitMQ;
using Dev2.Common.Interfaces.SaveDialog;
using Dev2.Interfaces;
using Microsoft.Practices.Prism.Commands;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable VirtualMemberCallInContructor
// ReSharper disable ValueParameterNotUsed
// ReSharper disable UnusedMember.Global
// ReSharper disable InconsistentNaming

namespace Warewolf.Studio.ViewModels
{
    public class ManageRabbitMQSourceViewModel : SourceBaseImpl<IRabbitMQServiceSourceDefinition>, IManageRabbitMQSourceViewModel
    {
        private string _hostName;
        private int _port;
        private string _userName;
        private string _password;
        private string _virtualHost;

        private string _resourceName;
        private string _testErrorMessage;
        private bool _testPassed;
        private bool _testFailed;
        private bool _testing;
        private string _headerText;

        private IRabbitMQServiceSourceDefinition _rabbitMQServiceSource;

        private readonly IRabbitMQSourceModel _rabbitMQSourceModel;

        public ManageRabbitMQSourceViewModel(IRabbitMQSourceModel rabbitMQSourceModel, Task<IRequestServiceNameViewModel> requestServiceNameViewModel)
            : this(rabbitMQSourceModel)
        {
            VerifyArgument.IsNotNull("requestServiceNameViewModel", requestServiceNameViewModel);

            _rabbitMQSourceModel = rabbitMQSourceModel;
            RequestServiceNameViewModel = requestServiceNameViewModel;

            HeaderText = Resources.Languages.Core.RabbitMQSourceNewHeaderLabel;
            Header = Resources.Languages.Core.RabbitMQSourceNewHeaderLabel;
            HostName = String.Empty;
            Port = 5672;
            UserName = String.Empty;
            Password = String.Empty;
            VirtualHost = "/";
        }

        public ManageRabbitMQSourceViewModel(IRabbitMQSourceModel rabbitMQSourceModel, IRabbitMQServiceSourceDefinition rabbitMQServiceSource)
            : this(rabbitMQSourceModel)
        {
            VerifyArgument.IsNotNull("rabbitMQServiceSource", rabbitMQServiceSource);

            _rabbitMQServiceSource = rabbitMQServiceSource;
            SetupHeaderTextFromExisting();
            FromModel(rabbitMQServiceSource);
        }

        private ManageRabbitMQSourceViewModel(IRabbitMQSourceModel rabbitMQSourceModel)
            : base(ResourceType.RabbitMQSource)
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

        private void SetupHeaderTextFromExisting()
        {
            if (_rabbitMQServiceSource != null)
            {
                string headerText = _rabbitMQServiceSource.ResourceName ?? ResourceName;
                HeaderText = Header = !string.IsNullOrWhiteSpace(headerText) ? headerText.Trim() : "";
            }
        }

        public override void FromModel(IRabbitMQServiceSourceDefinition rabbitMQServiceSource)
        {
            HostName = rabbitMQServiceSource.HostName;
            Port = rabbitMQServiceSource.Port;
            UserName = rabbitMQServiceSource.UserName;
            Password = rabbitMQServiceSource.Password;
            VirtualHost = rabbitMQServiceSource.VirtualHost;
            ResourceName = rabbitMQServiceSource.ResourceName;
        }

        public override bool CanSave()
        {
            return TestPassed;
        }

        public bool CanTest()
        {
            if (Testing)
            {
                return false;
            }

            if (String.IsNullOrEmpty(HostName) ||
                (Port >= 49152 && Port <= 65535) ||
                String.IsNullOrEmpty(UserName) ||
                String.IsNullOrEmpty(Password) ||
                String.IsNullOrEmpty(VirtualHost))
            {
                return false;
            }
            return true;
        }

        public override void UpdateHelpDescriptor(string helpText)
        {
            var mainViewModel = CustomContainer.Get<IMainViewModel>();
            if (mainViewModel != null)
            {
                mainViewModel.HelpViewModel.UpdateHelpText(helpText);
            }
        }

        public override void Save()
        {
            SaveConnection();
        }

        private void SaveConnection()
        {
            if (_rabbitMQServiceSource == null)
            {
                RequestServiceNameViewModel.Wait();
                if (RequestServiceNameViewModel.Exception == null)
                {
                    var res = RequestServiceNameViewModel.Result.ShowSaveDialog();

                    if (res == MessageBoxResult.OK)
                    {                        
                        SaveAndSetItem();
                        if (_rabbitMQServiceSource != null)
                        {
                            _rabbitMQServiceSource.ResourceName = RequestServiceNameViewModel.Result.ResourceName.Name;
                            _rabbitMQServiceSource.ResourcePath = RequestServiceNameViewModel.Result.ResourceName.Path ?? RequestServiceNameViewModel.Result.ResourceName.Name;
                            _resourceName = _rabbitMQServiceSource.ResourceName;
                        }
                    }
                }
            }
            else
            {
                SaveAndSetItem();
            }
        }

        private void SaveAndSetItem()
        {
            IRabbitMQServiceSourceDefinition source = ToSource();
            Save(source);
            Item = source;
            _rabbitMQServiceSource = source;
            SetupHeaderTextFromExisting();
        }

        private void Save(IRabbitMQServiceSourceDefinition source)
        {
            _rabbitMQSourceModel.SaveSource(source);
        }

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

            private set
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
                ViewModelUtils.RaiseCanExecuteChanged(OkCommand);
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

        [ExcludeFromCodeCoverage]
        public string HostNameLabel
        {
            get
            {
                return Resources.Languages.Core.HostNameLabel;
            }
        }

        [ExcludeFromCodeCoverage]
        public string PortLabel
        {
            get
            {
                return Resources.Languages.Core.PortLabel;
            }
        }

        [ExcludeFromCodeCoverage]
        public string UserNameLabel
        {
            get
            {
                return Resources.Languages.Core.UserNameLabel;
            }
        }

        [ExcludeFromCodeCoverage]
        public string PasswordLabel
        {
            get
            {
                return Resources.Languages.Core.PasswordLabel;
            }
        }

        [ExcludeFromCodeCoverage]
        public string VirtualHostLabel
        {
            get
            {
                return Resources.Languages.Core.VirtualHostLabel;
            }
        }

        private void TestConnection()
        {
            try
            {
                TestErrorMessage = "";
                _rabbitMQSourceModel.TestSource(ToNewSource());
                TestPassed = true;
            }
            catch (Exception e)
            {
                TestPassed = false;
                TestErrorMessage = "Failed: " + e.Message;
            }
        }

        private IRabbitMQServiceSourceDefinition ToNewSource()
        {
            return new RabbitMQServiceSourceDefinition
            {
                HostName = HostName,
                Port = Port,
                UserName = UserName,
                Password = Password,
                VirtualHost = VirtualHost,
                ResourceID = _rabbitMQServiceSource == null ? Guid.NewGuid() : _rabbitMQServiceSource.ResourceID
            };
        }

        private IRabbitMQServiceSourceDefinition ToSource()
        {
            if (_rabbitMQServiceSource == null)
            {
                return ToNewSource();
            }
            // ReSharper disable once RedundantIfElseBlock
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

        public override IRabbitMQServiceSourceDefinition ToModel()
        {
            if (Item == null)
            {
                Item = ToSource();
                return Item;
            }
            return new RabbitMQServiceSourceDefinition
                {
                    HostName = HostName,
                    Port = Port,
                    UserName = UserName,
                    Password = Password,
                    VirtualHost = VirtualHost,
                    ResourceID = _rabbitMQServiceSource == null ? Guid.NewGuid() : _rabbitMQServiceSource.ResourceID
                };
        }
    }
}