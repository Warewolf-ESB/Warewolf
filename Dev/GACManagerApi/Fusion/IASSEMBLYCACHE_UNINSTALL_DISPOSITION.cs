

namespace GACManagerApi.Fusion
{
    public enum IASSEMBLYCACHE_UNINSTALL_DISPOSITION
    {
        Unknown = 0,

        /// <summary>
        /// The assembly files have been removed from the GAC.
        /// </summary>
        IASSEMBLYCACHE_UNINSTALL_DISPOSITION_UNINSTALLED = 1,

        /// <summary>
        /// An application is using the assembly. This value is returned on Microsoft Windows 95 and Microsoft Windows 98.
        /// </summary>
        IASSEMBLYCACHE_UNINSTALL_DISPOSITION_STILL_IN_USE = 2,

        /// <summary>
        /// The assembly does not exist in the GAC.
        /// </summary>
        IASSEMBLYCACHE_UNINSTALL_DISPOSITION_ALREADY_UNINSTALLED = 3,

        /// <summary>
        /// Not used.
        /// </summary>
        IASSEMBLYCACHE_UNINSTALL_DISPOSITION_DELETE_PENDING = 4,

        /// <summary>
        /// The assembly has not been removed from the GAC because another application reference exists.
        /// </summary>
        IASSEMBLYCACHE_UNINSTALL_DISPOSITION_HAS_INSTALL_REFERENCES = 5,

        /// <summary>
        /// The reference that is specified in pRefData is not found in the GAC.
        /// </summary>
        IASSEMBLYCACHE_UNINSTALL_DISPOSITION_REFERENCE_NOT_FOUND = 6
    }
}
