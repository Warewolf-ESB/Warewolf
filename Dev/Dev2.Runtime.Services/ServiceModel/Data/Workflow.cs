using System.Linq;
using System.Xml.Linq;
using Dev2.Common.ServiceModel;

namespace Dev2.Runtime.ServiceModel.Data
{
    public class Workflow : Resource
    {
        #region CTOR

        public Workflow()
        {
            ResourceType = ResourceType.WorkflowService;
            DataList = new XElement("DataList");
        }

        public Workflow(XElement xml)
            : base(xml)
        {
            ResourceType = ResourceType.WorkflowService;
            DataList = xml.Element("DataList") ?? new XElement("DataList");
            Comment = xml.ElementSafe("Comment");
            IconPath = xml.ElementSafe("IconPath");
            Tags = xml.ElementSafe("Tags");
            HelpLink = xml.ElementSafe("HelpLink");

            var action = xml.Descendants("Action").FirstOrDefault();
            if(action == null)
            {
                return;
            }
            XamlDefinition = action.ElementSafe("XamlDefinition");
        }

        #endregion

        public string XamlDefinition { get; set; }
        public XElement DataList { get; set; }

        public string Comment { get; set; }
        public string IconPath { get; set; }
        public string Tags { get; set; }
        public string HelpLink { get; set; }

        #region ToXml

        public override XElement ToXml()
        {
            var result = base.ToXml();
            result.AddFirst(new XElement("Action",
                new XAttribute("Name", "InvokeWorkflow"),
                new XAttribute("Type", "Workflow"),
                new XElement("XamlDefinition", XamlDefinition ?? string.Empty)
                ));
            result.Add(new XElement("Comment", Comment ?? string.Empty));
            result.Add(new XElement("IconPath", IconPath ?? string.Empty));
            result.Add(new XElement("Tags", Tags ?? string.Empty));
            result.Add(new XElement("HelpLink", HelpLink ?? string.Empty));
            result.Add(DataList);
            return result;
        }

        #endregion

    }
}
