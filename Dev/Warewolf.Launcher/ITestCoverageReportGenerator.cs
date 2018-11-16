using System.Collections.Generic;

namespace Warewolf.Launcher
{
    public interface ITestCoverageReportGenerator
    {
        string ToolPath { get; set; }
        void GenerateCoverageReport(string DestinationFilePath, string LogFilePath);
    }
}
