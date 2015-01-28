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

        #endregion
    }
}