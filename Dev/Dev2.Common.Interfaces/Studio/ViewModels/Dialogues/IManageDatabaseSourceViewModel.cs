using System.Collections.Generic;
using System.Windows.Input;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Common.Interfaces.Runtime.ServiceModel;
using Dev2.Common.Interfaces.ServerProxyLayer;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Dev2.Common.Interfaces.Studio.ViewModels.Dialogues
{
    public interface IManageDatabaseSourceViewModel
    {
        /// <summary>
        /// The Database Server Type
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        enSourceType ServerType { get; set; }
        /// <summary>
        ///  Windows or user or publlic
        /// </summary>
        AuthenticationType AuthenticationType { get; set; }

        /// <summary>
        /// The Database Server Name
        /// </summary>
        string ServerName { get; set; }
        
        /// <summary>
        /// The Database that the source is reading from
        /// </summary>
        string DatabaseName { get; set; }
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
        /// Cancel a test that has started
        /// </summary>
        ICommand CancelTestCommand { get; set; }
        /// <summary>
        /// The message that will be set if the test is either successful or not
        /// </summary>
        string TestMessage { get; }

        /// <summary>
        /// Localized text for the Server Type label
        /// </summary>
        string ServerTypeLabel { get; }

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
        /// The localized text for the Database Server label
        /// </summary>
        string ServerLabel { get;  }

        /// <summary>
        /// The localized text for the Database label
        /// </summary>
        string DatabaseLabel { get;  }

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
        string WindowsAuthenticationToolTip { get; }

        /// <summary>
        /// Tooltip for the User Authentication option
        /// </summary>
        string UserAuthenticationToolTip { get; }

        /// <summary>
        /// Tooltip for the Database Server Type
        /// </summary>
        string ServerTypeTool { get; }

        /// <summary>
        /// List of database names for the user to choose from based on the server entered
        /// </summary>
        IList<string> DatabaseNames { get; set; }
        /// <summary>
        /// Cancel test display text
        /// </summary>
        string CancelTestLabel { get; }
        /// <summary>
        /// Has test passed
        /// </summary>
        bool TestPassed { get; }

        /// <summary>
        /// has test failed
        /// </summary>
        bool TestFailed { get; }
        /// <summary>
        /// IsTesting
        /// </summary>
        bool Testing { get; }
        /// <summary>
        /// Database Types avaialable 
        /// </summary>
        IList<enSourceType> Types { get; set; }
        /// <summary>
        /// The name of the resource
        /// </summary>
        string ResourceName { get; set; }

        /// <summary>
        /// The authentications Type
        /// </summary>
        bool UserAuthenticationSelected { get; }


        IList<string> ComputerNames { get; set; } 
    }

    public interface IManageDatabaseSourceModel
    {

        IList<string> GetComputerNames();
        IList<string> TestDbConnection(IDbSource resource);
        void Save(IDbSource toDbSource);
        string ServerName { get; }
    }

}