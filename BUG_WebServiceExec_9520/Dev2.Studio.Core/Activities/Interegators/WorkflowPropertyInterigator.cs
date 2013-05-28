
using System.Xml;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Studio.Core.Activities.Interegators
{
    public static class WorkflowPropertyInterigator
    {
        public static DsfActivity SetActivityProperties(string xml, DsfActivity activity)
        {

            XmlDocument document = new XmlDocument();
            document.LoadXml(xml);

            if (document.DocumentElement != null)
            {
                XmlNode node = document.SelectSingleNode("//HelpLink");
                if (node != null)
                {
                    activity.HelpLink = node.InnerText;
                }
            }
            activity.Type = "Workflow";
            return activity;
        }
    }
}
