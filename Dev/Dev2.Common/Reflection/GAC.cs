/*
*  Warewolf - The Easy Service Bus
*  Copyright 2016 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


// Source: Microsoft KB Article KB317540

/*
SUMMARY
The native code application programming interfaces (APIs) that allow you to interact with the Global Assembly Cache (GAC) are not documented 
in the .NET Framework Software Development Kit (SDK) documentation. 

MORE INFORMATION
CAUTION: Do not use these APIs in your application to perform assembly binds or to test for the presence of assemblies or other run time, 
development, or design-time operations. Only administrative tools and setup programs must use these APIs. If you use the GAC, this directly 
exposes your application to assembly binding fragility or may cause your application to work improperly on future versions of the .NET 
Framework.

The GAC stores assemblies that are shared across all applications on a computer. The actual storage location and structure of the GAC is 
not documented and is subject to change in future versions of the .NET Framework and the Microsoft Windows operating system.

The only supported method to access assemblies in the GAC is through the APIs that are documented in this article.

Most applications do not have to use these APIs because the assembly binding is performed automatically by the common language runtime. 
Only custom setup programs or management tools must use these APIs. Microsoft Windows Installer has native support for installing assemblies
 to the GAC.

For more information about assemblies and the GAC, see the .NET Framework SDK.

Use the GAC API in the following scenarios: 
When you install an assembly to the GAC.
When you remove an assembly from the GAC.
When you export an assembly from the GAC.
When you enumerate assemblies that are available in the GAC.
NOTE: CoInitialize(Ex) must be called before you use any of the functions and interfaces that are described in this specification. 
*/

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

// ReSharper disable InconsistentNaming

namespace Dev2.Common.Reflection
{

    #region Flags

    /// <summary>
    ///     <see cref="IAssemblyName.GetDisplayName" />
    /// </summary>
    [Flags]
    public enum ASM_DISPLAY_FLAGS
    {
        VERSION = 0x1,
        CULTURE = 0x2,
        PUBLIC_KEY_TOKEN = 0x4,
        PUBLIC_KEY = 0x8,
        CUSTOM = 0x10,
        PROCESSORARCHITECTURE = 0x20,
        LANGUAGEID = 0x40
    }

    [Flags]
    public enum ASM_CMP_FLAGS
    {
        NAME = 0x1,
        MAJOR_VERSION = 0x2,
        MINOR_VERSION = 0x4,
        BUILD_NUMBER = 0x8,
        REVISION_NUMBER = 0x10,
        PUBLIC_KEY_TOKEN = 0x20,
        CULTURE = 0x40,
        CUSTOM = 0x80,

        ALL = NAME | MAJOR_VERSION | MINOR_VERSION |
              REVISION_NUMBER | BUILD_NUMBER |
              PUBLIC_KEY_TOKEN | CULTURE | CUSTOM,
        DEFAULT = 0x100
    }

    /// <summary>
    ///     The ASM_NAME enumeration property ID describes the valid names of the name-value pairs in an assembly name.
    ///     See the .NET Framework SDK for a description of these properties.
    /// </summary>
    public enum ASM_NAME
    {
        ASM_NAME_PUBLIC_KEY = 0,
        ASM_NAME_PUBLIC_KEY_TOKEN,
        ASM_NAME_HASH_VALUE,
        ASM_NAME_NAME,
        ASM_NAME_MAJOR_VERSION,
        ASM_NAME_MINOR_VERSION,
        ASM_NAME_BUILD_NUMBER,
        ASM_NAME_REVISION_NUMBER,
        ASM_NAME_CULTURE,
        ASM_NAME_PROCESSOR_ID_ARRAY,
        ASM_NAME_OSINFO_ARRAY,
        ASM_NAME_HASH_ALGID,
        ASM_NAME_ALIAS,
        ASM_NAME_CODEBASE_URL,
        ASM_NAME_CODEBASE_LASTMOD,
        ASM_NAME_NULL_PUBLIC_KEY,
        ASM_NAME_NULL_PUBLIC_KEY_TOKEN,
        ASM_NAME_CUSTOM,
        ASM_NAME_NULL_CUSTOM,
        ASM_NAME_MVID,
        ASM_NAME_MAX_PARAMS
    }

