
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2015 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.ComponentModel;

namespace Dev2.Data.Enums
{
    public enum enTypeOfSystemInformationToGather
    {
        [Description("Operating System")]
        OperatingSystem,
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
        [Description("Warewolf Agents")]
        NumberOfWarewolfAgents
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
