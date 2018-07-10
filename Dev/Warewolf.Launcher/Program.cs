using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Principal;

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
                build.MoveArtifactsToTestResults(build.ApplyDotCover, File.Exists(Environment.ExpandEnvironmentVariables("%ProgramData%\\Warewolf\\Server Log\\wareWolf-Server.log")), File.Exists(Environment.ExpandEnvironmentVariables("%LocalAppData\\Warewolf\\Studio Logs\\Warewolf Studio.log")));
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

            if (build.MergeDotCoverSnapshotsInDirectory != null)
            {
                build.MergeDotCoverSnapshots();
            }

            if (!build.Cleanup && string.IsNullOrEmpty(build.AssemblyFileVersionsTest) && string.IsNullOrEmpty(build.JobName) && string.IsNullOrEmpty(build.RunWarewolfServiceTests) && string.IsNullOrEmpty(build.MergeDotCoverSnapshotsInDirectory))
            {
                if (build.AdminMode)
                {
                    if (!File.Exists(build.TestRunner.Path))
                    {
                        if (Path.GetFileName(build.TestRunner.Path).ToLower() == "vstest.console.exe")
                        {
                            Console.WriteLine("\nvstest.console.exe not found. Please enter the path to that file now.");
                            build.TestRunner.Path = WindowUtils.PromptForUserInput();
                            if (Path.GetFileName(build.TestRunner.Path).ToLower() == "mstest.exe")
                            {
                                throw new ArgumentException("Launcher must be run with a MSTest test runner in order to use mstest.exe. Use --MSTest commandline parameter to specify that a MSTest test runner is to be used.");
                            }
                            if (Path.GetFileName(build.TestRunner.Path).ToLower() == "vstest.console.exe" && File.Exists(build.TestRunner.Path))
                            {
                                Environment.SetEnvironmentVariable("VSTESTEXE", build.TestRunner.Path, EnvironmentVariableTarget.Machine);
                            }
                        }
                        else if (Path.GetFileName(build.TestRunner.Path).ToLower() == "mstest.exe")
                        {
                            Console.WriteLine("\nmstest.exe not found. Please enter the path to that file now.");
                            build.TestRunner.Path = WindowUtils.PromptForUserInput();
                            if (Path.GetFileName(build.TestRunner.Path).ToLower() == "vstest.console.exe")
                            {
                                throw new ArgumentException("Launcher must be run with a VSTest test runner in order to use vstest.console.exe. Use --VSTest commandline parameter to specify that a VSTest test runner is to be used.");
                            }
                            if (Path.GetFileName(build.TestRunner.Path).ToLower() == "mstest.exe" && File.Exists(build.TestRunner.Path))
                            {
                                Environment.SetEnvironmentVariable("MSTESTEXE", build.TestRunner.Path, EnvironmentVariableTarget.Machine);
                            }
                        }
                    }
                    Console.WriteLine("\nAdmin, What would you like to do?");
                    var options = new[] {
                    "[1]Single Job: Run One Test Job. (This is the default)",
                    "[2]Builds: Run Whole Builds."
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
                    Console.WriteLine("\n\nOr Press Enter to use default (Single Job)...");

                    var runType = WindowUtils.PromptForUserInput();
                    if (runType == "" || runType == "1")
                    {
                        Console.WriteLine("\nWhich test job would you like to run?");
                        int count = 0;
                        foreach (var option in build.JobSpecs.Keys.ToList())
                        {
                            Console.WriteLine();
                            var originalColour = Console.ForegroundColor;
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.Write("[" + (++count).ToString() + "]");
                            Console.ForegroundColor = originalColour;
                            Console.Write(option);
                        }
                        Console.WriteLine("\n\nType the name or number of the job (or comma seperated list of jobs). Press Enter to use default (Other Unit Tests)...");

                        string selectedOption = WindowUtils.PromptForUserInput();
                        if (string.IsNullOrEmpty(selectedOption))
                        {
                            selectedOption = "1";
                        }
                        if (!selectedOption.Contains(","))
                        {
                            var canParse = int.TryParse(selectedOption, out int jobNumber);
                            if (canParse)
                            {
                                build.JobName = build.JobSpecs.Keys.ToList()[jobNumber - 1];
                            }
                            else
                            {
                                if (build.JobSpecs.Keys.ToList().Contains(selectedOption))
                                {
                                    build.JobName = selectedOption;
                                }
                                else
                                {
                                    throw new ArgumentException($"{selectedOption} is an invalid option. Please type just the number of the option you would like to select and then press Enter.");
                                }
                            }
                        }
                        else
                        {
                            var resolvedSelectedOptions = new List<string>();
                            foreach (var option in selectedOption.Split(','))
                            {
                                var canParse = int.TryParse(option, out int jobNumber);
                                if (canParse)
                                {
                                    resolvedSelectedOptions.Add(build.JobSpecs.Keys.ToList()[jobNumber - 1]);
                                }
                                else
                                {
                                    if (build.JobSpecs.Keys.ToList().Contains(option))
                                    {
                                        resolvedSelectedOptions.Add(option);
                                    }
                                    else
                                    {
                                        throw new ArgumentException($"{option} is an invalid option. Please type just the number of the option you would like to select and then press Enter.");
                                    }
                                }
                            }
                            build.JobName = string.Join(",", resolvedSelectedOptions);
                        }
                        Console.WriteLine("\nWhich tests would you like to run? (Comma seperated list of test names to run or leave blank to run all)");

                        build.TestRunner.TestList = WindowUtils.PromptForUserInput();
                        Console.WriteLine("\nStart the Studio?[y|N]");

                        if (WindowUtils.PromptForUserInput().ToLower() == "y")
                        {
                            build.DoServerStart = "true";
                            build.DoStudioStart = "true";
                        }
                        else
                        {
                            Console.WriteLine("\nStart the Server?[y|N]");

                            if (WindowUtils.PromptForUserInput().ToLower() == "y")
                            {
                                build.DoServerStart = "true";
                                build.RecordScreen = "true";
                            }
                        }

                        build.RunTestJobs();
                    }
                    else if (runType == "2")
                    {
                        Console.WriteLine("\nWhat type of build do you want to run?");
                        options = new[] {
                        "[1]All: Run All Test Builds. (This is the default)",
                        "[2]Unit Tests: Run All Unit Test Jobs.",
                        "[3]Server Tests: Run All ServerTests Resource Test Jobs",
                        "[4]Release: Run All Release Resource Test Jobs.",
                        "[5]Desktop UI: Run All UI Test Jobs. (This is the default)",
                        "[6]Web UI: Run All Web UI Test Jobs.",
                        "[7]UI Load: Run All Load Test Jobs."
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
                        Console.WriteLine("\n\nOr Press Enter to use default (All)...");

                        var testType = WindowUtils.PromptForUserInput();

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
                        else if (testType == "2")
                        {
                            build.RunAllUnitTestJobs(0, NumberOfUnitTestJobs);
                        }
                        else if (testType == "3")
                        {
                            build.RunAllServerTestJobs(NumberOfUnitTestJobs, NumberOfServerTestJobs);
                        }
                        else if (testType == "4")
                        {
                            build.RunAllReleaseResourcesTestJobs(NumberOfUnitTestJobs + NumberOfServerTestJobs, NumberOfReleaseResourcesTestJobs);
                        }
                        else if (testType == "5")
                        {
                            build.RunAllDesktopUITestJobs(NumberOfUnitTestJobs + NumberOfServerTestJobs + NumberOfReleaseResourcesTestJobs, NumberOfDesktopUITestJobs);
                        }
                        else if (testType == "6")
                        {
                            build.RunAllWebUITestJobs(NumberOfUnitTestJobs + NumberOfServerTestJobs + NumberOfReleaseResourcesTestJobs + NumberOfDesktopUITestJobs, NumberOfWebUITestJobs);
                        }
                        else if (testType == "7")
                        {
                            build.RunAllLoadTestJobs(NumberOfUnitTestJobs + NumberOfServerTestJobs + NumberOfReleaseResourcesTestJobs + NumberOfDesktopUITestJobs + NumberOfWebUITestJobs, NumberOfLoadTestJobs);
                        }
                    }
                    WindowUtils.BringToFront();
                    var originalConsoleColour = Console.ForegroundColor;
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"Admin, build has completed. Test results have been published to {build.TestRunner.TestsResultsPath}. You can now close this window.");
                    Console.ForegroundColor = originalConsoleColour;
                    Console.ReadKey();
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
