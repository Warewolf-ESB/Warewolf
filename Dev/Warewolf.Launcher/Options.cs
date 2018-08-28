using CommandLine;
using System;
using System.Threading;
using Warewolf.Launcher.TestRunners;

namespace Warewolf.Launcher
{
    internal class Options
    {
        [Option("StartServer")]
        public bool DoServerStart { get; set; }

        [Option("StartStudio")]
        public bool DoStudioStart { get; set; }

        [Option("ServerPath")]
        public string ServerPath { get; set; }

        [Option("StudioPath")]
        public string StudioPath { get; set; }

        [Option("ResourcesType")]
        public string ResourcesType { get; set; }

        [Option("VSTest")]
        public bool VSTest { get; set; }

        [Option("MSTest")]
        public bool MSTest { get; set; }

        [Option("DotCoverPath")]
        public string DotCoverPath { get; set; }

        [Option("ServerUsername")]
        public string ServerUsername { get; set; }

        [Option("ServerPassword")]
        public string ServerPassword { get; set; }

        [Option("RunAllJobs")]
        public bool RunAllJobs { get; set; }

        [Option("Cleanup")]
        public bool Cleanup { get; set; }

        [Option("AssemblyFileVersionsTest")]
        public string AssemblyFileVersionsTest { get; set; }

        [Option("RecordScreen")]
        public bool RecordScreen { get; set; }

        [Option("Category")]
        public string Category { get; set; }

        [Option("ProjectName")]
        public string ProjectName { get; set; }

        [Option("RunAllUnitTests")]
        public bool RunAllUnitTests { get; set; }

        [Option("RunAllServerTests")]
        public bool RunAllServerTests { get; set; }

        [Option("RunAllReleaseResourcesTests")]
        public bool RunAllReleaseResourcesTests { get; set; }

        [Option("RunAllWebUITests")]
        public bool RunAllWebUITests { get; set; }

        [Option("RunAllDesktopUITests")]
        public bool RunAllDesktopUITests { get; set; }

        [Option("RunWarewolfServiceTests")]
        public bool RunWarewolfServiceTests { get; set; }

        [Option("DomywarewolfioStart")]
        public bool DomywarewolfioStart { get; set; }

        [Option("TestsPath")]
        public string TestsPath { get; set; }

        [Option("JobName")]
        public string JobName { get; set; }

        [Option("TestList")]
        public string TestList { get; set; }

        [Option("MergeDotCoverSnapshotsInDirectory")]
        public string MergeDotCoverSnapshotsInDirectory { get; set; }

        [Option("TestsResultsPath")]
        public string TestsResultsPath { get; set; }

        [Option("VSTestPath")]
        public string VSTestPath { get; set; }

        [Option("MSTestPath")]
        public string MSTestPath { get; set; }

        [Option("RetryCount")]
        public string RetryCount { get; private set; }

        [Option("RetryFile")]
        public string RetryFile { get; private set; }

        [Option("ConsoleServer")]
        public bool ConsoleServer { get; private set; }

        [Option("AdminMode")]
        public bool AdminMode { get; private set; }

        [Option("Parallelize")]
        public bool Parallelize { get; set; }

        [Option("StartContainer")]
        public string StartContainer { get; set; }

