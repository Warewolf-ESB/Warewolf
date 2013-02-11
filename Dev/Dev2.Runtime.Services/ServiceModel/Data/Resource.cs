using Dev2.DynamicServices;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Xml.Linq;

namespace Dev2.Runtime.ServiceModel.Data
{
    public class Resource
    {
        public Guid ResourceID { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public enSourceType ResourceType { get; set; }

        public string ResourceName { get; set; }
        public string ResourcePath { get; set; }

        #region CTOR

        public Resource()
        {
        }

        public Resource(XElement xml)
        {
            if(xml == null)
            {
                throw new ArgumentNullException("xml");
            }

            ResourceID = Guid.Parse(xml.AttributeSafe("ID"));
            ResourceType = (enSourceType)Enum.Parse(typeof(enSourceType), xml.AttributeSafe("Type"));
            ResourceName = xml.AttributeSafe("Name");
            ResourcePath = xml.ElementSafe("Category");
        }

        #endregion

        #region Save

        public void Save(Guid workspaceID, Guid dataListID)
        {
            var xml = ToXml();
            Resources.Save(workspaceID, Resources.RootFolders[ResourceType], ResourceName, xml.ToString());
        }

        #endregion

        #region ToXml

        public virtual XElement ToXml()
        {
            return new XElement(Resources.RootElements[ResourceType],
                new XAttribute("ID", ResourceID),
                new XAttribute("Name", ResourceName ?? string.Empty),
                new XAttribute("Type", ResourceType),
                new XElement("TypeOf", ResourceType),
                new XElement("DisplayName", ResourceName ?? string.Empty),
                new XElement("Category", ResourcePath ?? string.Empty)
                );
        }

        #endregion

        #region ToString

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }

        #endregion

    }
}
