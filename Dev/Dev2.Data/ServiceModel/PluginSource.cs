using System;
using System.Xml.Linq;
using Dev2.Common.Common;
using Dev2.Common.Interfaces.Core.DynamicServices;

// ReSharper disable CheckNamespace
namespace Dev2.Runtime.ServiceModel.Data
{
    [Serializable]
    public class PluginSource : Resource
    {
        #region CTOR

        public PluginSource()
        {
            ResourceID = Guid.Empty;
            ResourceType = Common.Interfaces.Data.ResourceType.PluginSource;
        }

        public PluginSource(XElement xml)
            : base(xml)
        {
            ResourceType = Common.Interfaces.Data.ResourceType.PluginSource;

            AssemblyLocation = xml.AttributeSafe("AssemblyLocation");
            AssemblyName = xml.AttributeSafe("AssemblyName");
        }

        #endregion

        #region Properties

        public string AssemblyLocation { get; set; }
        public string AssemblyName { get; set; }

        #endregion

        #region ToXml

        public override XElement ToXml()
        {
            var result = base.ToXml();
            result.Add(
                new XAttribute("AssemblyLocation", AssemblyLocation ?? string.Empty),
                new XAttribute("AssemblyName", AssemblyName ?? string.Empty),
                new XAttribute("Type", enSourceType.Plugin),
                new XElement("TypeOf", enSourceType.Plugin)
                );
            return result;
        }

        #endregion
    }
}
