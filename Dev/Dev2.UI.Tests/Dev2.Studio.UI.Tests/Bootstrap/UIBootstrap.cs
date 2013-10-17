using System.Diagnostics;
using System.IO;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Studio.UI.Tests.Bootstrap
{
    /// <summary>
    /// Used to bootstrap the server for coded ui test runs ;)
    /// </summary>
    [TestClass()]
    public class Bootstrap
    {
        private const string _exeRoot = @"C:\CodedUI";
        private const string _cleanupFileName = "exit.bat";
        private const string _runFileName = "run.bat";
        private const string StagingLocation = @"\\RSAKLFSVRTFSBLD\Automated Builds\DevMergeStaging";

        [AssemblyInitialize()]
        public static void Init(TestContext textCtx)
        {
            DirectoryCopy(StagingLocation, @"C:\CodedUI\Merge");
            const string runBatFileDir = _exeRoot + "\\" + _runFileName;
            if(File.Exists(runBatFileDir))
            {
                var proc = new Process();
                proc.StartInfo.FileName = runBatFileDir;
                proc.StartInfo.RedirectStandardError = true;
                proc.StartInfo.RedirectStandardOutput = true;
                proc.StartInfo.UseShellExecute = false;
                proc.Start();
                Thread.Sleep(30000);
            }
        }

        /// <summary>
        /// Teardowns this instance.
        /// </summary>
        [AssemblyCleanup()]
        public static void Teardown()
        {
            const string cleanupBatFileDir = _exeRoot + "\\" + _cleanupFileName;
            if(File.Exists(cleanupBatFileDir))
            {
                var proc = new Process();
                proc.StartInfo.FileName = cleanupBatFileDir;
                proc.StartInfo.RedirectStandardError = true;
                proc.StartInfo.RedirectStandardOutput = true;
                proc.StartInfo.UseShellExecute = false;
                proc.Start();
            }
        }

        private static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs = true)
        {
            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);
            DirectoryInfo[] dirs = dir.GetDirectories();

            if(!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }

            // If the destination directory doesn't exist, create it. 
            if(!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }

            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles();
            foreach(FileInfo file in files)
            {
                string temppath = Path.Combine(destDirName, file.Name);
                file.CopyTo(temppath, false);
            }

            // If copying subdirectories, copy them and their contents to new location. 
            if(copySubDirs)
            {
                foreach(DirectoryInfo subdir in dirs)
                {
                    string temppath = Path.Combine(destDirName, subdir.Name);
                    DirectoryCopy(subdir.FullName, temppath, copySubDirs);
                }
            }
        }
    }
}
