
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Runtime.InteropServices;

// ReSharper disable CheckNamespace
namespace Dev2.Studio.Core.Services.System
{
    /// <summary>
    /// Class wrapping the standard environment variables, and adding a little extra.
    /// adapted from http://www.csharp411.com/determine-windows-version-and-edition-with-c/
    /// </summary>
    /// <author>Jurie.smit</author>
    /// <datetime>2013/01/14-09:08 AM</datetime>
    public class SystemInfoService : ISystemInfoService
    {
        #region OSBITS
        /// <summary>
        /// Determines if the current application is 32 or 64-bit.
        /// </summary>
        static public string OsBits
        {
            get { return Environment.Is64BitOperatingSystem ? "64-bit" : "32-bit"; }
        }
        #endregion

        #region BITS
        /// <summary>
        /// Determines if the current application is 32 or 64-bit.
        /// </summary>
        static public int Bits
        {
            get
            {
                return IntPtr.Size * 8;
            }
        }
        #endregion BITS

        #region EDITION

        static private string _edition;
        /// <summary>
        /// Gets the edition of the operating system running on this computer.
        /// </summary>
        static public string Edition
        {
            get
            {
                if(_edition != null)
                    return _edition;  //***** RETURN *****//

                var edition = String.Empty;

                var osVersion = Environment.OSVersion;
                var osVersionInfo = new Osversioninfoex { dwOSVersionInfoSize = Marshal.SizeOf(typeof(Osversioninfoex)) };

                if(GetVersionEx(ref osVersionInfo))
                {
                    int majorVersion = osVersion.Version.Major;
                    int minorVersion = osVersion.Version.Minor;
                    byte productType = osVersionInfo.wProductType;
                    short suiteMask = osVersionInfo.wSuiteMask;

                    #region VERSION 4
                    if(majorVersion == 4)
                    {
                        if(productType == VerNtWorkstation)
                        {
                            // Windows NT 4.0 Workstation
                            edition = "Workstation";
                        }
                        else if(productType == VerNtServer)
                        {
                            edition = (suiteMask & VerSuiteEnterprise) != 0 ?
                                "Enterprise Server" : "Standard Server";
                        }
                    }
                    #endregion VERSION 4

                    #region VERSION 5
                    else if(majorVersion == 5)
                    {
                        if(productType == VerNtWorkstation)
                        {
                            if((suiteMask & VerSuitePersonal) != 0)
                            {
                                // Windows XP Home Edition
                                edition = "Home";
                            }
                            else
                            {
                                // Windows XP / Windows 2000 Professional
                                edition = GetSystemMetrics(86) == 0 ? "Professional"
                                    : "Tablet Edition";
                            }
                        }
                        else if(productType == VerNtServer)
                        {
                            if(minorVersion == 0)
                            {
                                if((suiteMask & VerSuiteDatacenter) != 0)
                                {
                                    // Windows 2000 Datacenter Server
                                    edition = "Datacenter Server";
                                }
                                else if((suiteMask & VerSuiteEnterprise) != 0)
                                {
                                    // Windows 2000 Advanced Server
                                    edition = "Advanced Server";
                                }
                                else
                                {
                                    // Windows 2000 Server
                                    edition = "Server";
                                }
                            }
                            else
                            {
                                if((suiteMask & VerSuiteDatacenter) != 0)
                                {
                                    // Windows Server 2003 Datacenter Edition
                                    edition = "Datacenter";
                                }
                                else if((suiteMask & VerSuiteEnterprise) != 0)
                                {
                                    // Windows Server 2003 Enterprise Edition
                                    edition = "Enterprise";
                                }
                                else if((suiteMask & VerSuiteBlade) != 0)
                                {
                                    // Windows Server 2003 Web Edition
                                    edition = "Web Edition";
                                }
                                else
                                {
                                    // Windows Server 2003 Standard Edition
                                    edition = "Standard";
                                }
                            }
                        }
                    }
                    #endregion VERSION 5

                    #region VERSION 6
                    else if(majorVersion == 6)
                    {
                        int ed;
                        if(GetProductInfo(majorVersion, minorVersion,
                            osVersionInfo.wServicePackMajor, osVersionInfo.wServicePackMinor,
                            out ed))
                        {
                            switch(ed)
                            {
                                case ProductBusiness:
                                    edition = "Business";
                                    break;
                                case ProductBusinessN:
                                    edition = "Business N";
                                    break;
                                case ProductClusterServer:
                                    edition = "HPC Edition";
                                    break;
                                case ProductDatacenterServer:
                                    edition = "Datacenter Server";
                                    break;
                                case ProductDatacenterServerCore:
                                    edition = "Datacenter Server (core installation)";
                                    break;
                                case ProductEnterprise:
                                    edition = "Enterprise";
                                    break;
                                case ProductEnterpriseN:
                                    edition = "Enterprise N";
                                    break;
                                case ProductEnterpriseServer:
                                    edition = "Enterprise Server";
                                    break;
                                case ProductEnterpriseServerCore:
                                    edition = "Enterprise Server (core installation)";
                                    break;
                                case ProductEnterpriseServerCoreV:
                                    edition = "Enterprise Server without Hyper-V (core installation)";
                                    break;
                                case ProductEnterpriseServerIa64:
                                    edition = "Enterprise Server for Itanium-based Systems";
                                    break;
                                case ProductEnterpriseServerV:
                                    edition = "Enterprise Server without Hyper-V";
                                    break;
                                case ProductHomeBasic:
                                    edition = "Home Basic";
                                    break;
                                case ProductHomeBasicN:
                                    edition = "Home Basic N";
                                    break;
                                case ProductHomePremium:
                                    edition = "Home Premium";
                                    break;
                                case ProductHomePremiumN:
                                    edition = "Home Premium N";
                                    break;
                                case ProductHyperv:
                                    edition = "Microsoft Hyper-V Server";
                                    break;
                                case ProductMediumbusinessServerManagement:
                                    edition = "Windows Essential Business Management Server";
                                    break;
                                case ProductMediumbusinessServerMessaging:
                                    edition = "Windows Essential Business Messaging Server";
                                    break;
                                case ProductMediumbusinessServerSecurity:
                                    edition = "Windows Essential Business Security Server";
                                    break;
                                case ProductServerForSmallbusiness:
                                    edition = "Windows Essential Server Solutions";
                                    break;
                                case ProductServerForSmallbusinessV:
                                    edition = "Windows Essential Server Solutions without Hyper-V";
                                    break;
                                case ProductSmallbusinessServer:
                                    edition = "Windows Small Business Server";
                                    break;
                                case ProductStandardServer:
                                    edition = "Standard Server";
                                    break;
                                case ProductStandardServerCore:
                                    edition = "Standard Server (core installation)";
                                    break;
                                case ProductStandardServerCoreV:
                                    edition = "Standard Server without Hyper-V (core installation)";
                                    break;
                                case ProductStandardServerV:
                                    edition = "Standard Server without Hyper-V";
                                    break;
                                case ProductStarter:
                                    edition = "Starter";
                                    break;
                                case ProductStorageEnterpriseServer:
                                    edition = "Enterprise Storage Server";
                                    break;
                                case ProductStorageExpressServer:
                                    edition = "Express Storage Server";
                                    break;
                                case ProductStorageStandardServer:
                                    edition = "Standard Storage Server";
                                    break;
                                case ProductStorageWorkgroupServer:
                                    edition = "Workgroup Storage Server";
                                    break;
                                case ProductUndefined:
                                    edition = "Unknown product";
                                    break;
                                case ProductUltimate:
                                    edition = "Ultimate";
                                    break;
                                case ProductUltimateN:
                                    edition = "Ultimate N";
                                    break;
                                case ProductWebServer:
                                    edition = "Web Server";
                                    break;
                                case ProductWebServerCore:
                                    edition = "Web Server (core installation)";
                                    break;
                                case ProductProfessional:
                                    edition = "Professional";
                                    break;
                            }
                        }
                    }
                    #endregion VERSION 6
                }

                _edition = edition;
                return edition;
            }
        }
        [DllImport("user32")]
        public static extern int GetSystemMetrics(int nIndex);
        #endregion EDITION

