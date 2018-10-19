using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Warewolf.Launcher.TestCoverageRunners
{
    public class DotCoverRunner : ITestCoverageRunner
    {
        string CoverageToolPath { get; set; }
        string ITestCoverageRunner.CoverageToolPath { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public void RunCoverageTool()
        {
            throw new NotImplementedException();
        }

        public string StartServiceWithCoverage(bool ServerService, string ServerPath, string TestsResultsPath)
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
            if (!ServerService)
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
    }
}