    /// <summary>
    ///     <see cref="IAssemblyCache.UninstallAssembly" />
    /// </summary>
    public enum IASSEMBLYCACHE_UNINSTALL_DISPOSITION
    {
        IASSEMBLYCACHE_UNINSTALL_DISPOSITION_UNINSTALLED = 1,
        IASSEMBLYCACHE_UNINSTALL_DISPOSITION_STILL_IN_USE = 2,
        IASSEMBLYCACHE_UNINSTALL_DISPOSITION_ALREADY_UNINSTALLED = 3,
        IASSEMBLYCACHE_UNINSTALL_DISPOSITION_DELETE_PENDING = 4,
        IASSEMBLYCACHE_UNINSTALL_DISPOSITION_HAS_INSTALL_REFERENCES = 5,
        IASSEMBLYCACHE_UNINSTALL_DISPOSITION_REFERENCE_NOT_FOUND = 6
    }

    /// <summary>
    ///     <see cref="IAssemblyCache.QueryAssemblyInfo" />
    /// </summary>
    public enum QUERYASMINFO_FLAG
    {
        QUERYASMINFO_FLAG_VALIDATE = 1,
        QUERYASMINFO_FLAG_GETSIZE = 2
    }

    /// <summary>
    ///     <see cref="IAssemblyCache.InstallAssembly" />
    /// </summary>
    public enum IASSEMBLYCACHE_INSTALL_FLAG
    {
        IASSEMBLYCACHE_INSTALL_FLAG_REFRESH = 1,
        IASSEMBLYCACHE_INSTALL_FLAG_FORCE_REFRESH = 2
    }

    public enum CREATE_ASM_NAME_OBJ_FLAGS
    {
        CANOF_PARSE_DISPLAY_NAME = 0x1,
        CANOF_SET_DEFAULT_VALUES = 0x2
    }

    /// <summary>
    ///     The ASM_CACHE_FLAGS enumeration contains the following values:
    ///     ASM_CACHE_ZAP - Enumerates the cache of precompiled assemblies by using Ngen.exe.
    ///     ASM_CACHE_GAC - Enumerates the GAC.
    ///     ASM_CACHE_DOWNLOAD - Enumerates the assemblies that have been downloaded on-demand or that have been shadow-copied.
    /// </summary>
    [Flags]
    public enum ASM_CACHE_FLAGS
    {
        ASM_CACHE_ZAP = 0x1,
        ASM_CACHE_GAC = 0x2,
        ASM_CACHE_DOWNLOAD = 0x4
    }

    #endregion

    #region Structs

    /// <summary>
    ///     The FUSION_INSTALL_REFERENCE structure represents a reference that is made when an application has installed an
    ///     assembly in the GAC.
    ///     The fields of the structure are defined as follows:
    ///     cbSize - The size of the structure in bytes.
    ///     dwFlags - Reserved, must be zero.
    ///     guidScheme - The entity that adds the reference.
    ///     szIdentifier - A unique string that identifies the application that installed the assembly.
    ///     szNonCannonicalData - A string that is only understood by the entity that adds the reference.
    ///     The GAC only stores this string.
    ///     Possible values for the guidScheme field can be one of the following:
    ///     FUSION_REFCOUNT_MSI_GUID - The assembly is referenced by an application that has been installed by using
    ///     Windows Installer. The szIdentifier field is set to MSI, and szNonCannonicalData is set to Windows Installer.
    ///     This scheme must only be used by Windows Installer itself.
    ///     FUSION_REFCOUNT_UNINSTALL_SUBKEY_GUID - The assembly is referenced by an application that appears in Add/Remove
    ///     Programs. The szIdentifier field is the token that is used to register the application with Add/Remove programs.
    ///     FUSION_REFCOUNT_FILEPATH_GUID - The assembly is referenced by an application that is represented by a file in
    ///     the file system. The szIdentifier field is the path to this file.
    ///     FUSION_REFCOUNT_OPAQUE_STRING_GUID - The assembly is referenced by an application that is only represented
    ///     by an opaque string. The szIdentifier is this opaque string. The GAC does not perform existence checking
    ///     for opaque references when you remove this.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct FUSION_INSTALL_REFERENCE
    {
        public uint cbSize;
        public uint dwFlags;
        public Guid guidScheme;
        public string szIdentifier;
        public string szNonCannonicalData;
    }

