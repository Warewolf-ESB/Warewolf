

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
        /// Property ID for the assembly's public key token. The value is a byte array.
        /// </summary>
        ASM_NAME_PUBLIC_KEY_TOKEN,

        /// <summary>
        /// Property ID for a reserved name-value pair. The value is a byte array.
        /// </summary>
        ASM_NAME_HASH_VALUE,

        /// <summary>
        /// Property ID for the assembly's simple name. The value is a string value.
        /// </summary>
        ASM_NAME_NAME,

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
        ASM_NAME_REVISION_NUMBER,

        /// <summary>
        /// Property ID for the assembly's culture. The value is a string value.
        /// </summary>
        ASM_NAME_CULTURE,

        /// <summary>
        /// Property ID for a reserved name-value pair.
        /// </summary>
        ASM_NAME_PROCESSOR_ID_ARRAY,

        /// <summary>
        /// Property ID for a reserved name-value pair.
        /// </summary>
        ASM_NAME_OSINFO_ARRAY,

        /// <summary>
        /// Property ID for a reserved name-value pair. The value is a DWORD value.
        /// </summary>
        ASM_NAME_HASH_ALGID,

        /// <summary>
        /// Property ID for a reserved name-value pair.
        /// </summary>
        ASM_NAME_ALIAS,

        /// <summary>
        /// Property ID for a reserved name-value pair.
        /// </summary>
        ASM_NAME_CODEBASE_URL,

        /// <summary>
        /// Property ID for a reserved name-value pair. The value is a FILETIME structure.
        /// </summary>
        ASM_NAME_CODEBASE_LASTMOD,

        /// <summary>
        /// Property ID for the assembly as a simply named assembly that does not have a public key.
        /// </summary>
        ASM_NAME_NULL_PUBLIC_KEY,

        /// <summary>
        /// Property ID for the assembly as a simply named assembly that does not have a public key token.
        /// </summary>
        ASM_NAME_NULL_PUBLIC_KEY_TOKEN,

        /// <summary>
        /// Property ID for a reserved name-value pair. The value is a string value.
        /// </summary>
        ASM_NAME_CUSTOM,

        /// <summary>
        /// Property ID for a reserved name-value pair.
        /// </summary>
        ASM_NAME_NULL_CUSTOM,

        /// <summary>
        /// Property ID for a reserved name-value pair.
        /// </summary>
        ASM_NAME_MVID,

        /// <summary>
        /// Reserved.
        /// </summary>
        ASM_NAME_MAX_PARAMS
    }
}