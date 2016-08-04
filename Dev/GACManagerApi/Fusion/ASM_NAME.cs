// ReSharper disable InconsistentNaming
namespace GACManagerApi.Fusion
{
    /// <summary>
    /// The values of the ASM_NAME enumeration are the property IDs for the name-value pairs included in a side-by-side assembly name.
    /// </summary>
    public enum ASM_NAME
    {
        /// <summary>
        /// Property ID for the assembly's public key. The value is a byte array.
        /// </summary>
        ASM_NAME_PUBLIC_KEY,

        /// <summary>
        /// Property ID for the assembly's major version. The value is a WORD value.
        /// </summary>
        ASM_NAME_MAJOR_VERSION,

        /// <summary>
        /// Property ID for the assembly's minor version. The value is a WORD value.
        /// </summary>
        ASM_NAME_MINOR_VERSION,

        /// <summary>
        /// Property ID for the assembly's build version. The value is a WORD value.
        /// </summary>
        ASM_NAME_BUILD_NUMBER,

        /// <summary>
        /// Property ID for the assembly's revision version. The value is a WORD value.
        /// </summary>
        ASM_NAME_REVISION_NUMBER
    }
}