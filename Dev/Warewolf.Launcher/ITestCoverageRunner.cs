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
        void RunCoverageTool();
        string StartServiceWithCoverage(bool ServerService, string ServerPath, string TestsResultsPath);
    }
}
