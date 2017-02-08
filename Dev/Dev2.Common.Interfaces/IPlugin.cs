using System;
using Dev2.Common.Interfaces.Data;

namespace Dev2.Common.Interfaces
{
    public interface IPlugin : IResource
    {
        string AssemblyLocation { get; set; }
        string AssemblyName { get; set; }
        string ConfigFilePath { get; set; }
        Version Version { get; set; }
    }
}