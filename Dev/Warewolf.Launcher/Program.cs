using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Xml;
using System.IO.Compression;
using System.ServiceProcess;
using System.Security.Principal;

namespace Bashley
{
    public class Program
    {
        public static string DoServerStart { get; set; }
        public static string DoStudioStart { get; set; }
        public static string ServerPath { get; set; }
        public static string StudioPath { get; set; }
        public static string ResourcesType { get; set; }
        public static string VSTest { get; set; }
        public static string MSTest { get; set; }
        public static string DotCoverPath { get; set; }
        public static string ServerUsername { get; set; }
        public static string ServerPassword { get; set; }
        public static string RunAllJobs { get; set; }
        public static string Cleanup { get; set; }
        public static string AssemblyFileVersionsTest { get; set; }
        public static string RecordScreen { get; set; }
        public static string Parallelize { get; set; }
        public static string Category { get; set; }
        public static string ProjectName { get; set; }
        public static string RunAllUnitTests { get; set; }
        public static string RunAllServerTests { get; set; }
        public static string RunAllReleaseResourcesTests { get; set; }
        public static string RunAllCodedUITests { get; set; }
        public static string RunWarewolfServiceTests { get; set; }
        public static string DomywarewolfioStart { get; set; }
        public static string TestsPath { get => testsPath; set => testsPath = value; }
        public static string JobName { get; set; }
        public static string TestList { get; set; }
        public static string MergeDotCoverSnapshotsInDirectory { get; set; }
        public static string StartDocker { get; set; }
        public static string TestsResultsPath { get => testsResultsPath; set => testsResultsPath = value; }
        public static string VSTestPath { get => vSTestPath; set => vSTestPath = value; }
        public static string MSTestPath { get => mSTestPath; set => mSTestPath = value; }

        private static string testsPath = Environment.CurrentDirectory;
        private static string testsResultsPath = TestsPath + "\\TestResults";
        private static string vSTestPath = "C:\\Program Files (x86)\\Microsoft Visual Studio\\2017\\Enterprise\\Common7\\IDE\\CommonExtensions\\Microsoft\\TestWindow\\vstest.console.exe";
        private static string mSTestPath = "C:\\Program Files (x86)\\Microsoft Visual Studio\\2017\\Enterprise\\Common7\\IDE\\MSTest.exe";
        private static string ServerExeName;
        private static string StudioExeName;
        private static List<string> ServerPathSpecs;
        private static List<string> StudioPathSpecs;
        private static bool ApplyDotCover;

        private static Dictionary<string, Tuple<string, string>> JobSpecs;

