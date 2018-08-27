using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Warewolf.Launcher.Utils
{
    static class AdminMode
    {
        public static void Run(TestLauncher build)
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
                Console.WriteLine("\n\nType the name or number of the job (or comma seperated list of jobs). Leave blank to use default (Other Unit Tests)...");

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
                    build.RecordScreen = "true";
                }
                else
                {
                    Console.WriteLine("\nStart the Server?[y|N]");

                    if (WindowUtils.PromptForUserInput().ToLower() == "y")
                    {
                        build.DoServerStart = "true";
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

                const int NumberOfUnitTestJobs = 37;
                const int NumberOfServerTestJobs = 32;
                const int NumberOfReleaseResourcesTestJobs = 1;
                const int NumberOfDesktopUITestJobs = 65;
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
    }
}
