using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            var DeletedSnapshots = new List<string>();
            foreach (var snapshot in SnapshotPaths)
            {
                var openCoverProcess = Process.GetProcessesByName("OpenCover.Console.exe").FirstOrDefault();
                if (openCoverProcess != null)
                {
                    openCoverProcess.WaitForExit(600000);
                }
                if (new FileInfo(snapshot).Length <= 0)
                {
                    File.Delete(snapshot);
                    Console.WriteLine($"Snapshot \"{snapshot}\" still contains no content after waiting until the timeout and has been deleted.");
                    DeletedSnapshots.Add(snapshot);
                }
            }
            var DotCoverSnapshotsString = string.Join("\";\"", SnapshotPaths.Where((snapshot) => { return !DeletedSnapshots.Contains(snapshot); }));
            Console.WriteLine($"Writing coverage report to \"{DestinationFilePath}\" with \"{ToolPath}\" see log at \"{LogFilePath}\".");
            ProcessUtils.RunFileInThisProcess(ToolPath, $"-reports:\"{DotCoverSnapshotsString}\" -targetdir:\"{DestinationFilePath}\" -reporttypes:Html;Cobertura");
        }
    }
}
