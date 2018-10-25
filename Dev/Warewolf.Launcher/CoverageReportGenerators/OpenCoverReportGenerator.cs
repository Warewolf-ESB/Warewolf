using System;
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
            foreach (var snapshot in SnapshotPaths)
            {
                if (!WaitForFileChanged(snapshot))
                {
                    throw new Exception($"Snapshot \"{snapshot}\" contains no content.");
                }
            }
            var DotCoverSnapshotsString = String.Join("\";\"", SnapshotPaths);
            Console.WriteLine($"Writing coverage report to {DestinationFilePath} with {ToolPath}");
            ProcessUtils.RunFileInThisProcess(ToolPath, $"-reports:\"{DotCoverSnapshotsString}\" -targetdir:\"{DestinationFilePath}\"");
        }

        bool WaitForFileChanged(string snapshot)
        {
            if (!File.Exists(snapshot))
            {
                return false;
            }
            var RetryCount = 0;
            var lastSnapshotSize = 0.0;
            var isChanging = false;
            var hasChanged = false;
#pragma warning disable S2589 // false positive - this expression is just complex
            while (!hasChanged && RetryCount < 200)
#pragma warning restore S2589
            {
                RetryCount++;
                hasChanged = isChanging;
                isChanging = new FileInfo(snapshot).Length != lastSnapshotSize;
                hasChanged = !isChanging && hasChanged;
                if (!hasChanged)
                {
                    Console.WriteLine($"Still waiting for {snapshot} file to contain something.");
                    Thread.Sleep(3000);
                }
                lastSnapshotSize = new FileInfo(snapshot).Length;
            }
            return hasChanged;
        }
    }
}
