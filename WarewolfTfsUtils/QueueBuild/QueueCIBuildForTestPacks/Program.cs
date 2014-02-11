using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xaml;
using Microsoft.TeamFoundation.Build.Client;
using Microsoft.TeamFoundation.Client;

namespace QueueCIBuildForTestPacks
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
            string changeSetID = args[4].Trim();
            string numberOfTestRunAgentsToQueueOn = args[5].Trim();

            BuildQueuer qb = new BuildQueuer();

            File.WriteAllText(LogFile(), buildTS + " :: Queuing Build With Args { Server : '" + server + "', Project : '" + project +
                                "', Definition : '" + def + "', for User : '" + user + "', for changeset : '" + changeSetID +
                                "' and number of test agents to queue on : '" + numberOfTestRunAgentsToQueueOn + "'}");

            try
            {
                return qb.Run(server, project, def, user, changeSetID, numberOfTestRunAgentsToQueueOn);
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

        public int Run(string server, string project, string def, string user, string changeSetID, string numberOfTestRunAgentsToQueueOn)
        {
            var tfs = TeamFoundationServerFactory.GetServer(server);
            var buildServer = (IBuildServer)tfs.GetService(typeof(IBuildServer));
            var buildDef = buildServer.GetBuildDefinition(project, def);
            var req = buildDef.CreateBuildRequest();

            // is there a changeset?
            if(!string.IsNullOrEmpty(changeSetID))
            {
                req.ProcessParameters = UpdateAgentTag(changeSetID, numberOfTestRunAgentsToQueueOn);
            }
            req.RequestedFor = user;

            IQueuedBuild qReq = req.BuildServer.QueueBuild(req);

            return qReq.Id;

        }

        private static string UpdateAgentTag(string changeSetID, string numberOfTestRunAgentsToQueueOn)
        {
            IDictionary<String, Object> paramValues = new Dictionary<string, object>();
            paramValues.Add("SpecifiedChangeSet", changeSetID);
            paramValues.Add("NumberOfTestRunAgentsToQueueOn", numberOfTestRunAgentsToQueueOn);
            return XamlServices.Save(paramValues);
        }

    }
}
