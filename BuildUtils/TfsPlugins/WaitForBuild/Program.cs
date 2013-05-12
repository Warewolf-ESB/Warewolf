using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.TeamFoundation.Build.Client;
using Microsoft.TeamFoundation.Client;

namespace WaitForBuild
{
    class WaitForProgram
    {
        static int Main(string[] args)
        {
           

            string server = "http://rsaklfsvrgendev:8080/tfs/";
            string project = "DEV2 SCRUM Project";
            string def = "Async Integration Run - Travis";
            int id;
            Int32.TryParse(args[0], out id);

            TeamFoundationServer tfs = TeamFoundationServerFactory.GetServer(server);
            IBuildServer buildServer = (IBuildServer)tfs.GetService(typeof(IBuildServer));
            IBuildDefinition buildDef = buildServer.GetBuildDefinition(project, def);

            BuildStatusWatcher bsw = new BuildStatusWatcher(id);

            bsw.Connect(buildServer, project);

            do
            {
            } while (bsw.Status != QueueStatus.Completed && bsw.Status != QueueStatus.Canceled);

            bsw.Disconnect();

            if (bsw.Build.CompilationStatus == BuildPhaseStatus.Succeeded)
            {
                return 0; // success ;)
            }
            else
            {
                return 1; // failure ;(
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
                _queuedBuildsView.Connect(10000, null);
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