        #region NAME
        static private string _name;
        /// <summary>
        /// Gets the name of the operating system running on this computer.
        /// </summary>
        static public string Name
        {
            get
            {
                if(_name != null)
                    return _name;  //***** RETURN *****//

                string name = "unknown";

                OperatingSystem osVersion = Environment.OSVersion;
                Osversioninfoex osVersionInfo = new Osversioninfoex { dwOSVersionInfoSize = Marshal.SizeOf(typeof(Osversioninfoex)) };

                if(GetVersionEx(ref osVersionInfo))
                {
                    int majorVersion = osVersion.Version.Major;
                    int minorVersion = osVersion.Version.Minor;

                    switch(osVersion.Platform)
                    {
                        case PlatformID.Win32Windows:
                            {
                                if(majorVersion == 4)
                                {
                                    string csdVersion = osVersionInfo.szCSDVersion;
                                    switch(minorVersion)
                                    {
                                        case 0:
                                            if(csdVersion == "B" || csdVersion == "C")
                                                name = "Windows 95 OSR2";
                                            else
                                                name = "Windows 95";
                                            break;
                                        case 10:
                                            name = csdVersion == "A" ? "Windows 98 Second Edition" : "Windows 98";
                                            break;
                                        case 90:
                                            name = "Windows Me";
                                            break;
                                    }
                                }
                                break;
                            }

                        case PlatformID.Win32NT:
                            {
                                byte productType = osVersionInfo.wProductType;

                                switch(majorVersion)
                                {
                                    case 3:
                                        name = "Windows NT 3.51";
                                        break;
                                    case 4:
                                        switch(productType)
                                        {
                                            case 1:
                                                name = "Windows NT 4.0";
                                                break;
                                            case 3:
                                                name = "Windows NT 4.0 Server";
                                                break;
                                        }
                                        break;
                                    case 5:
                                        switch(minorVersion)
                                        {
                                            case 0:
                                                name = "Windows 2000";
                                                break;
                                            case 1:
                                                name = "Windows XP";
                                                break;
                                            case 2:
                                                name = productType == VerNtWorkstation ? "Windows XP" : "Windows Server 2003";
                                                break;
                                        }
                                        break;
                                    case 6:
                                        switch(minorVersion)
                                        {
                                            case 0:

                                                switch(productType)
                                                {
                                                    case 1:
                                                        name = "Windows Vista";
                                                        break;
                                                    case 3:
                                                        name = "Windows Server 2008";
                                                        break;
                                                }
                                                break;
                                            case 1:
                                                switch(productType)
                                                {
                                                    case 1:
                                                        name = "Windows 7";
                                                        break;
                                                    case 3:
                                                        name = "Windows Server 2008 R2";
                                                        break;
                                                }
                                                break;
                                            case 3:
                                                name = "Windows Server 2008";
                                                break;
                                        }
                                        break;
                                }
                                break;
                            }
                    }
                }

                _name = name;
                return name;
            }
        }
        #endregion NAME

