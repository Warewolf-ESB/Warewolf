using Newtonsoft.Json;
using System.Collections.Generic;
using System.Xml.Linq;
using Unlimited.Framework.Converters.Graph.Interfaces;

namespace Dev2.Runtime.ServiceModel.Data
{
    public class ServiceMethod
    {
        #region CTOR

        public ServiceMethod() : this("", null, null, null)
        {
        }

        public ServiceMethod(string name, IOutputDescription outputDescription, IEnumerable<MethodParameter> parameters, IEnumerable<MethodOutput> outputs)
        {
            Name = name;
            OutputDescription = outputDescription;
            Parameters = new List<MethodParameter>();
            Outputs = new List<MethodOutput>();

            if (parameters != null)
            {
                Parameters.AddRange(parameters);
            }

            if (outputs != null)
            {
                Outputs.AddRange(outputs);
            }
        }

        #endregion

        #region Properties

        public string Name { get; set; }

        public List<MethodParameter> Parameters { get; private set; }

        public List<MethodOutput> Outputs { get; private set; }

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
