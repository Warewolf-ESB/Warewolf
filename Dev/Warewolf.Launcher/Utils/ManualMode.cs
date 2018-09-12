using System;
using System.IO;

namespace Warewolf.Launcher.Utils
{
    class ManualMode
    {
        public static void Run(TestLauncher build)
        {
            build.InstallServer();
            build.CleanupServerStudio();
            build.Startmywarewolfio();
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
            build.MoveArtifactsToTestResults(build.ApplyDotCover, true, true, build.JobName);
        }
    }
}
