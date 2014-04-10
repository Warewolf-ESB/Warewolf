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
        public static int DefaultWaitInSeconds = 300;

        /// <summary>
        /// The default wait information parameters
        /// </summary>
        public static int DefaultWaitInMs = 30000;

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

        /// <summary>
        /// Gets or sets a value indicating whether [view read memory].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [view read memory]; otherwise, <c>false</c>.
        /// </value>
        public static bool ViewReadMe { get; set; }

        public static bool RemoveLogFile = true;

        // firewall rule names
        public static readonly string OutboundHTTPWarewolfRule = "Warewolf HTTP Outbound Ports";
        public static readonly string OutboundHTTPSWarewolfRule = "Warewolf HTTPS Outbound Ports";
        public static readonly string InboundHTTPWarewolfRule = "Warewolf HTTP Inbound Ports";
        public static readonly string InboundHTTPSWarewolfRule = "Warewolf HTTPS Inbound Ports";

        // VC++ 2k8 SP1
        public static readonly string Vcplusplus2k8sp1x86Key = @"{FF66E9F6-83E7-3A3E-AF14-8DE9A809A6A4}";

    }
}
