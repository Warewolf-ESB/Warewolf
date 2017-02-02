using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Dev2.Common.Interfaces.Core.Graph;

namespace Dev2.Runtime.ServiceModel.Data
{
    [DataContract]
    [Serializable]
    public class ServiceConstructor
    {
        public ServiceConstructor()
            : this(string.Empty, null, null)
        {
        }

        public ServiceConstructor(string name, IEnumerable<ConstructorParameter> parameters, IOutputDescription outputDescription)
        {
            Name = name;
            Parameters = new List<ConstructorParameter>();
            if (parameters != null)
            {
                Parameters.AddRange(parameters);
            }
        }
        
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public List<ConstructorParameter> Parameters { get; set; }
        [DataMember]
        public bool IsExistingObject { get; set; }
    }
}