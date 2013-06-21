using System.ComponentModel;

namespace Dev2.Data.Enums
{
    /// <summary>
    /// Enum to for the foreach activity
    /// </summary>
    public enum enForEachType
    {
        [Description("* in Range")]
        InRange,
        [Description("* in CSV")]
        InCSV,
        [Description("No. of Executes")]
        NumOfExecution
    }
}
