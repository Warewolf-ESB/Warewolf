using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.TeamFoundation.Build.Client;
using Microsoft.TeamFoundation.Client;

namespace WaitForBuild
{
    public class WaitForProgram
    {
        private static string LogfileName = @"BuildQueueLog.txt";

        private static string LogFile()
        {
            var loc = Assembly.GetExecutingAssembly().Location;
            var dir = Path.GetDirectoryName(loc);

            return Path.Combine(dir, LogfileName);
        }

        public static int Main(string[] args)
        {

            if (args.Length == 3)
            {
                try
                {
                    string server = args[0].Trim();
                    string project = args[1].Trim();
                    int id;
                    Int32.TryParse(args[2].Trim(), out id);

                    TeamFoundationServer tfs = TeamFoundationServerFactory.GetServer(server);
                    IBuildServer buildServer = (IBuildServer) tfs.GetService(typeof (IBuildServer));

                    BuildStatusWatcher bsw = new BuildStatusWatcher(id);

                    bsw.Connect(buildServer, project);

                    do
                    {
                    } while (bsw.Status != QueueStatus.Completed && bsw.Status != QueueStatus.Canceled);

                    bsw.Disconnect();

                    // ensure both the build and test passed ;)
                    if (bsw.Build.CompilationStatus == BuildPhaseStatus.Succeeded && bsw.Build.TestStatus == BuildPhaseStatus.Succeeded)
                    {
                        return 0; // success ;)
                    }
                    else
                    {
                        return 1; // failure ;(
                    }
                }
                catch (Exception e)
                {
                    File.WriteAllText(LogFile(), DateTime.Now + " :: Execution Errors { " + e.Message + " }");
                    return 3; // exception failure ;(
                }
            }
            else
            {
                return 2; // failure with args ;(
            }
        }

        public class BuildResult
        {
            public bool WasSuccessfully { get; set; }
            public IBuildDetail BuildDetail { get; set; }
        }

        public class BuildStatusWatcher
        {
            private IQueuedBuildsView _queuedBuildsView;
            private readonly int _queueBuildId;
            private QueueStatus _status;
            private IBuildDetail _build;

            public BuildStatusWatcher(int queueBuildId)
            {
                _queueBuildId = queueBuildId;
            }

            public IBuildDetail Build
            {
                get { return _build; }
            }

            public QueueStatus Status
            {
                get { return _status; }
            }

            public void Connect(IBuildServer buildServer, string tfsProject)
            {
                _queuedBuildsView = buildServer.CreateQueuedBuildsView(tfsProject);
                _queuedBuildsView.StatusChanged += QueuedBuildsViewStatusChanged;
                _queuedBuildsView.Connect(5000, null);
            }

            public void Disconnect()
            {
                _queuedBuildsView.Disconnect();
            }

            private void QueuedBuildsViewStatusChanged(object sender, StatusChangedEventArgs e)
            {
                if (e.Changed)
                {
                    var queuedBuild = _queuedBuildsView.QueuedBuilds.FirstOrDefault(x => x.Id == _queueBuildId);
                    if (queuedBuild != null)
                    {
                        _status = queuedBuild.Status;
                        _build = queuedBuild.Build;
                    }
                }
            }
        }
    }
}
