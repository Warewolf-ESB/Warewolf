using System.Xml.Linq;
using Dev2.Data.ServiceModel;

namespace Dev2.Runtime.ServiceModel.Data
{
    public class PluginSource : Resource
    {
        #region CTOR

        public PluginSource()
        {
            ResourceType = ResourceType.PluginSource;
        }

        public PluginSource(XElement xml)
            : base(xml)
        {
            ResourceType = ResourceType.PluginSource;

            AssemblyLocation = xml.AttributeSafe("AssemblyLocation");
            AssemblyName = xml.AttributeSafe("AssemblyName");
        }

        #endregion

        #region Properties

        public string AssemblyLocation { get; set; }
        public string AssemblyName { get; set; }
        public string FullName { get; set; }

        #endregion

        #region ToXml

        public override XElement ToXml()
        {
            var result = base.ToXml();
            result.Add(new XAttribute("AssemblyLocation", AssemblyLocation ?? string.Empty));
            result.Add(new XAttribute("AssemblyName", AssemblyName ?? string.Empty));
            return result;
        }

        #endregion
    }
}
