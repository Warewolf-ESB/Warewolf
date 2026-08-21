using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Dev2.Common.Interfaces;

namespace Dev2.Common
{
    /// <summary>
    /// Args to pass into the plugin ;)
    /// </summary>
    [DataContract]
    public class Dev2MethodInfo : IDev2MethodInfo
    {
        [DataMember]
        public string Method { get; set; }

        [DataMember]
        public List<IMethodParameter> Parameters { get; set; }

        [DataMember]
        public string MethodResult { get; set; }

        [DataMember]
        public string OutputVariable { get; set; }

        [DataMember]
        public bool IsObject { get; set; }

        [DataMember]
        public bool IsVoid { get; set; }

        [DataMember]
        public Guid ID { get; set; }

        [DataMember]
        public bool HasError { get; set; }

        [DataMember]
        public string ErrorMessage { get; set; }

        [DataMember]
        public bool IsProperty { get; set; }
    }
}