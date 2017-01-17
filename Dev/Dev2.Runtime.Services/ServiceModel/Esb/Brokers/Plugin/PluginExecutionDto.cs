using System;

namespace Dev2.Runtime.ServiceModel.Esb.Brokers.Plugin
{
    [Serializable]
    public class PluginExecutionDto
    {
        public PluginExecutionDto(string existingObject)
        {
            ObjectString = existingObject;
        }
       
        public bool IsStatic { get; set; }
        public bool IsSealed { get; set; }
        public string ObjectString { get; set; }
        public PluginInvokeArgs Args { get; set; }
    }
}