        #region PINVOKE
        #region GET
        #region PRODUCT INFO
        [DllImport("Kernel32.dll")]
        internal static extern bool GetProductInfo(
            int osMajorVersion,
            int osMinorVersion,
            int spMajorVersion,
            int spMinorVersion,
            out int edition);
        #endregion PRODUCT INFO

        #region VERSION
        [DllImport("kernel32.dll")]
        private static extern bool GetVersionEx(ref Osversioninfoex osVersionInfo);
        #endregion VERSION
        #endregion GET

        #region OSVERSIONINFOEX
        [StructLayout(LayoutKind.Sequential)]
        private struct Osversioninfoex
        {
            public int dwOSVersionInfoSize;
            readonly int dwMajorVersion;
            readonly int dwMinorVersion;
            readonly int dwBuildNumber;
            readonly int dwPlatformId;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public readonly string szCSDVersion;
            public readonly short wServicePackMajor;
            public readonly short wServicePackMinor;
            public readonly short wSuiteMask;
            public readonly byte wProductType;
            readonly byte wReserved;
        }
        #endregion OSVERSIONINFOEX

        #region PRODUCT
        private const int ProductProfessional = 0x00000030;
        private const int ProductUndefined = 0x00000000;
        private const int ProductUltimate = 0x00000001;
        private const int ProductHomeBasic = 0x00000002;
        private const int ProductHomePremium = 0x00000003;
        private const int ProductEnterprise = 0x00000004;
        private const int ProductHomeBasicN = 0x00000005;
        private const int ProductBusiness = 0x00000006;
        private const int ProductStandardServer = 0x00000007;
        private const int ProductDatacenterServer = 0x00000008;
        private const int ProductSmallbusinessServer = 0x00000009;
        private const int ProductEnterpriseServer = 0x0000000A;
        private const int ProductStarter = 0x0000000B;
        private const int ProductDatacenterServerCore = 0x0000000C;
        private const int ProductStandardServerCore = 0x0000000D;
        private const int ProductEnterpriseServerCore = 0x0000000E;
        private const int ProductEnterpriseServerIa64 = 0x0000000F;
        private const int ProductBusinessN = 0x00000010;
        private const int ProductWebServer = 0x00000011;
        private const int ProductClusterServer = 0x00000012;
        private const int ProductStorageExpressServer = 0x00000014;
        private const int ProductStorageStandardServer = 0x00000015;
        private const int ProductStorageWorkgroupServer = 0x00000016;
        private const int ProductStorageEnterpriseServer = 0x00000017;
        private const int ProductServerForSmallbusiness = 0x00000018;
        private const int ProductHomePremiumN = 0x0000001A;
        private const int ProductEnterpriseN = 0x0000001B;
        private const int ProductUltimateN = 0x0000001C;
        private const int ProductWebServerCore = 0x0000001D;
        private const int ProductMediumbusinessServerManagement = 0x0000001E;
        private const int ProductMediumbusinessServerSecurity = 0x0000001F;
        private const int ProductMediumbusinessServerMessaging = 0x00000020;
        private const int ProductServerForSmallbusinessV = 0x00000023;
        private const int ProductStandardServerV = 0x00000024;
        private const int ProductEnterpriseServerV = 0x00000026;
        private const int ProductStandardServerCoreV = 0x00000028;
        private const int ProductEnterpriseServerCoreV = 0x00000029;
        private const int ProductHyperv = 0x0000002A;
        #endregion PRODUCT

