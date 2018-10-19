using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Warewolf.Launcher.TestCoverageRunners
{
    public class OpenCoverRunner : ITestCoverageRunner
    {
        public string CoverageToolPath { get; set; }

        public string RunCoverageTool(string TestsResultsPath, string JobName, List<string> TestAssembliesDirectories)
        {
            // Prepare OpenCover Output File
            var OpenCoverSnapshotFile = Path.Combine(TestsResultsPath, $"{JobName} OpenCover Output.xml");
            TestCleanupUtils.CopyOnWrite(OpenCoverSnapshotFile);

            // Create full OpenCover argument string.
            var FullArgsList = $" -target:\"" + TestsResultsPath + "\\..\\Run " + JobName + $".bat\" -register:user -output:\"{OpenCoverSnapshotFile}\"";

            // Write OpenCover Runner Batch File
            var OpenCoverRunnerPath = $"{TestsResultsPath}\\Run {JobName} OpenCover.bat";
            TestCleanupUtils.CopyOnWrite(OpenCoverRunnerPath);
            File.WriteAllText(OpenCoverRunnerPath, $"\"{CoverageToolPath}\"{FullArgsList}");
            // Run OpenCover Runner Batch File
            return ProcessUtils.RunFileInThisProcess(OpenCoverRunnerPath);
        }

        public string StartServiceWithCoverage(string ServerPath, string TestsResultsPath, bool IsExistingService)
        {
            var RunServerWithOpenCoverScript = "\"" + CoverageToolPath + "\" -target:\"Warewolf Server\" -service";
            if (!IsExistingService)
            {
                Process.Start("sc.exe", "create \"Warewolf Server\" binPath= \"" + RunServerWithOpenCoverScript + "\" start= demand");
                Process.Start(CoverageToolPath, "-target:\"Warewolf Server\" -service");
            }
            else
            {
                Console.WriteLine("Instrumenting service for coverage with " + CoverageToolPath);
                Process.Start(CoverageToolPath, "-target:\"Warewolf Server\" -service");
            }
            return RunServerWithOpenCoverScript;
        }

        public void StartProcessWithCoverage(string processPath, string OutputDirectory)
        {
            // Prepare OpenCover Output File
            if (!Directory.Exists(OutputDirectory))
            {
                Directory.CreateDirectory(OutputDirectory);
            }
            var OpenCoverSnapshotFile = Path.Combine(OutputDirectory, $"Studio OpenCover Output.xml");
            TestCleanupUtils.CopyOnWrite(OpenCoverSnapshotFile);

            // Run OpenCover
            Process.Start(CoverageToolPath, $" -target:\"" + processPath + $"\" -register:user -output:\"{OpenCoverSnapshotFile}\"");
        }
    }
}