    /// <summary>
    ///     The ASSEMBLY_INFO structure represents information about an assembly in the assembly cache.
    ///     The fields of the structure are defined as follows:
    ///     cbAssemblyInfo - Size of the structure in bytes. Permits additions to the structure in future version of the .NET
    ///     Framework.
    ///     dwAssemblyFlags - Indicates one or more of the ASSEMBLYINFO_FLAG_* bits.
    ///     uliAssemblySizeInKB - The size of the files that make up the assembly in kilobytes (KB).
    ///     pszCurrentAssemblyPathBuf - A pointer to a string buffer that holds the current path of the directory that contains
    ///     the
    ///     files that make up the assembly. The path must end with a zero.
    ///     cchBuf - Size of the buffer that the pszCurrentAssemblyPathBug field points to.
    ///     dwAssemblyFlags can have one of the following values:
    ///     ASSEMBLYINFO_FLAG__INSTALLED - Indicates that the assembly is actually installed. Always set in current version of
    ///     the
    ///     .NET Framework.
    ///     ASSEMBLYINFO_FLAG__PAYLOADRESIDENT - Never set in the current version of the .NET Framework.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct ASSEMBLY_INFO
    {
        public uint cbAssemblyInfo;
        public uint dwAssemblyFlags;
        public ulong uliAssemblySizeInKB;
        public string pszCurrentAssemblyPathBuf;
        public uint cchBuf;
    }

    #endregion

    public class GAC
    {
        #region DLL Entries

        /// <summary>
        ///     To obtain an instance of the CreateAssemblyEnum API, call the CreateAssemblyNameObject API.
        /// </summary>
        /// <param name="pEnum">Pointer to a memory location that contains the IAssemblyEnum pointer.</param>
        /// <param name="pUnkReserved">Must be null.</param>
        /// <param name="pName">
        ///     An assembly name that is used to filter the enumeration. Can be null to enumerate all assemblies in
        ///     the GAC.
        /// </param>
        /// <param name="dwFlags">Exactly one bit from the ASM_CACHE_FLAGS enumeration.</param>
        /// <param name="pvReserved">Must be NULL.</param>
        [DllImport("fusion.dll", SetLastError = true, PreserveSig = false)]
        private static extern void CreateAssemblyEnum(out IAssemblyEnum pEnum, IntPtr pUnkReserved, IAssemblyName pName,
            ASM_CACHE_FLAGS dwFlags, IntPtr pvReserved);

        /// <summary>
        ///     The GetCachePath API returns the storage location of the GAC.
        /// </summary>
        /// <param name="dwCacheFlags">Exactly one of the bits defined in the ASM_CACHE_FLAGS enumeration.</param>
        /// <param name="pwzCachePath">Pointer to a buffer that is to receive the path of the GAC as a Unicode string.</param>
        /// <param name="pcchPath">Length of the pwszCachePath buffer, in Unicode characters.</param>
        [DllImport("fusion.dll", SetLastError = true, CharSet = CharSet.Unicode, PreserveSig = false)]
        // ReSharper disable once UnusedMember.Local
        private static extern void GetCachePath(ASM_CACHE_FLAGS dwCacheFlags,
            [MarshalAs(UnmanagedType.LPWStr)] StringBuilder pwzCachePath,
            ref uint pcchPath);

        #endregion

        #region GUID Definition

        /// <summary>
        ///     GUID value for element guidScheme in the struct FUSION_INSTALL_REFERENCE
        ///     The assembly is referenced by an application that has been installed by using Windows Installer.
        ///     The szIdentifier field is set to MSI, and szNonCannonicalData is set to Windows Installer.
        ///     This scheme must only be used by Windows Installer itself.
        /// </summary>
        public static Guid FUSION_REFCOUNT_UNINSTALL_SUBKEY_GUID
        {
            get { return new Guid("8cedc215-ac4b-488b-93c0-a50a49cb2fb8"); }
        }

