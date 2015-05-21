using System.Windows.Controls.Primitives;
using System.Windows.Input;
using Dev2.Common.Interfaces.Runtime.ServiceModel;
using Dev2.Common.Interfaces.ServerProxyLayer;

namespace Dev2.Common.Interfaces.Studio.ViewModels.Dialogues
{
    public interface IManageWebserviceSourceViewModel
    {
        /// <summary>
        ///  User or Anonymous
        /// </summary>
        AuthenticationType AuthenticationType { get; set; }

        /// <summary>
        /// The Host Name
        /// </summary>
        string HostName { get; set; }
        
        
        /// <summary>
        /// User Name
        /// </summary>
        string UserName { get; set; }
        /// <summary>
        /// Password
        /// </summary>
        string Password { get; set; }
        /// <summary>
        /// Default Query
        /// </summary>
        string DefaultQuery { get; set; }

        string TestDefaultLabel { get; }
        /// <summary>
        /// Test if connection is successful
        /// </summary>
        ICommand TestCommand { get; set; }

        ICommand CancelTestCommand { get; set; }

        string CancelTestLabel { get; }
        /// <summary>
        /// Cancel a test that has started
        /// </summary>
        ICommand ViewInBrowserCommand { get; set; }
        /// <summary>
        /// The message that will be set if the test is either successful or not
        /// </summary>
        string TestMessage { get; }
        /// <summary>
        /// The message that will be set if the test is either successful or not
        /// </summary>
        string TestDefault { get; }

        /// <summary>
        /// Localized text for the UserName label
        /// </summary>
        string UserNameLabel { get; }

        /// <summary>
        /// Localized text for the Authentication Type label
        /// </summary>
        string AuthenticationLabel { get; }

        /// <summary>
        /// Localized text for the Password label
        /// </summary>
        string PasswordLabel { get; }

        /// <summary>
        /// Localized text for the Test label
        /// </summary>
        string TestLabel { get; }

        /// <summary>
        /// The localized text for the Host name label
        /// </summary>
        string HostNameLabel { get;  }

        /// <summary>
        /// The localized text for the Default Query label
        /// </summary>
        string DefaultQueryLabel { get;  }

        /// <summary>
        /// Command for save/ok
        /// </summary>
        ICommand OkCommand { get; set; }

        /// <summary>
        /// Header text that is used on the view
        /// </summary>
        string HeaderText { get; set; }

        /// <summary>
        /// Tooltip for the Windows Authentication option
        /// </summary>
        string AnonymousAuthenticationToolTip { get; }

        /// <summary>
        /// Tooltip for the User Authentication option
        /// </summary>
        string UserAuthenticationToolTip { get; }

        /// <summary>
        /// View In Browser label
        /// </summary>
        string ViewInBrowserLabel { get; }
        /// <summary>
        /// Has test passed
        /// </summary>
        bool TestPassed { get; set; }
        /// <summary>
        /// Has test passed
        /// </summary>
        bool IsHyperLinkEnabled { get; set; }

        /// <summary>
        /// has test failed
        /// </summary>
        bool TestFailed { get; set; }
        /// <summary>
        /// IsTesting
        /// </summary>
        bool Testing { get; }
        
        /// <summary>
        /// The name of the resource
        /// </summary>
        // ReSharper disable UnusedMemberInSuper.Global
        string ResourceName { get; set; }
        // ReSharper restore UnusedMemberInSuper.Global

        /// <summary>
        /// The authentications Type
        /// </summary>
        bool UserAuthenticationSelected { get; }
    }

    public interface IManageWebServiceSourceModel
    {

        void TestConnection(IWebServiceSource resource);

        void Save(IWebServiceSource toDbSource);


        string ServerName { get; set; }
    }

    public interface INameValue
    {
        string Name { get; set; }
        string Value { get; set; }

    }
}