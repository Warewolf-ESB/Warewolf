using System;
using System.Collections.Generic;

namespace Warewolf.Launcher
{
    public interface ITestCoverageRunner
    {
        string CoverageToolPath { get; set; }
        string RunCoverageTool(string TestsResultsPath, string JobName, List<string> TestAssembliesDirectories);
        string InstallServiceWithCoverage(string ServerPath, string TestsResultsPath, string JobName, bool IsExistingService);
        void StartServiceWithCoverage(string TestsResultsPath, string jobName);
        void StartProcessWithCoverage(string processPath, string TestsResultsPath, string JobName);
    }
}
