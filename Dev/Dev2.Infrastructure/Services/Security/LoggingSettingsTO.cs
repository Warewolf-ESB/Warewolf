using log4net.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Dev2.Services.Security
{
    public class LoggingSettingsTo
    {
        public string LogLevel { get; set; }
        public int LogSize { get; set; }
    }
}