using System.Windows.Input;
using Dev2.Common.Interfaces.Runtime.ServiceModel;

namespace Dev2.Common.Interfaces.Studio.ViewModels.Dialogues
{

    public interface INewServerDialogue : IInnerDialogueTemplate
    {
        /// <summary>
        /// The server address that we are trying to connect to
        /// </summary>
        string Address { get; set; }
        /// <summary>
        ///  Windows or user or publlic
        /// </summary>
        AuthenticationType AuthenticationType { get; set; }
        /// <summary>
        /// User Name
        /// </summary>
        string UserName { get; set; }
        /// <summary>
        /// Password
        /// </summary>
        string Password { get; set; }
        /// <summary>
        /// Test if connection is successful
        /// </summary>
        ICommand TestCommand { get; set; }
        /// <summary>
        /// The message that will be set if the test is either successful or not
        /// </summary>
        string TestMessage { get; }

        bool IsOkEnabled { get; }

        bool IsTestEnabled { get;  }

        bool IsUserNameVisible { get; }

        bool IsPasswordVisible { get; }

        string AddressLabel { get; }

        string UserNameLabel { get; }

        string AuthenticationLabel { get; }

        string PasswordLabel { get; }

        string TestLabel { get; }
  
    }
}