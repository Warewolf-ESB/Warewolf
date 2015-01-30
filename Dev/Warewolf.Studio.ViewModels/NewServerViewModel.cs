using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using Dev2;
using Dev2.Common.Interfaces.Communication;
using Dev2.Common.Interfaces.Core;
using Dev2.Common.Interfaces.Explorer;
using Dev2.Common.Interfaces.PopupController;
using Dev2.Common.Interfaces.Runtime.ServiceModel;
using Dev2.Common.Interfaces.SaveDialog;
using Dev2.Common.Interfaces.ServerDialogue;
using Dev2.Common.Interfaces.Studio.ViewModels.Dialogues;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Mvvm;

namespace Warewolf.Studio.ViewModels
{
    public class NewServerViewModel : BindableBase, INewServerDialogue
    {
        ICommand _okCommand;
        ICommand _cancelCommand;
        string _userName;
        string _password;
        string _testMessage;
        bool _isValid;
        string _address;
        bool _testPassed;
        AuthenticationType _authenticationType;

        #region Implementation of IInnerDialogueTemplate

        readonly IServerConnectionTest _connectionTest;
        readonly IStudioUpdateManager _updateManager;
        readonly ISaveDialog _saveDialog;
        IServerSource _serverSource;
        bool _testrun;
        bool _canClickOk;
        string _subHeaderText;
        string _headerText;
        DialogResult _result;

        public NewServerViewModel()
        {
          
        }

        // ReSharper disable TooManyDependencies
        public NewServerViewModel(IServerSource newServerSource, IServerConnectionTest connectionTest, IStudioUpdateManager updateManager,ISaveDialog saveDialog)
            // ReSharper restore TooManyDependencies
        {
            VerifyArgument.AreNotNull(new Dictionary<string, object> { { "newServerSource", newServerSource }, { "connectionTest", connectionTest }, { "updateManager", updateManager }, { "saveDialog", saveDialog } });

            _connectionTest = connectionTest;
            _updateManager = updateManager;
            _saveDialog = saveDialog;
            ServerSource = newServerSource;
            _subHeaderText = newServerSource.Address;
            _headerText = String.IsNullOrEmpty(newServerSource.Name) ? "New Server Source" : "Edit " + newServerSource.Name;

            IsValid = false;
            Address = newServerSource.Address;
            AuthenticationType = newServerSource.AuthenticationType;
            UserName = newServerSource.UserName;
            Password = newServerSource.Password;
            TestPassed = false;


            TestCommand = new DelegateCommand(Test);
            OkCommand = new DelegateCommand(Save);
            CancelCommand = new DelegateCommand(() => CloseAction.Invoke());
        }

        void Save()
        {   var res = MessageBoxResult.OK;
            if(String.IsNullOrEmpty(ServerSource.Name))
             res =  SaveDialog.ShowSaveDialog();
            if(res==MessageBoxResult.OK)
            {
                try
                {
                    var source = new ServerSource
                    {
                        Address = Address,
                        AuthenticationType = AuthenticationType,
                        ID = ServerSource.ID == Guid.Empty ? Guid.NewGuid() : ServerSource.ID,
                        Name = String.IsNullOrEmpty(ServerSource.Name) ? SaveDialog.ResourceName.Name : ServerSource.Name,
                        Password = Password,
                        ResourcePath = "" //todo: needs to come from explorer
                    };

                    ServerSource = source;
                    _updateManager.Save(source);
                    Result = DialogResult.Success;
                    if(CloseAction != null)
                        CloseAction.Invoke();
                }
                catch (Exception err)
                {

                    TestMessage = err.Message;
                }
            }

        }

        public Action CloseAction { get; set; }
        void Test()
        {
            TestFailed = false;
            TestPassed = false;
            Testing = true;
            
            TestMessage = _updateManager.TestConnection(new ServerSource()
            {
                Address = Address,
                AuthenticationType = AuthenticationType,
                ID = ServerSource.ID == Guid.Empty ? Guid.NewGuid() : ServerSource.ID,
                Name = String.IsNullOrEmpty(ServerSource.Name) ? "" : ServerSource.Name,
                Password = Password,
                ResourcePath = "" //todo: needs to come from explorer
            });
            Testrun = true;
            Testing = false;
            if (TestMessage == "")
            {
                TestPassed = true;
                TestFailed = false;
            }
            else
            {
                TestPassed = false;
                TestFailed = true;
            }
            
            OnPropertyChanged(()=>Validate);
            OnPropertyChanged(() => CanClickOk);
           
        }

