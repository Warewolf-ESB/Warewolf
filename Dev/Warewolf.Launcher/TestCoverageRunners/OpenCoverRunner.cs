using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.ServiceProcess;

namespace Warewolf.Launcher.TestCoverageRunners
{
    public class OpenCoverRunner : ITestCoverageRunner
    {
        public string CoverageToolPath { get; set; }

        public OpenCoverRunner(string ToolPath) => CoverageToolPath = ToolPath;

        public string RunCoverageTool(string TestsResultsPath, string JobName, List<string> TestAssembliesDirectories)
        {
            // Prepare OpenCover Output File
            var OpenCoverSnapshotFile = Path.Combine(TestsResultsPath, $"{JobName} OpenCover Output.xml");
            TestCleanupUtils.CopyOnWrite(OpenCoverSnapshotFile);
            string OpenCoverRunnerPath = WriteRunnerScriptFile(TestsResultsPath, JobName, OpenCoverSnapshotFile);
            // Run OpenCover Runner Batch File
            return ProcessUtils.RunFileInThisProcess(OpenCoverRunnerPath);
        }

        string WriteRunnerScriptFile(string TestsResultsPath, string JobName, string OpenCoverSnapshotFile)
        {
            // Create full OpenCover argument string.
            var FullArgsList = $" -target:\"" + TestsResultsPath + "\\..\\Run " + JobName + $".bat\" -register:user -output:\"{OpenCoverSnapshotFile}\"";

            // Write OpenCover Runner Batch File
            var OpenCoverRunnerPath = $"{TestsResultsPath}\\Run {JobName} OpenCover.bat";
            TestCleanupUtils.CopyOnWrite(OpenCoverRunnerPath);
            File.WriteAllText(OpenCoverRunnerPath, $"\"{CoverageToolPath}\"{FullArgsList}");
            return OpenCoverRunnerPath;
        }

        public string InstallServiceWithCoverage(string ServerPath, string TestsResultsPath, string JobName, bool IsExistingService)
        {
            // Prepare OpenCover Output File
            var OpenCoverSnapshotFile = Path.Combine(TestsResultsPath, $"{JobName} Server OpenCover Output.xml");
            TestCleanupUtils.CopyOnWrite(OpenCoverSnapshotFile);

            var RunServerWithOpenCoverScript = $"\"{CoverageToolPath}\" -target:\"Warewolf Server\" -service";
            if (!IsExistingService)
            {
                Process.Start("sc.exe", $"create \"Warewolf Server\" binPath= \"{RunServerWithOpenCoverScript}\" start= demand");
            }
            else
            {
                Console.WriteLine("Configuring service to " + ServerPath);
                Process.Start("sc.exe", $"config \"Warewolf Server\" binPath= \"{ServerPath}\"");                
            }
            return WriteRunnerScriptFile(TestsResultsPath, "Server", OpenCoverSnapshotFile);
        }

        public void StartServiceWithCoverage(string TestsResultsPath, string jobName)
        {
            Console.WriteLine($"Instrumenting service for coverage with {CoverageToolPath}");
            var service = new ServiceController("Warewolf Server");
            while (service.Status != ServiceControllerStatus.Stopped)
            {
                service.Stop();
            }
            Process.Start(CoverageToolPath, $"-target:\"Warewolf Server\" -service -output:\"{Path.Combine(TestsResultsPath, $"{jobName} Server OpenCover Output.xml")}\"");
        }

        public void StartProcessWithCoverage(string processPath, string TestsResultsPath, string JobName)
        {
            // Prepare OpenCover Output File
            if (!Directory.Exists(TestsResultsPath))
            {
                Directory.CreateDirectory(TestsResultsPath);
            }
            var OpenCoverSnapshotFile = Path.Combine(TestsResultsPath, $"{JobName} Studio OpenCover Output.xml");
            TestCleanupUtils.CopyOnWrite(OpenCoverSnapshotFile);

            // Run OpenCover
            Process.Start(CoverageToolPath, $" -target:\"" + processPath + $"\" -register:user -output:\"{OpenCoverSnapshotFile}\"");
        }
    }
}
