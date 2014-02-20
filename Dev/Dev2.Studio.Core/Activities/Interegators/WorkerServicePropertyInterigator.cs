using System.Xml;
using Dev2.Common.Common;
using Dev2.Studio.Core.Interfaces;
using Unlimited.Applications.BusinessDesignStudio.Activities;

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.Core.Activities.Interegators
{
    public static class WorkerServicePropertyInterigator
    {
        public static void SetActivityProperties(IContextualResourceModel resource, ref DsfActivity activity)
        {
            activity.IsWorkflow = false;

            if(resource.WorkflowXaml != null && resource.WorkflowXaml.Length > 0)
            {

                var startIdx = resource.WorkflowXaml.IndexOf("<Action ", 0, true);

                if(startIdx >= 0)
                {
                    var endIdx = resource.WorkflowXaml.IndexOf(">", startIdx, true);
                    if(endIdx > 0)
                    {
                        var len = (endIdx - startIdx) + 1;
                        var fragment = resource.WorkflowXaml.Substring(startIdx, len);

                        fragment += "</Action>";
                        fragment = fragment.Replace("&", "&amp;");
                        XmlDocument document = new XmlDocument();

                        document.LoadXml(fragment);

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
            activity.Type = resource.ServerResourceType;
        }
    }
}
