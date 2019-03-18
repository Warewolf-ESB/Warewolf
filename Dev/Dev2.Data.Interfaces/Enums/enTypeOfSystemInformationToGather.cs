#pragma warning disable CC0091, S1226, S100, CC0044, CC0045, CC0021, S1449, S1541, S1067, S3235, CC0015, S107, S2292, S1450, S105, CC0074, S1135, S101, S3776, CS0168, S2339, CC0031, S3240, CC0020, CS0108, S1694, S1481, CC0008, AD0001, S2328, S2696, S1643, CS0659, CS0067, S104, CC0030, CA2202, S3376, S1185, CS0219, S3253, S1066, CC0075, S3459, S1871, S1125, CS0649, S2737, S1858, CC0082, CC0001, S3241, S2223, S1301, CC0013, S2955, S1944, CS4014, S3052, S2674, S2344, S1939, S1210, CC0033, CC0002, S3458, S3254, S3220, S2197, S1905, S1699, S1659, S1155, CS0105, CC0019, S3626, S3604, S3440, S3256, S2692, S2345, S1109, FS0058, CS1998, CS0661, CS0660, CS0162, CC0089, CC0032, CC0011, CA1001
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.ComponentModel;



namespace Dev2.Data.Interfaces.Enums
{
    public enum enTypeOfSystemInformationToGather
    {
        [Description("Computer Name")]
        ComputerName,
        [Description("Operating System")]
        OperatingSystem,
        [Description("Operating System Version")]
        OperatingSystemVersion,
        [Description("Service Pack")]
        ServicePack,
        [Description("32/64 Bit")]
        OSBitValue,
        [Description("Date & Time")]
        FullDateTime,
        [Description("Date & Time Format")]
        DateTimeFormat,
        [Description("Disk Available (GB)")]
        DiskAvailable,
        [Description("Disk Total (GB)")]
        DiskTotal,
        [Description("Virtual Memory Available (MB)")]
        VirtualMemoryAvailable,
        [Description("Virtual Memory Total (MB)")]
        VirtualMemoryTotal,
        [Description("RAM Available (MB)")]
        PhysicalMemoryAvailable,
        [Description("RAM Total (MB)")]
        PhysicalMemoryTotal,
        [Description("CPU Available")]
        CPUAvailable,
        [Description("CPU Total")]
        CPUTotal,
        [Description("Language")]
        Language,
        [Description("Region")]
        Region,
        [Description("User Roles")]
        UserRoles,
        [Description("User Name")]
        UserName,
        [Description("Domain")]
        Domain,
        [Description("Number of NICS on the Server")]
        NumberOfServerNICS,
        [Description("MAC Addresses")]
        MacAddress,
        [Description("Defaut Gateway Addresses")]
        GateWayAddress,
        [Description("DNS Server Addresses")]
        DNSAddress,
        [Description("IPv4 Addresses")]
        IPv4Address,
        [Description("IPv6 Addresses")]
        IPv6Address,
        [Description("Warewolf Memory Usage")]
        WarewolfMemory,
        [Description("Warewolf Total CPU Usage (All Cores)")]
        WarewolfCPU,
        [Description("Warewolf Server Version")]
        WarewolfServerVersion
    }

    public enum enMailPriorityEnum
    {
        [Description("Normal")]
        Normal,
        [Description("High")]
        High,
        [Description("Low")]
        Low
    }
}
