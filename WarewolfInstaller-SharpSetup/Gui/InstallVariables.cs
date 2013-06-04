namespace Gui
{
    /// <summary>
    /// Temproal Variables for the Installer ;)
    /// </summary>
    public class InstallVariables
    {

        /// <summary>
        /// The server service name
        /// </summary>
        public static string ServerService = "Warewolf Server";

        /// <summary>
        /// The cancel message
        /// </summary>
        public static string RollbackMessage = "Rolling back install progress...";

        /// <summary>
        /// Gets or sets the install root.
        /// </summary>
        /// <value>
        /// The install root.
        /// </value>
        public static string InstallRoot { get; set; }
    }
}
