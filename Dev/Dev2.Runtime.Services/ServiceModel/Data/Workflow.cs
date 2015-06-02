
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2015 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Linq;
using System.Text;
using System.Xml.Linq;
using Dev2.Common.Common;

namespace Dev2.Runtime.ServiceModel.Data
{
    public class Workflow : Resource
    {
        #region CTOR

        public Workflow()
        {
            ResourceType = Common.Interfaces.Data.ResourceType.WorkflowService;
            DataList = new XElement("DataList");
        }

        public Workflow(XElement xml)
            : base(xml)
        {
            ResourceType = Common.Interfaces.Data.ResourceType.WorkflowService;
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

            XamlDefinition = action.ElementSafeStringBuilder("XamlDefinition");
        }

        #endregion

        public StringBuilder XamlDefinition { get; set; }
        public new XElement DataList { get; set; }

        public string Comment { get; set; }
        public string IconPath { get; set; }
        public string Tags { get; set; }
        public string HelpLink { get; set; }

        #region ToXml

        public override XElement ToXml()
        {
            var result = base.ToXml();
            var serviceDefinition = XamlDefinition.ToXElement();
            
            result.AddFirst(new XElement("Action",
                new XAttribute("Name", "InvokeWorkflow"),
                new XAttribute("Type", "Workflow"),
                serviceDefinition
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
