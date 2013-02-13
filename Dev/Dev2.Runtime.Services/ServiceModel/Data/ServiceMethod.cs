using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Linq;
using Newtonsoft.Json;
using Unlimited.Framework.Converters.Graph.Interfaces;

namespace Dev2.Runtime.ServiceModel.Data
{
    [DataContract]
    public class ServiceMethod
    {
        #region CTOR

        public ServiceMethod()
            : this("", null, null, null)
        {
        }

        public ServiceMethod(string name, IOutputDescription outputDescription, IEnumerable<MethodParameter> parameters, IEnumerable<MethodOutput> outputs)
        {
            Name = name;
            OutputDescription = outputDescription;
            Parameters = new List<MethodParameter>();
            Outputs = new List<MethodOutput>();

            if(parameters != null)
            {
                Parameters.AddRange(parameters);
            }

            if(outputs != null)
            {
                Outputs.AddRange(outputs);
            }
        }

        #endregion

        #region Properties

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public List<MethodParameter> Parameters { get; set; }

        [DataMember]
        public string SourceCode { get; set; }

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
