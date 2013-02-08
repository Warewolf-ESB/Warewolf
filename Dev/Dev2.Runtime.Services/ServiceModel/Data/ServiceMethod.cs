using System.Collections.Generic;
using System.Xml.Linq;
using Newtonsoft.Json;
using Unlimited.Framework.Converters.Graph.Interfaces;

namespace Dev2.Runtime.ServiceModel.Data
{
    public class ServiceMethod
    {
        #region CTOR

        public ServiceMethod()
        {
            Parameters = new List<MethodParameter>();
            Outputs = new List<MethodOutput>();
        }

        #endregion

        #region Properties

        public string Name { get; set; }

        public IList<MethodParameter> Parameters { get; private set; }

        public IList<MethodOutput> Outputs { get; private set; }

        public IOutputDescription OutputDescription { get; set; }

        #endregion

        #region ToString

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }

        #endregion

        #region ToXml

        public XElement ToXml()
        {
            return new XElement("ServiceMethod");
        }

        #endregion

    }
}
