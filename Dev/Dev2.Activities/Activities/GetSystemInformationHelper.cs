#pragma warning disable CC0091, S1226, S100, CC0044, CC0045, CC0021, CC0022, S1449, S1541, S1067, S3235, CC0015, S107, S2292, S1450, S105, CC0074, S1135, S101, S3776, CS0168, S2339, CC0031, S3240, CC0020, CS0108, S1694, S1481, CC0008, AD0001, S2328, S2696, S1643, CS0659, CS0067, S104, CC0030, CA2202, S3376, S1185, CS0219, S3253, S1066, CC0075, S3459, S1871, S1125, CS0649, S2737, S1858, CC0082, CC0001, S3241, S2223, S1301, CC0013, S2955, S1944, CS4014, S3052, S2674, S2344, S1939, S1210, CC0033, CC0002, S3458, S3254, S3220, S2197, S1905, S1699, S1659, S1155, CS0105, CC0019, S3626, S3604, S3440, S3256, S2692, S2345, S1109, FS0058, CS1998, CS0661, CS0660, CS0162, CC0089, CC0032, CC0011, CA1001, IDE0019, CC0105, RECS008, CA2202
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Management;
using System.Net;
using System.Security.Principal;
using System.Text;
using Dev2.Common;
using Dev2.Common.DateAndTime;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using Dev2.Runtime.ESB.Management.Services;


namespace Dev2.Activities
{
    public interface IGetSystemInformation
    {
        //CPU Available
        //CPU Total
        //Number of Warewolf Agents

        string GetOperatingSystemInformation();
        string GetOperatingSystemVersionInformation();
        string GetServicePackInformation();
        string GetOSBitValueInformation();
        string GetFullDateTimeInformation();
        string GetDateTimeFormatInformation();
        string GetDiskSpaceAvailableInformation();
        string GetDiskSpaceTotalInformation();
        string GetVirtualMemoryAvailableInformation();
        string GetVirtualMemoryTotalInformation();
        string GetPhysicalMemoryAvailableInformation();
        string GetPhysicalMemoryTotalInformation();
        string GetCPUAvailableInformation();
        string GetCPUTotalInformation();
        string GetLanguageInformation();
        string GetRegionInformation();
        string GetUserRolesInformation(IIdentity currentIdentity);
        string GetDomainInformation();
        string GetUserNameInformation();
        string GetNumberOfNICS();
        string GetMACAdresses();
        string GetDefaultGateway();
        string GetDNSServer();
        string GetIPv4Adresses();
        string GetIPv6Adresses();
        string GetComputerName();
        string GetWarewolfServerMemory();
        string GetWarewolfCPU();
        string GetWareWolfVersion();
    }

    public class GetSystemInformationHelper : IGetSystemInformation
    {
        #region Implementation of IGetSystemInformation

        public string GetOperatingSystemInformation() => GetOperatingSystemProperty("Caption");

        public string GetOperatingSystemVersionInformation() => GetOperatingSystemProperty("Version");

        string GetOperatingSystemProperty(string property)
        {
            var name = (from x in new ManagementObjectSearcher("SELECT * FROM Win32_OperatingSystem").Get().OfType<ManagementObject>()
                        select x.GetPropertyValue(property)).First();
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendFormat("{0}", name);
            return stringBuilder.ToString();
        }

        public string GetServicePackInformation()
        {

            var stringBuilder = new StringBuilder();
            var operatingSystem = Environment.OSVersion;
            stringBuilder.AppendFormat("{0}", operatingSystem.ServicePack.Replace("Service Pack ", ""));
            return stringBuilder.ToString();
        }

        public string GetOSBitValueInformation()
        {

            var stringBuilder = new StringBuilder();
            stringBuilder.AppendFormat(Environment.Is64BitOperatingSystem ? " 64" : " 32");
            return stringBuilder.ToString();
        }

        public virtual string GetFullDateTimeInformation() => DateTime.Now.ToString(GlobalConstants.PreviousGlobalDefaultNowFormat);

        public virtual string GetDateTimeFormatInformation()
        {
            var dateTimeParser = new Dev2DateTimeParser();
            var translatedDateTimeFormat = dateTimeParser.TranslateDotNetToDev2Format(CultureInfo.CurrentUICulture.DateTimeFormat.FullDateTimePattern, out string error);
            return translatedDateTimeFormat;
        }

        public string GetDiskSpaceAvailableInformation()
        {
            var stringBuilder = new StringBuilder();
            foreach (DriveInfo driveInfo1 in DriveInfo.GetDrives())
            {
                try
                {
                    stringBuilder.AppendFormat("{0}" +
                      " {1},",
                      driveInfo1.Name, ConvertToGB(driveInfo1.AvailableFreeSpace));
                }
                catch (Exception ex)
                {
                    Dev2Logger.Error(ex, GlobalConstants.WarewolfError);
                }
            }
            return stringBuilder.ToString().TrimEnd(',');
        }

