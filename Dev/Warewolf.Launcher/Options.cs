using CommandLine;
using System;

namespace Bashley
{
    internal class Options
    {
        [Option("DoServerStart")]
        public bool DoServerStart { get; set; }

        [Option("DoStudioStart")]
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

        [Option("RunAllCodedUITests")]
        public bool RunAllCodedUITests { get; set; }

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

        [Option("StartDocker")]
        public bool StartDocker { get; set; }

        public static void PargeArgs(string[] args)
        {
            var parser = new Parser(with => with.EnableDashDash = true);
            var parserResult = parser.ParseArguments<Options>(args);
            if (args.Length > 0 && parserResult.Tag.ToString() == "NotParsed")
            {
                throw new ArgumentException("Syntax Error in Args \"" + string.Join(" ", args) + "\". Some args take a string value for example: --ServerPath 'C:\\Builds\\Warewolf Server.exe'");
            }
            var result = parserResult.WithParsed(options =>
            {
                if (options.DoServerStart)
                {
                    Console.WriteLine("Doing Server Start.");
                    Program.DoServerStart = "true";
                }
                if (options.DoStudioStart)
                {
                    Console.WriteLine("Doing Studio Start.");
                    Program.DoStudioStart = "true";
                }
                if (options.ServerPath != null)
                {
                    Console.WriteLine("ServerPath: " + options.ServerPath);
                    Program.ServerPath = options.ServerPath;
                }
                if (options.StudioPath != null)
                {
                    Console.WriteLine("StudioPath: " + options.StudioPath);
                    Program.StudioPath = options.StudioPath;
                }
                if (options.ResourcesType != null)
                {
                    Console.WriteLine("ResourcesType: " + options.ResourcesType);
                    Program.ResourcesType = options.ResourcesType;
                }
                if (options.VSTest)
                {
                    Console.WriteLine("VSTest");
                    Program.VSTest = "true";
                }
                if (options.MSTest)
                {
                    Console.WriteLine("MSTest");
                    Program.MSTest = "true";
                }
                if (options.DotCoverPath != null)
                {
                    Console.WriteLine("DotCoverPath: " + options.DotCoverPath);
                    Program.DotCoverPath = options.DotCoverPath;
                }
                if (options.ServerUsername != null)
                {
                    Program.ServerUsername = options.ServerUsername;
                }
                if (options.ServerPassword != null)
                {
                    Console.WriteLine("ServerPassword: ****");
                    Program.ServerPassword = options.ServerPassword;
                }
                if (options.RunAllJobs)
                {
                    Console.WriteLine("RunAllJobs");
                    Program.RunAllJobs = "true";
                }
                if (options.Cleanup)
                {
                    Console.WriteLine("Cleaning Up.");
                    Program.Cleanup = "true";
                }
                if (options.AssemblyFileVersionsTest != null)
                {
                    Console.WriteLine("AssemblyFileVersionsTest: " + options.AssemblyFileVersionsTest);
                    Program.AssemblyFileVersionsTest = options.AssemblyFileVersionsTest;
                }
                if (options.RecordScreen)
                {
                    Console.WriteLine("Recording Screen.");
                    Program.RecordScreen = "true";
                }
                if (options.Category != null)
                {
                    Console.WriteLine("Category: " + options.Category);
                    Program.Category = options.Category;
                }
                if (options.ProjectName != null)
                {
                    Console.WriteLine("ProjectName: " + options.ProjectName);
                    Program.ProjectName = options.ProjectName;
                }
                if (options.RunAllUnitTests)
                {
                    Console.WriteLine("Running All Unit Tests.");
                    Program.RunAllUnitTests = "true";
                }
                if (options.RunAllServerTests)
                {
                    Console.WriteLine("Running All Server Tests.");
                    Program.RunAllServerTests = "true";
                }
                if (options.RunAllReleaseResourcesTests)
                {
                    Console.WriteLine("Running All Release Resources Tests.");
                    Program.RunAllReleaseResourcesTests = "true";
                }
                if (options.RunAllCodedUITests)
                {
                    Console.WriteLine("Running All Coded UI Tests.");
                    Program.RunAllCodedUITests = "true";
                }
                if (options.RunWarewolfServiceTests)
                {
                    Console.WriteLine("Running Warewolf Service Tests.");
                    Program.RunWarewolfServiceTests = "true";
                }
                if (options.DomywarewolfioStart)
                {
                    Console.WriteLine("Doing my.warewolf.io Start.");
                    Program.DomywarewolfioStart = "true";
                }
                if (options.TestsPath != null)
                {
                    Console.WriteLine("TestsPath: " + options.TestsPath);
                    Program.TestsPath = options.TestsPath;
                    Program.TestsResultsPath = Program.TestsPath + "\\TestResults";
                }
                if (options.JobName != null)
                {
                    Console.WriteLine("JobName: " + options.JobName);
                    Program.JobName = options.JobName;
                }
                if (options.TestList != null)
                {
                    Console.WriteLine("TestList: " + options.TestList);
                    Program.TestList = options.TestList;
                }
                if (options.MergeDotCoverSnapshotsInDirectory != null)
                {
                    Console.WriteLine("Merging DotCover Snapshots In Directory: " + options.MergeDotCoverSnapshotsInDirectory);
                    Program.MergeDotCoverSnapshotsInDirectory = options.MergeDotCoverSnapshotsInDirectory;
                }
                if (options.TestsResultsPath != null)
                {
                    Console.WriteLine("TestsResultsPath: " + options.TestsResultsPath);
                    Program.TestsResultsPath = options.TestsResultsPath;
                }
                if (options.VSTestPath != null)
                {
                    Console.WriteLine("VSTestPath: " + options.VSTestPath);
                    Program.VSTestPath = options.VSTestPath;
                }
                if (options.MSTestPath != null)
                {
                    Console.WriteLine("MSTestPath: " + options.MSTestPath);
                    Program.MSTestPath = options.MSTestPath;
                }
                if (options.StartDocker)
                {
                    Console.WriteLine("Starting Docker.");
                    Program.StartDocker = "true";
                }
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
        }
    }
}
