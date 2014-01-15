namespace Gui
{
    /// <summary>
    /// Temporal Variables for the Installer ;)
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
        /// The default wait in seconds
        /// </summary>
        public static int DefaultWaitInSeconds = 60;

        /// <summary>
        /// Gets or sets the install root.
        /// </summary>
        /// <value>
        /// The install root.
        /// </value>
        public static string InstallRoot { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [start studio configuration exit].
        /// </summary>
        /// <value>
        /// <c>true</c> if [start studio configuration exit]; otherwise, <c>false</c>.
        /// </value>
        public static bool StartStudioOnExit { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [is install mode].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [is install mode]; otherwise, <c>false</c>.
        /// </value>
        public static bool IsInstallMode { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [remove all items].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [remove all items]; otherwise, <c>false</c>.
        /// </value>
        public static bool RemoveAllItems { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [install shortcuts].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [install shortcuts]; otherwise, <c>false</c>.
        /// </value>
        public static bool InstallShortcuts { get; set; }
    }
}
