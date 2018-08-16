using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.ServiceProcess;
using System.Threading;
using System.Xml;
using Warewolf.Launcher.TestResultsMergers;

namespace Warewolf.Launcher
{
    public class TestLauncher
    {
        public string DoServerStart { get; set; }
        public string DoStudioStart { get; set; }
        public string ServerPath { get; set; }
        public string StudioPath { get; set; }
        public string ResourcesType { get; set; }
        public string DotCoverPath { get; set; }
        public string ServerUsername { get; set; }
        public string ServerPassword { get; set; }
        public string RunAllJobs { get; set; }
        public bool Cleanup { get; set; }
        public string AssemblyFileVersionsTest { get; set; }
        public string RecordScreen { get; set; }
        string Parallelize { get; set; }
        public string Category { get; set; }
        public string ProjectName { get; set; }
        public string RunAllUnitTests { get; set; }
        public string RunAllServerTests { get; set; }
        public string RunAllReleaseResourcesTests { get; set; }
        public string RunAllDesktopUITests { get; set; }
        public string RunAllWebUITests { get; set; }
        public string RunWarewolfServiceTests { get; set; }
        public string DomywarewolfioStart { get; set; }
        public string JobName { get; set; }
        public string MergeDotCoverSnapshotsInDirectory { get; set; }
        public string StartDocker { get; set; }
        public int RetryCount { get; internal set; } = 0;
        public bool StartServerAsConsole { get; internal set; } = false;
        public bool AdminMode { get; internal set; } = false;

        public ITestRunner TestRunner { get; internal set; }
        public ITestResultsMerger TestResultsMerger { get; internal set; }
        public ITestCoverageMerger TestCoverageMerger { get; internal set; }
        public string RetryFile { get; internal set; }

        public string ServerExeName;
        public string StudioExeName;
        public List<string> ServerPathSpecs;
        public List<string> StudioPathSpecs;
        public bool ApplyDotCover;
        public Dictionary<string, Tuple<string, string>> JobSpecs;
        public string WebsPath;
        public ContainerLauncher ciRemoteContainerLauncher;

        string RunServerWithDotcoverScript;

        public TestLauncher()
        {
            TestResultsMerger = new TRXMerger();
        }

        string FindFileInParent(List<string> FileSpecs, int NumberOfParentsToSearch = 7)
        {
            var NumberOfParentsSearched = -1;
            var FilePath = "";
            var CurrentDirectory = TestRunner.TestsPath;
            while (FilePath == "" && NumberOfParentsSearched++ < NumberOfParentsToSearch && CurrentDirectory != "")
            {
                var NumberOfFileSpecsSearched = -1;
                var FileSpec = "";
                while (FilePath == "" && ++NumberOfFileSpecsSearched < FileSpecs.Count)
                {
                    FileSpec = FileSpecs[NumberOfFileSpecsSearched];
                    if (Path.IsPathRooted(FileSpec))
                    {
                        if (CurrentDirectory != "")
                        {
                            CurrentDirectory = Path.GetDirectoryName(FileSpec);
                        }
                        FileSpec = Path.GetFileName(FileSpec);
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(CurrentDirectory))
                        {
                            CurrentDirectory = TestRunner.TestsPath;
                        }
                    }
                    if (Directory.Exists(Path.GetDirectoryName(Path.Combine(CurrentDirectory, FileSpec))))
                    {
                        string[] files = Directory.GetFiles(CurrentDirectory, FileSpec, SearchOption.TopDirectoryOnly);
                        string[] folders = Directory.GetDirectories(CurrentDirectory, FileSpec, SearchOption.TopDirectoryOnly);
                        if (files.Length > 0 || folders.Length > 0)
                        {
                            FilePath = Path.Combine(CurrentDirectory, FileSpec);
                        }
                    }
                }
                if (CurrentDirectory != null && CurrentDirectory != "" && (Path.GetFileNameWithoutExtension(CurrentDirectory)) != "\\")
                {
                    if (FilePath == "" && Path.GetPathRoot(CurrentDirectory) != CurrentDirectory)
                    {
                        CurrentDirectory = Directory.GetParent(CurrentDirectory).FullName;
                    }
                }
                else
                {
                    CurrentDirectory = "";
                }
            }
            return FilePath;
        }

        public bool TryFindWarewolfServerExe(out string serverPath)
        {
            serverPath = FindFileInParent(ServerPathSpecs);
            if (serverPath.EndsWith(".zip"))
            {
                ZipFile.ExtractToDirectory(serverPath, TestRunner.TestsResultsPath + "\\Server");
                serverPath = TestRunner.TestsResultsPath + "\\Server\\" + ServerExeName;
            }
            return (!string.IsNullOrEmpty(serverPath) && File.Exists(serverPath));
        }

