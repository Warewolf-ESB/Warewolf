using System;
using System.IO;
using System.Linq;

namespace Warewolf.Launcher.TestCoverageMergers
{
    class OpenCoverReportGenerator : ITestCoverageReportGenerator
    {
        public string ToolPath { get; set; }

        public OpenCoverReportGenerator(string ReportGeneratorToolPath) => ToolPath = ReportGeneratorToolPath;

        public void GenerateCoverageReport(string DestinationFilePath, string LogFilePath)
        {
            var SnapshotPaths = Directory.GetFiles(Path.GetDirectoryName(DestinationFilePath), "*OpenCover Output.xml", SearchOption.AllDirectories).ToList();
            var DotCoverSnapshotsString = String.Join("\";\"", SnapshotPaths);
            Console.WriteLine($"Writing coverage report to {DestinationFilePath} with {ToolPath}");
            ProcessUtils.RunFileInThisProcess(ToolPath, $"-reports:\"{DotCoverSnapshotsString}\" -targetdir:\"{DestinationFilePath}\"");
        }
    }
}
