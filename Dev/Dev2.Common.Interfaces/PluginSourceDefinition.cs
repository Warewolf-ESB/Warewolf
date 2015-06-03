using System;

namespace Dev2.Common.Interfaces
{
    public class PluginSourceDefinition : IPluginSource
    {
        #region Implementation of IPluginSource

        public string Name { get; set; }
        public Guid Id { get; set; }
        public IDllListing SelectedDll { get; set; }
        public string Path { get; set; }

        #endregion
    }
}