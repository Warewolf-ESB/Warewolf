using System.Diagnostics;
using System.Reflection;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Integration.Tests.Init
{
    public class EnviormentBootstrap
    {

        static Process serverProc;

        [AssemblyInitialize]
        public static void TestInitialize(TestContext testContext)
        {
            // Start up the server here ;)
            string file = (new System.Uri(Assembly.GetExecutingAssembly().CodeBase)).AbsolutePath;

            serverProc = new Process();

            serverProc.StartInfo.FileName = file = "/Dev2.Server.exe";

            serverProc.Start();

        }

        [AssemblyCleanup]
        public static void TestTerminate()
        {
            // kill the server here ;)
            serverProc.Kill();
            // wait for it to die ;)
            Thread.Sleep(1500);
        }
    }
}
