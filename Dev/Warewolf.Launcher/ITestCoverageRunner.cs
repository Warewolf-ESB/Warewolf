using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Warewolf.Launcher
{
    public interface ITestCoverageRunner
    {
        string CoverageToolPath { get; set; }
        string RunCoverageTool(string TestsResultsPath, string JobName, List<string> TestAssembliesDirectories);
        string StartServiceWithCoverage(string ServerPath, string OutputDirectory, bool IsExistingService);
        void StartProcessWithCoverage(string processPath, string OutputDirectory);
    }
}
