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
            
            var RunServerWithOpenCoverScript = "\\\"" + CoverageToolPath + $"\\\" -target:\\\"{ServerPath}\\\" -register:user -output:\\\"{OpenCoverSnapshotFile}\\\"";
            if (!IsExistingService)
            {
                Process.Start("sc.exe", "create \"Warewolf Server\" binPath= \"" + RunServerWithOpenCoverScript + "\" start= demand");
            }
            else
            {
                Console.WriteLine("Configuring service to " + RunServerWithOpenCoverScript);
                Process.Start("sc.exe", "config \"Warewolf Server\" binPath= \"" + RunServerWithOpenCoverScript + "\"");
            }
            return RunServerWithOpenCoverScript;
        }

        public void StartServiceWithCoverage(string TestsResultsPath, string jobName) => Process.Start("sc.exe", "start \"Warewolf Server\"");

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
