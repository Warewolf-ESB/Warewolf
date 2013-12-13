using Dev2.Common.Common;
using Dev2.Studio.Core.Interfaces;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Studio.Core.Activities.Interegators
{
    public static class WorkflowPropertyInterigator
    {
        // string xml
        public static void SetActivityProperties(IContextualResourceModel resource, ref DsfActivity activity)
        {
            if (resource.WorkflowXaml != null && resource.WorkflowXaml.Length > 0)
            {
                var startIdx = resource.WorkflowXaml.IndexOf("<HelpLink>", 0, true);

                if (startIdx >= 0)
                {
                    var endIdx = resource.WorkflowXaml.IndexOf("</HelpLink>", startIdx, true);

                    if (endIdx > 0)
                    {
                        startIdx += 10;
                        var len = (endIdx - startIdx);
                        
                        activity.HelpLink = resource.WorkflowXaml.Substring(startIdx, len);
                    }
                }

            }

            if(resource.Environment != null) activity.FriendlySourceName = resource.Environment.Name;
            activity.IsWorkflow = true;
            activity.Type = "Workflow";
        }
    }
}