        /// <summary>
        ///     GUID value for element guidScheme in the struct FUSION_INSTALL_REFERENCE
        /// </summary>
        public static Guid FUSION_REFCOUNT_FILEPATH_GUID
        {
            get { return new Guid("b02f9d65-fb77-4f7a-afa5-b391309f11c9"); }
        }

        /// <summary>
        ///     GUID value for element guidScheme in the struct FUSION_INSTALL_REFERENCE
        /// </summary>
        public static Guid FUSION_REFCOUNT_OPAQUE_STRING_GUID
        {
            get { return new Guid("2ec93463-b0c3-45e1-8364-327e96aea856"); }
        }

        /// <summary>
        ///     GUID value for element guidScheme in the struct FUSION_INSTALL_REFERENCE
        /// </summary>
        /// <value>
        ///     The FUSIO n_ REFCOUN t_ MS i_ GUID.
        /// </value>
        public static Guid FUSION_REFCOUNT_MSI_GUID
        {
            get { return new Guid("25df0fc1-7f97-4070-add7-4b13bbfd7cb8"); }
        }

        #endregion

        #region Public Functions for DLL - Assembly Cache

        #endregion

        #region Public Functions for DLL - AssemblyName

        /// <summary>
        ///     Gets the display name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="which">The which.</param>
        /// <returns></returns>
        public static String GetDisplayName(IAssemblyName name, ASM_DISPLAY_FLAGS which)
        {
            uint bufferSize = 255;
            var buffer = new StringBuilder((int) bufferSize);
            name.GetDisplayName(buffer, ref bufferSize, which);
            return buffer.ToString();
        }

        #endregion

        #region Public Functions for DLL - AssemblyEnum

        /// <summary>
        ///     Creates the GAC enum.
        /// </summary>
        /// <returns></returns>
        public static IAssemblyEnum CreateGACEnum()
        {
            IAssemblyEnum ae;

            CreateAssemblyEnum(out ae, (IntPtr) 0, null, ASM_CACHE_FLAGS.ASM_CACHE_GAC, (IntPtr) 0);

            return ae;
        }

        /// <summary>
        ///     Get the next assembly name in the current enumerator or fail
        /// </summary>
        /// <param name="enumerator"></param>
        /// <param name="name"></param>
        /// <returns>0 if the enumeration is not at its end</returns>
        public static int GetNextAssembly(IAssemblyEnum enumerator, out IAssemblyName name)
        {
            return enumerator.GetNextAssembly((IntPtr) 0, out name, 0);
        }

        #endregion

        #region GAC Resolution Handling

        private static GACAssemblyName[] _gacNameCache = new GACAssemblyName[0];

        /// <summary>
        ///     Tries the resolve GAC assembly.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="culture">The culture.</param>
        /// <param name="version">The version.</param>
        /// <param name="publicKeyToken">The public key token.</param>
        /// <returns></returns>
        public static string TryResolveGACAssembly(string name, string culture, string version, string publicKeyToken)
        {
            if (String.IsNullOrEmpty(name)) return null;
            GACAssemblyName[] matchingName = GetGACAssemblies(name);
            if (matchingName.Length == 0) return null;

            if (!String.IsNullOrEmpty(publicKeyToken))
                for (int i = 0; i < matchingName.Length; i++)
                    if (
                        !String.Equals(matchingName[i].PublicKeyToken, publicKeyToken,
                            StringComparison.OrdinalIgnoreCase))
                        matchingName[i] = null;

            if (!String.IsNullOrEmpty(culture))
                for (int i = 0; i < matchingName.Length; i++)
                    if (matchingName[i] != null &&
                        !String.Equals(matchingName[i].Culture, culture, StringComparison.OrdinalIgnoreCase))
                        matchingName[i] = null;

            GACAssemblyName winner = null;

            if (!String.IsNullOrEmpty(version))
            {
                for (int i = 0; i < matchingName.Length; i++)
                    if (matchingName[i] != null)
                    {
                        if (!String.Equals(matchingName[i].Version, version, StringComparison.OrdinalIgnoreCase))
                            matchingName[i] = null;
                        else winner = matchingName[i];
                    }
            }
            else
            {
                Version latest = null;

                foreach (GACAssemblyName t in matchingName)
                    if (t != null)
                    {
                        if (latest == null)
                        {
                            winner = t;
                            latest = new Version(winner.Version);
                        }
                        else
                        {
                            var compare = new Version(t.Version);

                            if (latest < compare)
                            {
                                winner = t;
                                latest = compare;
                            }
                        }
                    }
            }

            if (winner == null) return null;
            return winner.ToString();
        }

