using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
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

            if(args.Length == 3)
            {
                try
                {
                    string server = args[0].Trim();
                    string project = args[1].Trim();
                    int id = -1;
                    var tmp = args[2].Trim();

                    TeamFoundationServer tfs = TeamFoundationServerFactory.GetServer(server);
                    IBuildServer buildServer = (IBuildServer)tfs.GetService(typeof(IBuildServer));

                    if(tmp.IndexOf(",", StringComparison.Ordinal) < 0)
                    {
                        Int32.TryParse(args[2].Trim(), out id);

                        BuildStatusWatcher bsw = new BuildStatusWatcher(id);

                        bsw.Connect(buildServer, project);

                        do
                        {
                            Thread.Sleep(1000);
                        } while(bsw.Status != QueueStatus.Completed && bsw.Status != QueueStatus.Canceled);

                        bsw.Disconnect();

                        // ensure both the build and test passed ;)
                        if(bsw.Build.CompilationStatus == BuildPhaseStatus.Succeeded && bsw.Build.TestStatus == BuildPhaseStatus.Succeeded)
                        {
                            return 0; // success ;)
                        }
                        else
                        {
                            return 1; // failure ;(
                        }
                    }
                    else
                    {
                        // we have a list to process ;)

                        var ids = tmp.Split(',');
                        int[] vals = new int[ids.Length];
                        int pos = 0;

                        // build collection
                        foreach(var val in ids)
                        {
                            if(Int32.TryParse(val, out id))
                            {
                                vals[pos] = id;
                                pos++;
                            }
                        }

                        BuildStatusWatcher[] watchers = new BuildStatusWatcher[ids.Length];
                        pos = 0;

                        int returnStatus = 0;

                        // process collection ;)
                        foreach(var pid in vals)
                        {
                            if(pid > 0)
                            {
                                watchers[pos] = new BuildStatusWatcher(pid);

                                watchers[pos].Connect(buildServer, project);

                                pos++;
                            }
                        }

                        do
                        {
                            Thread.Sleep(5000);
                        } while(!IsCollectionFinished(watchers));

                        // check statuses ;)
                        foreach(var v in watchers)
                        {
                            v.Disconnect();
                            // ensure both the build and test passed ;)
                            if(v.Build.CompilationStatus == BuildPhaseStatus.Succeeded &&
                                v.Build.TestStatus == BuildPhaseStatus.Succeeded)
                            {
                                returnStatus += 0; // success ;)
                            }
                            else
                            {
                                returnStatus += 1; // failure ;(
                            }
                        }

                        return returnStatus;


                    }

                }
                catch(Exception e)
                {
                    File.WriteAllText(LogFile(), DateTime.Now + " :: Execution Errors { " + e.Message + " } with arg 0 { " + args[0] + " } arg 1 { " + args[1] + " } arg 2 { " + args[2] + " }");
                    return 3; // exception failure ;(
                }
            }
            else
            {
                return 2; // failure with args ;(
            }

            return 99;
        }

        /// <summary>
        /// Determines whether [is collection finished] [the specified col].
        /// </summary>
        /// <param name="col">The col.</param>
        /// <returns>
        ///   <c>true</c> if [is collection finished] [the specified col]; otherwise, <c>false</c>.
        /// </returns>
        private static bool IsCollectionFinished(IEnumerable<BuildStatusWatcher> col)
        {
            bool result = true;

            foreach(var v in col)
            {
                if(v != null)
                {
                    if(v.Status != QueueStatus.Completed && v.Status != QueueStatus.Canceled)
                    {
                        result = false;
                    }
                }
            }

            return result;
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

            private int _nullHits = 0;
            private readonly int _timeoutNullHits = 10;

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
                var queuedBuild = _queuedBuildsView.QueuedBuilds.FirstOrDefault(x => x.Id == _queueBuildId);
                if(queuedBuild != null)
                {
                    _status = queuedBuild.Status;
                    _build = queuedBuild.Build;
                }
                else
                {
                    _nullHits++;

                    if(_nullHits == _timeoutNullHits)
                    {
                        _status = QueueStatus.Canceled;
                    }
                }
            }
        }
    }
}
