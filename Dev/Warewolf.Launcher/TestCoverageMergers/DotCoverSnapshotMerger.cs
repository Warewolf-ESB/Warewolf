using System;
using System.Collections.Generic;
using System.IO;

namespace Warewolf.Launcher.TestCoverageMergers
{
    class DotCoverSnapshotMerger : ITestCoverageMerger
    {
        public void MergeCoverageSnapshots(List<string> SnapshotPaths, string DestinationFilePath, string LogFilePath, string ToolPath)
        {
            if (SnapshotPaths != null)
            {
                if (SnapshotPaths.Count > 1)
                {
                    var DotCoverSnapshotsString = String.Join("\";\"", SnapshotPaths);
                    TestCleanupUtils.CopyOnWrite($"{LogFilePath}.merge.log");
                    TestCleanupUtils.CopyOnWrite($"{LogFilePath}.report.log");
                    TestCleanupUtils.CopyOnWrite($"{DestinationFilePath}.dcvr");
                    TestCleanupUtils.CopyOnWrite($"{DestinationFilePath}.html");
                    ProcessUtils.RunFileInThisProcess(ToolPath, $"merge /Source=\"{DotCoverSnapshotsString}\" /Output=\"{DestinationFilePath}.dcvr\" /LogFile=\"{LogFilePath}.merge.log\"");
                }
                if (SnapshotPaths.Count == 1)
                {
                    var LoneSnapshot = SnapshotPaths[0];
                    if (SnapshotPaths.Count == 1 && (File.Exists(LoneSnapshot)))
                    {
                        ProcessUtils.RunFileInThisProcess(ToolPath, $"report /Source=\"{LoneSnapshot}\" /Output=\"{DestinationFilePath}\\DotCover Report.html\" /ReportType=HTML /LogFile=\"{LogFilePath}.report.log\"");
                        Console.WriteLine($"DotCover report written to {DestinationFilePath}\\DotCover Report.html");
                    }
                }
            }
            if (File.Exists($"{DestinationFilePath}.dcvr"))
            {
                ProcessUtils.RunFileInThisProcess(ToolPath, $"report /Source=\"{DestinationFilePath}.dcvr\" /Output=\"{DestinationFilePath}\\DotCover Report.html\" /ReportType=HTML /LogFile=\"{LogFilePath}.report.log\"");
                Console.WriteLine($"DotCover report written to {DestinationFilePath}\\DotCover Report.html");
            }
        }
    }
}