        public bool TryFindWarewolfStudioExe(out string studioPath)
        {
            studioPath = FindFileInParent(StudioPathSpecs);
            if (studioPath.EndsWith(".zip"))
            {
                ZipFile.ExtractToDirectory(studioPath, TestRunner.TestsResultsPath + "\\Studio");
                studioPath = $"{TestRunner.TestsResultsPath}\\Studio\\{StudioExeName}";
            }
            return (!string.IsNullOrEmpty(studioPath) && (File.Exists(studioPath)));
        }

        public static ContainerLauncher StartLocalCIRemoteContainer(string logDirectory)
        {
            var containerLauncher = new ContainerLauncher("ciremote", "test-remotewarewolf")
            {
                LogOutputDirectory = logDirectory
            };
            return containerLauncher;
        }

        public static ContainerLauncher StartLocalMSSQLContainer(string logDirectory)
        {
            var containerLauncher = new ContainerLauncher("mssql-connector-testing", "test-mssql", "localhost");
            Thread.Sleep(30000);
            containerLauncher.LogOutputDirectory = logDirectory;
            return containerLauncher;
        }

        public static ContainerLauncher StartLocalMySQLContainer(string logDirectory)
        {
            var containerLauncher = new ContainerLauncher("mysql-connector-testing", "test-mysql", "localhost", "withnewproc")
            {
                LogOutputDirectory = logDirectory
            };
            string sourcePath = Environment.ExpandEnvironmentVariables(@"%programdata%\Warewolf\Resources\Sources\Database\NewMySqlSource.bite");
            File.WriteAllText(sourcePath, InsertServerSourceAddress(File.ReadAllText(sourcePath), $"Server={containerLauncher.IP};Database=test;Uid=root;Pwd=admin;"));
            Thread.Sleep(30000);
            return containerLauncher;
        }

        public static ContainerLauncher StartLocalRabbitMQContainer(string logDirectory)
        {
            var containerLauncher = new ContainerLauncher("rabbitmq-connector-testing", "test-rabbitmq", "localhost")
            {
                LogOutputDirectory = logDirectory
            };
            Thread.Sleep(30000);
            return containerLauncher;
        }

        static string InsertServerSourceAddress(string serverSourceXML, string newAddress)
        {
            var startFrom = "ConnectionString=\"";
            var subStringTo = "\" ServerVersion=\"";
            int startIndex = serverSourceXML.IndexOf(startFrom) + startFrom.Length;
            int length = serverSourceXML.IndexOf(subStringTo) - startIndex;
            string oldAddress = serverSourceXML.Substring(startIndex, length);
            if (!string.IsNullOrEmpty(oldAddress))
            {
                serverSourceXML = serverSourceXML.Replace(oldAddress, "");
            }
            return serverSourceXML.Substring(0, startIndex) + newAddress + serverSourceXML.Substring(startIndex, serverSourceXML.Length - startIndex);
        }

        public void RetryTestFailures(string jobName, string testAssembliesList, List<string> TestAssembliesDirectories, string testSettingsFile, string FullTRXFilePath, int currentRetryCount)
        {
            TestRunner.TestsResultsPath = Path.Combine(TestRunner.TestsResultsPath, "..", NumberToWords(currentRetryCount) + "RetryTestResults");
            TestRunner.TestsResultsPath = Path.GetFullPath((new Uri(TestRunner.TestsResultsPath)).LocalPath);
            if (ciRemoteContainerLauncher != null)
            {
                ciRemoteContainerLauncher.LogOutputDirectory = TestRunner.TestsResultsPath;
            }

            TestCleanupUtils.WaitForFileUnlock(FullTRXFilePath);
            TestRunner.TestList = "";
            var TestFailures = new List<string>();
            XmlDocument trxContent = new XmlDocument();
            trxContent.Load(FullTRXFilePath);
            var namespaceManager = new XmlNamespaceManager(trxContent.NameTable);
            namespaceManager.AddNamespace("a", "http://microsoft.com/schemas/VisualStudio/TeamTest/2010");
            if (trxContent.DocumentElement.SelectNodes("/a:TestRun/a:TestDefinitions/a:UnitTest/a:TestMethod", namespaceManager).Count > 0)
            {
                foreach (XmlNode TestResult in trxContent.DocumentElement.SelectNodes("/a:TestRun/a:Results/a:UnitTestResult", namespaceManager))
                {
                    if (TestResult.Attributes["outcome"] == null || TestResult.Attributes["outcome"].InnerText == "Failed")
                    {
                        TestFailures.Add(TestResult.Attributes["testName"].InnerXml);
                    }
                }
            }
            else
            {
                Console.WriteLine($"Error parsing /TestRun/Results/UnitTestResult from trx file at {FullTRXFilePath}");
            }
            string TestRunnerPath;
            if (TestFailures.Count > 0)
            {
                TestRunner.TestList = string.Join(",", TestFailures);
                TestRunnerPath = TestRunner.WriteTestRunner(jobName, "", "", testAssembliesList, testSettingsFile, Path.Combine(TestRunner.TestsResultsPath, "RetryResults"), RecordScreen != null, JobSpecs);
            }
            else
            {
                Console.WriteLine($"No failing tests found to retry in trx file at {FullTRXFilePath}");
                return;
            }
            Console.WriteLine($"Re-running all test failures in \"{FullTRXFilePath}\".");
            var retryResults = RunTests(jobName, testAssembliesList, TestAssembliesDirectories, testSettingsFile, TestRunnerPath);
            if (!string.IsNullOrEmpty(retryResults) && retryResults != FullTRXFilePath)
            {
                TestResultsMerger.MergeRetryResults(FullTRXFilePath, retryResults);
            }
            else
            {
                Console.WriteLine($"{TestRunnerPath} did not produce a test result trx file in {TestRunner.TestsResultsPath}");
            }
        }

