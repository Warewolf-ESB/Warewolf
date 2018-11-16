using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace Warewolf.Launcher.TestCoverageMergers
{
    class OpenCoverReportGenerator : ITestCoverageReportGenerator
    {
        public string ToolPath { get; set; }

        public OpenCoverReportGenerator(string ReportGeneratorToolPath) => ToolPath = ReportGeneratorToolPath;

        public void GenerateCoverageReport(string DestinationFilePath, string LogFilePath)
        {
            var SnapshotPaths = Directory.GetFiles(Path.GetDirectoryName(DestinationFilePath), "*OpenCover Output.xml", SearchOption.AllDirectories).ToList();
            var DeletedSnapshots = new List<string>();
            foreach (var snapshot in SnapshotPaths)
            {
                if (!WaitForSnapshotReady(snapshot))
                {
                    File.Delete(snapshot);
                    Console.WriteLine($"Snapshot \"{snapshot}\" still contains no content after waiting until the timeout and has been deleted.");
                    DeletedSnapshots.Add(snapshot);
                }
            }
            var DotCoverSnapshotsString = String.Join("\";\"", SnapshotPaths.Where((snapshot) => { return !DeletedSnapshots.Contains(snapshot); }));
            Console.WriteLine($"Writing coverage report to \"{DestinationFilePath}\" with \"{ToolPath}\" see log at \"{LogFilePath}\".");
            ProcessUtils.RunFileInThisProcess(ToolPath, $"-reports:\"{DotCoverSnapshotsString}\" -targetdir:\"{DestinationFilePath}\"");
        }

        bool WaitForSnapshotReady(string snapshot)
        {
            if (!File.Exists(snapshot))
            {
                return false;
            }
            if (!WaitForAnySnapshotContent(snapshot))
            {
                return false;
            }
            BackOffAChangingSnapshot(snapshot);
            return true;
        }

        public static bool WaitForAnySnapshotContent(string snapshot)
        {
            var RetryCount = 0;
            long fileSize = 0;
            while (fileSize <= 0 && RetryCount < 200)
            {
                RetryCount++;
                fileSize = new FileInfo(snapshot).Length;
                if (fileSize <= 0)
                {
                    Console.WriteLine($"Still waiting for {snapshot} file to contain something.");
                    Thread.Sleep(3000);
                }
            }
            return new FileInfo(snapshot).Length > 0;
        }

        void BackOffAChangingSnapshot(string snapshot)
        {
            var RetryCount = 0;
            var snapshotSize = 0.0;
            var backOff = false;
            do
            {
                snapshotSize = new FileInfo(snapshot).Length;
                if (backOff)
                {
                    Console.WriteLine($"Backing off of {snapshot}.");
                }
                else
                {
                    Console.WriteLine($"Waiting to detect if {snapshot} is still changing.");
                    backOff = true;
                }
                Thread.Sleep(10000);
            }
            while (new FileInfo(snapshot).Length != snapshotSize && RetryCount++ < 5);
        }
    }
}
