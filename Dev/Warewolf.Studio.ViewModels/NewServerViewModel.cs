using System;
using System.Windows.Input;
using Dev2.Common.Interfaces.Communication;
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
        bool _isOkEnabled;
        bool _isTestEnabled;
        bool _isUserNameVisible;
        bool _isPasswordVisible;
        string _addressLabel;
        string _userNameLabel;
        string _authenticationLabel;
        string _passwordLabel;
        string _testLabel;

        #region Implementation of IInnerDialogueTemplate

        IServerConnectionTest _connectionTest;



        public NewServerViewModel()
        {

        }



        public NewServerViewModel(IServerSource newServerSource, IServerConnectionTest connectionTest)
        {
            _connectionTest = connectionTest;

            if (newServerSource == null)
            {
                throw new ArgumentNullException("newServerSource");
            }

            IsValid = false;
            Address = newServerSource.Address;
            AuthenticationType = newServerSource.AuthenticationType;
            UserName = newServerSource.UserName;
            Password = newServerSource.Password;
            TestMessage = String.Empty;
            TestPassed = false;

            TestCommand = new DelegateCommand(() =>
            {
                TestMessage = _connectionTest.Test(new ServerSource()
                {
                    Address = Address,
                    AuthenticationType = AuthenticationType,
                    Password = Password,
                    UserName = UserName

                });
                if(String.IsNullOrEmpty(TestMessage))
                    TestPassed = true;
            });

           // OkCommand = new DelegateCommand(() => );
        }


        /// <summary>
        /// called by outer when validating
        /// </summary>
        /// <returns></returns>
        public string Validate()
        {
            if(String.IsNullOrEmpty(Address))
                return Resources.Languages.Core.ServerDialogNoAddressErrorMessage;
            
            if (!TestPassed)
            {
                


            }


            return String.Empty;
        }

        /// <summary>
        /// Is valid 
        /// </summary>
        public bool IsValid { get; set; }
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
        /// The message that will be set if the test is either successful or not
        /// </summary>
        public string TestMessage { get; set; }

        #endregion


        bool TestPassed  { get; set; }

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


        //public ICommand OkCommand
        //{ get; set; }

        //public ICommand CancelCommand
        //{ get; set; }

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


        #endregion
    }
}