        /// <summary>
        ///     Tries the resolve GAC assembly.
        /// </summary>
        /// <param name="displayName">The display name.</param>
        /// <returns></returns>
        public static GACAssemblyName TryResolveGACAssembly(string displayName)
        {
            if (displayName.StartsWith(GlobalConstants.GACPrefix)) displayName = displayName.Substring(4);

            string[] split = displayName.Split(',');

            string culture = null, version = null, publicKeyToken = null, name = null;

            foreach (string part in split)
            {
                int index = part.IndexOf("=", StringComparison.OrdinalIgnoreCase);
                if (name == null && index == -1)
                    name = part.Trim();
                else
                {
                    if (culture == null && (index = part.IndexOf("Culture=", StringComparison.OrdinalIgnoreCase)) != -1)
                        culture = part.Substring(index + 8).Trim();
                    else if (version == null &&
                             (index = part.IndexOf("Version=", StringComparison.OrdinalIgnoreCase)) != -1)
                        version = part.Substring(index + 8).Trim();
                    else if (publicKeyToken == null &&
                             (index = part.IndexOf("PublicKeyToken=", StringComparison.OrdinalIgnoreCase)) != -1)
                        publicKeyToken = part.Substring(index + 15).Trim();
                }
            }

            if (String.IsNullOrEmpty(name)) return null;
            GACAssemblyName[] matchingName = GetGACAssemblies(name);
            if (matchingName.Length == 0) return null;

            if (!String.IsNullOrEmpty(publicKeyToken))
                for (int i = 0; i < matchingName.Length; i++)
                    if (
                        !String.Equals(matchingName[i].PublicKeyToken, publicKeyToken,
                            StringComparison.OrdinalIgnoreCase))
                        matchingName[i] = null;

            if (!String.IsNullOrEmpty(culture))
                for (int i = 0; i < matchingName.Length; i++)
                    if (matchingName[i] != null &&
                        !String.Equals(matchingName[i].Culture, culture, StringComparison.OrdinalIgnoreCase))
                        matchingName[i] = null;

            GACAssemblyName winner = null;

            if (!String.IsNullOrEmpty(version))
            {
                for (int i = 0; i < matchingName.Length; i++)
                    if (matchingName[i] != null)
                    {
                        if (!String.Equals(matchingName[i].Version, version, StringComparison.OrdinalIgnoreCase))
                            matchingName[i] = null;
                        else winner = matchingName[i];
                    }
            }
            else
            {
                Version latest = null;

                foreach (GACAssemblyName t in matchingName)
                    if (t != null)
                    {
                        if (latest == null)
                        {
                            winner = t;
                            latest = new Version(winner.Version);
                        }
                        else
                        {
                            var compare = new Version(t.Version);

                            if (latest < compare)
                            {
                                winner = t;
                                latest = compare;
                            }
                        }
                    }
            }

            return winner;
        }

        /// <summary>
        ///     Gets the GAC assemblies.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        private static GACAssemblyName[] GetGACAssemblies(string name)
        {
            if (name.EndsWith(".dll", StringComparison.OrdinalIgnoreCase)) name = name.Remove(name.Length - 4);

            List<GACAssemblyName> result = null;

            foreach (GACAssemblyName current in _gacNameCache)
                if (String.Equals(current.Name, name, StringComparison.OrdinalIgnoreCase))
                    (result ?? (result = new List<GACAssemblyName>())).Add(current);

            return result == null ? GACAssemblyName.EmptyNames : result.ToArray();
        }

