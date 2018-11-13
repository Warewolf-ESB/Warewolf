using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Warewolf.Launcher.TestCoverageRunners
{
    public class OpenCoverRunner : ITestCoverageRunner
    {
        public string CoverageToolPath { get; set; }

        public OpenCoverRunner(string ToolPath) => CoverageToolPath = ToolPath;

        public string RunCoverageTool(string TestsResultsPath, string JobName, List<string> TestAssembliesDirectories)
        {
            var OpenCoverSnapshotFile = Path.Combine(TestsResultsPath, $"{JobName} OpenCover Output.xml");
            string OpenCoverRunnerPath = WriteRunnerScriptFile(TestsResultsPath, JobName, OpenCoverSnapshotFile);
            return ProcessUtils.RunFileInThisProcess(OpenCoverRunnerPath);
        }

        string WriteRunnerScriptFile(string TestsResultsPath, string JobName, string OpenCoverSnapshotFile)
        {
            var mergeOutput = "";
            if (File.Exists(OpenCoverSnapshotFile))
            {
                mergeOutput = " -mergeoutput";
            }
            var commandLine = $"\"{CoverageToolPath}\" -target:\"{TestsResultsPath}\\..\\Run {JobName}.bat\" -register:user -output:\"{OpenCoverSnapshotFile}\" -filter:\"+[Warewolf*]* +[Dev2*]* -[Dev2*Tests]* -[Warewolf*Tests]* -[Dev2*Specs]* -[Warewolf*Specs]* -[Warewolf.UIBindingTests.*]* -[Warewolf.Launcher]* -[Warewolf.TestingDotnetDllCascading.dll]* -[Warewolf.Storage.Interfaces]*\"{mergeOutput}";
            var OpenCoverRunnerPath = $"{TestsResultsPath}\\Run {JobName} OpenCover.bat";
            TestCleanupUtils.CopyOnWrite(OpenCoverRunnerPath);
            File.WriteAllText(OpenCoverRunnerPath, commandLine);
            return OpenCoverRunnerPath;
        }

        public string InstallServiceWithCoverage(string ServerPath, string TestsResultsPath, string JobName, bool IsExistingService)
        {
            var OpenCoverSnapshotFile = Path.Combine(TestsResultsPath, $"{JobName} Server OpenCover Output.xml");
            var mergeOutput = "";
            if (File.Exists(OpenCoverSnapshotFile))
            {
                mergeOutput = " -mergeoutput";
            }
            var doubleEscapedCommandLine = $"\\\"{CoverageToolPath}\\\" -target:\\\"{ServerPath}\\\" -register:user -output:\\\"{OpenCoverSnapshotFile}\\\" -filter:\\\"+[Warewolf*]* +[Dev2*]* -[Dev2*Tests]* -[Warewolf*Tests]* -[Dev2*Specs]* -[Warewolf*Specs]* -[Warewolf.UIBindingTests.*]* -[Warewolf.Launcher]* -[Warewolf.TestingDotnetDllCascading.dll]* -[Warewolf.Storage.Interfaces]*\\\"{mergeOutput}";
            if (!IsExistingService)
            {
                Process.Start("sc.exe", "create \"Warewolf Server\" binPath= \"" + doubleEscapedCommandLine + "\" start= demand");
            }
            else
            {
                Console.WriteLine("Configuring service to " + doubleEscapedCommandLine);
                Process.Start("sc.exe", "config \"Warewolf Server\" binPath= \"" + doubleEscapedCommandLine + "\"");
            }
            return doubleEscapedCommandLine;
        }

        public void StartServiceWithCoverage(string TestsResultsPath, string jobName) => Process.Start("sc.exe", "start \"Warewolf Server\"");

        public void StartProcessWithCoverage(string processPath, string TestsResultsPath, string JobName)
        {
            if (!Directory.Exists(TestsResultsPath))
            {
                Directory.CreateDirectory(TestsResultsPath);
            }
            var OpenCoverSnapshotFile = Path.Combine(TestsResultsPath, $"{JobName} Studio OpenCover Output.xml");
            var mergeOutput = "";
            if (File.Exists(OpenCoverSnapshotFile))
            {
                mergeOutput = " -mergeoutput";
            }
            Process.Start(CoverageToolPath, $" -target:\"{processPath}\" -register:user -output:\"{OpenCoverSnapshotFile}\" -filter:\"+[Warewolf*]* +[Dev2*]* -[Dev2*Tests]* -[Warewolf*Tests]* -[Dev2*Specs]* -[Warewolf*Specs]* -[Warewolf.UIBindingTests.*]* -[Warewolf.Launcher]* -[Warewolf.TestingDotnetDllCascading.dll]* -[Warewolf.Storage.Interfaces]*\"{mergeOutput}");
        }
    }
}