        public string GetDiskSpaceTotalInformation()
        {
            var stringBuilder = new StringBuilder();
            foreach (DriveInfo driveInfo1 in DriveInfo.GetDrives())
            {
                try
                {
                    stringBuilder.AppendFormat("{0}" + " {1},",
                        driveInfo1.Name, ConvertToGB(driveInfo1.TotalSize));
                }
                catch (Exception ex)
                {
                    Dev2Logger.Error(ex, GlobalConstants.WarewolfError);
                }
            }
            return stringBuilder.ToString().TrimEnd(',');
        }

        public string GetPhysicalMemoryAvailableInformation()
        {
            var stringBuilder = new StringBuilder();
            var winQuery = new ObjectQuery("SELECT * FROM Win32_OperatingSystem");
            var searcher = new ManagementObjectSearcher(winQuery);
            foreach (var o in searcher.Get())
            {
                var item = (ManagementObject)o;
                var availablePhysicalMemory = (uint.Parse(item["FreePhysicalMemory"].ToString()) / 1024).ToString();
                stringBuilder.Append(availablePhysicalMemory.ToString(CultureInfo.InvariantCulture));
            }
            return stringBuilder.ToString();
        }

        public string GetPhysicalMemoryTotalInformation()
        {
            var stringBuilder = new StringBuilder();
            var winQuery = new ObjectQuery("SELECT * FROM Win32_OperatingSystem");
            var searcher = new ManagementObjectSearcher(winQuery);
            foreach (var o in searcher.Get())
            {
                var item = (ManagementObject)o;
                var totalPhysicalMemory = (uint.Parse(item["TotalVisibleMemorySize"].ToString()) / 1024).ToString();
                stringBuilder.Append(totalPhysicalMemory.ToString(CultureInfo.InvariantCulture));
            }
            return stringBuilder.ToString();
        }

        public string GetVirtualMemoryAvailableInformation()
        {
            var stringBuilder = new StringBuilder();
            var winQuery = new ObjectQuery("SELECT * FROM Win32_OperatingSystem");
            var searcher = new ManagementObjectSearcher(winQuery);
            foreach (var o in searcher.Get())
            {
                var item = (ManagementObject)o;
                var totalVirtualMemory = (uint.Parse(item["FreeVirtualMemory"].ToString()) / 1024).ToString();
                stringBuilder.Append(totalVirtualMemory.ToString(CultureInfo.InvariantCulture));
            }
            return stringBuilder.ToString();
        }

        public string GetVirtualMemoryTotalInformation()
        {
            var stringBuilder = new StringBuilder();
            var winQuery = new ObjectQuery("SELECT * FROM Win32_OperatingSystem");
            var searcher = new ManagementObjectSearcher(winQuery);
            foreach (var o in searcher.Get())
            {
                var item = (ManagementObject)o;
                var availableVirtualMemory = (uint.Parse(item["TotalVirtualMemorySize"].ToString()) / 1024).ToString();
                stringBuilder.Append(availableVirtualMemory.ToString(CultureInfo.InvariantCulture));
            }
            return stringBuilder.ToString();
        }

        ulong ConvertToMB(ulong valueToConvert)
        {
            var convertedValue = valueToConvert / 1024 / 1024;
            return convertedValue;
        }

        ulong ConvertToGB(long valueToConvert)
        {
            var convertedValue = ConvertToMB((ulong)valueToConvert) / 1024;
            return convertedValue;
        }

        public string GetCPUAvailableInformation()
        {
            var stringBuilder = new StringBuilder();
            var winQuery = new ObjectQuery("SELECT LoadPercentage FROM Win32_Processor");
            var searcher = new ManagementObjectSearcher(winQuery);

            foreach (var o in searcher.Get())
            {
                var item = (ManagementObject)o;
                stringBuilder.Append(100 - Convert.ToInt32(item["LoadPercentage"]) + "%");
            }
            return stringBuilder.ToString();
        }

        public string GetCPUTotalInformation()
        {
            var stringBuilder = new StringBuilder();
            var winQuery = new ObjectQuery("SELECT MaxClockSpeed,NumberOfLogicalProcessors FROM Win32_Processor");
            var searcher = new ManagementObjectSearcher(winQuery);
            foreach (var o in searcher.Get())
            {
                var item = (ManagementObject)o;
                var maxClockSpeed = Convert.ToInt32(item["MaxClockSpeed"]);
                var numberOfProcessors = Convert.ToInt32(item["NumberOfLogicalProcessors"]);
                stringBuilder.Append(numberOfProcessors + "*" + maxClockSpeed + " Mhz");
            }
            return stringBuilder.ToString();
        }