        /// <summary>
        ///     Rebuilds the GAC assembly cache.
        /// </summary>
        /// <param name="forceRebuild">if set to <c>true</c> [force rebuild].</param>
        /// <returns></returns>
        public static bool RebuildGACAssemblyCache(bool forceRebuild)
        {
            if (_gacNameCache.Length != 0 && !forceRebuild) return true;

            IAssemblyEnum iterator = CreateGACEnum();

            if (iterator == null) return false;
            IAssemblyName currentName;
            var gacNames = new List<GACAssemblyName>();

            while (GetNextAssembly(iterator, out currentName) == 0)
            {
                if (currentName == null) continue;
                string displayName = GetDisplayName(currentName,
                    ASM_DISPLAY_FLAGS.PUBLIC_KEY_TOKEN | ASM_DISPLAY_FLAGS.VERSION | ASM_DISPLAY_FLAGS.CULTURE);
                gacNames.Add(new GACAssemblyName(displayName));
            }

            _gacNameCache = gacNames.ToArray();
            return true;
        }

        #endregion
    }

    #region GACAssemblyName

    public sealed class GACAssemblyName
    {
        public static readonly GACAssemblyName[] EmptyNames = new GACAssemblyName[0];

        private readonly string _culture;
        private readonly string _name;
        private readonly string _publicKeyToken;
        private readonly string _version;

        public GACAssemblyName(string displayName)
        {
            string[] split = displayName.Split(',');

            foreach (string part in split)
            {
                int index = part.IndexOf("Culture=", StringComparison.OrdinalIgnoreCase);
                if (_culture == null && index != -1)
                    _culture = part.Substring(index + 8).Trim();
                else if (_version == null && (index = part.IndexOf("Version=", StringComparison.OrdinalIgnoreCase)) != -1)
                    _version = part.Substring(index + 8).Trim();
                else if (_publicKeyToken == null &&
                         (index = part.IndexOf("PublicKeyToken=", StringComparison.OrdinalIgnoreCase)) != -1)
                    _publicKeyToken = part.Substring(index + 15).Trim();
                else if (_name == null)
                    _name = part.Trim();
            }
        }

        public string Name
        {
            get { return _name; }
        }

        public string Version
        {
            get { return _version; }
        }

        public string Culture
        {
            get { return _culture; }
        }

        public string PublicKeyToken
        {
            get { return _publicKeyToken; }
        }

        public override string ToString()
        {
            return String.Format("{0}, Version={1}, Culture={2}, PublicKeyToken={3}", _name, _version, _culture,
                _publicKeyToken);
        }
    }

    #endregion

    #region COM Interface Definitions

