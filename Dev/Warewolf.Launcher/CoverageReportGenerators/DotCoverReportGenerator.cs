using System;
using System.IO;
using System.Linq;

namespace Warewolf.Launcher.TestCoverageMergers
{
    class DotCoverReportGenerator : ITestCoverageReportGenerator
    {
        public string ToolPath { get; set; }

        public DotCoverReportGenerator(string ReportGeneratorToolPath) => ToolPath = ReportGeneratorToolPath;

        public void GenerateCoverageReport(string DestinationFilePath, string LogFilePath)
        {
            var SnapshotPaths = Directory.GetFiles(Path.GetDirectoryName(DestinationFilePath), "*.dcvr", SearchOption.AllDirectories).ToList();
            if (SnapshotPaths != null)
            {
                if (SnapshotPaths.Count > 1)
                {
                    var DotCoverSnapshotsString = String.Join("\";\"", SnapshotPaths);
                    TestCleanupUtils.CopyOnWrite($"{LogFilePath}.merge.log");
                    ProcessUtils.RunFileInThisProcess(ToolPath, $"merge /Source=\"{DotCoverSnapshotsString}\" /Output=\"{DestinationFilePath}.dcvr\" /LogFile=\"{LogFilePath}.merge.log\"");
                    SnapshotPaths.ForEach((dotCoverSnapshot) => { File.Delete(dotCoverSnapshot); });
                }
                if (SnapshotPaths.Count == 1)
                {
                    var LoneSnapshot = SnapshotPaths[0];
                    if (SnapshotPaths.Count == 1 && File.Exists(LoneSnapshot))
                    {
                        ProcessUtils.RunFileInThisProcess(ToolPath, $"report /Source=\"{LoneSnapshot}\" /Output=\"{DestinationFilePath}\\DotCover Report.html\" /ReportType=HTML /LogFile=\"{LogFilePath}.report.log\"");
                        Console.WriteLine($"DotCover report written to {DestinationFilePath}\\DotCover Report.html");
                    }
                }
            }
            if (File.Exists($"{DestinationFilePath}.dcvr"))
            {
                TestCleanupUtils.CopyOnWrite($"{LogFilePath}.report.log");
                TestCleanupUtils.CopyOnWrite($"{DestinationFilePath}.html");
                ProcessUtils.RunFileInThisProcess(ToolPath, $"report /Source=\"{DestinationFilePath}.dcvr\" /Output=\"{DestinationFilePath}\\DotCover Report.html\" /ReportType=HTML /LogFile=\"{LogFilePath}.report.log\"");
                Console.WriteLine($"DotCover report written to {DestinationFilePath}\\DotCover Report.html");
            }
        }
    }
}
