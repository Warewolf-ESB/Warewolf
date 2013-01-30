
using System.Xml;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Studio.Core.Activities.Interegators
{
    public static class WorkerServicePropertyInterigator
    {
        public static DsfActivity SetActivityProperties(string xml, DsfActivity activity)
        {

            XmlDocument document = new XmlDocument();
            document.LoadXml(xml);

            if (document.DocumentElement != null)
            {
                XmlNode node = document.SelectSingleNode("//Action");
                if (node != null)
                {
                    if (node.Attributes != null)
                    {
                        activity.FriendlySourceName = node.Attributes["SourceName"].Value;
                        activity.Type = node.Attributes["Type"].Value;
                        activity.ActionName = node.Attributes["SourceMethod"].Value;
                    }
                }
            }
            return activity;
        }
    }
}
