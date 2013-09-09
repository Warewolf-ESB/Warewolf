
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
                        var attr = node.Attributes["SourceName"];
                        if (attr != null)
                        {
                            activity.FriendlySourceName = attr.Value;
                        }

                        attr = node.Attributes["Type"];
                        if (attr != null)
                        {
                            activity.Type = attr.Value;
                        }

                        attr = node.Attributes["SourceMethod"];
                        if (attr != null)
                        {
                            activity.ActionName = attr.Value;
                        }
                    }
                }
            }
            return activity;
        }
    }
}
