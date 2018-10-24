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
                if (!WaitForFileContent(snapshot))
                {
                    throw new Exception($"Snapshot \"{snapshot}\" contains no content.");
                }
            }
            var DotCoverSnapshotsString = String.Join("\";\"", SnapshotPaths);
            Console.WriteLine($"Writing coverage report to {DestinationFilePath} with {ToolPath}");
            ProcessUtils.RunFileInThisProcess(ToolPath, $"-reports:\"{DotCoverSnapshotsString}\" -targetdir:\"{DestinationFilePath}\"");
        }

        bool WaitForFileContent(string snapshot)
        {
            if (!File.Exists(snapshot))
            {
                return false;
            }
            var hasContent = false;
            var RetryCount = 0;
            while (!hasContent && RetryCount < 100)
            {
                RetryCount++;
                try
                {
                    var content = File.ReadAllBytes(snapshot);
                    hasContent = content.Length > 0;
                }
                catch
                {
                    Console.WriteLine($"Still waiting for {snapshot} file to contain something.");
                    Thread.Sleep(3000);
                }
            }
            return hasContent;
        }
    }
}
