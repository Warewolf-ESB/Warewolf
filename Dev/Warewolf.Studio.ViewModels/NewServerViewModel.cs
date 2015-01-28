using System;
using System.Windows.Input;
using Dev2.Common.Interfaces.Runtime.ServiceModel;
using Dev2.Common.Interfaces.ServerDialogue;
using Dev2.Common.Interfaces.Studio.ViewModels.Dialogues;
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

        public NewServerViewModel(IServerSource newServerSource)
        {

            if (newServerSource == null)
            {
                throw new ArgumentNullException("newServerSource");
            }

            //IsValid = newServerSource.IsValid;
            Address = newServerSource.Address;
            AuthenticationType = newServerSource.AuthenticationType;
            UserName = newServerSource.UserName;
            Password = newServerSource.Password;
            TestCommand = newServerSource.TestCommand;
            TestMessage = newServerSource.TestMessage;


        }


        /// <summary>
        /// called by outer when validating
        /// </summary>
        /// <returns></returns>
        public string Validate()
        {
            return null;
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
        /// Test if connection is successful
        /// </summary>
        public ICommand TestCommand { get; set; }
        /// <summary>
        /// The message that will be set if the test is either successful or not
        /// </summary>
        public string TestMessage { get; set; }
        public bool IsOkEnabled
        {
            get
            {
                return _isOkEnabled;
            }
            set
            {
                _isOkEnabled = value;
            }
        }
        public bool IsTestEnabled
        {
            get
            {
                return _isTestEnabled;
            }
            set
            {
                _isTestEnabled = value;
            }
        }
        public bool IsUserNameVisible
        {
            get
            {
                return _isUserNameVisible;
            }
            set
            {
                _isUserNameVisible = value;
            }
        }
        public bool IsPasswordVisible
        {
            get
            {
                return _isPasswordVisible;
            }
            set
            {
                _isPasswordVisible = value;
            }
        }
        public string AddressLabel
        {
            get
            {
                return _addressLabel;
            }
        }
        public string UserNameLabel
        {
            get
            {
                return _userNameLabel;
            }
        }
        public string AuthenticationLabel
        {
            get
            {
                return _authenticationLabel;
            }
        }
        public string PasswordLabel
        {
            get
            {
                return _passwordLabel;
            }
        }
        public string TestLabel
        {
            get
            {
                return _testLabel;
            }
        }

        #endregion
    }
}