    /// <summary>
    ///     The IAssemblyCache interface is the top-level interface that provides access to the GAC.
    /// </summary>
    [ComImport, Guid("e707dcde-d1cd-11d2-bab9-00c04f8eceae"),
     InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IAssemblyCache
    {
        /// <summary>
        ///     The IAssemblyCache::UninstallAssembly method removes a reference to an assembly from the GAC.
        ///     If other applications hold no other references to the assembly, the files that make up the assembly are removed
        ///     from the GAC.
        /// </summary>
        /// <param name="dwFlags">No flags defined. Must be zero.</param>
        /// <param name="pszAssemblyName">The name of the assembly. A zero-ended Unicode string.</param>
        /// <param name="pRefData">
        ///     A pointer to a FUSION_INSTALL_REFERENCE structure. Although this is not recommended,
        ///     this parameter can be null. The assembly is installed without an application reference, or all existing application
        ///     references are gone.
        /// </param>
        /// <param name="pulDisposition">Pointer to an integer that indicates the action that is performed by the function.</param>
        /// <returns>
        ///     The return values are defined as follows:
        ///     S_OK - The assembly has been uninstalled.
        ///     S_FALSE - The operation succeeded, but the assembly was not removed from the GAC.
        ///     The reason is described in pulDisposition.
        /// </returns>
        /// <remarks>
        ///     NOTE: If pulDisposition is not null, pulDisposition contains one of the following values:
        ///     IASSEMBLYCACHE_UNINSTALL_DISPOSITION_UNINSTALLED - The assembly files have been removed from the GAC.
        ///     IASSEMBLYCACHE_UNINSTALL_DISPOSITION_STILL_IN_USE - An application is using the assembly.
        ///     This value is returned on Microsoft Windows 95 and Microsoft Windows 98.
        ///     IASSEMBLYCACHE_UNINSTALL_DISPOSITION_ALREADY_UNINSTALLED - The assembly does not exist in the GAC.
        ///     IASSEMBLYCACHE_UNINSTALL_DISPOSITION_DELETE_PENDING - Not used.
        ///     IASSEMBLYCACHE_UNINSTALL_DISPOSITION_HAS_INSTALL_REFERENCES - The assembly has not been removed from the GAC
        ///     because
        ///     another application reference exists.
        ///     IASSEMBLYCACHE_UNINSTALL_DISPOSITION_REFERENCE_NOT_FOUND - The reference that is specified in pRefData is not found
        ///     in the GAC.
        /// </remarks>
        [PreserveSig]
        // ReSharper disable once UnusedMember.Global
        int UninstallAssembly(
            uint dwFlags,
            [MarshalAs(UnmanagedType.LPWStr)] string pszAssemblyName,
            [MarshalAs(UnmanagedType.LPArray)] FUSION_INSTALL_REFERENCE[] pRefData,
            out uint pulDisposition);

        /// <summary>
        ///     The IAssemblyCache::QueryAssemblyInfo method retrieves information about an assembly from the GAC.
        /// </summary>
        /// <param name="dwFlags">
        ///     One of QUERYASMINFO_FLAG_VALIDATE or QUERYASMINFO_FLAG_GETSIZE:
        ///     *_VALIDATE - Performs validation of the files in the GAC against the assembly manifest, including hash verification
        ///     and strong name signature verification.
        ///     *_GETSIZE - Returns the size of all files in the assembly (disk footprint). If this is not specified, the
        ///     ASSEMBLY_INFO::uliAssemblySizeInKB field is not modified.
        /// </param>
        /// <param name="pszAssemblyName"></param>
        /// <param name="pAsmInfo"></param>
        /// <returns></returns>
        [PreserveSig]
        // ReSharper disable once UnusedMember.Global
        int QueryAssemblyInfo(
            uint dwFlags,
            [MarshalAs(UnmanagedType.LPWStr)] string pszAssemblyName,
            ref ASSEMBLY_INFO pAsmInfo);

        /// <summary>
        ///     Undocumented
        /// </summary>
        /// <param name="dwFlags"></param>
        /// <param name="pvReserved"></param>
        /// <param name="ppAsmItem"></param>
        /// <param name="pszAssemblyName"></param>
        /// <returns></returns>
        [PreserveSig]
        // ReSharper disable once UnusedMember.Global
        int CreateAssemblyCacheItem(
            uint dwFlags,
            IntPtr pvReserved,
            out IAssemblyCacheItem ppAsmItem,
            [MarshalAs(UnmanagedType.LPWStr)] string pszAssemblyName);

        /// <summary>
        ///     The IAssemblyCache::InstallAssembly method adds a new assembly to the GAC. The assembly must be persisted in the
        ///     file
        ///     system and is copied to the GAC.
        /// </summary>
        /// <param name="dwFlags">
        ///     At most, one of the bits of the IASSEMBLYCACHE_INSTALL_FLAG_* values can be specified:
        ///     *_REFRESH - If the assembly is already installed in the GAC and the file version numbers of the assembly being
        ///     installed are the same or later, the files are replaced.
        ///     *_FORCE_REFRESH - The files of an existing assembly are overwritten regardless of their version number.
        /// </param>
        /// <param name="pszManifestFilePath">
        ///     A string pointing to the dynamic-linked library (DLL) that contains the assembly manifest.
        ///     Other assembly files must reside in the same directory as the DLL that contains the assembly manifest.
        /// </param>
        /// <param name="pRefData">
        ///     A pointer to a FUSION_INSTALL_REFERENCE that indicates the application on whose behalf the
        ///     assembly is being installed. Although this is not recommended, this parameter can be null, but this leaves the
        ///     assembly
        ///     without any application reference.
        /// </param>
        /// <returns></returns>
        [PreserveSig]
        // ReSharper disable once UnusedMember.Global
        int InstallAssembly(
            uint dwFlags,
            [MarshalAs(UnmanagedType.LPWStr)] string pszManifestFilePath,
            [MarshalAs(UnmanagedType.LPArray)] FUSION_INSTALL_REFERENCE[] pRefData);
    }


    /// <summary>
    ///     The IAssemblyName interface represents an assembly name. An assembly name includes a predetermined set of
    ///     name-value pairs.
    ///     The assembly name is described in detail in the .NET Framework SDK.
    /// </summary>
    [ComImport, Guid("CD193BC0-B4BC-11d2-9833-00C04FC31D2E"),
     InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IAssemblyName
    {
#pragma warning restore 465

        //		*_VERSION - Includes the version number as part of the display name.
        /// <summary>
        ///     The IAssemblyName::GetDisplayName method returns a string representation of the assembly name.
        /// </summary>
        /// <param name="szDisplayName">
        ///     A pointer to a buffer that is to contain the display name. The display name is returned in
        ///     Unicode.
        /// </param>
        /// <param name="pccDisplayName">
        ///     The size of the buffer in characters (on input). The length of the returned display name
        ///     (on return).
        /// </param>
        /// <param name="dwDisplayFlags">
        ///     One or more of the bits defined in the ASM_DISPLAY_FLAGS enumeration:
        ///     *_CULTURE - Includes the culture.
        ///     *_PUBLIC_KEY_TOKEN - Includes the public key token.
        ///     *_PUBLIC_KEY - Includes the public key.
        ///     *_CUSTOM - Includes the custom part of the assembly name.
        ///     *_PROCESSORARCHITECTURE - Includes the processor architecture.
        ///     *_LANGUAGEID - Includes the language ID.
        /// </param>
        /// <returns></returns>
        /// <remarks>http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpguide/html/cpcondefaultmarshalingforstrings.asp</remarks>
        [PreserveSig]
        int GetDisplayName(
            [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder szDisplayName,
            ref uint pccDisplayName,
            ASM_DISPLAY_FLAGS dwDisplayFlags);
    }


    /// <summary>
    ///     The IAssemblyEnum interface enumerates the assemblies in the GAC.
    /// </summary>
    [ComImport, Guid("21b8916c-f28e-11d2-a473-00c04f8ef448"),
     InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IAssemblyEnum
    {
        /// <summary>
        ///     The IAssemblyEnum::GetNextAssembly method enumerates the assemblies in the GAC.
        /// </summary>
        /// <param name="pvReserved">Must be null.</param>
        /// <param name="ppName">
        ///     Pointer to a memory location that is to receive the interface pointer to the assembly
        ///     name of the next assembly that is enumerated.
        /// </param>
        /// <param name="dwFlags">Must be zero.</param>
        /// <returns></returns>
        [PreserveSig]
        int GetNextAssembly(
            IntPtr pvReserved,
            out IAssemblyName ppName,
            uint dwFlags);
    }


    /// <summary>
    ///     The IInstallReferenceItem interface represents a reference that has been set on an assembly in the GAC.
    ///     Instances of IInstallReferenceIteam are returned by the IInstallReferenceEnum interface.
    /// </summary>
    [ComImport, Guid("582dac66-e678-449f-aba6-6faaec8a9394"),
     InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IInstallReferenceItem
    {
    }


    /// <summary>
    ///     The IInstallReferenceEnum interface enumerates all references that are set on an assembly in the GAC.
    ///     NOTE: References that belong to the assembly are locked for changes while those references are being enumerated.
    /// </summary>
    [ComImport, Guid("56b1a988-7c0c-4aa2-8639-c3eb5a90226f"),
     InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IInstallReferenceEnum
    {
    }


    /// <summary>
    ///     Undocumented. Probably only for internal use.
    ///     <see cref="IAssemblyCache.CreateAssemblyCacheItem" />
    /// </summary>
    [ComImport, Guid("9E3AAEB4-D1CD-11D2-BAB9-00C04F8ECEAE"),
     InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IAssemblyCacheItem
    {
    }

    #endregion
}