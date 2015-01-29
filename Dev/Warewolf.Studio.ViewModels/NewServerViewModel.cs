using System;
using System.Collections.Generic;
using System.Windows.Input;
using Dev2;
using Dev2.Common.Interfaces.Communication;
using Dev2.Common.Interfaces.Explorer;
using Dev2.Common.Interfaces.Runtime.ServiceModel;
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
        IServerSource _serverSource;

        public NewServerViewModel()
        {

        }

        public NewServerViewModel(IServerSource newServerSource, IServerConnectionTest connectionTest, IStudioUpdateManager updateManager)
        {
            VerifyArgument.AreNotNull(new Dictionary<string, object> { { "newServerSource", newServerSource }, { "connectionTest", connectionTest }, { "updateManager", updateManager } });

            _connectionTest = connectionTest;
            _updateManager = updateManager;
            _serverSource = newServerSource;


            IsValid = false;
            Address = newServerSource.Address;
            AuthenticationType = newServerSource.AuthenticationType;
            UserName = newServerSource.UserName;
            Password = newServerSource.Password;
            TestPassed = false;


            TestCommand = new DelegateCommand(Test);
            OkCommand = new DelegateCommand(Save);
            CancelCommand = new DelegateCommand(() => { });
        }

        void Save()
        {
            _updateManager.Save(new ServerSource()
            {
                Address = Address,
                AuthenticationType = AuthenticationType,
                ID = _serverSource.ID == Guid.Empty ? Guid.NewGuid() : _serverSource.ID,
                Name = String.IsNullOrEmpty(_serverSource.Name) ? "" : _serverSource.Name,
                Password = Password,
                ResourcePath = "" //todo: needs to come from explorer
            });
        }


        void Test()
        {
            TestMessage = _updateManager.TestConnection(new ServerSource()
            {
                Address = Address,
                AuthenticationType = AuthenticationType,
                ID = _serverSource.ID == Guid.Empty ? Guid.NewGuid() : _serverSource.ID,
                Name = String.IsNullOrEmpty(_serverSource.Name) ? "" : _serverSource.Name,
                Password = Password,
                ResourcePath = "" //todo: needs to come from explorer
            });

            if (TestMessage == "Success")
                TestPassed = true;

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
                if (String.IsNullOrEmpty(Address))
                    return Resources.Languages.Core.ServerDialogNoAddressErrorMessage;

                if (!TestPassed)
                {
                    return Resources.Languages.Core.ServerDialogNoTestMessage;
                }

                IsValid = true;
                return String.Empty;
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

        /// <summary>
        /// Test if connection is successful
        /// </summary>
        public ICommand TestCommand
        { get; set; }


    }




    public class ServerSource : IServerSource
    {
        #region Implementation of IServerSource

        /// <summary>
        /// The server address that we are trying to connect to
        /// </summary>
        public string Address { get; set; }
        /// <summary>
        ///  Windows or user or publlic
        /// </summary>
        public AuthenticationType AuthenticationType { get; set; }
        /// <summary>
        /// User Name
        /// </summary>
        public string UserName { get; set; }
        /// <summary>
        /// Password
        /// </summary>
        public string Password { get; set; }
        /// <summary>
        /// Test if connection is successful
        /// </summary>
        public ICommand TestCommand { get; set; }
        /// <summary>
        /// The message that will be set if the test is either successful or not
        /// </summary>
        public string TestMessage { get; set; }
        public Guid ID { get; set; }
        public string Name { get; set; }
        public string ResourcePath { get; set; }

        #endregion
    }
}