using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Enums;

namespace Dev2.Data
{
    public class Dev2WorkflowSettingsTO : IDev2WorkflowSettings
    {
        public bool EnableDetailedLogging { get; set; }

        public LoggerType LoggerType { get; set; }

        public int KeepLogsForDays { get; set; }

        public bool CompressOldLogFiles { get; set; }
    }
}
