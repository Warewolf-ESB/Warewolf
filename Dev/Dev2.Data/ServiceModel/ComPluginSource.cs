using System;
using System.Xml.Linq;
using Dev2.Common.Common;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core.DynamicServices;

// ReSharper disable CheckNamespace
namespace Dev2.Runtime.ServiceModel.Data
{
    [Serializable]
    public class ComPluginSource : Resource, IResourceSource
    {
        #region CTOR

        public ComPluginSource()
        {
            ResourceID = Guid.Empty;
            ResourceType = typeof(ComPluginSource).Name;
        }

        public ComPluginSource(XElement xml)
            : base(xml)
        {
            ResourceType = typeof(ComPluginSource).Name;

            ClsId = xml.AttributeSafe("ClsId");
            ProgId = xml.AttributeSafe("ProgId");
        }

        #endregion

        #region Properties

        public string ClsId { get; set; }
        public string ProgId { get; set; }

        #endregion

        #region ToXml

        public override XElement ToXml()
        {
            var result = base.ToXml();
            result.Add(
                new XAttribute("ClsId", ClsId ?? string.Empty),
                new XAttribute("ProgId", ProgId ?? string.Empty),
                new XAttribute("Type", GetType().Name),
                new XElement("TypeOf", enSourceType.ComPluginSource)
                );
            return result;
        }

        public override bool IsSource => true;

        public override bool IsService => false;

        public override bool IsFolder => false;

        public override bool IsReservedService => false;

        public override bool IsServer => false;
        public override bool IsResourceVersion => false;

        #endregion
    }
}