        public static TestLauncher PargeArgs(string[] args)
        {
            var testLauncher = new TestLauncher();
            var parser = new Parser(with => with.EnableDashDash = true);
            var parserResult = parser.ParseArguments<Options>(args);
            if (args.Length > 0 && parserResult.Tag == ParserResultType.NotParsed)
            {
                throw new ArgumentException("Syntax Error in Args \"" + string.Join(" ", args) + "\". Some args take a string value for example: --ServerPath 'C:\\Builds\\Warewolf Server.exe'");
            }
            var result = parserResult.WithParsed(options =>
            {
                if (options.DoServerStart)
                {
                    Console.WriteLine("Doing Server Start.");
                    testLauncher.DoServerStart = "true";
                }
                if (options.DoStudioStart)
                {
                    Console.WriteLine("Doing Studio Start.");
                    testLauncher.DoStudioStart = "true";
                }
                if (options.ServerPath != null)
                {
                    Console.WriteLine("ServerPath: " + options.ServerPath);
                    testLauncher.ServerPath = options.ServerPath;
                }
                if (options.StudioPath != null)
                {
                    Console.WriteLine("StudioPath: " + options.StudioPath);
                    testLauncher.StudioPath = options.StudioPath;
                }
                if (options.ResourcesType != null)
                {
                    Console.WriteLine("ResourcesType: " + options.ResourcesType);
                    testLauncher.ResourcesType = options.ResourcesType;
                }
                if (options.VSTest)
                {
                    Console.WriteLine("Test Runner: VSTest");
                }
                if (options.VSTest || (!options.MSTest && !options.VSTest))
                {
                    testLauncher.TestRunner = new VSTestRunner();
                }
                if (options.MSTest)
                {
                    Console.WriteLine("Test Runner: MSTest");
                    testLauncher.TestRunner = new MSTestRunner();
                }
                if (options.DotCoverPath != null)
                {
                    Console.WriteLine("DotCoverPath: " + options.DotCoverPath);
                    testLauncher.DotCoverPath = options.DotCoverPath;
                }
                if (options.ServerUsername != null)
                {
                    testLauncher.ServerUsername = options.ServerUsername;
                }
                if (options.ServerPassword != null)
                {
                    Console.WriteLine("ServerPassword: ****");
                    testLauncher.ServerPassword = options.ServerPassword;
                }
                if (options.RunAllJobs)
                {
                    Console.WriteLine("RunAllJobs");
                    testLauncher.RunAllJobs = "true";
                }
                if (options.Cleanup)
                {
                    Console.WriteLine("Cleaning Up.");
                    testLauncher.Cleanup = true;
                }
                if (options.AssemblyFileVersionsTest != null)
                {
                    Console.WriteLine("AssemblyFileVersionsTest: " + options.AssemblyFileVersionsTest);
                    testLauncher.AssemblyFileVersionsTest = options.AssemblyFileVersionsTest;
                }
                if (options.RecordScreen)
                {
                    Console.WriteLine("Recording Screen.");
                    testLauncher.RecordScreen = "true";
                }
                if (options.Category != null)
                {
                    Console.WriteLine("Category: " + options.Category);
                    testLauncher.Category = options.Category;
                }
                if (options.ProjectName != null)
                {
                    Console.WriteLine("ProjectName: " + options.ProjectName);
                    testLauncher.ProjectName = options.ProjectName;
                }
                if (options.RunAllUnitTests)
                {
                    Console.WriteLine("Running All Unit Tests.");
                    testLauncher.RunAllUnitTests = "true";
                }
                if (options.RunAllServerTests)
                {
                    Console.WriteLine("Running All Server Tests.");
                    testLauncher.RunAllServerTests = "true";
                }
                if (options.RunAllReleaseResourcesTests)
                {
                    Console.WriteLine("Running All Release Resources Tests.");
                    testLauncher.RunAllReleaseResourcesTests = "true";
                }
                if (options.RunAllDesktopUITests)
                {
                    Console.WriteLine("Running All Desktop UI Tests.");
                    testLauncher.RunAllDesktopUITests = "true";
                }
                if (options.RunAllWebUITests)
                {
                    Console.WriteLine("Running All Web UI Tests.");
                    testLauncher.RunAllWebUITests = "true";
                }
                if (options.RunWarewolfServiceTests)
                {
                    Console.WriteLine("Running Warewolf Service Tests.");
                    testLauncher.RunWarewolfServiceTests = "true";
                }
                if (options.DomywarewolfioStart)
                {
                    Console.WriteLine("Doing my.warewolf.io Start.");
                    testLauncher.DomywarewolfioStart = "true";
                }
                if (options.TestsPath != null)
                {
                    Console.WriteLine("TestsPath: " + options.TestsPath);
                    testLauncher.TestRunner.TestsPath = options.TestsPath;
                    testLauncher.TestRunner.TestsResultsPath = testLauncher.TestRunner.TestsPath + "\\TestResults";
                }
                if (options.JobName != null)
                {
                    Console.WriteLine("JobName: " + options.JobName);
                    testLauncher.JobName = options.JobName;
                }
                if (options.TestList != null)
                {
                    Console.WriteLine("TestList: " + options.TestList);
                    testLauncher.TestRunner.TestList = options.TestList;
                }
                if (options.MergeDotCoverSnapshotsInDirectory != null)
                {
                    Console.WriteLine("Merging DotCover Snapshots In Directory: " + options.MergeDotCoverSnapshotsInDirectory);
                    testLauncher.MergeDotCoverSnapshotsInDirectory = options.MergeDotCoverSnapshotsInDirectory;
                }
                if (options.TestsResultsPath != null)
                {
                    Console.WriteLine("TestsResultsPath: " + options.TestsResultsPath);
                    testLauncher.TestRunner.TestsResultsPath = options.TestsResultsPath;
                }
                if (options.VSTestPath != null)
                {
                    Console.WriteLine("VSTestPath: " + options.VSTestPath);
                    testLauncher.TestRunner.Path = options.VSTestPath;
                }
                if (options.MSTestPath != null)
                {
                    Console.WriteLine("MSTestPath: " + options.MSTestPath);
                    testLauncher.TestRunner.Path = options.MSTestPath;
                }
                if (options.RetryCount != null)
                {
                    Console.WriteLine("RetryCount: Re-trying failures " + options.RetryCount + " number of times.");
                    if (int.TryParse(options.RetryCount, out int retryCount))
                    {
                        testLauncher.RetryCount = retryCount;
                    }
                    else
                    {
                        Console.WriteLine("RetryCount: Expects a number of times to re-try failing tests. Cannot parse " + options.RetryCount);
                    }
                }
                if (options.RetryFile != null)
                {
                    Console.WriteLine("Retrying all failures in file: " + options.RetryFile);
                    testLauncher.RetryFile = options.RetryFile;
                }
                if (options.ConsoleServer)
                {
                    Console.WriteLine("ConsoleServer: Starting the server in a console window.");
                    testLauncher.StartServerAsConsole = true;
                }
                if (options.AdminMode)
                {
                    testLauncher.AdminMode = true;
                }
                if (options.Parallelize)
                {
                    testLauncher.Parallelize = true;
                }
                if (options.StartContainer != null)
                {
                    Console.WriteLine("Starting Container: " + options.StartContainer);
                    var containerLauncher = new ContainerLauncher(options.StartContainer, $"test-{options.StartContainer.Split('-')[0]}", "localhost");
                    Thread.Sleep(30000);
                    containerLauncher.LogOutputDirectory = testLauncher.TestRunner.TestsResultsPath;
                    testLauncher.StartContainer = containerLauncher;
                }
                testLauncher.TestCoverageMerger = new TestCoverageMergers.DotCoverSnapshotMerger();
            }).WithNotParsed(errs =>
            {
                foreach (var err in errs)
                {
                    if (err.Tag != ErrorType.MissingValueOptionError)
                    {
                        throw new ArgumentException(err.Tag.ToString());
                    }
                }
            });
            return testLauncher;
        }
    }
}