        public static void Main(string[] args)
        {
            using (WindowsIdentity identity = WindowsIdentity.GetCurrent())
            {
                WindowsPrincipal principal = new WindowsPrincipal(identity);
                if (!principal.IsInRole(WindowsBuiltInRole.Administrator))
                {
                    throw new UnauthorizedAccessException("Must run as an administrator.");
                }
            }

            Options.PargeArgs(args);

            JobSpecs = Job_Definitions.GetJobDefinitions();
            
            ServerExeName = "Warewolf Server.exe";
            ServerPathSpecs = new List<string>
            {
                ServerExeName,
                "Server\\" + ServerExeName,
                "DebugServer\\" + ServerExeName,
                "ReleaseServer\\" + ServerExeName,
                "Dev2.Server\\bin\\Debug\\" + ServerExeName,
                "Bin\\Server\\" + ServerExeName,
                "Dev2.Server\\bin\\Release\\" + ServerExeName,
                "*Server.zip"
            };

            StudioExeName = "Warewolf Studio.exe";
            StudioPathSpecs = new List<string>
            {
                StudioExeName,
                "Studio\\" + StudioExeName,
                "DebugStudio\\" + StudioExeName,
                "ReleaseStudio\\" + StudioExeName,
                "Dev2.Studio\\bin\\Debug\\" + StudioExeName,
                "Bin\\Studio\\" + StudioExeName,
                "Dev2.Studio\\bin\\Release\\" + StudioExeName,
                "*Studio.zip"
            };

            if (JobName != null && JobName.Contains(" DotCover"))
            {
                ApplyDotCover = true;
                JobName = JobName.Replace(" DotCover", "");
            }
            else
            {
                ApplyDotCover = !string.IsNullOrEmpty(DotCoverPath);
            }

            if (!File.Exists(TestsResultsPath))
            {
                Directory.CreateDirectory(TestsResultsPath);
            }

            // Unpack jobs
            var JobNames = new List<string>();
            var JobAssemblySpecs = new List<string>();
            var JobCategories = new List<string>();
            if (!string.IsNullOrEmpty(JobName) && string.IsNullOrEmpty(MergeDotCoverSnapshotsInDirectory) && string.IsNullOrEmpty(Cleanup))
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
                        Console.WriteLine("Unrecognized Job " + Job + " was ignored from the run");
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
            var TotalNumberOfJobsToRun = JobNames.Count;
            if (TotalNumberOfJobsToRun > 0)
            {
                if (!string.IsNullOrEmpty(VSTestPath) && !File.Exists(VSTestPath))
                {
                    if (File.Exists(VSTestPath.Replace("Enterprise", "Professional")))
                    {
                        VSTestPath = VSTestPath.Replace("Enterprise", "Professional");
                    }
                    if (File.Exists(VSTestPath.Replace("Enterprise", "Community")))
                    {
                        VSTestPath = VSTestPath.Replace("Enterprise", "Community");
                    }
                }
                if (!string.IsNullOrEmpty(MSTestPath) && !(File.Exists(MSTestPath)))
                {
                    if (File.Exists(MSTestPath.Replace("Enterprise", "Professional")))
                    {
                        MSTestPath = MSTestPath.Replace("Enterprise", "Professional");
                    }
                    if (File.Exists(MSTestPath.Replace("Enterprise", "Community")))
                    {
                        MSTestPath = MSTestPath.Replace("Enterprise", "Community");
                    }
                }
                if (!File.Exists(VSTestPath) && !(File.Exists(MSTestPath)))
                {
                    throw new Exception("Error cannot find VSTest.console.exe or MSTest.exe. Use either -VSTestPath or -MSTestPath parameters to pass paths to one of those files.");
                }

                if (ApplyDotCover && DotCoverPath != "" && !(File.Exists(DotCoverPath)))
                {
                    throw new Exception("Error cannot find dotcover.exe. Use -DotCoverPath parameter to pass a path to that file.");
                }

                if (File.Exists(Environment.ExpandEnvironmentVariables("%vs140comntools%..\\IDE\\CommonExtensions\\Microsoft\\TestWindow\\TestResults\\*.trx")))
                {
                    File.Move(Environment.ExpandEnvironmentVariables("%vs140comntools%..\\IDE\\CommonExtensions\\Microsoft\\TestWindow\\TestResults\\*.trx"), TestsResultsPath);
                    Console.WriteLine("Removed loose TRX files from VS install directory.");
                }

                if (!string.IsNullOrEmpty(DoServerStart) || !string.IsNullOrEmpty(DoStudioStart))
                {
                    InstallServer();
                }

                if (!string.IsNullOrEmpty(MSTest))
                {
                    // Read playlists and args.
                    if (string.IsNullOrEmpty(TestList))
                    {
                        foreach (var playlistFile in Directory.GetFiles(TestsPath, "*.playlist"))
                        {
                            XmlDocument playlistContent = new XmlDocument();
                            playlistContent.Load(playlistFile);
                            if (playlistContent.DocumentElement.SelectNodes("/Playlist/Add").Count > 0)
                            {
                                foreach (XmlNode TestName in playlistContent.DocumentElement.SelectNodes("/Playlist/Add"))
                                {
                                    TestList += "," + TestName.Attributes["Test"].InnerText.Substring(TestName.Attributes["Test"].InnerText.LastIndexOf(".") + 1);
                                }
                            }
                            else
                            {
                                if (playlistContent.SelectSingleNode("/Playlist/Add") != null && playlistContent.SelectSingleNode("/Playlist/Add").Attributes["Test"] != null)
                                {
                                    TestList = " /Tests:" + playlistContent.SelectSingleNode("/Playlist/Add").Attributes["Test"].InnerText.Substring(playlistContent.SelectSingleNode("/Playlist/Add").Attributes["Test"].InnerText.LastIndexOf(".") + 1);
                                }
                                else
                                {
                                    Console.WriteLine("Error parsing Playlist.Add from playlist file at " + playlistFile);
                                }
                            }
                        }
                        if (TestList.StartsWith(","))
                        {
                            TestList = TestList.Replace("^.", " /Tests:");
                        }
                    }
                }
                else
                {
                    if (string.IsNullOrEmpty(TestList))
                    {
                        foreach (var playlistFile in Directory.GetFiles(TestsPath, "*.playlist"))
                        {
                            XmlDocument playlistContent = new XmlDocument();
                            playlistContent.Load(playlistFile);
                            if (playlistContent.DocumentElement.SelectNodes("/Playlist/Add").Count > 0)
                            {
                                foreach (XmlNode TestName in playlistContent.DocumentElement.SelectNodes("/Playlist/Add"))
                                {
                                    TestList += " /test:" + TestName.Attributes["Test"].InnerText.Substring(TestName.Attributes["Test"].InnerText.LastIndexOf(".") + 1);
                                }
                            }
                            else
                            {
                                if (playlistContent.SelectSingleNode("/Playlist/Add").Attributes["Test"] != null)
                                {
                                    TestList = " /test:" + playlistContent.SelectSingleNode("/Playlist/Add").Attributes["Test"].InnerText.Substring(playlistContent.SelectSingleNode("/Playlist/Add").Attributes["Test"].InnerText.LastIndexOf(".") + 1);
                                }
                                else
                                {
                                    Console.WriteLine("Error parsing Playlist.Add from playlist file at " + playlistFile);
                                }
                            }
                        }
                    }
                }
                for (var i = 0; i < TotalNumberOfJobsToRun; i++)
                {
                    var JobName = JobNames[i].ToString();
                    var ProjectSpec = JobAssemblySpecs[i].ToString();
                    var TestCategories = JobCategories[i].ToString();
                    var TestAssembliesList = "";
                    var TestAssembliesDirectories = new List<string>();
                    if (!TestsPath.EndsWith("\\"))
                    {
                        TestsPath += "\\";
                    }
                    foreach (var Project in ProjectSpec.Split(','))
                    {
                        Tuple<string, List<string>> UnPackTestAssembliesListAndDirectories = ResolveTestAssemblyFileSpecs(TestsPath + Project + ".dll");
                        TestAssembliesList += UnPackTestAssembliesListAndDirectories.Item1;
                        if (UnPackTestAssembliesListAndDirectories.Item2.Count > 0)
                        {
                            TestAssembliesDirectories = TestAssembliesDirectories.Concat(UnPackTestAssembliesListAndDirectories.Item2).ToList();
                        }
                        if (TestAssembliesList == "")
                        {
                            UnPackTestAssembliesListAndDirectories = ResolveProjectFolderSpecs(TestsPath + Project);
                            TestAssembliesList += UnPackTestAssembliesListAndDirectories.Item1;
                            if (UnPackTestAssembliesListAndDirectories.Item2.Count > 0)
                            {
                                TestAssembliesDirectories = TestAssembliesDirectories.Concat(UnPackTestAssembliesListAndDirectories.Item2).ToList();
                            }
                        }
                    }
                    if (string.IsNullOrEmpty(TestAssembliesList) || string.IsNullOrEmpty(TestAssembliesList))
                    {
                        throw new Exception("Cannot find any " + ProjectSpec + " project folders or assemblies at " + TestsPath + ".");
                    }

                    // Setup for screen recording
                    var TestSettingsFile = "";
                    var TestSettings = "";
                    if (RecordScreen != null)
                    {
                        var TestSettingsId = Guid.NewGuid();

                        // Create test settings.
                        TestSettingsFile = TestsResultsPath + "\\" + JobName + ".testsettings";
                        CopyOnWrite(TestSettingsFile);
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

                    string TestRunnerPath;
                    if (string.IsNullOrEmpty(MSTest))
                    {
                        // Resolve test results file name
                        Environment.CurrentDirectory = TestsResultsPath + "\\..";

                        // Create full VSTest argument string.
                        if (string.IsNullOrEmpty(TestList))
                        {
                            if (!string.IsNullOrEmpty(TestCategories))
                            {
                                TestCategories = " /TestCaseFilter:\"(TestCategory=" + TestCategories + ")\"";
                            }
                            else
                            {
                                var DefinedCategories = AllCategoriesDefinedForProject(ProjectSpec);
                                if (DefinedCategories.Count() > 0)
                                {
                                    TestCategories = String.Join(")&(TestCategory!=", DefinedCategories);
                                    TestCategories = " /TestCaseFilter:\"(TestCategory!=" + TestCategories + ")\"";
                                }
                            }
                        }
                        else
                        {
                            TestCategories = "";
                            if (!TestList.StartsWith(" /Tests:"))
                            {
                                TestList = " /Tests:" + TestList;
                            }
                        }
                        if (RecordScreen != null)
                        {
                            TestSettings = " /Settings:\"" + TestSettingsFile + "\"";
                        }
                        else
                        {
                            TestSettings = "";
                        }

                        var FullArgsList = TestAssembliesList + " /logger:trx" + TestList + TestSettings + TestCategories;

                        // Write full command including full argument string.
                        TestRunnerPath = TestsResultsPath + "\\..\\Run " + JobName + ".bat";
                        CopyOnWrite("TestRunnerPath");
                        File.WriteAllText(TestRunnerPath, "\"" + VSTestPath + "\"" + FullArgsList);
                    }
                    else
                    {
                        // Resolve test results file name
                        var TestResultsFile = TestsResultsPath + "\"" + JobName + " Results.trx";
                        CopyOnWrite(TestResultsFile);

                        if (RecordScreen != null)
                        {
                            TestSettings = " /Settings:\"" + TestSettingsFile + "\"";
                        }
                        else
                        {
                            TestSettings = "";
                        }

                        // Create full MSTest argument string.
                        if (String.IsNullOrEmpty(TestList))
                        {
                            if (!String.IsNullOrEmpty(TestCategories))
                            {
                                TestCategories = " /category:\"" + TestCategories + "\"";
                            }
                            else
                            {
                                var DefinedCategories = AllCategoriesDefinedForProject(ProjectSpec);
                                if (DefinedCategories.Any())
                                {
                                    TestCategories = string.Join("&!", DefinedCategories);
                                    TestCategories = " /category:\"!" + TestCategories + "\"";
                                }
                            }
                        }
                        else
                        {
                            TestCategories = "";
                            if (!(TestList.StartsWith(" /test:")))
                            {
                                var TestNames = string.Join(" /test:", TestList.Split(','));
                                TestList = " /test:" + TestNames;
                            }
                        }
                        var FullArgsList = TestAssembliesList + " /resultsfile:\"" + TestResultsFile + "\"" + TestList + TestSettings + TestCategories;

                        // Write full command including full argument string.
                        TestRunnerPath = TestsResultsPath + "\\..\\Run " + JobName + ".bat";
                        CopyOnWrite("TestRunnerPath");
                        File.WriteAllText(TestRunnerPath, "\"" + MSTestPath + "\"" + FullArgsList);
                    }
                    TestRunnerPath = TestsResultsPath + "\\..\\Run " + JobName + ".bat";
                    if (File.Exists(TestRunnerPath))
                    {
                        if (!string.IsNullOrEmpty(DoServerStart) || !string.IsNullOrEmpty(DoStudioStart) || !string.IsNullOrEmpty(DomywarewolfioStart))
                        {
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
                            // Write DotCover Runner XML 
                            var DotCoverSnapshotFile = TestsResultsPath + "\\JobName DotCover Output.dcvr";
                            CopyOnWrite(DotCoverSnapshotFile);
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
                            var DotCoverRunnerXMLPath = TestsResultsPath + "\\JobName DotCover Runner.xml";
                            CopyOnWrite(DotCoverRunnerXMLPath);
                            File.WriteAllText(DotCoverRunnerXMLPath, DotCoverArgs);

                            // Create full DotCover argument string.
                            var DotCoverLogFile = TestsResultsPath + "\\DotCover.xml.log";
                            CopyOnWrite(DotCoverLogFile);
                            var FullArgsList = " cover \"" + DotCoverRunnerXMLPath + "\" /LogFile=\"" + DotCoverLogFile + "\"";

                            // Write DotCover Runner Batch File
                            var DotCoverRunnerPath = TestsResultsPath + "\\Run JobName DotCover.bat";
                            CopyOnWrite(DotCoverRunnerPath);
                            File.WriteAllText(DotCoverRunnerPath, "\"" + DotCoverPath + "\"" + FullArgsList);

                            // Run DotCover Runner Batch File
                            Process.Start(DotCoverRunnerPath).WaitForExit();
                            if (!string.IsNullOrEmpty(DoServerStart) || !string.IsNullOrEmpty(DoStudioStart) || !string.IsNullOrEmpty(DomywarewolfioStart))
                            {
                                CleanupServerStudio(false);
                            }
                        }
                        else
                        {
                            Process.Start(TestRunnerPath).WaitForExit();
                            if (!string.IsNullOrEmpty(DoServerStart) || !string.IsNullOrEmpty(DoStudioStart) || !string.IsNullOrEmpty(DomywarewolfioStart))
                            {
                                CleanupServerStudio(!ApplyDotCover);
                            }
                        }
                        MoveArtifactsToTestResults(ApplyDotCover, (!string.IsNullOrEmpty(DoServerStart) || !string.IsNullOrEmpty(DoStudioStart)), !string.IsNullOrEmpty(DoStudioStart));
                    }
                }
                if (ApplyDotCover && TotalNumberOfJobsToRun > 1)
                {
                    MergeDotCoverSnapshotsInDirectory = "true";
                    Main(null);
                }
            }

            if (!string.IsNullOrEmpty(AssemblyFileVersionsTest))
            {
                Console.WriteLine("Testing Warewolf assembly file versions...");
                var HighestReadVersion = "0.0.0.0";
                var LastReadVersion = "0.0.0.0";
                foreach (var file in Directory.GetFiles(TestsPath, "*", SearchOption.AllDirectories))
                {
                    if ((file.EndsWith(".dll") || (file.EndsWith(".exe") && !file.EndsWith(".vshost.exe"))) && (file.StartsWith("Dev2.") || file.StartsWith("Warewolf.") || file.StartsWith("WareWolf")))
                    {
                        // Get version.
                        var ReadVersion = FileVersionInfo.GetVersionInfo(file).FileVersion;

                        // Find highest version
                        var SeperateVersionNumbers = ReadVersion.Split('.');

                        var SeperateVersionNumbersHighest = HighestReadVersion.Split('.');
                        if (Convert.ToInt32(SeperateVersionNumbers[0], 10) > Convert.ToInt32(SeperateVersionNumbersHighest[0], 10) || Convert.ToInt32(SeperateVersionNumbers[1], 10) > Convert.ToInt32(SeperateVersionNumbersHighest[1], 10) || Convert.ToInt32(SeperateVersionNumbers[2], 10) > Convert.ToInt32(SeperateVersionNumbersHighest[2], 10) || Convert.ToInt32(SeperateVersionNumbers[3], 10) > Convert.ToInt32(SeperateVersionNumbersHighest[3], 10))
                        {
                            HighestReadVersion = ReadVersion;
                        }

                        // Check for invalid.
                        if (ReadVersion.StartsWith("0.0.") || (LastReadVersion != ReadVersion && LastReadVersion != "0.0.0.0"))
                        {
                            throw new Exception("ERROR! \"" + file + " " + ReadVersion + "\" is either an invalid version or not equal to \"" + LastReadVersion + "\". All Warewolf assembly versions in \"" + TestsPath + "\" must conform and cannot start with 0.0. or end with .0");
                        }
                        LastReadVersion = ReadVersion;
                    }
                }
                File.WriteAllText("FullVersionString", "FullVersionString=" + HighestReadVersion);
            }

            if (MergeDotCoverSnapshotsInDirectory != null)
            {
                var DotCoverSnapshots = Directory.GetFiles(MergeDotCoverSnapshotsInDirectory, "*.dcvr", SearchOption.AllDirectories).ToList();
                if (string.IsNullOrEmpty(JobName))
                {
                    JobName = "DotCover";
                }
                var MergedSnapshotFileName = JobName.Split(',')[0];
                MergedSnapshotFileName = "Merged " + MergedSnapshotFileName + " Snapshots";
                MergeDotCoverSnapshots(DotCoverSnapshots, MergeDotCoverSnapshotsInDirectory + "\\" + MergedSnapshotFileName, MergeDotCoverSnapshotsInDirectory + "\\DotCover");
            }

            if (!string.IsNullOrEmpty(Cleanup))
            {
                if (ApplyDotCover)
                {
                    CleanupServerStudio(false);
                }
                else
                {
                    CleanupServerStudio();
                }
                if (!string.IsNullOrEmpty(JobName))
                {
                    if (!string.IsNullOrEmpty(ProjectName))
                    {
                        JobName = ProjectName;
                    }
                    else
                    {
                        JobName = "Manual Tests";
                    }
                }
                MoveArtifactsToTestResults(ApplyDotCover, File.Exists(Environment.ExpandEnvironmentVariables("%ProgramData%\\Warewolf\\Server Log\\wareWolf-Server.log")), File.Exists(Environment.ExpandEnvironmentVariables("%LocalAppData\\Warewolf\\Studio Logs\\Warewolf Studio.log")));
            }

            if (string.IsNullOrEmpty(Cleanup) && string.IsNullOrEmpty(AssemblyFileVersionsTest) && string.IsNullOrEmpty(JobName) && string.IsNullOrEmpty(RunWarewolfServiceTests) && string.IsNullOrEmpty(MergeDotCoverSnapshotsInDirectory) && string.IsNullOrEmpty(StartDocker))
            {
                Startmywarewolfio();
                if (String.IsNullOrEmpty(DomywarewolfioStart))
                {
                    InstallServer();
                    StartServer();
                    if (String.IsNullOrEmpty(DoServerStart) && String.IsNullOrEmpty(DomywarewolfioStart))
                    {
                        StartStudio();
                    }
                }
            }
        }

        static string FindFileInParent(List<string> FileSpecs, int NumberOfParentsToSearch=7)
        {
            var NumberOfParentsSearched = -1;
            var FilePath = "";
            var CurrentDirectory = TestsPath;
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
                            CurrentDirectory = TestsPath;
                        }
                    }
	        	    if (File.Exists(CurrentDirectory + "\\" + FileSpec) || Directory.Exists($"{CurrentDirectory}\\{FileSpec}"))
                    {
                        FilePath = Path.Combine(CurrentDirectory, FileSpec);
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

        static void CopyOnWrite(string FileSpec)
        {
            if (File.Exists(FileSpec))
            {
                var num = 1;
                var FileExtention = Path.GetExtension(FileSpec);
                var FileSpecWithoutExtention = FileSpec.Substring(0, FileSpec.LastIndexOf('.') + 1);
                while(File.Exists(FileSpecWithoutExtention + num + FileExtention))
                {
                    num++;
                }
                File.Move(FileSpec, FileSpecWithoutExtention + num + FileExtention);
            }
        }

        static void MoveFileToTestResults(string SourceFilePath, string DestinationFileName)
        {
            var DestinationFilePath = Path.Combine(TestsResultsPath, DestinationFileName);
            if (File.Exists(SourceFilePath))
            {
                CopyOnWrite(DestinationFilePath);
                Console.WriteLine("Moving \"SourceFilePath\" to \"DestinationFilePath\"");
                File.Move(SourceFilePath, DestinationFilePath);
            }
        }

        static void CleanupServerStudio(bool Force=true)
        {
            int WaitForCloseTimeout = Force ? 10 : 1800;
            int WaitForCloseRetryCount = Force ? 1 : 10;
            //Stop Studio
            Process process = StartProcess("taskkill", "/im \"Warewolf Studio.exe\"");
            var Output = process.StandardOutput.ReadToEnd() + process.StandardError.ReadToEnd();

            //Soft Kill
            int i = 0;
            string WaitTimeoutMessage = "This command stopped operation because process ";
            string WaitOutput = WaitTimeoutMessage;
            while (!(Output.StartsWith("ERROR: ")) && WaitOutput.StartsWith(WaitTimeoutMessage) && i < WaitForCloseRetryCount)
            {
                i++;
                Console.WriteLine(Output);
                Process.GetProcessesByName("Warewolf Studio")[0].WaitForExit(WaitForCloseTimeout);
                var FormatWaitForCloseTimeoutMessage = WaitOutput.Replace(WaitTimeoutMessage, "");
                if (FormatWaitForCloseTimeoutMessage != "" && !(FormatWaitForCloseTimeoutMessage.StartsWith("Cannot find a process with the name ")))
                {
                    Console.WriteLine(FormatWaitForCloseTimeoutMessage);
                }
                process.Start();
                process.WaitForExit();
                Output = process.StandardOutput.ReadToEnd() + process.StandardError.ReadToEnd();
            }

            //Force Kill
            process.StartInfo.Arguments = "/im \"Warewolf Studio.exe\" /f";
            process.Start();
            process.WaitForExit();
            Output = process.StandardOutput.ReadToEnd() + process.StandardError.ReadToEnd();
            if (!(Output.StartsWith("ERROR: ")))
            {
                Console.WriteLine(Output);
            }

            //Stop my.warewolf.io
            process.StartInfo.Arguments = "/im iisexpress.exe /f";
            process.Start();
            process.WaitForExit();
            Output = process.StandardOutput.ReadToEnd() + process.StandardError.ReadToEnd();
            if (!(Output.StartsWith("ERROR: ")))
            {
                Console.WriteLine(Output);
            }

            //Stop Server
            var stopServerService = new Process();
            stopServerService.StartInfo.UseShellExecute = false;
            stopServerService.StartInfo.RedirectStandardOutput = true;
            stopServerService.StartInfo.RedirectStandardError = true;
            stopServerService.StartInfo.FileName = "sc.exe";
            stopServerService.StartInfo.Arguments = "stop \"Warewolf Server\"";
            stopServerService.Start();
            stopServerService.WaitForExit();
            var ServiceOutput = stopServerService.StandardOutput.ReadToEnd() + stopServerService.StandardError.ReadToEnd();
            if (ServiceOutput != "[SC] ControlService FAILED 1062:\r\n\r\nThe service has not been started.\r\n\r\n")
            {
                Console.WriteLine(ServiceOutput.TrimStart('\n'));
                Process.GetProcessesByName("Warewolf Server")[0].WaitForExit(WaitForCloseTimeout);
            }
            process.StartInfo.Arguments = "/im \"Warewolf Server.exe\" /f";
            process.Start();
            process.StartInfo.Arguments = "/im \"operadriver.exe\" /f";
            process.Start();
            process.StartInfo.Arguments = "/im \"geckodriver.exe\" /f";
            process.Start();
            process.StartInfo.Arguments = "/im \"IEDriverServer.exe\" /f";
            process.Start();

            //Delete Certain Studio and Server Resources
            var ToClean = new[]
            {
                "LOCALAPPDATA%\\Warewolf\\DebugData\\PersistSettings.dat",
                "LOCALAPPDATA%\\Warewolf\\UserInterfaceLayouts\\WorkspaceLayout.xml",
                "PROGRAMDATA%\\Warewolf\\Workspaces",
                "PROGRAMDATA%\\Warewolf\\Server Settings",
                "PROGRAMDATA%\\Warewolf\\VersionControl",
                Path.GetDirectoryName(ServerPath) + "\\ServerStarted"
            };

            foreach (var FileOrFolder in ToClean)
            {
                var ActualPath = Environment.ExpandEnvironmentVariables(FileOrFolder);
                if (File.Exists(ActualPath))
                {
                    File.Delete(ActualPath);
                }
                if (Directory.Exists(ActualPath))
                {
                    Directory.Delete(ActualPath);
                }
                if ((File.Exists(FileOrFolder) || Directory.Exists(FileOrFolder)))
                {
                    Console.Error.WriteLine("Cannot delete " + FileOrFolder);
                }
            }
            if (String.IsNullOrEmpty(JobName))
            {
                JobName = "Test Run";
            }

            MoveFileToTestResults(Environment.ExpandEnvironmentVariables("%PROGRAMDATA%\\Warewolf\\Resources"), "Server Resources JobName");
            MoveFileToTestResults(Environment.ExpandEnvironmentVariables("%PROGRAMDATA%\\Warewolf\\Tests"), "Server Service Tests JobName");
        }

        static bool WaitForFileUnlock(string FileSpec)
        {
            var locked = true;
            var RetryCount = 0;
            while(locked && RetryCount < 12) 
            {
                RetryCount++;
                try
                {
                    File.OpenWrite(FileSpec).Close();
                    locked = false;
                }
                catch
                {
                    Console.WriteLine("Still waiting for " + FileSpec + " file to unlock.");
                    Thread.Sleep(10000);
                }
            }
            return locked;
        }

        static bool WaitForFileExist(string FileSpec)
        {
            var exists = false;
            var RetryCount = 0;
            while(!exists && RetryCount < 12)
            {
                RetryCount++;
                if (File.Exists(FileSpec))
                {
                    exists = true;
                }
                else
                {
                    Console.WriteLine("Still waiting for " + FileSpec + " file to exist.");
                    Thread.Sleep(10000);
                }
            }
            return exists;
        }

        static void MergeDotCoverSnapshots(List<string> DotCoverSnapshots, string DestinationFilePath, string LogFilePath)
        {
            if (DotCoverSnapshots != null) {
                if (DotCoverSnapshots.Count > 1)
                {
                    var DotCoverSnapshotsString = String.Join("\";\"", DotCoverSnapshots);
                    CopyOnWrite(LogFilePath + ".merge.log");
                    CopyOnWrite(LogFilePath + ".report.log");
                    CopyOnWrite(DestinationFilePath + ".dcvr");
                    CopyOnWrite(DestinationFilePath + ".html");
                    Process.Start(DotCoverPath, "merge /Source=\"" + DotCoverSnapshotsString + "\" /Output=\"" + DestinationFilePath + ".dcvr\" /LogFile=\"" + LogFilePath + ".merge.log\"");
                }
                if (DotCoverSnapshots.Count == 1)
                {
                    var LoneSnapshot = DotCoverSnapshots[0];
                    if (DotCoverSnapshots.Count == 1 && (File.Exists(LoneSnapshot)))
                    {
                        Process.Start(DotCoverPath, "report /Source=\"" + LoneSnapshot + "\" /Output=\"" + DestinationFilePath + "\\DotCover Report.html\" /ReportType=HTML /LogFile=\"" + LogFilePath + ".report.log\"");
                        Console.WriteLine("DotCover report written to " + DestinationFilePath + "\\DotCover Report.html");
                    }
                }
            }
            if (File.Exists(DestinationFilePath + ".dcvr"))
            {
                Process.Start(DotCoverPath, "report /Source=\"" + DestinationFilePath + ".dcvr\" /Output=\"" + DestinationFilePath + "\\DotCover Report.html\" /ReportType=HTML /LogFile=\"" + LogFilePath + ".report.log\"");
                Console.WriteLine("DotCover report written to " + DestinationFilePath + "\\DotCover Report.html");
            }
        }

        static void MoveArtifactsToTestResults(bool DotCover, bool Server, bool Studio)
        {
            if (File.Exists("C:\\Program Files (x86)\\Microsoft Visual Studio\\2017\\Enterprise\\Common7\\IDE\\CommonExtensions\\Microsoft\\TestWindow\\TestResults\\*.trx"))
            {
                File.Move("C:\\Program Files (x86)\\Microsoft Visual Studio\\2017\\Enterprise\\Common7\\IDE\\CommonExtensions\\Microsoft\\TestWindow\\TestResults\\*.trx", TestsResultsPath);
                Console.WriteLine("Moved loose TRX files from VS install directory into TestResults.");
            }

            if (Cleanup != null)
            {
                //Write failing tests playlist.
                Console.WriteLine("Writing all test failures in \"" + TestsResultsPath + "\" to a playlist file");

                var PlayList = "<Playlist Version=\"1.0\">";
                foreach (var FullTRXFilePath in Directory.GetFiles(TestsResultsPath, "*.trx"))
                {
                    XmlDocument trxContent = new XmlDocument();
                    trxContent.Load(FullTRXFilePath);
                    if (trxContent.DocumentElement.SelectNodes("/TestRun/Results/UnitTestResult").Count > 0) {
                        foreach (XmlNode TestResult in trxContent.DocumentElement.SelectNodes("/TestRun/Results/UnitTestResult"))
                        {
                            if (TestResult.Attributes["outcome"].InnerText == "Failed")
                            {
                                if (trxContent.DocumentElement.SelectNodes("/TestRun/TestDefinitions/UnitTest/TestMethod").Count > 0)
                                {
                                    foreach (XmlNode TestDefinition in trxContent.DocumentElement.SelectNodes("/TestRun/TestDefinitions/UnitTest/TestMethod"))
                                    {
                                        if (TestDefinition.Name == TestResult.Attributes["TestName"].InnerText)
                                        {
                                            PlayList += "<Add Test=\"" + TestDefinition.Attributes["ClassName"] + "." + TestDefinition.Name + "\" />";
                                        }
                                    }
                                }
                                else
                                {
                                    Console.WriteLine("Error parsing /TestRun/TestDefinitions/UnitTest/TestMethod from trx file at trxFile");
                                    continue;
                                }
                            }
                        }
                    }
                    else
                    {
                        if (trxContent.DocumentElement.SelectSingleNode("/TestRun/Results/UnitTestResult").Attributes["outcome"].InnerText == "Failed") 
                        {
                            PlayList += "<Add Test=\"" + trxContent.DocumentElement.SelectSingleNode("/TestRun/TestDefinitions/UnitTest/TestMethod").Attributes["className"].InnerText + "." + trxContent.DocumentElement.SelectSingleNode("/TestRun/TestDefinitions/UnitTest/TestMethod").Name + "\" />";
                        } 
                        else
                        {
                            if (trxContent.DocumentElement.SelectSingleNode("/TestRun/Results/UnitTestResult") == null) 
                            {
                                Console.WriteLine("Error parsing /TestRun/Results/UnitTestResult from trx file at " + FullTRXFilePath);
                            }
                        }
                    }
                }
                PlayList += "</Playlist>";
                var OutPlaylistPath = $"{TestsResultsPath}\\{JobName} Failures.playlist";
                CopyOnWrite(OutPlaylistPath);
                File.WriteAllText(OutPlaylistPath, PlayList);
                Console.WriteLine("Playlist file written to \"" + OutPlaylistPath + "\".");
            }

            if (Studio)
            {
                MoveFileToTestResults(Environment.ExpandEnvironmentVariables("%LocalAppData%\\Warewolf\\Studio Logs\\Warewolf Studio.log"), "JobName Studio.log");
            }
            if (Studio && DotCover)
            {
                var StudioSnapshot = Environment.ExpandEnvironmentVariables("%LocalAppData%\\Warewolf\\Studio Logs\\dotCover.dcvr");
                Console.WriteLine("Trying to move Studio coverage snapshot file from " + StudioSnapshot + " to " + TestsResultsPath + "\\" + JobName + " Studio DotCover.dcvr");
                var exists = WaitForFileExist(StudioSnapshot);
                if (exists)
                {
                    var locked = WaitForFileUnlock(StudioSnapshot);
                    if (!(locked))
                    {
                        Console.WriteLine("Moving Studio coverage snapshot file from StudioSnapshot to " + TestsResultsPath + "\\JobName Studio DotCover.dcvr");
                        CopyOnWrite($"{TestsResultsPath}\\{JobName} Studio DotCover.dcvr");
                        File.Move(StudioSnapshot, $"{TestsResultsPath}\\{JobName} Studio DotCover.dcvr");
                    }
                    else
                    {
                        Console.WriteLine("Studio Coverage Snapshot File is locked.");
                    }
                }
                else
                {
                    throw new FileNotFoundException("Studio coverage snapshot not found at " + StudioSnapshot);
                }
                if (File.Exists(Environment.ExpandEnvironmentVariables("%LocalAppData%\\Warewolf\\Studio Logs\\dotCover.log")))
                {
                    MoveFileToTestResults(Environment.ExpandEnvironmentVariables("%LocalAppData%\\Warewolf\\Studio Logs\\dotCover.log"), "JobName Studio DotCover.log");
                }
            }
            if (Server)
            {
                MoveFileToTestResults(Environment.ExpandEnvironmentVariables("%ProgramData%\\Warewolf\\Server Log\\wareWolf-Server.log"), "JobName Server.log");
                MoveFileToTestResults(Environment.ExpandEnvironmentVariables("%ProgramData%\\Warewolf\\Server Log\\my.warewolf.io.log"), "JobName my.warewolf.io Server.log");
                MoveFileToTestResults(Environment.ExpandEnvironmentVariables("%ProgramData%\\Warewolf\\Server Log\\my.warewolf.io.errors.log"), "JobName my.warewolf.io Server Errors.log");
            }
            if (Server && DotCover)
            {
                var ServerSnapshot = Environment.ExpandEnvironmentVariables("%ProgramData%\\Warewolf\\Server Log\\dotCover.dcvr");
                Console.WriteLine("Trying to move Server coverage snapshot file from " + ServerSnapshot + " to " + TestsResultsPath + "\\" + JobName + " Server DotCover.dcvr");
                var exists = WaitForFileExist(ServerSnapshot);
                if (exists)
                {
                    var locked = WaitForFileUnlock(ServerSnapshot);
                    if (!locked)
                    {
                        Console.WriteLine("Moving Server coverage snapshot file from " + ServerSnapshot + " to " + TestsResultsPath + "\\" + JobName + " Server DotCover.dcvr");
                        MoveFileToTestResults(ServerSnapshot, "JobName Server DotCover.dcvr");
                    }
                    else
                    {
                        Console.WriteLine("Server Coverage Snapshot File still locked after retrying for 2 minutes.");
                    }
                }
                if (File.Exists(Environment.ExpandEnvironmentVariables("%ProgramData%\\Warewolf\\Server Log\\dotCover.log")))
                {
                    MoveFileToTestResults(Environment.ExpandEnvironmentVariables("%ProgramData%\\Warewolf\\Server Log\\dotCover.log"), "JobName Server DotCover.log");
                }
                if (File.Exists(Environment.ExpandEnvironmentVariables("%ProgramData%\\Warewolf\\Server Log\\my.warewolf.io.log")))
                {
                    MoveFileToTestResults(Environment.ExpandEnvironmentVariables("%ProgramData%\\Warewolf\\Server Log\\my.warewolf.io.log"), "JobName my.warewolf.io.log");
                }
                if (File.Exists(Environment.ExpandEnvironmentVariables("%ProgramData%\\Warewolf\\Server Log\\my.warewolf.io.errors.log")))
                {
                    MoveFileToTestResults(Environment.ExpandEnvironmentVariables("%ProgramData%\\Warewolf\\Server Log\\my.warewolf.io.errors.log"), "JobName my.warewolf.io Errors.log");
                }
            }
            if (Server && Studio && DotCover)
            {
                MergeDotCoverSnapshots(new List<string> { TestsResultsPath + "\\JobName Server DotCover.dcvr", TestsResultsPath + "\\JobName Studio DotCover.dcvr" }, TestsResultsPath + "\\JobName Merged Server and Studio DotCover", TestsResultsPath + "\\ServerAndStudioDotCoverSnapshot");
            }
            if (RecordScreen != null)
            {
                MoveScreenRecordingsToTestResults();
            }
            if (File.Exists(TestsResultsPath + "\\..\\Run *.bat"))
            {
                foreach (var testRunner in (Directory.GetFiles(TestsResultsPath + "\\..\\Run *.bat")))
                {
                    MoveFileToTestResults(testRunner, Path.GetFileName(testRunner));
                }
            }
        }

        static void MoveScreenRecordingsToTestResults()
        {
            Console.WriteLine("Getting UI test screen recordings from \"TestsResultsPath\"");
            var ScreenRecordingsFolder = TestsResultsPath + "\\ScreenRecordings";
            if (File.Exists(ScreenRecordingsFolder + "\\In"))
            {
                File.Move(ScreenRecordingsFolder + "\\In\\*", ScreenRecordingsFolder);
            }
            else
            {
                Console.WriteLine(ScreenRecordingsFolder + "\\In not found.");
            }
        }

        static string FindWarewolfServerExe()
        {
            var ServerPath = FindFileInParent(ServerPathSpecs);
            if (ServerPath.EndsWith(".zip"))
            {
                ZipFile.ExtractToDirectory(ServerPath, TestsResultsPath + "\\Server");
                ServerPath = TestsResultsPath + "\\Server\\" + ServerExeName;
	        }
            if (string.IsNullOrEmpty(ServerPath) || !(File.Exists(ServerPath)))
            {
                throw new FileNotFoundException("Cannot find Warewolf Server.exe. Please provide a path to that file as a commandline parameter like this: -ServerPath");
            }
            else
            {
                return ServerPath;
            }
        }

        static void InstallServer()
        {
            if (string.IsNullOrEmpty(ServerPath) || !(File.Exists(ServerPath)))
            {
                ServerPath = FindWarewolfServerExe();
            }
            Console.WriteLine("Will now stop any currently running Warewolf servers and studios. Resources will be backed up to " + TestsResultsPath + ".");
            if (string.IsNullOrEmpty(ResourcesType))
            {
                var title = "Server Resources";
                var message = "What type of resources would you like to install the server with?";
                var UITest = new Tuple<string,string>("UITests", "Use these resources for running UI Tests.");
                var ServerTest = new Tuple<string, string>("ServerTests", "Use these resources for running everything except unit tests and Coded UI tests.");
                var Release = new Tuple<string, string>("Release", "Use these resources for Warewolf releases.");
                var UILoad = new Tuple<string, string>("Load", "Use these resources for Studio UI Load Testing.");
                var options = new Tuple<string, string>[] { UITest, ServerTest, Release, UILoad };
                string optionStrings = "";
                foreach (var option in options)
                {
                    optionStrings += '\n' + option.Item1 + ": " + option.Item2;
                }
                Console.WriteLine(title + '\n' + message + optionStrings);
                ResourcesType = Console.ReadLine();
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

                if (string.IsNullOrEmpty(JobName))
                {
			        if (ProjectName != "")
                    {
                        JobName = ProjectName;
			        }
                    else
                    {
                        JobName = "Manual Tests";
			        }
                }
                var DotCoverRunnerXMLPath = TestsResultsPath + "\\Server DotCover Runner.xml";
                CopyOnWrite(DotCoverRunnerXMLPath);
                File.WriteAllText(DotCoverRunnerXMLPath, RunnerXML);
                var BinPathWithDotCover = "\\\"" + DotCoverPath + "\\\" cover \\\"" + DotCoverRunnerXMLPath + "\\\" /LogFile=\\\"" + TestsResultsPath + "\\ServerDotCover.log\\\"";
                if (!ServerService)
                {
                    Process.Start("sc.exe", "create \"Warewolf Server\" binPath= \"" + BinPathWithDotCover + "\" start= demand");
	            }
                else
                {
                    Console.WriteLine("Configuring service to " + BinPathWithDotCover);
                    Process.Start("sc.exe", "config \"Warewolf Server\" binPath= \"" + BinPathWithDotCover + "\"");
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

        static void StartServer()
        {
            var ServerFolderPath = Path.GetDirectoryName(ServerPath);
            Console.WriteLine("Deploying New resources from " + ServerFolderPath + "\\Resources - " + ResourcesType + "\\*");
            RecursiveFolderCopy(ServerFolderPath + "\\Resources - " + ResourcesType, Environment.ExpandEnvironmentVariables("%ProgramData%\\Warewolf"));
            try
            {
                ServiceController.GetServices().FirstOrDefault(serviceController => serviceController.ServiceName.Equals("Warewolf Server"))?.Start();
            }
            catch (InvalidOperationException e)
            {
                Console.WriteLine(e.Message);
            }

            var process = StartProcess("sc.exe", "interrogate \"Warewolf Server\"");
            var Output = process.StandardOutput.ReadToEnd() + process.StandardError.ReadToEnd();
            if (!(Output.EndsWith("RUNNING ")))
            {
                Console.WriteLine(Output);
                process.StartInfo.Arguments = "start \"Warewolf Server\"";
                process.Start();
                process.WaitForExit();
            }

            WaitForServerStart(ServerFolderPath);
        }

        static void WaitForServerStart(string ServerFolderPath)
        {
            var TimeoutCounter = 0;
            var ServerStartedFilePath = ServerFolderPath + "\\ServerStarted";
            while (!(File.Exists(ServerStartedFilePath)) && TimeoutCounter++ < 100)
            {
                Thread.Sleep(3000);
            }
            if (!(File.Exists(ServerStartedFilePath)))
            {
                throw new Exception("Server Cannot Start.");
            }
            else
            {
                Console.WriteLine("Server has started.");
            }
        }

        static void Startmywarewolfio()
        {
            var WebsPath = "";
            if (TestsPath.EndsWith("\\"))
            {
                WebsPath = TestsPath + "_PublishedWebsites\\Dev2.Web";
            }
            else
            {
                WebsPath = TestsPath + "\\_PublishedWebsites\\Dev2.Web";
            }
            Console.WriteLine("Starting my.warewolf.io from " + WebsPath);
            if (!(File.Exists( WebsPath)))
            {
                if (string.IsNullOrEmpty(ServerPath) || !File.Exists(ServerPath))
                {
                    ServerPath = FindWarewolfServerExe();
                }
                WebsPath = Path.GetDirectoryName(ServerPath) + "\\_PublishedWebsites\\Dev2.Web";
            }
            CleanupServerStudio();
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
                Console.WriteLine("my.warewolf.io cannot be hosted. Webs not found at " + TestsPath + "\\_PublishedWebsites\\Dev2.Web or at " + ServerPath + "\\_PublishedWebsites\\Dev2.Web");
            }
        }

        static void StartStudio()
        {
            if (string.IsNullOrEmpty(StudioPath) || !(File.Exists(StudioPath)))
            {
                StudioPath = FindFileInParent(StudioPathSpecs);
                if (StudioPath.EndsWith(".zip"))
                {
                    ZipFile.ExtractToDirectory(StudioPath, TestsResultsPath + "\\Studio");
                    StudioPath = $"{TestsResultsPath}\\Studio\\{StudioExeName}";
                }
                if (string.IsNullOrEmpty(ServerPath) || !(File.Exists( StudioPath)))
                {
                    throw new Exception("Studio path not found: " + StudioPath);
                }
            }
	        if (string.IsNullOrEmpty(StudioPath))
            {
                throw new FileNotFoundException("Cannot find Warewolf Studio. To run the studio provide a path to the Warewolf Studio exe file as a commandline parameter like this: -StudioPath");
	        }
            var StudioLogFile = Environment.ExpandEnvironmentVariables("%LocalAppData%\\Warewolf\\Studio Logs\\Warewolf Studio.log");
            CopyOnWrite(StudioLogFile);
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
                var DotCoverRunnerXMLPath = TestsResultsPath + "\\Studio DotCover Runner.xml";
                CopyOnWrite(DotCoverRunnerXMLPath);
                File.WriteAllText(DotCoverRunnerXMLPath, RunnerXML);
                Process.Start(DotCoverPath, "cover \"" + DotCoverRunnerXMLPath + "\" /LogFile=\"" + TestsResultsPath + "\\StudioDotCover.log\"");
            }
            var i = 0;
            while (!File.Exists(StudioLogFile) && i++ < 200)
            {
                Console.WriteLine("Waiting for Studio to start...");
                Thread.Sleep(3000);
            }
            if (File.Exists(StudioLogFile))
            {
                Console.WriteLine("Studio has started.");
            }
            else
            {
                throw new Exception("Warewolf studio failed to start within 10 minutes.");
            }
        }

        static bool AssemblyIsNotAlreadyDefinedWithoutWildcards(string AssemblyNameToCheck)
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

        static List<string> AllCategoriesDefinedForProject(string AssemblyNameToCheck)
        {
            var JobCategorySpecs = new List<string>();
            foreach (var Job in JobSpecs.Values)
            {
                if (!string.IsNullOrEmpty(Job.Item2) && AssemblyNameToCheck == Job.Item1)
                {
                    JobCategorySpecs.Add(Job.Item2);
                }
            }
            return JobCategorySpecs;
        }

        static Tuple<string, List<string>> ResolveProjectFolderSpecs(string ProjectFolderSpec)
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
                        if (File.Exists(VSTestPath) && string.IsNullOrEmpty(MSTest))
                        {
                            TestAssembliesList += " \"" + projectFolder + "\\bin\\Debug\\" + Path.GetFileName(projectFolder) + ".dll\"";
                        }
                        else
                        {
                            TestAssembliesList += " /testcontainer:\"" + projectFolder + "\\bin\\Debug\\" + Path.GetFileName(projectFolder) + ".dll\"";
                        }
                        if (!TestAssembliesDirectories.Contains(projectFolder + "\\bin\\Debug"))
                        {
                            TestAssembliesDirectories.Add(projectFolder + "\\bin\\Debug");
                        }
                    }
                }
                else
                {
                    if (File.Exists(VSTestPath) && string.IsNullOrEmpty(MSTest))
                    {
		                TestAssembliesList += " \"" + ProjectFolderSpecInParent + "\\bin\\Debug\\" + Path.GetFileName(ProjectFolderSpecInParent) + ".dll\"";
                    }
                    else
                    {
		                TestAssembliesList += " /testcontainer:\"" + ProjectFolderSpecInParent + "\\bin\\Debug\\" + Path.GetFileName(ProjectFolderSpecInParent) + ".dll\"";
                    }
                    if (!TestAssembliesDirectories.Contains(ProjectFolderSpecInParent + "\\bin\\Debug"))
                    {
                        TestAssembliesDirectories.Add(ProjectFolderSpecInParent + "\\bin\\Debug");
                    }
                }
                return new Tuple<string, List<string>>(TestAssembliesList, TestAssembliesDirectories);
            }
            throw new Exception("Cannot resolve spec: " + ProjectFolderSpec);
        }

        static Tuple<string, List<string>> ResolveTestAssemblyFileSpecs(string TestAssemblyFileSpecs)
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
                        resolveStarNotation = Directory.GetFiles(TestAssembliesFileSpecsInParent).ToList();
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
                        if (string.IsNullOrEmpty(MSTest))
                        {
                            TestAssembliesList = TestAssembliesList + " \"" + file + "\"";
                        }
                        else
                        {
                            TestAssembliesList += " /testcontainer:\"" + file + "\"";
                        }
                        if (!TestAssembliesDirectories.Contains(Path.GetDirectoryName(file)))
                        {
                            TestAssembliesDirectories.Add(Path.GetDirectoryName(file));
                        }
	                }
                }
                return new Tuple<string,List<string>>(TestAssembliesList, TestAssembliesDirectories);
            }
            throw new Exception("Cannot resolve spec: " + TestAssemblyFileSpecs);
        }

        static void RecursiveFolderCopy(string sourceDir, string targetDir)
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

        private static Process StartProcess(string exeName, string args)
        {
            var process = new Process();
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.FileName = exeName;
            process.StartInfo.Arguments = args;
            process.Start();
            process.WaitForExit();
            return process;
        }
    }
}