        /// <summary>
        /// called by outer when validating
        /// </summary>
        /// <returns></returns>
        public string Validate
        {

            get
            {
                IsValid = false;



                if (!Testrun)
                {
                    return Resources.Languages.Core.ServerDialogNoTestMessage;
                }

                if(TestFailed)
                {
                    return TestMessage;
                }
               
                IsValid = true;
                return String.Empty;
            }



        }
        public bool Testrun
        {
            get
            {
                return _testrun;
            }
            set
            {
                _testrun = value;
            }
        }

        /// <summary>
        /// called by outer when validating
        /// </summary>
        /// <returns></returns>
        string IInnerDialogueTemplate.Validate()
        {
            return Validate;
        }

        /// <summary>
        /// Is valid 
        /// </summary>
        public bool IsValid
        {
            get
            {
                return _isValid;
            }
            set
            {
                _isValid = value;
            }
        }
        /// <summary>
        /// Command for save/ok
        /// </summary>
        public ICommand OkCommand
        {
            get
            {
                return _okCommand;
            }
            set
            {
                _okCommand = value;
            }
        }
        /// <summary>
        /// Command for cancel
        /// </summary>
        public ICommand CancelCommand
        {
            get
            {
                return _cancelCommand;
            }
            set
            {
                _cancelCommand = value;
            }
        }
        public bool CanClickOk
        {
            get
            {
                return Validate == "";
            }
        }
        public string SubHeaderText
        {
            get
            {
                return _subHeaderText;
            }

        }
        public string HeaderText
        {
            get
            {
                return _headerText;
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
                _address = value;
                OnPropertyChanged(() => Address);
                OnPropertyChanged(()=>Validate);
                OnPropertyChanged(() => CanClickOk);
            }

        }
        /// <summary>
        ///  Windows or user or publlic
        /// </summary>
        public AuthenticationType AuthenticationType
        {
            get
            {
                return _authenticationType;
            }
            set
            {
                _authenticationType = value;
                OnPropertyChanged(() => AuthenticationType);
                OnPropertyChanged(() => IsUserNameVisible);
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
                _userName = value;
                OnPropertyChanged(() => UserName);
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
                _password = value;
                OnPropertyChanged(() => Password);
            }
        }

        /// <summary>
        /// The message that will be set if the test is either successful or not
        /// </summary>
        public string TestMessage
        {
            get
            {
                return _testMessage;
            }

            set
            {
                _testMessage = value;
                OnPropertyChanged(() => TestMessage);
            }

        }

        #endregion


        public bool TestPassed
        {
            get
            {
                return _testPassed;
            }
            set
            {
                _testPassed = value;
                OnPropertyChanged(() => TestPassed);
                OnPropertyChanged(() => CanClickOk);
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
                OnPropertyChanged(() => CanClickOk);
            }
        }
        public bool Testing
        {
            get
            {
                return _testPassed;
            }
            set
            {
                _testPassed = value;
                OnPropertyChanged(() => Testing);
                OnPropertyChanged(() => CanClickOk);
            }
        }

        public bool IsOkEnabled
        {
            get
            {
                return IsValid;
            }

        }

        public bool IsTestEnabled
        {
            get
            {
                return (Address.Length > 0);
            }

        }

        public bool IsUserNameVisible
        {
            get
            {
                return AuthenticationType == AuthenticationType.User;
            }

        }

        public bool IsPasswordVisible
        {
            get
            {
                return AuthenticationType == AuthenticationType.User;
            }

        }

        public string AddressLabel
        {
            get
            {
                return Resources.Languages.Core.ServerDialogAddressLabel;
            }
        }

        public string UserNameLabel
        {
            get
            {
                return Resources.Languages.Core.ServerDialogUserNameLabel;
            }
        }

        public string AuthenticationLabel
        {
            get
            {
                return Resources.Languages.Core.ServerDialogAuthenticationTypeLabel;
            }
        }

        public string PasswordLabel
        {
            get
            {
                return Resources.Languages.Core.ServerDialogPasswordLabel;

            }
        }

        public string TestLabel
        {
            get
            {
                return Resources.Languages.Core.ServerDialogTestConnectionLabel;
            }
        }
        public DialogResult Result
        {
            get
            {
                return _result;
            }
            set
            {
                _result = value;
            }
        }

        /// <summary>
        /// Test if connection is successful
        /// </summary>
        public ICommand TestCommand
        { get; set; }
        public IServerSource ServerSource
        {
            get
            {
                return _serverSource;
            }
            set
            {
                _serverSource = value;
            }
        }
        public ISaveDialog SaveDialog
        {
            get
            {
                return _saveDialog;
            }
        }
    }
}