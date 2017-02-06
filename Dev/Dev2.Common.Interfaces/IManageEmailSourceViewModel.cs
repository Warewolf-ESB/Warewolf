using System;
using System.Windows.Input;

namespace Dev2.Common.Interfaces
{
    public interface IManageEmailSourceViewModel
    {
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
        /// Port
        /// </summary>
        int Port { get; set; }
        /// <summary>
        /// Timeout
        /// </summary>
        int Timeout { get; set; }

        /// <summary>
        /// Localized text for the Test label
        /// </summary>
        string TestLabel { get; }
        /// <summary>
        /// The message that will be set if the test is either successful or not
        /// </summary>
        string TestMessage { get; }

        /// <summary>
        /// has test failed
        /// </summary>
        bool TestFailed { get; set; }
        /// <summary>
        /// IsTesting
        /// </summary>
        bool Testing { get; }
        /// <summary>
        /// Has test passed
        /// </summary>
        bool TestPassed { get; set; }

        /// <summary>
        /// EmailFrom
        /// </summary>
        string EmailFrom { get; set; }
        /// <summary>
        /// EmailTo
        /// </summary>
        string EmailTo { get; set; }

        /// <summary>
        /// The localized text for the Host name label
        /// </summary>
        string HostNameLabel { get; }
        /// <summary>
        /// Localized text for the UserName label
        /// </summary>
        string UserNameLabel { get; }
        /// <summary>
        /// Localized text for the Password label
        /// </summary>
        string PasswordLabel { get; }
        /// <summary>
        /// Localized text for the Enable SSL label
        /// </summary>
        string EnableSslLabel { get; }
        /// <summary>
        /// Localized text for the Port label
        /// </summary>
        string PortLabel { get; }
        /// <summary>
        /// Localized text for the Timeout label
        /// </summary>
        string TimeoutLabel { get; }
        /// <summary>
        /// Localized text for the Email From label
        /// </summary>
        string EmailFromLabel { get; }
        /// <summary>
        /// Localized text for the Email To label
        /// </summary>
        string EmailToLabel { get; }

        /// <summary>
        /// Command for save/ok
        /// </summary>
        ICommand OkCommand { get; set; }
        /// <summary>
        /// Command for send
        /// </summary>
        ICommand SendCommand { get; set; }

        bool EnableSsl { get; set; }
        bool EnableSslYes { get; set; }
        bool EnableSslNo { get; set; }
    }

    public interface IManageEmailSourceModel
    {
        IEmailServiceSource FetchSource(Guid resourceIDh);
        string TestConnection(IEmailServiceSource resource);
        void Save(IEmailServiceSource toDbSource);
        string ServerName { get; }
    }
}
