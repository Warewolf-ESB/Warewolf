using System.IO;
using Microsoft.TeamFoundation.Build.Client;
using Microsoft.TeamFoundation.Client;
using System;

namespace QueueBuild
{
    class Program
    {
        private static string Logfile = @"BuildQueueLog.txt";

        static void Main(string[] args)
        {
            DateTime buildTS = DateTime.Now;

            if (args != null && args.Length == 3)
            {
                string server = args[0].Trim();
                string project = args[1].Trim();
                string def = args[2].Trim();

                BuildQueuer qb = new BuildQueuer();

                File.WriteAllText(Logfile, buildTS + " :: Queuing Build With Args { Server : '" + server + "', Project : '" + project +
                                  "', Definition : '" + def + "' }");

                qb.Run(server, project, def);

            }
            else
            {
                string argsPayload = string.Empty;
                foreach (string str in args)
                {
                    argsPayload += str + ", ";
                }
                File.WriteAllText(Logfile, buildTS + " :: *** Arguments Error With {  " + argsPayload + " }");
            }
        }
    }

    public class BuildQueuer
    {

        public void Run(string server, string project, string def)
        {

            TeamFoundationServer tfs = TeamFoundationServerFactory.GetServer(server);
            IBuildServer buildServer = (IBuildServer)tfs.GetService(typeof(IBuildServer));
            IBuildDefinition buildDef = buildServer.GetBuildDefinition(project, def);

            IBuildRequest req = buildDef.CreateBuildRequest();

            req.BuildServer.QueueBuild(req);
        }

    }
}
