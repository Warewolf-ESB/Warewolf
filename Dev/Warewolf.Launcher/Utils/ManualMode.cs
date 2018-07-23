using System;

namespace Warewolf.Launcher.Utils
{
    class ManualMode
    {
        public static void Run(TestLauncher build)
        {
            build.InstallServer();
            build.CleanupServerStudio();
            build.Startmywarewolfio();
            build.ciRemoteContainerLauncher = TestLauncher.TryStartLocalCIRemoteContainer(build.TestRunner.TestsResultsPath);
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
