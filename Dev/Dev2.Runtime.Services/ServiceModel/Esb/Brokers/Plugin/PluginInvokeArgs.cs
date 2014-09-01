using System;
using System.Collections.Generic;
using Dev2.Common.Interfaces.Core.Graph;
using Dev2.Runtime.ServiceModel.Data;

namespace Dev2.Runtime.ServiceModel.Esb.Brokers.Plugin
{
    /// <summary>
    /// Args to pass into the plugin ;)
    /// </summary>
    [Serializable]
    public class PluginInvokeArgs
    {
        public string AssemblyLocation { get; set; }

        public string AssemblyName { get; set; }

        public string Fullname { get; set; }

        public string Method { get; set; }

        public List<MethodParameter> Parameters { get; set; }

        public IOutputFormatter OutputFormatter { get; set; }

    }
}
