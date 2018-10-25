using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Xml;

namespace Warewolf.Launcher
{
    public static class TestCleanupUtils
    {
        public static string NumberToWords(int number) => new[] { "None", "First", "Second", "Third", "Fourth", "Fifth", "Sixth", "Seventh", "Eighth", "Nineth", "Tenth", "Eleventh", "Twelveth", "Thirteenth", "Fourteenth", "Fifteenth", "Sixteenth", "Seventeenth", "Eighteenth", "Nineteenth" }[number];

        static readonly string[] ToClean = new[]
        {
            "%LOCALAPPDATA%\\Warewolf\\DebugData\\PersistSettings.dat",
            "%LOCALAPPDATA%\\Warewolf\\UserInterfaceLayouts\\WorkspaceLayout.xml",
            "%PROGRAMDATA%\\Warewolf\\Workspaces",
            "%PROGRAMDATA%\\Warewolf\\Server Settings",
            "%PROGRAMDATA%\\Warewolf\\VersionControl",
            "%PROGRAMDATA%\\Warewolf\\Audits\\auditDB.db"
        };

        static readonly string[] ToPublish = new[]
        {
            "%PROGRAMDATA%\\Warewolf\\Resources",
            "%PROGRAMDATA%\\Warewolf\\Tests",
            "%PROGRAMDATA%\\Warewolf\\VersionControl",
            "%PROGRAMDATA%\\Warewolf\\DetailedLogs"
        };

        public static void CopyOnWrite(string FileSpec)
        {
            var DirectoryPath = Path.GetDirectoryName(FileSpec);
            var FileNameWithoutExtention = Path.GetFileNameWithoutExtension(FileSpec);
            if (File.Exists(FileSpec))
            {
                var num = 1;
                var FileExtention = Path.GetExtension(FileSpec);
                while (File.Exists($"{DirectoryPath}\\{NumberToWords(num)} {FileNameWithoutExtention}{FileExtention}"))
                {
                    num++;
                }
                File.Move(FileSpec, $"{DirectoryPath}\\{NumberToWords(num)} {FileNameWithoutExtention}{FileExtention}");
            }
            else if (Directory.Exists(FileSpec))
            {
                var num = 1;
                while (Directory.Exists($"{DirectoryPath}\\{NumberToWords(num)} {FileNameWithoutExtention}"))
                {
                    num++;
                }
                Directory.Move(FileSpec, $"{DirectoryPath}\\{NumberToWords(num)} {FileNameWithoutExtention}");
            }
        }

        public static void MoveFileToTestResults(string SourceFilePath, string DestinationFileName, string TestsResultsPath)
        {
            var DestinationFilePath = Path.Combine(TestsResultsPath, DestinationFileName);
            if (File.Exists(SourceFilePath))
            {
                CopyOnWrite(DestinationFilePath);
                Console.WriteLine($"Moving \"{SourceFilePath}\" to \"{DestinationFilePath}\"");
                var DestinationFolderPath = Path.GetDirectoryName(DestinationFilePath);
                if (!Directory.Exists(DestinationFolderPath))
                {
                    Directory.CreateDirectory(DestinationFolderPath);
                }
                File.Move(SourceFilePath, DestinationFilePath);
            }
        }

