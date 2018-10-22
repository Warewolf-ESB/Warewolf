using System;
using System.Collections.Generic;

namespace Warewolf.Launcher
{
    public interface ITestCoverageRunner
    {
        string CoverageToolPath { get; set; }
        string RunCoverageTool(string TestsResultsPath, string JobName, List<string> TestAssembliesDirectories);
        string StartServiceWithCoverage(string ServerPath, string TestsResultsPath, string JobName, bool IsExistingService);
        void StartProcessWithCoverage(string processPath, string TestsResultsPath, string JobName);
    }
}
