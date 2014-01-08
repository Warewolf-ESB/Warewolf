// ReSharper disable once CheckNamespace
namespace Dev2.Studio.Core.Services.System
{

    /// <summary>
    /// Transfer object returned from the System info Service
    /// </summary>
    /// <author>Jurie.smit</author>
    /// <datetime>2013/01/14-09:06 AM</datetime>
    public class SystemInfoTO
    {
        public string Name { get; set; }
        public string Edition { get; set; }
        public string ServicePack { get; set; }
        public string Version { get; set; }
        public string OsBits { get; set; }
        public int ApplicationExecutionBits { get; set; }
    }
}