        public string GetLanguageInformation()
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendFormat("{0}", CultureInfo.CurrentCulture.Parent.DisplayName);
            return stringBuilder.ToString();
        }

        public string GetRegionInformation()
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendFormat("{0}", RegionInfo.CurrentRegion.DisplayName);
            return stringBuilder.ToString();
        }

        public string GetUserRolesInformation() => GetUserRolesInformation(null);
        public string GetUserRolesInformation(IIdentity currentIdentity)
        {
            var stringBuilder = new StringBuilder();
            var identity = currentIdentity as WindowsIdentity ?? WindowsIdentity.GetCurrent();

            if (identity.Groups != null)
            {
                foreach (var sid in identity.Groups)
                {
                    stringBuilder = TryAppendGroup(stringBuilder, sid);
                }
            }
            return stringBuilder.ToString().TrimEnd(',');
        }

        static StringBuilder TryAppendGroup(StringBuilder stringBuilder, IdentityReference sid)
        {
            try
            {
                var translatedGroup = sid.Translate(typeof(NTAccount));
                var name = translatedGroup.Value;
                stringBuilder.AppendFormat(name + ",");
            }
            catch (Exception)
            {
                var winQuery = new ObjectQuery("SELECT * FROM Win32_Group WHERE SID='" + sid.Value + "'");
                var searcher = new ManagementObjectSearcher(winQuery);
                foreach (var o in searcher.Get())
                {
                    var item = (ManagementObject)o;
                    var name = Convert.ToString(item["Name"]);
                    stringBuilder.AppendFormat(name + ",");
                }
            }
            return stringBuilder;
        }

        public string GetUserNameInformation() => Environment.UserName;

        public string GetDomainInformation() => Environment.UserDomainName;

        public string GetComputerName() => Environment.MachineName;

        public string GetWareWolfVersion() => GetServerVersion.GetVersion();

        public string GetWarewolfServerMemory()
        {
            using (var proc = Process.GetCurrentProcess())
            {
                var privateBytes = (ulong)proc.PrivateMemorySize64;
                return ConvertToMB(privateBytes).ToString(CultureInfo.InvariantCulture);
            }
        }

        public string GetWarewolfCPU()
        {
            using (var proc = Process.GetCurrentProcess())
            {
                var stringBuilder = new StringBuilder();
                var winQuery = new ObjectQuery($"SELECT PercentProcessorTime FROM Win32_PerfFormattedData_PerfProc_Process Where Name LIKE '%{proc.ProcessName}%'");
                var searcher = new ManagementObjectSearcher(winQuery);
                foreach (var o in searcher.Get())
                {
                    var item = (ManagementObject)o;
                    var maxClockSpeed = Convert.ToInt32(item["PercentProcessorTime"]);
                    stringBuilder.Append(maxClockSpeed + " %");
                }
                return stringBuilder.ToString();
            }
        }

        public string GetNumberOfNICS()
        {
            var stringBuilder = new StringBuilder();
            var nics = NetworkInterface.GetAllNetworkInterfaces();
            stringBuilder.Append(nics.Length.ToString(CultureInfo.InvariantCulture));
            return stringBuilder.ToString();
        }

        public string GetMACAdresses()
        {
            var nics = NetworkInterface.GetAllNetworkInterfaces();
            var macs = nics.Select(nic => nic.GetPhysicalAddress().ToString()).Where(n => !string.IsNullOrEmpty(n)).ToList();
            return string.Join(",", macs);
        }

        public string GetIPv4Adresses() => GetIPAddress(AddressFamily.InterNetwork);

        public string GetIPv6Adresses() => GetIPAddress(AddressFamily.InterNetworkV6);

        string GetIPAddress(AddressFamily ipv)
        {
            var hosts = Dns.GetHostEntry(Dns.GetHostName()).AddressList;
            var ips = (from host in hosts where host.AddressFamily == ipv select host.ToString()).ToList();
            return string.Join(",", ips);
        }

        public string GetDefaultGateway()
        {
            var nics = NetworkInterface.GetAllNetworkInterfaces();
            var gateway = (from nic in nics from gateWayIp in nic.GetIPProperties().GatewayAddresses select gateWayIp.Address.ToString()).ToList();
            return string.Join(",", gateway);
        }

        public string GetDNSServer()
        {
            var nics = NetworkInterface.GetAllNetworkInterfaces();
            var dns = (from nic in nics from dnsIp in nic.GetIPProperties().DnsAddresses select dnsIp.MapToIPv4().ToString()).ToList();
            return string.Join(",", dns);
        }

        #endregion
    }
}
