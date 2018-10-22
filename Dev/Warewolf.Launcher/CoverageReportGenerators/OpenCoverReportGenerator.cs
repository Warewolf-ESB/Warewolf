using System;
using System.Collections.Generic;

namespace Warewolf.Launcher.TestCoverageMergers
{
    class OpenCoverReportGenerator : ITestCoverageReportGenerator
    {
        public string ToolPath { get; set; }

        public OpenCoverReportGenerator(string ReportGeneratorToolPath) => ToolPath = ReportGeneratorToolPath;

        public void GenerateCoverageReport(List<string> SnapshotPaths, string DestinationFilePath, string LogFilePath)
        {
            TestCleanupUtils.CopyOnWrite($"{DestinationFilePath}.html");
            var DotCoverSnapshotsString = String.Join("\";\"", SnapshotPaths);
            Console.WriteLine($"Writing coverage report to {DestinationFilePath} with {ToolPath}");
            ProcessUtils.RunFileInThisProcess(ToolPath, $"-reports:\"{DotCoverSnapshotsString}\" -targetdir:\"{DestinationFilePath}\"");
        }
    }
}
