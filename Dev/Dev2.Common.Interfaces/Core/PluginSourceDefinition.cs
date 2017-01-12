using System;

namespace Dev2.Common.Interfaces.Core
{
    public class PluginSourceDefinition : IPluginSource
    {
        public bool Equals(IPluginSource other)
        {
            throw new NotImplementedException();
        }

        public string Name { get; set; }
        public Guid Id { get; set; }
        public IFileListing SelectedDll { get; set; }
        public string FileSystemAssemblyName { get; set; }
        public string GACAssemblyName { get; set; }
        public string Path { get; set; }
        public string ConfigFilePath { get; set; }
    }
}
