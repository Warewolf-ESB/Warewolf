/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.ComponentModel;
// ReSharper disable InconsistentNaming

namespace Dev2.Data.Enums
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
