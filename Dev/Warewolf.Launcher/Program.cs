using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml;
using System.Security.Principal;
using System.Threading;
using System.Runtime.InteropServices;

namespace Warewolf.Launcher
{
    public class Program
    {
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll", EntryPoint = "FindWindow", SetLastError = true)]
        static extern IntPtr FindWindowByCaption(IntPtr zeroOnly, string lpWindowName);

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
            
            var build = Options.PargeArgs(args);

            build.JobSpecs = Job_Definitions.GetJobDefinitions();

            build.ServerExeName = "Warewolf Server.exe";
            build.ServerPathSpecs = new List<string>
            {
                build.ServerExeName,
                "Server\\" + build.ServerExeName,
                "DebugServer\\" + build.ServerExeName,
                "ReleaseServer\\" + build.ServerExeName,
                "Dev2.Server\\bin\\Debug\\" + build.ServerExeName,
                "Bin\\Server\\" + build.ServerExeName,
                "Dev2.Server\\bin\\Release\\" + build.ServerExeName,
                "*Server.zip"
            };

            build.StudioExeName = "Warewolf Studio.exe";
            build.StudioPathSpecs = new List<string>
            {
                build.StudioExeName,
                "Studio\\" + build.StudioExeName,
                "DebugStudio\\" + build.StudioExeName,
                "ReleaseStudio\\" + build.StudioExeName,
                "Dev2.Studio\\bin\\Debug\\" + build.StudioExeName,
                "Bin\\Studio\\" + build.StudioExeName,
                "Dev2.Studio\\bin\\Release\\" + build.StudioExeName,
                "*Studio.zip"
            };

            if (build.JobName != null && build.JobName.Contains(" DotCover"))
            {
                build.ApplyDotCover = true;
                build.JobName = build.JobName.Replace(" DotCover", "");
            }
            else
            {
                build.ApplyDotCover = !string.IsNullOrEmpty(build.DotCoverPath);
            }
            
            if (build.TestsPath != null && build.TestsPath.StartsWith(".."))
            {
                build.TestsPath = Path.Combine(Environment.CurrentDirectory, build.TestsPath);
            }
            
            if (build.TestsResultsPath != null && build.TestsResultsPath.StartsWith(".."))
            {
                build.TestsResultsPath = Path.Combine(Environment.CurrentDirectory, build.TestsResultsPath);
            }

            if (build.ServerPath != null && build.ServerPath.StartsWith(".."))
            {
                build.ServerPath = Path.Combine(Environment.CurrentDirectory, build.ServerPath);
            }

            if (build.StudioPath != null && build.StudioPath.StartsWith(".."))
            {
                build.StudioPath = Path.Combine(Environment.CurrentDirectory, build.StudioPath);
            }

            if (!File.Exists(build.TestsResultsPath))
            {
                Directory.CreateDirectory(build.TestsResultsPath);
            }
            if (!string.IsNullOrEmpty(build.JobName) && build.JobName.Split(',').Count() > 0)
            {
                RunTestJobs(build);
            }

