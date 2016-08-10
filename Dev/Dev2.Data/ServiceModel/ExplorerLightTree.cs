using System.Xml.Linq;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Runtime.ServiceModel.Data;

namespace Dev2.Data.ServiceModel
{
    public class ExplorerLightTree: Resource, IResourceSource
    {
        public ExplorerLightTree()
        {
            ResourceType = typeof(ExplorerLightTree).Name;
            VersionInfo = new VersionInfo();
        }
        public string IconPath { get; set; }
        public string ResourceParentId { get; set; }
        public string ResourceResourceId { get; set; }

        public override XElement ToXml()
        {
            var result = base.ToXml();
            result.Add(
                new XAttribute("IconPath", IconPath),
                new XAttribute("ResourceParentId", ResourceParentId),
                new XAttribute("ResourceResourceId", ResourceResourceId),
                new XAttribute("Type", GetType().Name),
                new XElement("TypeOf", enSourceType.ExplorerTreeSoure)
                );
            return result;
        }

        public override bool IsSource => true;
        public override bool IsService => false;
        public override bool IsFolder => false;
        public override bool IsReservedService => false;
        public override bool IsServer => false;
        public override bool IsResourceVersion => false;
    }


}