        public static void MoveFolderToTestResults(string SourceFolderPath, string DestinationFolderName, string TestsResultsPath)
        {
            var DestinationFolderPath = Path.Combine(TestsResultsPath, DestinationFolderName);
            if (Directory.Exists(SourceFolderPath))
            {
                CopyOnWrite(DestinationFolderPath);
                Console.WriteLine($"Moving \"{SourceFolderPath}\" to \"{DestinationFolderPath}\"");
                if (!Directory.Exists(Path.GetDirectoryName(DestinationFolderPath)))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(DestinationFolderPath));
                }
                RecursivelyCopyFolder(SourceFolderPath, DestinationFolderPath);
                Directory.Delete(SourceFolderPath, true);
            }
        }

        static void RecursivelyCopyFolder(string SourcePath, string DestinationPath)
        {
            foreach (string dirPath in Directory.GetDirectories(SourcePath, "*", SearchOption.AllDirectories))
            {
                try
                {
                    Directory.CreateDirectory(dirPath.Replace(SourcePath, DestinationPath));
                }
                catch (PathTooLongException e)
                {
                    Console.WriteLine(e.Message);
                }
            }

            foreach (string newPath in Directory.GetFiles(SourcePath, "*.*", SearchOption.AllDirectories))
            {
                try
                {
                    File.Copy(newPath, newPath.Replace(SourcePath, DestinationPath), true);
                }
                catch (DirectoryNotFoundException e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }

        public static void CleanupServerStudio(this TestLauncher build, bool Force = true, string JobName = "")
        {
            if (!string.IsNullOrEmpty(build.ServerPath) && File.Exists(build.ServerPath))
            {
                string serverStartedFile = Path.Combine(Path.GetDirectoryName(build.ServerPath), "ServerStarted");
                if (File.Exists(serverStartedFile))
                {
                    File.Delete(serverStartedFile);
                }
            }

            //Find Webs
            if (string.IsNullOrEmpty(build.WebsPath))
            {
                build.WebsPath = Path.Combine(build.TestRunner.TestsPath, "_PublishedWebsites", "Dev2.Web");
                if (!File.Exists(build.WebsPath) && !String.IsNullOrEmpty(build.ServerPath))
                {
                    build.WebsPath = Path.Combine(Path.GetDirectoryName(build.ServerPath), "_PublishedWebsites", "Dev2.Web");
                }
            }
            else
            {
                if (!Directory.Exists(build.WebsPath))
                {
                    throw new ArgumentException("No webs folder found at " + build.WebsPath);
                }
            }

            //Find Studio
            if (string.IsNullOrEmpty(build.StudioPath))
            {
                bool foundStudio = build.TryFindWarewolfStudioExe(out string studioPath);
                if (foundStudio)
                {
                    build.StudioPath = studioPath;
                }
            }
            else
            {
                if (!File.Exists(build.StudioPath))
                {
                    throw new ArgumentException("No studio found at " + build.StudioPath);
                }
            }
            if (!string.IsNullOrEmpty(build.StudioPath))
            {
                string studioStartedFile = Path.Combine(Path.GetDirectoryName(build.StudioPath), "StudioStarted");
                if (File.Exists(studioStartedFile))
                {
                    File.Delete(studioStartedFile);
                }
            }

            int WaitForCloseTimeout = Force ? 10 : 1800;
            int WaitForCloseRetryCount = Force ? 1 : 10;

            //Stop Studio
            Process process = ProcessUtils.StartProcess("taskkill", "/im \"Warewolf Studio.exe\"");
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

            if (!build.StartServerAsConsole)
            {
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
                    var allServerProcesses = Process.GetProcessesByName("Warewolf Server");
                    if (allServerProcesses.Length > 0)
                    {
                        allServerProcesses[0].WaitForExit(WaitForCloseTimeout);
                    }
                }
            }
            process.StartInfo.Arguments = "/im \"Warewolf Server.exe\" /f";
            process.Start();
            process.StartInfo.Arguments = "/im \"operadriver.exe\" /f";
            process.Start();
            process.StartInfo.Arguments = "/im \"geckodriver.exe\" /f";
            process.Start();
            process.StartInfo.Arguments = "/im \"IEDriverServer.exe\" /f";
            process.Start();
            if (Force)
            {
                process.StartInfo.Arguments = "/im \"opencover.console.exe\" /f";
                process.Start();
                process.StartInfo.Arguments = "/im \"dotcover.exe\" /f";
                process.Start();
            }

            //Delete Certain Studio and Server Resources
            foreach (var FileOrFolder in ToClean)
            {
                var ActualPath = Environment.ExpandEnvironmentVariables(FileOrFolder);
                if (File.Exists(ActualPath))
                {
                    WaitForFileUnlock(ActualPath);
                    File.Delete(ActualPath);
                }
                if (Directory.Exists(ActualPath))
                {
                    WaitForFolderUnlock(ActualPath);
                    Directory.Delete(ActualPath, true);
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

            //Publish Certain Studio and Server Resources
            foreach (var FileOrFolder in ToPublish)
            {
                var ActualPath = Environment.ExpandEnvironmentVariables(FileOrFolder);
                if (Directory.Exists(ActualPath))
                {
                    MoveFolderToTestResults(Environment.ExpandEnvironmentVariables(ActualPath), $"{JobName} Server {Path.GetFileName(ActualPath)} Folder", build.TestRunner.TestsResultsPath);
                }
            }
        }

        public static void MoveArtifactsToTestResults(this TestLauncher build, bool DotCover, bool Server, bool Studio, string jobName)
        {
            string serverOpenCoverSnapshot = Path.Combine(build.TestRunner.TestsResultsPath, $"{jobName} Server OpenCover Output.xml");
            string studioOpenCoverSnapshot = Path.Combine(build.TestRunner.TestsResultsPath, $"{jobName} Studio OpenCover Output.xml");
            if ((Server && File.Exists(serverOpenCoverSnapshot)) || (Server && Studio && File.Exists(serverOpenCoverSnapshot) && File.Exists(studioOpenCoverSnapshot)))
            {
                DotCover = false;
            }
            build.TestRunner.WritePlaylist(jobName);

            string containerLogFile = Environment.ExpandEnvironmentVariables(@"%ProgramData%\Warewolf\Server Log\Container Launcher.log");
            if (File.Exists(containerLogFile))
            {
                WaitForFileUnlock(containerLogFile);
                MoveFileToTestResults(containerLogFile, $"{jobName} Container Launcher.log", build.TestRunner.TestsResultsPath);
            }

            if (Studio)
            {
                string studioLogFile = Environment.ExpandEnvironmentVariables(@"%LocalAppData%\Warewolf\Studio Logs\Warewolf Studio.log");
                WaitForFileUnlock(studioLogFile);
                MoveFileToTestResults(studioLogFile, $"{jobName} Studio.log", build.TestRunner.TestsResultsPath);
            }
            if (Studio && DotCover)
            {
                var StudioSnapshot = Environment.ExpandEnvironmentVariables(@"%LocalAppData%\Warewolf\Studio Logs\dotCover.dcvr");
                Console.WriteLine($"Trying to move Studio coverage snapshot file from {StudioSnapshot} to {build.TestRunner.TestsResultsPath}\\{jobName} Studio DotCover.dcvr");
                var exists = WaitForFileExist(StudioSnapshot);
                if (exists)
                {
                    var locked = WaitForFileUnlock(StudioSnapshot);
                    if (!(locked))
                    {
                        Console.WriteLine($"Moving Studio coverage snapshot file from StudioSnapshot to {build.TestRunner.TestsResultsPath}\\{jobName} Studio DotCover.dcvr");
                        CopyOnWrite($"{build.TestRunner.TestsResultsPath}\\{jobName} Studio DotCover.dcvr");
                        File.Move(StudioSnapshot, $"{build.TestRunner.TestsResultsPath}\\{jobName} Studio DotCover.dcvr");
                    }
                    else
                    {
                        Console.WriteLine("Studio Coverage Snapshot File is locked.");
                    }
                }
                else
                {
                    throw new FileNotFoundException($"Studio coverage snapshot not found at {StudioSnapshot}");
                }
                if (File.Exists(Environment.ExpandEnvironmentVariables(@"%LocalAppData%\Warewolf\Studio Logs\dotCover.log")))
                {
                    MoveFileToTestResults(Environment.ExpandEnvironmentVariables(@"%LocalAppData%\Warewolf\Studio Logs\dotCover.log"), $"{jobName} Studio DotCover.log", build.TestRunner.TestsResultsPath);
                }
            }
            if (Server)
            {
                string serverLogFile = Environment.ExpandEnvironmentVariables(@"%ProgramData%\Warewolf\Server Log\wareWolf-Server.log");
                WaitForFileUnlock(serverLogFile);
                MoveFileToTestResults(serverLogFile, $"{jobName} Server.log", build.TestRunner.TestsResultsPath);

                string myWarewolfIoLogFile = Environment.ExpandEnvironmentVariables(@"%ProgramData%\Warewolf\Server Log\my.warewolf.io.log");
                WaitForFileUnlock(serverLogFile);
                MoveFileToTestResults(myWarewolfIoLogFile, $"{jobName} my.warewolf.io Server.log", build.TestRunner.TestsResultsPath);

                string myWarewolfIoErrorsLogFile = Environment.ExpandEnvironmentVariables(@"%ProgramData%\Warewolf\Server Log\my.warewolf.io.errors.log");
                WaitForFileUnlock(myWarewolfIoErrorsLogFile);
                MoveFileToTestResults(myWarewolfIoErrorsLogFile, $"{jobName} my.warewolf.io Server Errors.log", build.TestRunner.TestsResultsPath);
            }
            if (Server && DotCover)
            {
                var ServerSnapshot = Environment.ExpandEnvironmentVariables(@"%ProgramData%\Warewolf\Server Log\dotCover.dcvr");
                Console.WriteLine($"Trying to move Server coverage snapshot file from {ServerSnapshot} to {build.TestRunner.TestsResultsPath}\\{jobName} Server DotCover.dcvr");
                var exists = WaitForFileExist(ServerSnapshot);
                if (exists)
                {
                    var locked = WaitForFileUnlock(ServerSnapshot);
                    if (!locked)
                    {
                        Console.WriteLine($"Moving Server coverage snapshot file from {ServerSnapshot} to {build.TestRunner.TestsResultsPath}\\{jobName} Server DotCover.dcvr");
                        MoveFileToTestResults(ServerSnapshot, $"{jobName} Server DotCover.dcvr", build.TestRunner.TestsResultsPath);
                    }
                    else
                    {
                        Console.WriteLine("Server Coverage Snapshot File still locked after retrying for 2 minutes.");
                    }
                }
                if (File.Exists(Environment.ExpandEnvironmentVariables(@"%ProgramData%\Warewolf\Server Log\dotCover.log")))
                {
                    MoveFileToTestResults(Environment.ExpandEnvironmentVariables(@"%ProgramData%\Warewolf\Server Log\dotCover.log"), $"{jobName} Server DotCover.log", build.TestRunner.TestsResultsPath);
                }
                if (File.Exists(Environment.ExpandEnvironmentVariables(@"%ProgramData%\Warewolf\Server Log\my.warewolf.io.log")))
                {
                    MoveFileToTestResults(Environment.ExpandEnvironmentVariables(@"%ProgramData%\Warewolf\Server Log\my.warewolf.io.log"), $"{jobName} my.warewolf.io.log", build.TestRunner.TestsResultsPath);
                }
                if (File.Exists(Environment.ExpandEnvironmentVariables(@"%ProgramData%\Warewolf\Server Log\my.warewolf.io.errors.log")))
                {
                    MoveFileToTestResults(Environment.ExpandEnvironmentVariables(@"%ProgramData%\Warewolf\Server Log\my.warewolf.io.errors.log"), $"{jobName} my.warewolf.io Errors.log", build.TestRunner.TestsResultsPath);
                }
            }
            if (Server && Studio && DotCover)
            {
                build.TestCoverageReportGenerator.GenerateCoverageReport(Path.Combine(build.TestRunner.TestsResultsPath, $"{jobName} Merged Server and Studio DotCover"), Path.Combine(build.TestRunner.TestsResultsPath, "ServerAndStudioDotCoverSnapshot"));
            }
            if (build.RecordScreen != null)
            {
                MoveScreenRecordingsToTestResults(build.TestRunner.TestsResultsPath);
            }
            foreach (var scriptFile in Directory.GetFiles(Path.GetDirectoryName(build.TestRunner.TestsResultsPath)))
            {
                if (Path.GetFileName(scriptFile).StartsWith("Run ") && Path.GetExtension(scriptFile) == ".bat")
                {
                    MoveFileToTestResults(scriptFile, Path.GetFileName(scriptFile), build.TestRunner.TestsResultsPath);
                }
            }
        }

        public static bool WaitForFileUnlock(string FileSpec)
        {
            if (!File.Exists(FileSpec))
            {
                return false;
            }
            var locked = true;
            var RetryCount = 0;
            while (locked && RetryCount < 100)
            {
                RetryCount++;
                try
                {
                    File.OpenWrite(FileSpec).Close();
                    locked = false;
                }
                catch
                {
                    Console.WriteLine($"Still waiting for {FileSpec} file to unlock.");
                    Thread.Sleep(3000);
                }
            }
            return locked;
        }

        public static bool WaitForFolderUnlock(string FolderSpec)
        {
            var locked = true;
            foreach (var file in Directory.GetFiles(FolderSpec, "*", SearchOption.AllDirectories))
            {
                var RetryCount = 0;
                while (locked && RetryCount < 100)
                {
                    RetryCount++;
                    if (WaitForFileUnlock(file))
                    {
                        Console.WriteLine($"Still waiting for {FolderSpec} folder to unlock.");
                        Thread.Sleep(3000);
                    }
                    else
                    {
                        locked = false;
                    }
                }
            }
            return locked;
        }

        public static bool WaitForFileExist(string FileSpec, int timeout=3)
        {
            var exists = false;
            var RetryCount = 0;
            while (!exists && RetryCount < timeout*20)
            {
                RetryCount++;
                if (File.Exists(FileSpec))
                {
                    exists = true;
                }
                else
                {
                    Console.WriteLine($"Still waiting for {FileSpec} file to exist.");
                    Thread.Sleep(3000);
                }
            }
            return exists;
        }

        static void MoveScreenRecordingsToTestResults(string TestsResultsPath)
        {
            var ScreenRecordingsFolder = GetLatestScreenRecordingsFolder(TestsResultsPath);
            if (!string.IsNullOrEmpty(ScreenRecordingsFolder))
            {
                Console.WriteLine($"Getting UI test screen recordings from \"{TestsResultsPath}\"");
                string directoryToRemove = Path.Combine(ScreenRecordingsFolder + "\\In");
                if (Directory.Exists(directoryToRemove))
                {
                    foreach (var subDir in Directory.GetDirectories(directoryToRemove))
                    {
                        string subDirName = Path.GetFileName(subDir);
                        string newDirFullPath = Path.Combine(ScreenRecordingsFolder, subDirName);
                        Directory.Move(subDir, newDirFullPath);
                    }
                    try
                    {
                        Directory.Delete(directoryToRemove);
                    }
                    catch (IOException e)
                    {
                        Console.WriteLine(e.InnerException == null ? e.Message : e.InnerException.Message);
                    }
                }
                else
                {
                    Console.WriteLine(directoryToRemove + " not found.");
                }
            }
        }

        static string GetLatestScreenRecordingsFolder(string TestsResultsPath)
        {
            var directory = new DirectoryInfo(TestsResultsPath);
            var screenRecordingFolders = directory.GetDirectories().Where((folderPath) => { return folderPath.Name.StartsWith("ScreenRecordings"); });
            if (screenRecordingFolders.Count() > 0)
            {
                return screenRecordingFolders.OrderByDescending(f => f.LastWriteTime).First().FullName;
            }
            return "";
        }
    }
}
