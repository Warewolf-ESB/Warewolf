using System;
using System.Collections.Generic;
using Dev2.Common.Interfaces;

namespace Dev2.Common
{
    /// <summary>
    /// Args to pass into the plugin ;)
    /// </summary>
    [Serializable]
    public class Dev2MethodInfo : IDev2MethodInfo
    {
        public string Method { get; set; }
        public List<IMethodParameter> Parameters { get; set; }
        public string MethodResult { get; set; }
        public string OutputVariable { get; set; }
        public bool IsObject { get; set; }
        public bool IsVoid { get; set; }
    }
}