using System.Collections.Generic;
using System.IO;
using System.Xaml;
using Microsoft.TeamFoundation.Build.Client;
using Microsoft.TeamFoundation.Client;
using System;
using System.Reflection;

namespace QueueCIBuild
{
    class Program
    {

        private static string LogfileName = @"BuildQueueLog.txt";

        public static string LogFile()
        {
            var loc = Assembly.GetExecutingAssembly().Location;
            var dir = Path.GetDirectoryName(loc);

            return Path.Combine(dir, LogfileName);
        }

        static int Main(string[] args)
        {
            DateTime buildTS = DateTime.Now;

            if(args == null || args.Length < 3)
            {
                string argsPayload = string.Empty;
                foreach(string str in args)
                {
                    argsPayload += str + ", ";
                }
                File.WriteAllText(LogFile(), buildTS + " :: *** Arguments Error With {  " + argsPayload + " }");
            }

            string server = args[0].Trim();
            string project = args[1].Trim();
            string def = args[2].Trim();
            string user = args[3].Trim();
            string changeSetID = args.Length > 4 ? args[4].Trim() : string.Empty;

            BuildQueuer qb = new BuildQueuer();

            File.WriteAllText(LogFile(), buildTS + " :: Queuing Build With Args { Server : '" + server + "', Project : '" + project +
                                "', Definition : '" + def + "', for User : '" + user + "', for changeset : '" + changeSetID + "'}");

            try
            {
                return qb.Run(server, project, def, user, changeSetID);
            }
            catch(Exception e)
            {
                File.WriteAllText(LogFile(), buildTS + " :: Execution Errors { " + e.Message + " }");
            }

            return -1; // 
        }
    }

    public class BuildQueuer
    {

        public int Run(string server, string project, string def, string user, string changeSetID)
        {
            var tfs = TeamFoundationServerFactory.GetServer(server);
            var buildServer = (IBuildServer)tfs.GetService(typeof(IBuildServer));
            var buildDef = buildServer.GetBuildDefinition(project, def);
            var req = buildDef.CreateBuildRequest();

            // is there a changeset?
            if(!string.IsNullOrEmpty(changeSetID))
            {
                req.ProcessParameters = UpdateAgentTag(changeSetID);
            }
            req.RequestedFor = user;

            IQueuedBuild qReq = req.BuildServer.QueueBuild(req);

            return qReq.Id;

        }

        private static string UpdateAgentTag(string changeSetID)
        {
            IDictionary<String, Object> paramValues = new Dictionary<string, object>();
            paramValues.Add("SpecifiedChangeSet", changeSetID);
            paramValues.Add("UseStagedBuild", false);
            return XamlServices.Save(paramValues);
        }

    }
}
