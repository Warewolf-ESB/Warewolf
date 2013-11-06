using System.Collections.Generic;
using System.IO;
using Microsoft.TeamFoundation.Build.Client;
using Microsoft.TeamFoundation.Build.Workflow.Activities;
using Microsoft.TeamFoundation.Client;
using System;
using System.Reflection;
using Microsoft.TeamFoundation.Build.Workflow;

namespace QueueBuild
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
                foreach (string str in args)
                {
                    argsPayload += str + ", ";
                }
                File.WriteAllText(LogFile(), buildTS + " :: *** Arguments Error With {  " + argsPayload + " }");
            }

            string server = args[0].Trim();
            string project = args[1].Trim();
            string def = args[2].Trim();
            string user = args[3].Trim();
            string shelveSet = args.Length > 4 ? args[4].Trim() : string.Empty;
            string agentTag = args.Length > 5 ? args[5].Trim() : string.Empty;

            BuildQueuer qb = new BuildQueuer();

            File.WriteAllText(LogFile(), buildTS + " :: Queuing Build With Args { Server : '" + server + "', Project : '" + project +
                                "', Definition : '" + def + "', for User : '" + user + "', with shelveset : '" + shelveSet + "', and running on agents with tag : '" + agentTag + "'}");

            try
            {
                return qb.Run(server, project, def, shelveSet, user, agentTag);
            }
            catch (Exception e)
            {
                File.WriteAllText(LogFile(), buildTS + " :: Execution Errors { " + e.Message + " }");
            }

            return -1; // 
        }
    }

    public class BuildQueuer
    {

        public int Run(string server, string project, string def, string shelveSet, string user, string agentTag)
        {

            TeamFoundationServer tfs = TeamFoundationServerFactory.GetServer(server);
            IBuildServer buildServer = (IBuildServer)tfs.GetService(typeof(IBuildServer));
            IBuildDefinition buildDef = buildServer.GetBuildDefinition(project, def);

            IBuildRequest req = buildDef.CreateBuildRequest();
            if(!string.IsNullOrEmpty(agentTag))
            {
                buildDef.ProcessParameters = UpdateAgentTag(buildDef.ProcessParameters, agentTag);
            }
            
            // is there a shelveset?
            if (!string.IsNullOrEmpty(shelveSet))
            {
                req.ShelvesetName = shelveSet;
                req.Reason = BuildReason.ValidateShelveset;
            }
            req.RequestedFor = user;

            IQueuedBuild qReq = req.BuildServer.QueueBuild(req);

            return qReq.Id;

        }

        private static string UpdateAgentTag(string processParameters, string agentTag)
        {
            IDictionary<String, Object> paramValues = WorkflowHelpers.DeserializeProcessParameters(processParameters);
            var agentSettings = paramValues[ProcessParameterMetadata.StandardParameterNames.AgentSettings] as AgentSettings;
            if (agentSettings != null)
            {
                agentSettings.Tags.Clear();
                agentSettings.Tags.Add(agentTag);
                paramValues[ProcessParameterMetadata.StandardParameterNames.AgentSettings] = agentSettings;
            }
            return WorkflowHelpers.SerializeProcessParameters(paramValues);
        }

    }
}
