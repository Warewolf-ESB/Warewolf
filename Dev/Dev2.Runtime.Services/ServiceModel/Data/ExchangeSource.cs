using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Dev2.Common.Common;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Common.Interfaces.Data;

namespace Dev2.Runtime.ServiceModel.Data
{
    [Serializable]
    public class ExchangeSource : Resource
    {
        #region CTOR

        public ExchangeSource()
        {
            ResourceID = Guid.Empty;
            ResourceType = ResourceType.ExchangeSource;
        }

        public ExchangeSource(XElement xml)
            : base(xml)
        {
            ResourceType = ResourceType.ExchangeSource;

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
                new XAttribute("Type", enSourceType.ExchangeEmailSource),
                new XElement("TypeOf", enSourceType.ExchangeEmailSource)
                );
            return result;
        }

        #endregion
    }
}