        public static string NumberToWords(int number) => new[] { "None", "First", "Second", "Third", "Fourth", "Fifth", "Sixth", "Seventh", "Eighth", "Nineth", "Tenth", "Eleventh", "Twelveth", "Thirteenth", "Fourteenth", "Fifteenth", "Sixteenth", "Seventeenth", "Eighteenth", "Nineteenth" }[number];

        public void InstallServer()
        {
            //Find Server
            if (string.IsNullOrEmpty(ServerPath))
            {
                bool foundServer = TryFindWarewolfServerExe(out string serverPath);
                if (foundServer)
                {
                    ServerPath = serverPath;
                }
                else
                {
                    throw new ArgumentException($"No server found. Make sure your server is compiled and try again.");
                }
            }
            else
            {
                if (!File.Exists(ServerPath))
                {
                    throw new ArgumentException($"No server found at {ServerPath}. Make sure your server is compiled and try again.");
                }
            }
            Console.WriteLine("Will now stop any currently running Warewolf servers and studios. Resources will be backed up to " + TestRunner.TestsResultsPath + ".");
            if (string.IsNullOrEmpty(ResourcesType))
            {
                Console.WriteLine("\nWhat type of resources would you like to install the server with?");
                var options = new[] {
                    "[u]UITests: Use these resources for running UI Tests. (This is the default)",
                    "[s]ServerTests: Use these resources for running everything except unit tests and Coded UI tests.",
                    "[r]Release: Use these resources for Warewolf releases.",
                    "[l]Load: Use these resources for Desktop UI Load Testing."
                };
                foreach (var option in options)
                {
                    Console.WriteLine();
                    var originalColour = Console.ForegroundColor;
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Write(option.Substring(0, 3));
                    Console.ForegroundColor = originalColour;
                    Console.Write(option.Substring(3, option.Length-3));
                }
                Console.WriteLine("\n\nOr Press Enter to use default (UITest)...");

                ResourcesType = WindowUtils.PromptForUserInput();
                if (ResourcesType == "" || ResourcesType.ToLower() == "u")
                {
                    ResourcesType = "UITests";
                }
                if (ResourcesType.ToLower() == "s")
                {
                    ResourcesType = "ServerTests";
                }
                if (ResourcesType.ToLower() == "r")
                {
                    ResourcesType = "Release";
                }
                if (ResourcesType.ToLower() == "l")
                {
                    ResourcesType = "Load";
                }
            }
            
            string resourcesPath = Path.Combine(Path.GetDirectoryName(ServerPath), $"Resources - {ResourcesType}");
            if (!Directory.Exists(resourcesPath))
            {
                throw new ArgumentException($"Invalid resource type. Folder not found {resourcesPath}");
            }

            if (!StartServerAsConsole)
            {
                var ServerService = ServiceController.GetServices().Any(serviceController => serviceController.ServiceName.Equals("Warewolf Server"));
                if (!ApplyDotCover)
                {
                    if (!ServerService)
                    {
                        Process.Start("sc.exe", "create \"Warewolf Server\" binPath= \"" + ServerPath + "\" start= demand");
                    }
                    else
                    {
                        Console.WriteLine("Configuring service to " + ServerPath);
                        Process.Start("sc.exe", "config \"Warewolf Server\" binPath= \"" + ServerPath + "\" start= demand");
                    }
                }
                else
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
                    
                    var DotCoverRunnerXMLPath = TestRunner.TestsResultsPath + "\\Server DotCover Runner.xml";
                    TestCleanupUtils.CopyOnWrite(DotCoverRunnerXMLPath);
                    File.WriteAllText(DotCoverRunnerXMLPath, RunnerXML);
                    RunServerWithDotcoverScript = "\\\"" + DotCoverPath + "\\\" cover \\\"" + DotCoverRunnerXMLPath + "\\\" /LogFile=\\\"" + TestRunner.TestsResultsPath + "\\ServerDotCover.log\\\"";
                    if (!ServerService)
                    {
                        Process.Start("sc.exe", "create \"Warewolf Server\" binPath= \"" + RunServerWithDotcoverScript + "\" start= demand");
                    }
                    else
                    {
                        Console.WriteLine("Configuring service to " + RunServerWithDotcoverScript);
                        Process.Start("sc.exe", "config \"Warewolf Server\" binPath= \"" + RunServerWithDotcoverScript + "\"");
                    }
                }
            }
            if (!string.IsNullOrEmpty(ServerUsername) && string.IsNullOrEmpty(ServerPassword))
            {
                Process.Start("sc.exe", "config \"Warewolf Server\" obj= \"" + ServerUsername + "\"");
            }
            if (!string.IsNullOrEmpty(ServerUsername) && !string.IsNullOrEmpty(ServerPassword))
            {
                Process.Start("sc.exe", "config \"Warewolf Server\" obj= \"" + ServerUsername + "\" password= \"" + ServerPassword + "\"");
            }

