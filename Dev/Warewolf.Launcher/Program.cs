using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml;
using System.Security.Principal;
using Warewolf.Launcher;

namespace Bashley
{
    public class Program
    {
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
            
            if (build.TestsPath.StartsWith(".."))
            {
                build.TestsPath = Path.Combine(Environment.CurrentDirectory, build.TestsPath);
            }


            if (build.TestsResultsPath.StartsWith(".."))
            {
                build.TestsResultsPath = Path.Combine(Environment.CurrentDirectory, build.TestsResultsPath);
            }

            if (!File.Exists(build.TestsResultsPath))
            {
                Directory.CreateDirectory(build.TestsResultsPath);
            }

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
            var TotalNumberOfJobsToRun = JobNames.Count;
            if (TotalNumberOfJobsToRun > 0)
            {
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
                    throw new Exception("Error cannot find VSTest.console.exe or MSTest.exe. Use either --VSTestPath or --MSTestPath parameters to pass paths to one of those files.");
                }

                if (build.ApplyDotCover && build.DotCoverPath != "" && !(File.Exists(build.DotCoverPath)))
                {
                    throw new Exception("Error cannot find dotcover.exe. Use -build.DotCoverPath parameter to pass a path to that file.");
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
                for (var i = 0; i < TotalNumberOfJobsToRun; i++)
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
                    var TestSettingsFile = TestLauncher.ScreenRecordingTestSettingsFile(build, JobName);

                    string TestRunnerPath;
                    if (string.IsNullOrEmpty(build.MSTest))
                    {
                        TestRunnerPath = TestLauncher.VSTestRunner(build, JobName, ProjectSpec, TestCategories, TestAssembliesList, TestSettingsFile);
                    }
                    else
                    {
                        TestRunnerPath = TestLauncher.MSTestRunner(build, JobName, ProjectSpec, TestCategories, TestAssembliesList, TestSettingsFile, build.TestsResultsPath);
                    }
                    var latestTrxFile = TestLauncher.RunTests(build, JobName, TestAssembliesList, TestAssembliesDirectories, TestSettingsFile, TestRunnerPath);
                    TestLauncher.RetryOnTestError(build, JobName, TestAssembliesList, TestAssembliesDirectories, TestSettingsFile, latestTrxFile);
                }
                if (build.ApplyDotCover && TotalNumberOfJobsToRun > 1)
                {
                    MergeDotCoverSnapshots(build);
                }
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
                MergeDotCoverSnapshots(build);
            }

            if (!string.IsNullOrEmpty(build.Cleanup))
            {
                if (build.ApplyDotCover)
                {
                    build.CleanupServerStudio(false);
                }
                else
                {
                    build.CleanupServerStudio();
                }
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

            if (string.IsNullOrEmpty(build.Cleanup) && string.IsNullOrEmpty(build.AssemblyFileVersionsTest) && string.IsNullOrEmpty(build.JobName) && string.IsNullOrEmpty(build.RunWarewolfServiceTests) && string.IsNullOrEmpty(build.MergeDotCoverSnapshotsInDirectory) && string.IsNullOrEmpty(build.StartDocker))
            {
                build.Startmywarewolfio();
                if (String.IsNullOrEmpty(build.DomywarewolfioStart))
                {
                    build.InstallServer();
                    build.StartServer();
                    if (String.IsNullOrEmpty(build.DoServerStart) && String.IsNullOrEmpty(build.DomywarewolfioStart))
                    {
                        build.StartStudio();
                    }
                }
            }
        }

        private static void MergeDotCoverSnapshots(TestLauncher build)
        {
            var DotCoverSnapshots = Directory.GetFiles(build.MergeDotCoverSnapshotsInDirectory, "*.dcvr", SearchOption.AllDirectories).ToList();
            if (string.IsNullOrEmpty(build.JobName))
            {
                build.JobName = "DotCover";
            }
            var MergedSnapshotFileName = build.JobName.Split(',')[0];
            MergedSnapshotFileName = "Merged " + MergedSnapshotFileName + " Snapshots";
            build.MergeDotCoverSnapshots(DotCoverSnapshots, build.MergeDotCoverSnapshotsInDirectory + "\\" + MergedSnapshotFileName, build.MergeDotCoverSnapshotsInDirectory + "\\DotCover");
        }
    }
}
