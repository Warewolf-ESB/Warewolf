using Newtonsoft.Json;
using System.Collections.Generic;
using System.Xml.Linq;
using Unlimited.Framework.Converters.Graph.Interfaces;

namespace Dev2.Runtime.ServiceModel.Data
{
    public abstract class ServiceMethod
    {
        #region Properties

        public string Name { get; set; }

        public IList<MethodParameter> Parameters { get; set; }

        public IList<MethodOutput> Outputs { get; set; }

        public IOutputDescription OutputDescription { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }

        public abstract XElement ToXml();

        #endregion
    }
}
