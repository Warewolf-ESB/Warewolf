using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xaml;
using Microsoft.TeamFoundation.Build.Client;
using Microsoft.TeamFoundation.Client;

namespace QueueGatedBuildForTestPacks
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
            string NumberOfTestPacks = args[4].Trim();

            BuildQueuer qb = new BuildQueuer();

            File.WriteAllText(LogFile(), buildTS + " :: Queuing Build With Args { Server : '" + server + "', Project : '" + project +
                                "', Definition : '" + def + "', for User : '" + user + "', with number of test packs : '" + NumberOfTestPacks + "'}");

            try
            {
                return qb.Run(server, project, def, NumberOfTestPacks, user);
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

        public int Run(string server, string project, string def, string NumberOfTestPacks, string user)
        {
            var tfs = TeamFoundationServerFactory.GetServer(server);
            var buildServer = (IBuildServer)tfs.GetService(typeof(IBuildServer));
            var buildDef = buildServer.GetBuildDefinition(project, def);
            var req = buildDef.CreateBuildRequest();

            req.ProcessParameters = UpdateProcessParams(NumberOfTestPacks);
            req.RequestedFor = user;

            IQueuedBuild qReq = req.BuildServer.QueueBuild(req);

            return qReq.Id;

        }

        private static string UpdateProcessParams(string NumberOfTestPacks)
        {
            IDictionary<String, Object> paramValues = new Dictionary<string, object>();
            paramValues.Add("NumberOfTestPacks", NumberOfTestPacks);
            return XamlServices.Save(paramValues);
        }

    }
}
