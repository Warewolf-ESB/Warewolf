using System.Collections.Generic;
using System.Windows.Input;

namespace Dev2.Settings.Logging
{
    public interface ILogSettings
    {
        ICommand GetServerLogFileCommand { get; }
        ICommand GetStudioLogFileCommand { get; }
        LogLevel ServerEventLogLevel { get; set; }
        LogLevel StudioEventLogLevel { get; set; }
        string StudioLogMaxSize { get; }
        string ServerLogMaxSize { get; }
        bool CanEditStudioLogSettings { get; }
        bool CanEditLogSettings { get; }
        LogLevel StudioFileLogLevel { get; }
        IEnumerable<string> LoggingTypes { get; }
    }
}