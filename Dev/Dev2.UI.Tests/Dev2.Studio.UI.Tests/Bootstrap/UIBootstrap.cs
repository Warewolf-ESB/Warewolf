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

        /// <summary>
        /// Setup for coded ui if setup file found
        /// </summary>
        /// <param name="textCtx">The text CTX.</param>
        [AssemblyInitialize()]
        public static void Init(TestContext textCtx)
        {
            const string runBatFileDir = _exeRoot + "\\" + _runFileName;
            if(File.Exists(runBatFileDir))
            {
                var proc = new Process();
                proc.StartInfo.FileName = runBatFileDir;
                proc.StartInfo.RedirectStandardError = true;
                proc.StartInfo.RedirectStandardOutput = true;
                proc.StartInfo.UseShellExecute = true;
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
                proc.StartInfo.UseShellExecute = true;
                proc.Start();
            }
        }
    }
}
