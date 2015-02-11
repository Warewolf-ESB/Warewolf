using System.Windows.Input;

namespace Dev2.Common.Interfaces.Infrastructure.Logging
{
    public interface ILogSettings
    {
        ICommand GetServerLogFileCommand { get; }
        ICommand GetStudioLogFileCommand { get; }
        LogLevel ServerLogLevel { get; set; }
        LogLevel StudioLogLevel { get; set; }
        string StudioLogMaxSize { get; }
        string ServerLogMaxSize { get; }
        bool CanEditStudioLogSettings { get; }
        bool CanEditLogSettings { get; }
    }
}