using System;
using System.Windows.Input;
using Dev2.Common.Interfaces.ToolBase.ExchangeEmail;

namespace Dev2.Common.Interfaces
{
    public interface IManageExchangeSourceViewModel
    {
        /// <summary>
        /// The Host Name
        /// </summary>
        string AutoDiscoverUrl { get; set; }
        /// <summary>
        /// User Name
        /// </summary>
        string UserName { get; set; }
        /// <summary>
        /// Password
        /// </summary>
        string Password { get; set; }
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
        /// EmailTo
        /// </summary>
        string EmailTo { get; set; }

        /// <summary>
        /// The localized text for the Host name label
        /// </summary>
        string AutoDiscoverLabel { get; }
        /// <summary>
        /// Localized text for the UserName label
        /// </summary>
        string UserNameLabel { get; }
        /// <summary>
        /// Localized text for the Password label
        /// </summary>
        string PasswordLabel { get; }
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
    }

    public interface IManageExchangeSourceModel
    {
        string TestConnection(IExchangeSource resource);
        void Save(IExchangeSource toDbSource);
        string ServerName { get; }

        IExchangeSource FetchSource(Guid exchangeSourceResourceID);
    }
}

