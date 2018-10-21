using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Warewolf.Launcher.TestCoverageRunners
{
    public class DotCoverRunner : ITestCoverageRunner
    {
        public string CoverageToolPath { get; set; }

        public DotCoverRunner(string ToolPath) => CoverageToolPath = ToolPath;

        public string RunCoverageTool(string TestsResultsPath, string JobName, List<string> TestAssembliesDirectories)
        {
            // Write DotCover Runner XML 
            var DotCoverSnapshotFile = Path.Combine(TestsResultsPath, $"{JobName} DotCover Output.dcvr");
            TestCleanupUtils.CopyOnWrite(DotCoverSnapshotFile);
            var DotCoverArgs = @"<AnalyseParams>
    <TargetExecutable>" + TestsResultsPath + "\\..\\Run " + JobName + @".bat</TargetExecutable>
    <Output>" + DotCoverSnapshotFile + @"</Output>
    <Scope>";
            foreach (var TestAssembliesDirectory in TestAssembliesDirectories)
            {
                DotCoverArgs += @"
        <ScopeEntry>" + TestAssembliesDirectory + @"\*.dll</ScopeEntry>
        <ScopeEntry>" + TestAssembliesDirectory + @"\*.exe</ScopeEntry>";
            }
            DotCoverArgs += @"
    </Scope>
    <Filters>
        <ExcludeFilters>
            <FilterEntry>
                <ModuleMask>*.tests</ModuleMask>
                <ModuleMask>*.specs</ModuleMask>
            </FilterEntry>
        </ExcludeFilters>
        <AttributeFilters>
            <AttributeFilterEntry>
                <ClassMask>System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverageAttribute</ClassMask>
            </AttributeFilterEntry>
        </AttributeFilters>
    </Filters>
</AnalyseParams>";
            var DotCoverRunnerXMLPath = Path.Combine(TestsResultsPath, JobName + " DotCover Runner.xml");
            TestCleanupUtils.CopyOnWrite(DotCoverRunnerXMLPath);
            File.WriteAllText(DotCoverRunnerXMLPath, DotCoverArgs);

            // Create full DotCover argument string.
            var DotCoverLogFile = TestsResultsPath + "\\DotCover.xml.log";
            TestCleanupUtils.CopyOnWrite(DotCoverLogFile);
            var FullArgsList = $" cover \"{DotCoverRunnerXMLPath}\" /LogFile=\"{DotCoverLogFile}\"";

            // Write DotCover Runner Batch File
            var DotCoverRunnerPath = $"{TestsResultsPath}\\Run {JobName} DotCover.bat";
            TestCleanupUtils.CopyOnWrite(DotCoverRunnerPath);
            File.WriteAllText(DotCoverRunnerPath, $"\"{CoverageToolPath}\"{FullArgsList}");
            // Run DotCover Runner Batch File
            return ProcessUtils.RunFileInThisProcess(DotCoverRunnerPath);
        }

        public string StartServiceWithCoverage(string ServerPath, string TestsResultsPath, bool IsExistingService)
        {
            var ServerBinDir = Path.GetDirectoryName(ServerPath);
            var RunnerXML = @"<AnalyseParams>
    <TargetExecutable>" + ServerPath + @"</TargetExecutable>
    <Output>" + Environment.ExpandEnvironmentVariables("%ProgramData%") + @"\Warewolf\Server Log\dotCover.dcvr</Output>
    <Scope>
	    <ScopeEntry>" + ServerBinDir + @"\*.dll</ScopeEntry>
	    <ScopeEntry>" + ServerBinDir + @"\*.exe</ScopeEntry>
    </Scope>
    <Filters>
        <ExcludeFilters>
            <FilterEntry>
                <ModuleMask>*.tests</ModuleMask>
                <ModuleMask>*.specs</ModuleMask>
            </FilterEntry>
        </ExcludeFilters>
        <AttributeFilters>
            <AttributeFilterEntry>
                <ClassMask>System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverageAttribute</ClassMask>
            </AttributeFilterEntry>
        </AttributeFilters>
    </Filters>
</AnalyseParams>";

            var DotCoverRunnerXMLPath = Path.Combine(TestsResultsPath, "Server DotCover Runner.xml");
            TestCleanupUtils.CopyOnWrite(DotCoverRunnerXMLPath);
            File.WriteAllText(DotCoverRunnerXMLPath, RunnerXML);
            var RunServerWithDotcoverScript = "\\\"" + CoverageToolPath + "\\\" cover \\\"" + DotCoverRunnerXMLPath + "\\\" /LogFile=\\\"" + TestsResultsPath + "\\ServerDotCover.log\\\"";
            if (!IsExistingService)
            {
                Process.Start("sc.exe", "create \"Warewolf Server\" binPath= \"" + RunServerWithDotcoverScript + "\" start= demand");
            }
            else
            {
                Console.WriteLine("Configuring service to " + RunServerWithDotcoverScript);
                Process.Start("sc.exe", "config \"Warewolf Server\" binPath= \"" + RunServerWithDotcoverScript + "\"");
            }
            return RunServerWithDotcoverScript;
        }

        public void StartProcessWithCoverage(string processPath, string OutputDirectory)
        {
            var StudioBinDir = Path.GetDirectoryName(processPath);
            var RunnerXML = @"
<AnalyseParams>
    <TargetExecutable>" + processPath + @"</TargetExecutable>
    <Output>" + Environment.ExpandEnvironmentVariables("%LocalAppData%") + @"\Warewolf\Studio Logs\dotCover.dcvr</Output>
    <Scope>
    	<ScopeEntry>" + StudioBinDir + @"\*.dll</ScopeEntry>
    	<ScopeEntry>" + StudioBinDir + @"\*.exe</ScopeEntry>
    </Scope>
    <Filters>
        <ExcludeFilters>
            <FilterEntry>
                <ModuleMask>*.tests</ModuleMask>
                <ModuleMask>*.specs</ModuleMask>
            </FilterEntry>
        </ExcludeFilters>
        <AttributeFilters>
            <AttributeFilterEntry>
                <ClassMask>System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverageAttribute</ClassMask>
            </AttributeFilterEntry>
        </AttributeFilters>
    </Filters>
</AnalyseParams>
";
            var DotCoverRunnerXMLPath = OutputDirectory + "\\Studio DotCover Runner.xml";
            TestCleanupUtils.CopyOnWrite(DotCoverRunnerXMLPath);
            if (!Directory.Exists(OutputDirectory))
            {
                Directory.CreateDirectory(OutputDirectory);
            }
            File.WriteAllText(DotCoverRunnerXMLPath, RunnerXML);
            Process.Start(CoverageToolPath, "cover \"" + DotCoverRunnerXMLPath + "\" /LogFile=\"" + OutputDirectory + "\\StudioDotCover.log\"");
        }
    }
}
