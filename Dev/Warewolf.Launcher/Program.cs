using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Security.Principal;
using Warewolf.Launcher.Utils;

namespace Warewolf.Launcher
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
                    var exeName = Process.GetCurrentProcess().MainModule.FileName;
                    ProcessStartInfo startInfo = new ProcessStartInfo(exeName)
                    {
                        Verb = "runas",
                        Arguments = string.Join(" ", args)
                    };
                    Process.Start(startInfo);
                    return;
                }
            }

            TestLauncher build = null;
            try
            {
                build = Options.PargeArgs(args);
                build.JobSpecs = Job_Definitions.GetJobDefinitions();
                TestLauncher.EnableDocker = Job_Definitions.GetEnableDockerValue();

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

                if (!string.IsNullOrEmpty(build.TestRunner.TestsPath) && build.TestRunner.TestsPath.StartsWith(".."))
                {
                    build.TestRunner.TestsPath = Path.Combine(Environment.CurrentDirectory, build.TestRunner.TestsPath);
                }

                if (!string.IsNullOrEmpty(build.TestRunner.TestsResultsPath) && build.TestRunner.TestsResultsPath.StartsWith(".."))
                {
                    build.TestRunner.TestsResultsPath = Path.Combine(Environment.CurrentDirectory, build.TestRunner.TestsResultsPath);
                }

                if (!string.IsNullOrEmpty(build.ServerPath) && build.ServerPath.StartsWith(".."))
                {
                    build.ServerPath = Path.Combine(Environment.CurrentDirectory, build.ServerPath);
                }

                if (!string.IsNullOrEmpty(build.StudioPath) && build.StudioPath.StartsWith(".."))
                {
                    build.StudioPath = Path.Combine(Environment.CurrentDirectory, build.StudioPath);
                }

                if (!string.IsNullOrEmpty(build.TestRunner.TestList))
                {
                    build.TestRunner.TestList = build.TestRunner.TestList.Trim();
                }

                if (!File.Exists(build.TestRunner.TestsResultsPath))
                {
                    Directory.CreateDirectory(build.TestRunner.TestsResultsPath);
                }

                if (build.MergeDotCoverSnapshotsInDirectory != null)
                {
                    build.MergeDotCoverSnapshots();
                }

                if (!build.Cleanup)
                {
                    if (!string.IsNullOrEmpty(build.JobName) && string.IsNullOrEmpty(build.AssemblyFileVersionsTest) && string.IsNullOrEmpty(build.RunWarewolfServiceTests) && string.IsNullOrEmpty(build.MergeDotCoverSnapshotsInDirectory))
                    {
                        build.RunTestJobs();
                    }
                }
                else
                {
                    build.CleanupServerStudio(!build.ApplyDotCover);
                    if (string.IsNullOrEmpty(build.JobName))
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
                    build.MoveArtifactsToTestResults(build.ApplyDotCover, File.Exists(Environment.ExpandEnvironmentVariables("%ProgramData%\\Warewolf\\Server Log\\wareWolf-Server.log")), File.Exists(Environment.ExpandEnvironmentVariables("%LocalAppData\\Warewolf\\Studio Logs\\Warewolf Studio.log")), build.JobName);
                }

                if (!string.IsNullOrEmpty(build.AssemblyFileVersionsTest))
                {
                    Console.WriteLine("Testing Warewolf assembly file versions...");
                    var HighestReadVersion = "0.0.0.0";
                    var LastReadVersion = "0.0.0.0";
                    foreach (var file in Directory.GetFiles(build.TestRunner.TestsPath, "*", SearchOption.AllDirectories))
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
                                throw new Exception("ERROR! \"" + file + " " + ReadVersion + "\" is either an invalid version or not equal to \"" + LastReadVersion + "\". All Warewolf assembly versions in \"" + build.TestRunner.TestsPath + "\" must conform and cannot start with 0.0. or end with .0");
                            }
                            LastReadVersion = ReadVersion;
                        }
                    }
                    File.WriteAllText("FullVersionString", "FullVersionString=" + HighestReadVersion);
                }

                if (!build.Cleanup && string.IsNullOrEmpty(build.AssemblyFileVersionsTest) && string.IsNullOrEmpty(build.JobName) && string.IsNullOrEmpty(build.RunWarewolfServiceTests) && string.IsNullOrEmpty(build.MergeDotCoverSnapshotsInDirectory))
                {
                    if (build.AdminMode)
                    {
                        AdminMode.Run(build);
                    }
                    else
                    {
                        ManualMode.Run(build);
                    }
                }
            }
            finally
            {
                if (build != null)
                {
                    build.Dispose();
                }
            }
        }
    }
}
