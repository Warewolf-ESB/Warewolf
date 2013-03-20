using Microsoft.TeamFoundation.Build.Client;
using Microsoft.TeamFoundation.Client;
using System;
using System.Activities;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dev2.TfsPlugins
{
    [BuildActivity(HostEnvironmentOption.Agent)]
    public sealed class QueueBuild : CodeActivity
    {

        [RequiredArgument]
        public InArgument<IBuildDetail> BuildDetail { get; set; }

        [RequiredArgument]
        public InArgument<String> BuildDefinition { get; set; }

        protected override void Execute(CodeActivityContext context)
        {
            string buildDef = context.GetValue(this.BuildDefinition);
            IBuildDetail buildDetail = context.GetValue(this.BuildDetail);

            bool queuedBld = false;

            if (!string.IsNullOrEmpty(buildDef) && buildDetail != null)
            {
                var workspace = buildDetail.BuildDefinition.Workspace;

                if (workspace != null)
                {
                    string proj = buildDetail.BuildDefinition.TeamProject;

                    if (!string.IsNullOrEmpty(proj))
                    {
                        string configURI = buildDetail.BuildServer.TeamProjectCollection.Uri.ToString();

                        if (!string.IsNullOrEmpty(configURI))
                        {
                            TfsTeamProjectCollection projCollection = new TfsTeamProjectCollection(new Uri(configURI));

                            projCollection.EnsureAuthenticated();

                            IBuildServer bldServer = (IBuildServer)projCollection.GetService(typeof(IBuildServer));

                            IBuildDefinition queueBldDef = bldServer.GetBuildDefinition(proj, buildDef);

                            if (bldServer != null && queueBldDef != null)
                            {
                                bldServer.QueueBuild(queueBldDef);
                                queuedBld = true;
                            }
                        }
                    }
                }
            }

            if (!queuedBld)
            {
                throw new Exception("Failed to queue build");
            }
        }

    }
}
