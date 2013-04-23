using System.IO;
using Microsoft.TeamFoundation.Build.Client;
using Microsoft.TeamFoundation.Client;
using System;
using System.Reflection;

namespace QueueBuild
{
    class Program
    {
        
        private static string LogfileName = @"BuildQueueLog.txt";

        private static string LogFile()
        {
            var loc = Assembly.GetExecutingAssembly().Location;
            var dir = Path.GetDirectoryName(loc);

            return Path.Combine(dir, LogfileName);
        }

        static void Main(string[] args)
        {
            DateTime buildTS = DateTime.Now;

            

            if (args != null && args.Length == 3)
            {
                string server = args[0].Trim();
                string project = args[1].Trim();
                string def = args[2].Trim();

                BuildQueuer qb = new BuildQueuer();

                File.WriteAllText(LogFile(), buildTS + " :: Queuing Build With Args { Server : '" + server + "', Project : '" + project +
                                  "', Definition : '" + def + "' }");

                try
                {
                    qb.Run(server, project, def);
                }
                catch(Exception e)
                {
                    File.WriteAllText(LogFile(), buildTS + " :: Execution Errors { " + e.Message + " }" );
                }

            }
            else
            {
                string argsPayload = string.Empty;
                foreach (string str in args)
                {
                    argsPayload += str + ", ";
                }
                File.WriteAllText(LogFile(), buildTS + " :: *** Arguments Error With {  " + argsPayload + " }");
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