            if (!string.IsNullOrEmpty(build.AssemblyFileVersionsTest))
            {
                Console.WriteLine("Testing Warewolf assembly file versions...");
                var HighestReadVersion = "0.0.0.0";
                var LastReadVersion = "0.0.0.0";
                foreach (var file in Directory.GetFiles(build.TestsPath, "*", SearchOption.AllDirectories))
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
                            throw new Exception("ERROR! \"" + file + " " + ReadVersion + "\" is either an invalid version or not equal to \"" + LastReadVersion + "\". All Warewolf assembly versions in \"" + build.TestsPath + "\" must conform and cannot start with 0.0. or end with .0");
                        }
                        LastReadVersion = ReadVersion;
                    }
                }
                File.WriteAllText("FullVersionString", "FullVersionString=" + HighestReadVersion);
            }

            if (build.MergeDotCoverSnapshotsInDirectory != null)
            {
                build.MergeDotCoverSnapshots();
            }

            if (!string.IsNullOrEmpty(build.Cleanup))
            {
                build.CleanupServerStudio(!build.ApplyDotCover);
                if (!string.IsNullOrEmpty(build.JobName))
                {
                    if (!string.IsNullOrEmpty(build.ProjectName))
                    {
                        build.JobName = build.ProjectName;
                    }
                    else
                    {
                        build.JobName = "Manual Tests";
                    }
                }
                build.MoveArtifactsToTestResults(build.ApplyDotCover, File.Exists(Environment.ExpandEnvironmentVariables("%ProgramData%\\Warewolf\\Server Log\\wareWolf-Server.log")), File.Exists(Environment.ExpandEnvironmentVariables("%LocalAppData\\Warewolf\\Studio Logs\\Warewolf Studio.log")));
            }

            if (string.IsNullOrEmpty(build.Cleanup) && string.IsNullOrEmpty(build.AssemblyFileVersionsTest) && string.IsNullOrEmpty(build.JobName) && string.IsNullOrEmpty(build.RunWarewolfServiceTests) && string.IsNullOrEmpty(build.MergeDotCoverSnapshotsInDirectory))
            {
                if (build.AdminMode)
                {
                    Console.WriteLine("\nAdmin, What type of tests do you want to run?");
                    var options = new[] {
                    "[1]All: Run All Tests. (This is the default)",
                    "[2]Unit Tests: Run All Unit Tests.",
                    "[3]Server Tests: Run everything except unit tests, release resource tests, load tests and Coded UI tests.",
                    "[4]Release: Run release resource tests.",
                    "[5]Desktop UI: Run All UI Tests. (This is the default)",
                    "[6]Web UI: Run Web UI Tests.",
                    "[7]UI Load: Run Load Testing."
                    };
                    foreach (var option in options)
                    {
                        Console.WriteLine();
                        var originalColour = Console.ForegroundColor;
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.Write(option.Substring(0, 3));
                        Console.ForegroundColor = originalColour;
                        Console.Write(option.Substring(3, option.Length - 3));
                    }
                    Console.WriteLine("\n\nOr Press Enter to use default (UITest)...");
                    string originalTitle = Console.Title;
                    string uniqueTitle = Guid.NewGuid().ToString();
                    Console.Title = uniqueTitle;
                    Thread.Sleep(50);
                    IntPtr handle = FindWindowByCaption(IntPtr.Zero, uniqueTitle);

                    if (handle != IntPtr.Zero)
                    {
                        Console.Title = originalTitle;
                        SetForegroundWindow(handle);
                    }

                    var testType = Console.ReadLine();

                    const int NumberOfUnitTestJobs = 34;
                    const int NumberOfServerTestJobs = 30;
                    const int NumberOfReleaseResourcesTestJobs = 1;
                    const int NumberOfDesktopUITestJobs = 64;
                    const int NumberOfWebUITestJobs = 3;
                    const int NumberOfLoadTestJobs = 3;
                    if (testType == "" || testType == "1")
                    {
                        RunAllUnitTestJobs(build, 0, NumberOfUnitTestJobs);
                        RunAllServerTestJobs(build, NumberOfUnitTestJobs, NumberOfServerTestJobs);
                        RunAllReleaseResourcesTestJobs(build, NumberOfUnitTestJobs + NumberOfServerTestJobs, NumberOfReleaseResourcesTestJobs);
                        RunAllDesktopUITestJobs(build, NumberOfUnitTestJobs + NumberOfServerTestJobs + NumberOfReleaseResourcesTestJobs, NumberOfDesktopUITestJobs);
                        RunAllWebUITestJobs(build, NumberOfUnitTestJobs + NumberOfServerTestJobs + NumberOfReleaseResourcesTestJobs + NumberOfDesktopUITestJobs, NumberOfWebUITestJobs);
                        RunAllLoadTestJobs(build, NumberOfUnitTestJobs + NumberOfServerTestJobs + NumberOfReleaseResourcesTestJobs + NumberOfDesktopUITestJobs + NumberOfWebUITestJobs, NumberOfLoadTestJobs);
                    }
                    if (testType == "2")
                    {
                        RunAllUnitTestJobs(build, 0, NumberOfUnitTestJobs);
                    }
                    if (testType == "3")
                    {
                        RunAllServerTestJobs(build, NumberOfUnitTestJobs, NumberOfServerTestJobs);
                    }
                    if (testType == "4")
                    {
                        RunAllReleaseResourcesTestJobs(build, NumberOfUnitTestJobs + NumberOfServerTestJobs, NumberOfReleaseResourcesTestJobs);
                    }
                    if (testType == "5")
                    {
                        RunAllDesktopUITestJobs(build, NumberOfUnitTestJobs + NumberOfServerTestJobs + NumberOfReleaseResourcesTestJobs, NumberOfDesktopUITestJobs);
                    }
                    if (testType == "6")
                    {
                        RunAllWebUITestJobs(build, NumberOfUnitTestJobs + NumberOfServerTestJobs + NumberOfReleaseResourcesTestJobs + NumberOfDesktopUITestJobs, NumberOfWebUITestJobs);
                    }
                    if (testType == "7")
                    {
                        RunAllLoadTestJobs(build, NumberOfUnitTestJobs + NumberOfServerTestJobs + NumberOfReleaseResourcesTestJobs + NumberOfDesktopUITestJobs + NumberOfWebUITestJobs, NumberOfLoadTestJobs);
                    }
                }
                else
                {
                    build.InstallServer();
                    build.CleanupServerStudio();
                    build.Startmywarewolfio();
                    build.TryStartLocalCIRemoteContainer();
                    if (String.IsNullOrEmpty(build.DomywarewolfioStart))
                    {
                        build.StartServer();
                        if (String.IsNullOrEmpty(build.DoServerStart) && String.IsNullOrEmpty(build.DomywarewolfioStart))
                        {
                            build.StartStudio();
                        }
                    }
                    Console.WriteLine("Press Enter to Shutdown.");
                    Console.ReadKey();
                    build.CleanupServerStudio();
                }
            }
        }

        private static void RunAllUnitTestJobs(TestLauncher build, int startIndex, int NumberOfUnitTestJobs)
        {
            build.JobName = string.Join(",", build.JobSpecs.Keys.ToList().GetRange(startIndex, NumberOfUnitTestJobs));
            build.CleanupServerStudio();
            RunTestJobs(build);
            build.CleanupServerStudio();
        }

        private static void RunAllServerTestJobs(TestLauncher build, int startIndex, int NumberOfServerTestJobs)
        {
            build.JobName = string.Join(",", build.JobSpecs.Keys.ToList().GetRange(startIndex, NumberOfServerTestJobs));
            build.ResourcesType = "ServerTests";
            build.DoServerStart = "true";
            RunTestJobs(build);
            build.CleanupServerStudio();
        }

        private static void RunAllReleaseResourcesTestJobs(TestLauncher build, int startIndex, int NumberOfReleaseResourcesTestJobs)
        {
            build.JobName = string.Join(",", build.JobSpecs.Keys.ToList().GetRange(startIndex, NumberOfReleaseResourcesTestJobs));
            build.ResourcesType = "Release";
            build.DoServerStart = "true";
            RunTestJobs(build);
            build.CleanupServerStudio();
        }

        private static void RunAllDesktopUITestJobs(TestLauncher build, int startIndex, int NumberOfDesktopUITestJobs)
        {
            build.JobName = string.Join(",", build.JobSpecs.Keys.ToList().GetRange(startIndex, NumberOfDesktopUITestJobs));
            build.ResourcesType = "UITests";
            build.DoStudioStart = "true";
            RunTestJobs(build);
            build.CleanupServerStudio();
        }

        private static void RunAllWebUITestJobs(TestLauncher build, int startIndex, int NumberOfWebUITestJobs)
        {
            build.JobName = string.Join(",", build.JobSpecs.Keys.ToList().GetRange(startIndex, NumberOfWebUITestJobs));
            build.DomywarewolfioStart = "true";
            RunTestJobs(build);
            build.CleanupServerStudio();
        }

        private static void RunAllLoadTestJobs(TestLauncher build, int startIndex, int NumberOfLoadTestJobs)
        {
            build.JobName = string.Join(",", build.JobSpecs.Keys.ToList().GetRange(startIndex, NumberOfLoadTestJobs));
            build.ResourcesType = "Load";
            build.DoStudioStart = "true";
            RunTestJobs(build);
            build.CleanupServerStudio();
        }

        static void RunTestJobs(TestLauncher build)
        {
            // Unpack jobs
            var JobNames = new List<string>();
            var JobAssemblySpecs = new List<string>();
            var JobCategories = new List<string>();
            if (!string.IsNullOrEmpty(build.JobName) && string.IsNullOrEmpty(build.MergeDotCoverSnapshotsInDirectory) && string.IsNullOrEmpty(build.Cleanup))
            {
                foreach (var Job in build.JobName.Split(','))
                {
                    var TrimJobName = Job.TrimEnd('1', '2', '3', '4', '5', '6', '7', '8', '9', '0', ' ');
                    if (build.JobSpecs.ContainsKey(TrimJobName))
                    {
                        JobNames.Add(TrimJobName);
                        if (build.JobSpecs[TrimJobName].Item2 == null)
                        {
                            JobAssemblySpecs.Add(build.JobSpecs[TrimJobName].Item1);
                            JobCategories.Add("");
                        }
                        else
                        {
                            JobAssemblySpecs.Add(build.JobSpecs[Job].Item1);
                            JobCategories.Add(build.JobSpecs[Job].Item2);
                        }
                    }
                    else
                    {
                        Console.WriteLine("Unrecognized Job " + Job + " was ignored from the run");
                    }
                }
            }
            if (!string.IsNullOrEmpty(build.ProjectName))
            {
                JobNames.Add(build.ProjectName);
                JobAssemblySpecs.Add(build.ProjectName);
                if (!string.IsNullOrEmpty(build.Category))
                {
                    JobCategories.Add(build.Category);
                }
                else
                {
                    JobCategories.Add("");
                }
            }
            if (!string.IsNullOrEmpty(build.VSTestPath) && !File.Exists(build.VSTestPath))
            {
                if (File.Exists(build.VSTestPath.Replace("Enterprise", "Professional")))
                {
                    build.VSTestPath = build.VSTestPath.Replace("Enterprise", "Professional");
                }
                if (File.Exists(build.VSTestPath.Replace("Enterprise", "Community")))
                {
                    build.VSTestPath = build.VSTestPath.Replace("Enterprise", "Community");
                }
            }
            if (!string.IsNullOrEmpty(build.MSTestPath) && !(File.Exists(build.MSTestPath)))
            {
                if (File.Exists(build.MSTestPath.Replace("Enterprise", "Professional")))
                {
                    build.MSTestPath = build.MSTestPath.Replace("Enterprise", "Professional");
                }
                if (File.Exists(build.MSTestPath.Replace("Enterprise", "Community")))
                {
                    build.MSTestPath = build.MSTestPath.Replace("Enterprise", "Community");
                }
            }
            if (!File.Exists(build.VSTestPath) && !(File.Exists(build.MSTestPath)))
            {
                throw new ArgumentException("Error cannot find VSTest.console.exe or MSTest.exe. Use either --VSTestPath or --MSTestPath parameters to pass paths to one of those files.");
            }

            if (build.ApplyDotCover && build.DotCoverPath != "" && !(File.Exists(build.DotCoverPath)))
            {
                throw new ArgumentException("Error cannot find dotcover.exe. Use -build.DotCoverPath parameter to pass a path to that file.");
            }

            if (File.Exists(Environment.ExpandEnvironmentVariables("%vs140comntools%..\\IDE\\CommonExtensions\\Microsoft\\TestWindow\\TestResults\\*.trx")))
            {
                File.Move(Environment.ExpandEnvironmentVariables("%vs140comntools%..\\IDE\\CommonExtensions\\Microsoft\\TestWindow\\TestResults\\*.trx"), build.TestsResultsPath);
                Console.WriteLine("Removed loose TRX files from VS install directory.");
            }

            if (!string.IsNullOrEmpty(build.DoServerStart) || !string.IsNullOrEmpty(build.DoStudioStart))
            {
                build.InstallServer();
            }

            if (!string.IsNullOrEmpty(build.MSTest))
            {
                // Read playlists and args.
                if (string.IsNullOrEmpty(build.TestList))
                {
                    foreach (var playlistFile in Directory.GetFiles(build.TestsPath, "*.playlist"))
                    {
                        XmlDocument playlistContent = new XmlDocument();
                        playlistContent.Load(playlistFile);
                        if (playlistContent.DocumentElement.SelectNodes("/Playlist/Add").Count > 0)
                        {
                            foreach (XmlNode TestName in playlistContent.DocumentElement.SelectNodes("/Playlist/Add"))
                            {
                                build.TestList += "," + TestName.Attributes["Test"].InnerText.Substring(TestName.Attributes["Test"].InnerText.LastIndexOf(".") + 1);
                            }
                        }
                        else
                        {
                            if (playlistContent.SelectSingleNode("/Playlist/Add") != null && playlistContent.SelectSingleNode("/Playlist/Add").Attributes["Test"] != null)
                            {
                                build.TestList = " /Tests:" + playlistContent.SelectSingleNode("/Playlist/Add").Attributes["Test"].InnerText.Substring(playlistContent.SelectSingleNode("/Playlist/Add").Attributes["Test"].InnerText.LastIndexOf(".") + 1);
                            }
                            else
                            {
                                Console.WriteLine("Error parsing Playlist.Add from playlist file at " + playlistFile);
                            }
                        }
                    }
                    if (build.TestList.StartsWith(","))
                    {
                        build.TestList = build.TestList.Replace("^.", " /Tests:");
                    }
                }
            }
            else
            {
                if (string.IsNullOrEmpty(build.TestList))
                {
                    foreach (var playlistFile in Directory.GetFiles(build.TestsPath, "*.playlist"))
                    {
                        XmlDocument playlistContent = new XmlDocument();
                        playlistContent.Load(playlistFile);
                        if (playlistContent.DocumentElement.SelectNodes("/Playlist/Add").Count > 0)
                        {
                            foreach (XmlNode TestName in playlistContent.DocumentElement.SelectNodes("/Playlist/Add"))
                            {
                                build.TestList += " /test:" + TestName.Attributes["Test"].InnerText.Substring(TestName.Attributes["Test"].InnerText.LastIndexOf(".") + 1);
                            }
                        }
                        else
                        {
                            if (playlistContent.SelectSingleNode("/Playlist/Add").Attributes["Test"] != null)
                            {
                                build.TestList = " /test:" + playlistContent.SelectSingleNode("/Playlist/Add").Attributes["Test"].InnerText.Substring(playlistContent.SelectSingleNode("/Playlist/Add").Attributes["Test"].InnerText.LastIndexOf(".") + 1);
                            }
                            else
                            {
                                Console.WriteLine("Error parsing Playlist.Add from playlist file at " + playlistFile);
                            }
                        }
                    }
                }
            }
            for (var i = 0; i < build.JobName.Split(',').Count(); i++)
            {
                var JobName = JobNames[i].ToString();
                var ProjectSpec = JobAssemblySpecs[i].ToString();
                var TestCategories = JobCategories[i].ToString();
                var TestAssembliesList = "";
                var TestAssembliesDirectories = new List<string>();
                if (!build.TestsPath.EndsWith("\\"))
                {
                    build.TestsPath += "\\";
                }
                foreach (var Project in ProjectSpec.Split(','))
                {
                    Tuple<string, List<string>> UnPackTestAssembliesListAndDirectories = build.ResolveTestAssemblyFileSpecs(build.TestsPath + Project + ".dll");
                    TestAssembliesList += UnPackTestAssembliesListAndDirectories.Item1;
                    if (UnPackTestAssembliesListAndDirectories.Item2.Count > 0)
                    {
                        TestAssembliesDirectories = TestAssembliesDirectories.Concat(UnPackTestAssembliesListAndDirectories.Item2).ToList();
                    }
                    if (TestAssembliesList == "")
                    {
                        UnPackTestAssembliesListAndDirectories = build.ResolveProjectFolderSpecs(build.TestsPath + Project);
                        TestAssembliesList += UnPackTestAssembliesListAndDirectories.Item1;
                        if (UnPackTestAssembliesListAndDirectories.Item2.Count > 0)
                        {
                            TestAssembliesDirectories = TestAssembliesDirectories.Concat(UnPackTestAssembliesListAndDirectories.Item2).ToList();
                        }
                    }
                }
                if (string.IsNullOrEmpty(TestAssembliesList) || string.IsNullOrEmpty(TestAssembliesList))
                {
                    throw new Exception("Cannot find any " + ProjectSpec + " project folders or assemblies at " + build.TestsPath + ".");
                }

                // Setup for screen recording
                var TestSettingsFile = build.ScreenRecordingTestSettingsFile(build, JobName);

                string TestRunnerPath;
                if (string.IsNullOrEmpty(build.MSTest))
                {
                    TestRunnerPath = build.VSTestRunner(JobName, ProjectSpec, TestCategories, TestAssembliesList, TestSettingsFile);
                }
                else
                {
                    TestRunnerPath = build.MSTestRunner(JobName, ProjectSpec, TestCategories, TestAssembliesList, TestSettingsFile, build.TestsResultsPath);
                }

                //Run Tests
                var TrxFile = build.RunTests(JobName, TestAssembliesList, TestAssembliesDirectories, TestSettingsFile, TestRunnerPath);

                //Re-try Failures
                for (var count = 0; count < build.RetryCount; count++)
                {
                    build.RetryTestFailures(JobName, TestAssembliesList, TestAssembliesDirectories, TestSettingsFile, TrxFile, count + 1);
                }
            }
            if (build.ApplyDotCover && build.JobName.Split(',').Count() > 1)
            {
                build.MergeDotCoverSnapshots();
            }
        }
    }
}