        #region VERSIONS
        private const int VerNtWorkstation = 1;
        private const int VerNtServer = 3;
        private const int VerSuiteEnterprise = 2;
        private const int VerSuiteDatacenter = 128;
        private const int VerSuitePersonal = 512;
        private const int VerSuiteBlade = 1024;
        #endregion VERSIONS
        #endregion PINVOKE

        #region SERVICE PACK
        /// <summary>
        /// Gets the service pack information of the operating system running on this computer.
        /// </summary>
        static public string ServicePack
        {
            get
            {
                string servicePack = String.Empty;
                Osversioninfoex osVersionInfo = new Osversioninfoex { dwOSVersionInfoSize = Marshal.SizeOf(typeof(Osversioninfoex)) };

                if(GetVersionEx(ref osVersionInfo))
                {
                    servicePack = osVersionInfo.szCSDVersion;
                }

                return servicePack;
            }
        }
        #endregion SERVICE PACK

        #region VERSION
        #region BUILD
        /// <summary>
        /// Gets the build version number of the operating system running on this computer.
        /// </summary>
        static public int BuildVersion
        {
            get
            {
                return Environment.OSVersion.Version.Build;
            }
        }
        #endregion BUILD

        #region FULL
        #region STRING
        /// <summary>
        /// Gets the full version string of the operating system running on this computer.
        /// </summary>
        static public string VersionString
        {
            get
            {
                return Environment.OSVersion.Version.ToString();
            }
        }
        #endregion STRING

        #region VERSION
        /// <summary>
        /// Gets the full version of the operating system running on this computer.
        /// </summary>
        static public Version Version
        {
            get
            {
                return Environment.OSVersion.Version;
            }
        }
        #endregion VERSION
        #endregion FULL

        #region MAJOR
        /// <summary>
        /// Gets the major version number of the operating system running on this computer.
        /// </summary>
        static public int MajorVersion
        {
            get
            {
                return Environment.OSVersion.Version.Major;
            }
        }
        #endregion MAJOR

        #region MINOR
        /// <summary>
        /// Gets the minor version number of the operating system running on this computer.
        /// </summary>
        static public int MinorVersion
        {
            get
            {
                return Environment.OSVersion.Version.Minor;
            }
        }
        #endregion MINOR

        #region REVISION
        /// <summary>
        /// Gets the revision version number of the operating system running on this computer.
        /// </summary>
        static public int RevisionVersion
        {
            get
            {
                return Environment.OSVersion.Version.Revision;
            }
        }
        #endregion REVISION
        #endregion VERSION

        /// <summary>
        /// Gets the system info.
        /// </summary>
        /// <returns></returns>
        /// <author>Jurie.smit</author>
        /// <datetime>2013/01/14-09:10 AM</datetime>
        public SystemInfoTO GetSystemInfo()
        {
            return new SystemInfoTO
                {
                    ApplicationExecutionBits = Bits,
                    OsBits = OsBits,
                    Edition = Edition,
                    Name = Name,
                    ServicePack = ServicePack,
                    Version = Version.ToString()
                };
        }

    }
}