            var ResourcePathSpecs = new List<string>();
            foreach (var ServerPathSpec in ServerPathSpecs)
            {
                if (ServerPathSpec.EndsWith(ServerExeName))
                {
                    ResourcePathSpecs.Add(ServerPathSpec.Replace(ServerExeName, "Resources - " + ResourcesType));
                }
            }
            var ResourcesDirectory = FindFileInParent(ResourcePathSpecs);

            if (ResourcesDirectory != "" && ResourcesDirectory != Path.GetDirectoryName(ServerPath) + "\\" + Path.GetFileName(ResourcesDirectory))
            {
                RecursiveFolderCopy(ResourcesDirectory, Path.GetDirectoryName(ServerPath));
            }
        }

        public void StartServer()
        {
            var ServerFolderPath = Path.GetDirectoryName(ServerPath);
            Console.WriteLine($"Deploying New resources from {ServerFolderPath}\\Resources - {ResourcesType}\\*");
            RecursiveFolderCopy(Path.Combine(ServerFolderPath, $"Resources - {ResourcesType}"), Environment.ExpandEnvironmentVariables("%ProgramData%\\Warewolf"));

            if (!StartServerAsConsole)
            {
                try
                {
                    ServiceController.GetServices().FirstOrDefault(serviceController => serviceController.ServiceName.Equals("Warewolf Server"))?.Start();
                }
                catch (InvalidOperationException e)
                {
                    Console.WriteLine(e.InnerException == null ? e.Message : e.InnerException.Message);
                }

                var process = ProcessUtils.StartProcess("sc.exe", "interrogate \"Warewolf Server\"");
                var Output = process.StandardOutput.ReadToEnd() + process.StandardError.ReadToEnd();
                if (!(Output.EndsWith("RUNNING ")))
                {
                    Console.WriteLine(Output);
                    process.StartInfo.Arguments = "start \"Warewolf Server\"";
                    process.Start();
                    process.WaitForExit();
                }
            }
            else
            {
                if (!ApplyDotCover)
                {
                    Process.Start(ServerPath);
                }
                else
                {
                    Process.Start(RunServerWithDotcoverScript);
                }
            }

            WaitForServerStart(ServerFolderPath);
        }

        void WaitForServerStart(string ServerFolderPath)
        {
            var ServerStartedFilePath = ServerFolderPath + "\\ServerStarted";
            TestCleanupUtils.WaitForFileExist(ServerStartedFilePath);
            if (!(File.Exists(ServerStartedFilePath)))
            {
                throw new Exception("Server Cannot Start.");
            }
            else
            {
                Console.WriteLine("Server has started.");
            }
        }

        public void Startmywarewolfio()
        {
            if (Directory.Exists(WebsPath))
            {
                var IISExpressPath = "C:\\Program Files (x86)\\IIS Express\\iisexpress.exe";
                if (!(File.Exists(IISExpressPath)))
                {
                    Console.WriteLine("my.warewolf.io cannot be hosted. IISExpressPath not found.");
                }
                else
                {
                    Console.WriteLine("\"" + IISExpressPath + "\" /path:\"" + WebsPath + "\" /port:18405 /trace:error");
                    Process.Start(IISExpressPath, "/path:\"" + WebsPath + "\" /port:18405 /trace:error");
                    Console.WriteLine("my.warewolf.io has started.");
                }
            }
            else
            {
                Console.WriteLine("my.warewolf.io cannot be hosted. Webs not found at " + TestRunner.TestsPath + "\\_PublishedWebsites\\Dev2.Web");
                if (!string.IsNullOrEmpty(ServerPath))
                {
                    Console.Write(" or at " + Path.GetDirectoryName(ServerPath) + "\\_PublishedWebsites\\Dev2.Web");
                }
            }
        }

        public void StartStudio()
        {
            if (string.IsNullOrEmpty(StudioPath))
            {
                throw new FileNotFoundException("Cannot find Warewolf Studio. To run the studio provide a path to the Warewolf Studio exe file as a commandline parameter like this: -StudioPath");
            }
            var StudioLogFile = Environment.ExpandEnvironmentVariables("%LocalAppData%\\Warewolf\\Studio Logs\\Warewolf Studio.log");
            TestCleanupUtils.CopyOnWrite(StudioLogFile);
            if (!ApplyDotCover)
            {
                Process.Start(StudioPath);
            }
            else
            {
                var StudioBinDir = Path.GetDirectoryName(StudioPath);
                var RunnerXML = @"
<AnalyseParams>
    <TargetExecutable>" + StudioPath + @"</TargetExecutable>
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
                var DotCoverRunnerXMLPath = TestRunner.TestsResultsPath + "\\Studio DotCover Runner.xml";
                TestCleanupUtils.CopyOnWrite(DotCoverRunnerXMLPath);
                File.WriteAllText(DotCoverRunnerXMLPath, RunnerXML);
                Process.Start(DotCoverPath, "cover \"" + DotCoverRunnerXMLPath + "\" /LogFile=\"" + TestRunner.TestsResultsPath + "\\StudioDotCover.log\"");
            }
            try
            {
                WaitForStudioStart(Path.GetDirectoryName(StudioPath));
            }
            catch (Exception)
            {
                if (!ApplyDotCover)
                {
                    Process.Start(StudioPath);
                }
                else
                {
                    Process.Start(DotCoverPath, "cover \"" + TestRunner.TestsResultsPath + "\\Studio DotCover Runner.xml\" /LogFile=\"" + TestRunner.TestsResultsPath + "\\StudioDotCover.log\"");
                }
                WaitForStudioStart(Path.GetDirectoryName(StudioPath));
            }
        }

        void WaitForStudioStart(string StudioFolderPath)
        {
            var StudioStartedFilePath = Path.Combine(StudioFolderPath, "StudioStarted");
            TestCleanupUtils.WaitForFileExist(StudioStartedFilePath);
            if (!(File.Exists(StudioStartedFilePath)))
            {
                throw new Exception("Studio Cannot Start.");
            }
            else
            {
                Console.WriteLine("Studio has started.");
            }
        }

        bool AssemblyIsNotAlreadyDefinedWithoutWildcards(string AssemblyNameToCheck)
        {
            var JobAssemblySpecs = new List<string>();
            foreach (var Job in JobSpecs.Values)
            {
                if (!Job.Item1.Contains("*") && !JobAssemblySpecs.Contains(Job.Item1))
                {
                    JobAssemblySpecs.Add(Job.Item1);
                }
            }
            return !JobAssemblySpecs.Contains(AssemblyNameToCheck);
        }

        public Tuple<string, List<string>> ResolveProjectFolderSpecs(string ProjectFolderSpec)
        {
            var TestAssembliesList = "";
            var TestAssembliesDirectories = new List<string>();
            var ProjectFolderSpecInParent = FindFileInParent(new List<string> { ProjectFolderSpec });
            if (ProjectFolderSpecInParent != "")
            {
                if (ProjectFolderSpecInParent.Contains("*"))
                {
                    foreach (var projectFolder in Directory.GetDirectories(ProjectFolderSpecInParent))
                    {
                        TestAssembliesList += TestRunner.AppendProjectFolder(projectFolder);
                        if (!TestAssembliesDirectories.Contains(projectFolder + "\\bin\\Debug"))
                        {
                            TestAssembliesDirectories.Add(projectFolder + "\\bin\\Debug");
                        }
                    }
                }
                else
                {
                    TestAssembliesList += TestRunner.AppendProjectFolder(ProjectFolderSpecInParent);
                    if (!TestAssembliesDirectories.Contains(ProjectFolderSpecInParent + "\\bin\\Debug"))
                    {
                        TestAssembliesDirectories.Add(ProjectFolderSpecInParent + "\\bin\\Debug");
                    }
                }
                return new Tuple<string, List<string>>(TestAssembliesList, TestAssembliesDirectories);
            }
            throw new Exception("Cannot resolve spec: " + ProjectFolderSpec);
        }

        public Tuple<string, List<string>> ResolveTestAssemblyFileSpecs(string TestAssemblyFileSpecs)
        {
            var TestAssembliesList = "";
            var TestAssembliesDirectories = new List<string>();
            var TestAssembliesFileSpecsInParent = FindFileInParent(new List<string> { TestAssemblyFileSpecs });
            if (!string.IsNullOrEmpty(TestAssembliesFileSpecsInParent))
            {
                List<string> resolveCommaNotation = new List<string>();
                if (TestAssembliesFileSpecsInParent.Contains(','))
                {
                    resolveCommaNotation = TestAssembliesFileSpecsInParent.Split(',').ToList();
                }
                else
                {
                    resolveCommaNotation.Add(TestAssembliesFileSpecsInParent);
                }
                List<string> resolveStarNotation = new List<string>();
                foreach (var file in resolveCommaNotation)
                {
                    if (TestAssembliesFileSpecsInParent.Contains('*'))
                    {
                        resolveStarNotation = Directory.GetFiles(Path.GetDirectoryName(TestAssembliesFileSpecsInParent), Path.GetFileName(TestAssembliesFileSpecsInParent), SearchOption.TopDirectoryOnly).ToList();
                    }
                    else
                    {
                        resolveStarNotation.Add(TestAssembliesFileSpecsInParent);
                    }
                }
                foreach (var file in resolveStarNotation)
                {
                    var AssemblyNameToCheck = Path.GetFileNameWithoutExtension(file);
                    if (!TestAssembliesFileSpecsInParent.Contains("*") || (AssemblyIsNotAlreadyDefinedWithoutWildcards(AssemblyNameToCheck)))
                    {
                        TestAssembliesList = TestRunner.AppendTestAssembly(TestAssembliesList, file);
                        if (!TestAssembliesDirectories.Contains(Path.GetDirectoryName(file)))
                        {
                            TestAssembliesDirectories.Add(Path.GetDirectoryName(file));
                        }
                    }
                }
                return new Tuple<string, List<string>>(TestAssembliesList, TestAssembliesDirectories);
            }
            throw new Exception($"Cannot find test assemblies at {TestAssemblyFileSpecs}. Make sure your test assemblies are compiled and try again.");
        }

        public string ScreenRecordingTestSettingsFile(string JobName)
        {
            var TestSettingsFile = "";
            if (RecordScreen != null)
            {
                var TestSettingsId = Guid.NewGuid();

                // Create test settings.
                TestSettingsFile = TestRunner.TestsResultsPath + "\\" + JobName + ".testsettings";
                TestCleanupUtils.CopyOnWrite(TestSettingsFile);
                File.WriteAllText(TestSettingsFile, @"<?xml version=""1.0"" encoding=""UTF-8""?>
<TestSettings id=""" + TestSettingsId + @""" name=""JobName"" xmlns=""http://microsoft.com/schemas/VisualStudio/TeamTest/2010"">
    <Description>Run " + JobName + @" With Screen Recording.</Description>
    <NamingScheme baseName=""ScreenRecordings"" appendTimeStamp=""false"" useDefault=""false""/>
    <Execution>
    <AgentRule name=""LocalMachineDefaultRole"">
        <DataCollectors>
        <DataCollector uri=""datacollector://microsoft/VideoRecorder/1.0"" assemblyQualifiedName=""Microsoft.VisualStudio.TestTools.DataCollection.VideoRecorder.VideoRecorderDataCollector, Microsoft.VisualStudio.TestTools.DataCollection.VideoRecorder, Version=12.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"" friendlyName=""Screen and Voice Recorder"">
            <Configuration>
            <MediaRecorder sendRecordedMediaForPassedTestCase=""false"" xmlns=""""/>
            </Configuration>
        </DataCollector>
        </DataCollectors>
    </AgentRule>
    </Execution>
</TestSettings>
");
            }
            return TestSettingsFile;
        }

        public string DotCoverRunner(string JobName, List<string> TestAssembliesDirectories)
        {
            // Write DotCover Runner XML 
            var DotCoverSnapshotFile = Path.Combine(TestRunner.TestsResultsPath, $"{JobName} DotCover Output.dcvr");
            TestCleanupUtils.CopyOnWrite(DotCoverSnapshotFile);
            var DotCoverArgs = @"<AnalyseParams>
    <TargetExecutable>" + TestRunner.TestsResultsPath + "\\..\\Run " + JobName + @".bat</TargetExecutable>
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
            var DotCoverRunnerXMLPath = Path.Combine(TestRunner.TestsResultsPath, JobName + " DotCover Runner.xml");
            TestCleanupUtils.CopyOnWrite(DotCoverRunnerXMLPath);
            File.WriteAllText(DotCoverRunnerXMLPath, DotCoverArgs);

            // Create full DotCover argument string.
            var DotCoverLogFile = TestRunner.TestsResultsPath + "\\DotCover.xml.log";
            TestCleanupUtils.CopyOnWrite(DotCoverLogFile);
            var FullArgsList = $" cover \"{DotCoverRunnerXMLPath}\" /LogFile=\"{DotCoverLogFile}\"";

            // Write DotCover Runner Batch File
            var DotCoverRunnerPath = $"{TestRunner.TestsResultsPath}\\Run {JobName} DotCover.bat";
            TestCleanupUtils.CopyOnWrite(DotCoverRunnerPath);
            File.WriteAllText(DotCoverRunnerPath, $"\"{DotCoverPath}\"{FullArgsList}");
            return DotCoverRunnerPath;
        }

        public string RunTests(string JobName, string TestAssembliesList, List<string> TestAssembliesDirectories, string TestSettingsFile, string TestRunnerPath)
        {
            var trxTestResultsFile = "";
            if (File.Exists(TestRunnerPath))
            {
                if (!string.IsNullOrEmpty(DoServerStart) || !string.IsNullOrEmpty(DoStudioStart) || !string.IsNullOrEmpty(DomywarewolfioStart))
                {
                    this.CleanupServerStudio();
                    Startmywarewolfio();
                    if (!string.IsNullOrEmpty(DoServerStart) || !string.IsNullOrEmpty(DoStudioStart))
                    {
                        StartServer();
                        if (!string.IsNullOrEmpty(DoStudioStart))
                        {
                            StartStudio();
                        }
                    }
                }
                if (ApplyDotCover && string.IsNullOrEmpty(DoServerStart) && string.IsNullOrEmpty(DoStudioStart))
                {
                    string DotCoverRunnerPath = DotCoverRunner(JobName, TestAssembliesDirectories);

                    // Run DotCover Runner Batch File
                    trxTestResultsFile = ProcessUtils.RunFileInThisProcess(DotCoverRunnerPath);
                    if (!string.IsNullOrEmpty(DoServerStart) || !string.IsNullOrEmpty(DoStudioStart) || !string.IsNullOrEmpty(DomywarewolfioStart))
                    {
                        this.CleanupServerStudio(false);
                    }
                }
                else
                {
                    // Run Test Runner Batch File
                    trxTestResultsFile = ProcessUtils.RunFileInThisProcess(TestRunnerPath);
                    if (!string.IsNullOrEmpty(DoServerStart) || !string.IsNullOrEmpty(DoStudioStart) || !string.IsNullOrEmpty(DomywarewolfioStart))
                    {
                        this.CleanupServerStudio(!ApplyDotCover);
                    }
                }
                this.MoveArtifactsToTestResults(ApplyDotCover, (!string.IsNullOrEmpty(DoServerStart) || !string.IsNullOrEmpty(DoStudioStart)), !string.IsNullOrEmpty(DoStudioStart));
            }
            return trxTestResultsFile;
        }

        void RecursiveFolderCopy(string sourceDir, string targetDir)
        {
            if (!Directory.Exists(targetDir))
            {
                Directory.CreateDirectory(targetDir);
            }

            foreach (var file in Directory.GetFiles(sourceDir))
            {
                File.Copy(file, Path.Combine(targetDir, Path.GetFileName(file)), true);
            }

            foreach (var directory in Directory.GetDirectories(sourceDir))
            {
                RecursiveFolderCopy(directory, Path.Combine(targetDir, Path.GetFileName(directory)));
            }
        }

        public void MergeDotCoverSnapshots()
        {
            var DotCoverSnapshots = Directory.GetFiles(MergeDotCoverSnapshotsInDirectory, "*.dcvr", SearchOption.AllDirectories).ToList();
            if (string.IsNullOrEmpty(JobName))
            {
                JobName = "DotCover";
            }
            var MergedSnapshotFileName = JobName.Split(',')[0];
            MergedSnapshotFileName = "Merged " + MergedSnapshotFileName + " Snapshots";
            TestCoverageMerger.MergeCoverageSnapshots(DotCoverSnapshots, MergeDotCoverSnapshotsInDirectory + "\\" + MergedSnapshotFileName, MergeDotCoverSnapshotsInDirectory + "\\DotCover", DotCoverPath);
        }

        public void RunAllUnitTestJobs(int startIndex, int NumberOfUnitTestJobs)
        {
            JobName = string.Join(",", JobSpecs.Keys.ToList().GetRange(startIndex, NumberOfUnitTestJobs));
            RunTestJobs();
            this.CleanupServerStudio(ApplyDotCover);
        }

        public void RunAllServerTestJobs(int startIndex, int NumberOfServerTestJobs)
        {
            JobName = string.Join(",", JobSpecs.Keys.ToList().GetRange(startIndex, NumberOfServerTestJobs));
            ResourcesType = "ServerTests";
            DoServerStart = "true";
            RunTestJobs();
            this.CleanupServerStudio(ApplyDotCover);
        }

        public void RunAllReleaseResourcesTestJobs(int startIndex, int NumberOfReleaseResourcesTestJobs)
        {
            JobName = string.Join(",", JobSpecs.Keys.ToList().GetRange(startIndex, NumberOfReleaseResourcesTestJobs));
            ResourcesType = "Release";
            DoServerStart = "true";
            RunTestJobs();
            this.CleanupServerStudio(ApplyDotCover);
        }

        public void RunAllDesktopUITestJobs(int startIndex, int NumberOfDesktopUITestJobs)
        {
            JobName = string.Join(",", JobSpecs.Keys.ToList().GetRange(startIndex, NumberOfDesktopUITestJobs));
            ResourcesType = "UITests";
            DoStudioStart = "true";
            RunTestJobs();
            this.CleanupServerStudio(ApplyDotCover);
        }

        public void RunAllWebUITestJobs(int startIndex, int NumberOfWebUITestJobs)
        {
            JobName = string.Join(",", JobSpecs.Keys.ToList().GetRange(startIndex, NumberOfWebUITestJobs));
            DomywarewolfioStart = "true";
            RunTestJobs();
            this.CleanupServerStudio(ApplyDotCover);
        }

        public void RunAllLoadTestJobs(int startIndex, int NumberOfLoadTestJobs)
        {
            JobName = string.Join(",", JobSpecs.Keys.ToList().GetRange(startIndex, NumberOfLoadTestJobs));
            ResourcesType = "Load";
            DoStudioStart = "true";
            RunTestJobs();
            this.CleanupServerStudio(ApplyDotCover);
        }

        public void RunTestJobs(string jobName = "")
        {
            if (jobName != "")
            {
                JobName = jobName;
            }

            // Unpack jobs
            var JobNames = new List<string>();
            var JobAssemblySpecs = new List<string>();
            var JobCategories = new List<string>();
            if (!string.IsNullOrEmpty(JobName) && string.IsNullOrEmpty(MergeDotCoverSnapshotsInDirectory))
            {
                foreach (var Job in JobName.Split(','))
                {
                    var TrimJobName = Job.TrimEnd('1', '2', '3', '4', '5', '6', '7', '8', '9', '0', ' ');
                    if (JobSpecs.ContainsKey(TrimJobName))
                    {
                        JobNames.Add(TrimJobName);
                        if (JobSpecs[TrimJobName].Item2 == null)
                        {
                            JobAssemblySpecs.Add(JobSpecs[TrimJobName].Item1);
                            JobCategories.Add("");
                        }
                        else
                        {
                            JobAssemblySpecs.Add(JobSpecs[Job].Item1);
                            JobCategories.Add(JobSpecs[Job].Item2);
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Unrecognized Job {Job} was ignored from the run");
                    }
                }
            }
            if (!string.IsNullOrEmpty(ProjectName))
            {
                JobNames.Add(ProjectName);
                JobAssemblySpecs.Add(ProjectName);
                if (!string.IsNullOrEmpty(Category))
                {
                    JobCategories.Add(Category);
                }
                else
                {
                    JobCategories.Add("");
                }
            }
            if (!File.Exists(TestRunner.Path))
            {
                throw new ArgumentException("Error cannot find VSTest.console.exe or MSTest.exe. Use either --VSTestPath or --MSTestPath parameters to pass paths to one of those files.");
            }

            if (ApplyDotCover && DotCoverPath != "" && !(File.Exists(DotCoverPath)))
            {
                throw new ArgumentException("Error cannot find dotcover.exe. Use --DotCoverPath parameter to pass a path to that file.");
            }

            if (!string.IsNullOrEmpty(DoServerStart) || !string.IsNullOrEmpty(DoStudioStart))
            {
                InstallServer();
            }

            TestRunner.ReadPlaylist();

            for (var i = 0; i < JobNames.Count; i++)
            {
                var ThisJobName = JobNames[i].ToString();
                var ProjectSpec = JobAssemblySpecs[i].ToString();
                var TestCategories = JobCategories[i].ToString();
                var TestAssembliesList = "";
                var TestAssembliesDirectories = new List<string>();
                if (!TestRunner.TestsPath.EndsWith("\\"))
                {
                    TestRunner.TestsPath += "\\";
                }
                foreach (var Project in ProjectSpec.Split(','))
                {
                    Tuple<string, List<string>> UnPackTestAssembliesListAndDirectories = ResolveTestAssemblyFileSpecs(TestRunner.TestsPath + Project + ".dll");
                    TestAssembliesList += UnPackTestAssembliesListAndDirectories.Item1;
                    if (UnPackTestAssembliesListAndDirectories.Item2.Count > 0)
                    {
                        TestAssembliesDirectories = TestAssembliesDirectories.Concat(UnPackTestAssembliesListAndDirectories.Item2).ToList();
                    }
                    if (TestAssembliesList == "")
                    {
                        UnPackTestAssembliesListAndDirectories = ResolveProjectFolderSpecs(TestRunner.TestsPath + Project);
                        TestAssembliesList += UnPackTestAssembliesListAndDirectories.Item1;
                        if (UnPackTestAssembliesListAndDirectories.Item2.Count > 0)
                        {
                            TestAssembliesDirectories = TestAssembliesDirectories.Concat(UnPackTestAssembliesListAndDirectories.Item2).ToList();
                        }
                    }
                }
                if (string.IsNullOrEmpty(TestAssembliesList))
                {
                    throw new Exception($"Cannot find any {ProjectSpec} project folders or assemblies at {TestRunner.TestsPath}.");
                }

                // Setup for screen recording
                var TestSettingsFile = ScreenRecordingTestSettingsFile(ThisJobName);

                string TestRunnerPath = TestRunner.WriteTestRunner(ThisJobName, ProjectSpec, TestCategories, TestAssembliesList, TestSettingsFile, TestRunner.TestsResultsPath, RecordScreen != null, JobSpecs);

                string TrxFile;
                if (string.IsNullOrEmpty(RetryFile))
                {
                    //Run Tests
                    TrxFile = RunTests(JobName, TestAssembliesList, TestAssembliesDirectories, TestSettingsFile, TestRunnerPath);
                }
                else
                {
                    TrxFile = RetryFile;
                }
                if (!string.IsNullOrEmpty(TrxFile))
                {
                    //Re-try Failures
                    for (var count = 0; count < RetryCount; count++)
                    {
                        RetryTestFailures(ThisJobName, TestAssembliesList, TestAssembliesDirectories, TestSettingsFile, TrxFile, count + 1);
                    }
                }
            }
            if (ApplyDotCover)
            {
                MergeDotCoverSnapshotsInDirectory = TestRunner.TestsResultsPath;
                MergeDotCoverSnapshots();
            }
        }
    }
}
