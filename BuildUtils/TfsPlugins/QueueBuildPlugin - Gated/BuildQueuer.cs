using System;
using System.IO;
using System.Reflection;
using Microsoft.TeamFoundation.Build.Client;
using Microsoft.TeamFoundation.Client;

namespace QueueBuildPlugin
{
    public class BuildQueuer
    {
        private static string LogfileName = @"BuildQueueLog.txt";

        private static string LogFile()
        {
            var loc = Assembly.GetExecutingAssembly().Location;
            var dir = Path.GetDirectoryName(loc);

            return Path.Combine(dir, LogfileName);
        }

        public int QueueBuild(string server, string project, string def, string shelveSet, string user)
        {
            DateTime buildTS = DateTime.Now;

            if(!string.IsNullOrEmpty(shelveSet) && !string.IsNullOrEmpty(server) && !string.IsNullOrEmpty(project) && !string.IsNullOrEmpty(def))
            {

                var qb = new BuildQueuerImpl();

                File.WriteAllText(LogFile(), buildTS + " :: Queuing Build With Args { Server : '" + server + "', Project : '" + project +
                                  "', Definition : '" + def + "', for user : '" + user + ", selveset : '" + shelveSet + "'}");
                try
                {
                    return qb.Run(server, project, def, shelveSet, user);
                }
                catch(Exception e)
                {
                    File.WriteAllText(LogFile(), buildTS + " :: Execution Errors { " + e.Message + " }");
                }

            }
            else
            {
                File.WriteAllText(LogFile(), buildTS + " :: *** Arguments Error With {  " + server + ", " + project + ", " + def + ", " + user + ", " + shelveSet + " }");
            }

            return -1;
        }
    }

    public class BuildQueuerImpl
    {
        public int Run(string server, string project, string def, string shelveSet, string user)
        {
            var tfs = TeamFoundationServerFactory.GetServer(server);
            var buildServer = (IBuildServer)tfs.GetService(typeof(IBuildServer));
            var buildDef = buildServer.GetBuildDefinition(project, def);
            var req = buildDef.CreateBuildRequest();

            // is there a shelveset?
            if(!string.IsNullOrEmpty(shelveSet))
            {
                req.ShelvesetName = shelveSet;
                req.Reason = BuildReason.ValidateShelveset;
            } 
            req.RequestedFor = user;

            var qReq = req.BuildServer.QueueBuild(req);

            return qReq.Id;
        }
    }
}
