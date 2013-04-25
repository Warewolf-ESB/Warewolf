using System.ComponentModel;
using Dev2.Common;

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
        [Description("Date and Time")]
        FullDateTime,
        [Description("Date and Time Format")]
        DateTimeFormat,
        [Description("Disk Available (GB)")]
        DiskAvailable,
        [Description("Disk Total (GB)")]
        DiskTotal,
        [Description("Memory Available (MB)")]
        PhysicalMemoryAvailable,
        [Description("Memory Total (MB)")]
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
}