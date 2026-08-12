using System;
using System.Runtime.Serialization;

namespace Dev2.Runtime.ServiceModel.Esb.Brokers.Plugin
{
    [DataContract]
    public class PluginExecutionDto
    {
        public PluginExecutionDto(string existingObject)
        {
            ObjectString = existingObject;
        }

        [DataMember]
        public bool IsStatic { get; set; }
        [DataMember]
        public bool IsSealed { get; set; }
        [DataMember]
        public string ObjectString { get; set; }
        [DataMember]
        public PluginInvokeArgs Args { get; set; }
    }
}
