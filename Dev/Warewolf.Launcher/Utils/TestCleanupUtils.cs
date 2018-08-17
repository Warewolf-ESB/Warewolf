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
            if (File.Exists(FileSpec))
            {
                var num = 1;
                var FileExtention = Path.GetExtension(FileSpec);
                var FileSpecWithoutExtention = FileSpec.Substring(0, FileSpec.LastIndexOf('.') + 1);
                while (File.Exists($"{FileSpecWithoutExtention}{num}{FileExtention}"))
                {
                    num++;
                }
                File.Move(FileSpec, $"{FileSpecWithoutExtention}{num}{FileExtention}");
            }
            else if (Directory.Exists(FileSpec))
            {
                var num = 1;
                while (Directory.Exists($"{FileSpec}{num}"))
                {
                    num++;
                }
                Directory.Move(FileSpec, $"{FileSpec}{num}");
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

        public static void CleanupServerStudio(this TestLauncher build, bool Force = true)
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

            if (String.IsNullOrEmpty(build.JobName))
            {
                build.JobName = "Test Run";
            }

            //Publish Certain Studio and Server Resources
            foreach (var FileOrFolder in ToPublish)
            {
                var ActualPath = Environment.ExpandEnvironmentVariables(FileOrFolder);
                if (Directory.Exists(ActualPath))
                {
                    MoveFolderToTestResults(Environment.ExpandEnvironmentVariables(ActualPath), $"{build.JobName} Server {Path.GetFileName(ActualPath)} Folder", build.TestRunner.TestsResultsPath);
                }
            }
        }

        public static void MoveArtifactsToTestResults(this TestLauncher build, bool DotCover, bool Server, bool Studio)
        {
            foreach (var FullTRXFilePath in Directory.GetFiles(build.TestRunner.TestsResultsPath, "*.trx"))
            {
                XmlDocument trxContent = new XmlDocument();
                trxContent.Load(FullTRXFilePath);
                var namespaceManager = new XmlNamespaceManager(trxContent.NameTable);
                namespaceManager.AddNamespace("a", "http://microsoft.com/schemas/VisualStudio/TeamTest/2010");
                if (trxContent.DocumentElement.SelectSingleNode("/a:TestRun/a:ResultSummary", namespaceManager).Attributes["outcome"].Value != "Completed")
                {
                    WriteFailingTestPlaylist($"{build.TestRunner.TestsResultsPath}\\{build.JobName} Failures.playlist", FullTRXFilePath, trxContent, namespaceManager);
                }
            }

            string containerLogFile = Environment.ExpandEnvironmentVariables(@"%ProgramData%\Warewolf\Server Log\Container Launcher.log");
            if (File.Exists(containerLogFile))
            {
                WaitForFileUnlock(containerLogFile);
                MoveFileToTestResults(containerLogFile, $"{build.JobName} Container Launcher.log", build.TestRunner.TestsResultsPath);
            }

            if (Studio)
            {
                string studioLogFile = Environment.ExpandEnvironmentVariables(@"%LocalAppData%\Warewolf\Studio Logs\Warewolf Studio.log");
                WaitForFileUnlock(studioLogFile);
                MoveFileToTestResults(studioLogFile, $"{build.JobName} Studio.log", build.TestRunner.TestsResultsPath);
            }
            if (Studio && DotCover)
            {
                var StudioSnapshot = Environment.ExpandEnvironmentVariables(@"%LocalAppData%\Warewolf\Studio Logs\dotCover.dcvr");
                Console.WriteLine($"Trying to move Studio coverage snapshot file from {StudioSnapshot} to {build.TestRunner.TestsResultsPath}\\{build.JobName} Studio DotCover.dcvr");
                var exists = WaitForFileExist(StudioSnapshot);
                if (exists)
                {
                    var locked = WaitForFileUnlock(StudioSnapshot);
                    if (!(locked))
                    {
                        Console.WriteLine($"Moving Studio coverage snapshot file from StudioSnapshot to {build.TestRunner.TestsResultsPath}\\{build.JobName} Studio DotCover.dcvr");
                        CopyOnWrite($"{build.TestRunner.TestsResultsPath}\\{build.JobName} Studio DotCover.dcvr");
                        File.Move(StudioSnapshot, $"{build.TestRunner.TestsResultsPath}\\{build.JobName} Studio DotCover.dcvr");
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
                    MoveFileToTestResults(Environment.ExpandEnvironmentVariables(@"%LocalAppData%\Warewolf\Studio Logs\dotCover.log"), $"{build.JobName} Studio DotCover.log", build.TestRunner.TestsResultsPath);
                }
            }
            if (Server)
            {
                string serverLogFile = Environment.ExpandEnvironmentVariables(@"%ProgramData%\Warewolf\Server Log\wareWolf-Server.log");
                WaitForFileUnlock(serverLogFile);
                MoveFileToTestResults(serverLogFile, $"{build.JobName} Server.log", build.TestRunner.TestsResultsPath);

                string myWarewolfIoLogFile = Environment.ExpandEnvironmentVariables(@"%ProgramData%\Warewolf\Server Log\my.warewolf.io.log");
                WaitForFileUnlock(serverLogFile);
                MoveFileToTestResults(myWarewolfIoLogFile, $"{build.JobName} my.warewolf.io Server.log", build.TestRunner.TestsResultsPath);

                string myWarewolfIoErrorsLogFile = Environment.ExpandEnvironmentVariables(@"%ProgramData%\Warewolf\Server Log\my.warewolf.io.errors.log");
                WaitForFileUnlock(myWarewolfIoErrorsLogFile);
                MoveFileToTestResults(myWarewolfIoErrorsLogFile, $"{build.JobName} my.warewolf.io Server Errors.log", build.TestRunner.TestsResultsPath);
            }
            if (Server && DotCover)
            {
                var ServerSnapshot = Environment.ExpandEnvironmentVariables(@"%ProgramData%\Warewolf\Server Log\dotCover.dcvr");
                Console.WriteLine($"Trying to move Server coverage snapshot file from {ServerSnapshot} to {build.TestRunner.TestsResultsPath}\\{build.JobName} Server DotCover.dcvr");
                var exists = WaitForFileExist(ServerSnapshot);
                if (exists)
                {
                    var locked = WaitForFileUnlock(ServerSnapshot);
                    if (!locked)
                    {
                        Console.WriteLine($"Moving Server coverage snapshot file from {ServerSnapshot} to {build.TestRunner.TestsResultsPath}\\{build.JobName} Server DotCover.dcvr");
                        MoveFileToTestResults(ServerSnapshot, $"{build.JobName} Server DotCover.dcvr", build.TestRunner.TestsResultsPath);
                    }
                    else
                    {
                        Console.WriteLine("Server Coverage Snapshot File still locked after retrying for 2 minutes.");
                    }
                }
                if (File.Exists(Environment.ExpandEnvironmentVariables(@"%ProgramData%\Warewolf\Server Log\dotCover.log")))
                {
                    MoveFileToTestResults(Environment.ExpandEnvironmentVariables(@"%ProgramData%\Warewolf\Server Log\dotCover.log"), $"{build.JobName} Server DotCover.log", build.TestRunner.TestsResultsPath);
                }
                if (File.Exists(Environment.ExpandEnvironmentVariables(@"%ProgramData%\Warewolf\Server Log\my.warewolf.io.log")))
                {
                    MoveFileToTestResults(Environment.ExpandEnvironmentVariables(@"%ProgramData%\Warewolf\Server Log\my.warewolf.io.log"), $"{build.JobName} my.warewolf.io.log", build.TestRunner.TestsResultsPath);
                }
                if (File.Exists(Environment.ExpandEnvironmentVariables(@"%ProgramData%\Warewolf\Server Log\my.warewolf.io.errors.log")))
                {
                    MoveFileToTestResults(Environment.ExpandEnvironmentVariables(@"%ProgramData%\Warewolf\Server Log\my.warewolf.io.errors.log"), $"{build.JobName} my.warewolf.io Errors.log", build.TestRunner.TestsResultsPath);
                }
            }
            if (Server && Studio && DotCover)
            {
                build.TestCoverageMerger.MergeCoverageSnapshots(new List<string> { Path.Combine(build.TestRunner.TestsResultsPath, $"{build.JobName} Server DotCover.dcvr"), Path.Combine(build.TestRunner.TestsResultsPath, $"{build.JobName} Studio DotCover.dcvr") }, Path.Combine(build.TestRunner.TestsResultsPath, $"{build.JobName} Merged Server and Studio DotCover"), Path.Combine(build.TestRunner.TestsResultsPath, "ServerAndStudioDotCoverSnapshot"), build.DotCoverPath);
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

        static void WriteFailingTestPlaylist(string OutPlaylistPath, string FullTRXFilePath, XmlDocument trxContent, XmlNamespaceManager namespaceManager)
        {
            //Write failing tests playlist.
            Console.WriteLine($"Writing all test failures in \"{FullTRXFilePath}\" to a playlist file.");
            var PlayList = "<Playlist Version=\"1.0\">";
            if (trxContent.DocumentElement.SelectNodes("/a:TestRun/a:Results/a:UnitTestResult", namespaceManager).Count > 0)
            {
                foreach (XmlNode TestResult in trxContent.DocumentElement.SelectNodes("/a:TestRun/a:Results/a:UnitTestResult", namespaceManager))
                {
                    if (TestResult.Attributes["outcome"].InnerText == "Failed")
                    {
                        if (trxContent.DocumentElement.SelectNodes("/a:TestRun/a:TestDefinitions/a:UnitTest/a:TestMethod", namespaceManager).Count > 0)
                        {
                            foreach (XmlNode TestDefinition in trxContent.DocumentElement.SelectNodes("/a:TestRun/a:TestDefinitions/a:UnitTest/a:TestMethod", namespaceManager))
                            {
                                if (TestResult.Attributes["testName"] != null && TestDefinition.Attributes["name"].InnerText == TestResult.Attributes["testName"].InnerText)
                                {
                                    PlayList += "<Add Test=\"" + TestDefinition.Attributes["className"].InnerText + "." + TestDefinition.Attributes["name"].InnerText + "\" />";
                                }
                            }
                        }
                        else
                        {
                            Console.WriteLine("Error parsing /TestRun/TestDefinitions/UnitTest/TestMethod from trx file at trxFile");
                        }
                    }
                }
            }
            else
            {
                if (trxContent.DocumentElement.SelectSingleNode("/a:TestRun/a:Results/a:UnitTestResult", namespaceManager).Attributes["outcome"].InnerText == "Failed")
                {
                    PlayList += "<Add Test=\"" + trxContent.DocumentElement.SelectSingleNode("/a:TestRun/a:TestDefinitions/a:UnitTest/a:TestMethod", namespaceManager).Attributes["className"].InnerText + "." + trxContent.DocumentElement.SelectSingleNode("/a:TestRun/a:TestDefinitions/a:UnitTest/a:TestMethod", namespaceManager).Attributes["name"].InnerText + "\" />";
                }
                else
                {
                    if (trxContent.DocumentElement.SelectSingleNode("/a:TestRun/a:Results/a:UnitTestResult", namespaceManager) == null)
                    {
                        Console.WriteLine("Error parsing /TestRun/Results/UnitTestResult from trx file at " + FullTRXFilePath);
                    }
                }
            }
            PlayList += "</Playlist>";
            CopyOnWrite(OutPlaylistPath);
            File.WriteAllText(OutPlaylistPath, PlayList);
            Console.WriteLine($"Playlist file written to \"{OutPlaylistPath}\".");
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

        public static bool WaitForFileExist(string FileSpec)
        {
            var exists = false;
            var RetryCount = 0;
            while (!exists && RetryCount < 100)
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
