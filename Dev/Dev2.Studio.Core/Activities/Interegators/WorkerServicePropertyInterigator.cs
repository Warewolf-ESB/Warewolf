
using System.Xml;
using Dev2.Studio.Core.Interfaces;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Studio.Core.Activities.Interegators
{
    public static class WorkerServicePropertyInterigator
    {
        // string xml

        public static void SetActivityProperties(IContextualResourceModel resource, ref DsfActivity activity)
        {
            activity.IsWorkflow = false;

            if (resource.WorkflowXaml != null)
            {
            XmlDocument document = new XmlDocument();
            document.LoadXml(resource.WorkflowXaml);

            if(document.DocumentElement != null)
            {
                XmlNode node = document.SelectSingleNode("//Action");
                if(node != null)
                {
                    if(node.Attributes != null)
                    {
                        var attr = node.Attributes["SourceName"];
                        if(attr != null)
                        {
                            activity.FriendlySourceName = attr.Value;
                        }

                        attr = node.Attributes["Type"];
                        if(attr != null)
                        {
                            activity.Type = attr.Value;
                        }

                        attr = node.Attributes["SourceMethod"];
                        if(attr != null)
                        {
                            activity.ActionName = attr.Value;
                        }
                    }
                }
            }
            }
        }
    }
}
