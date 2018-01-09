using System.Windows.Input;



namespace Dev2.Common.Interfaces
{
    public interface IManageRabbitMQSourceViewModel
    {
        /// <summary>
        /// The name of the resource
        /// </summary>
        
        string ResourceName { get; set; }

        /// <summary>
        /// The Host Name
        /// </summary>
        string HostName { get; set; }

        /// <summary>
        /// Port
        /// </summary>
        int Port { get; set; }

        /// <summary>
        /// User Name
        /// </summary>
        string UserName { get; set; }

        /// <summary>
        /// Password
        /// </summary>
        string Password { get; set; }

        /// <summary>
        /// Virtual Host
        /// </summary>
        string VirtualHost { get; set; }

        /// <summary>
        /// IsTesting
        /// </summary>
        bool Testing { get; }

        /// <summary>
        /// The message that will be set if the test unsuccessfull
        /// </summary>
        string TestErrorMessage { get; }

        /// <summary>
        /// Has test passed
        /// </summary>
        bool TestPassed { get; set; }

        /// <summary>
        /// has test failed
        /// </summary>
        bool TestFailed { get; set; }

        /// <summary>
        /// Header text that is used on the view
        /// </summary>
        string HeaderText { get; set; }

        /// <summary>
        /// Command for save/ok
        /// </summary>
        ICommand PublishCommand { get; set; }

        /// <summary>
        /// Command for save/ok
        /// </summary>
        ICommand OkCommand { get; set; }

        /// <summary>
        /// The localized text for the Host name label
        /// </summary>
        string HostNameLabel { get; }

        /// <summary>
        /// Localized text for the Port label
        /// </summary>
        string PortLabel { get; }

        /// <summary>
        /// Localized text for the UserName label
        /// </summary>
        string UserNameLabel { get; }

        /// <summary>
        /// Localized text for the Password label
        /// </summary>
        string PasswordLabel { get; }

        ///// <summary>
        ///// Localized text for the Virtual Host label
        ///// </summary>
        string VirtualHostLabel { get; }
    }
}