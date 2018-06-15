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
                build.RunTestJobs();
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
                        build.RunAllUnitTestJobs(0, NumberOfUnitTestJobs);
                        build.RunAllServerTestJobs(NumberOfUnitTestJobs, NumberOfServerTestJobs);
                        build.RunAllReleaseResourcesTestJobs(NumberOfUnitTestJobs + NumberOfServerTestJobs, NumberOfReleaseResourcesTestJobs);
                        build.RunAllDesktopUITestJobs(NumberOfUnitTestJobs + NumberOfServerTestJobs + NumberOfReleaseResourcesTestJobs, NumberOfDesktopUITestJobs);
                        build.RunAllWebUITestJobs(NumberOfUnitTestJobs + NumberOfServerTestJobs + NumberOfReleaseResourcesTestJobs + NumberOfDesktopUITestJobs, NumberOfWebUITestJobs);
                        build.RunAllLoadTestJobs(NumberOfUnitTestJobs + NumberOfServerTestJobs + NumberOfReleaseResourcesTestJobs + NumberOfDesktopUITestJobs + NumberOfWebUITestJobs, NumberOfLoadTestJobs);
                    }
                    if (testType == "2")
                    {
                        build.RunAllUnitTestJobs(0, NumberOfUnitTestJobs);
                    }
                    if (testType == "3")
                    {
                        build.RunAllServerTestJobs(NumberOfUnitTestJobs, NumberOfServerTestJobs);
                    }
                    if (testType == "4")
                    {
                        build.RunAllReleaseResourcesTestJobs(NumberOfUnitTestJobs + NumberOfServerTestJobs, NumberOfReleaseResourcesTestJobs);
                    }
                    if (testType == "5")
                    {
                        build.RunAllDesktopUITestJobs(NumberOfUnitTestJobs + NumberOfServerTestJobs + NumberOfReleaseResourcesTestJobs, NumberOfDesktopUITestJobs);
                    }
                    if (testType == "6")
                    {
                        build.RunAllWebUITestJobs(NumberOfUnitTestJobs + NumberOfServerTestJobs + NumberOfReleaseResourcesTestJobs + NumberOfDesktopUITestJobs, NumberOfWebUITestJobs);
                    }
                    if (testType == "7")
                    {
                        build.RunAllLoadTestJobs(NumberOfUnitTestJobs + NumberOfServerTestJobs + NumberOfReleaseResourcesTestJobs + NumberOfDesktopUITestJobs + NumberOfWebUITestJobs, NumberOfLoadTestJobs);
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
    }
}
