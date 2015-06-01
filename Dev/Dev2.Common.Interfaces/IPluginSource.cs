using System;

namespace Dev2.Common.Interfaces
{
    public interface IPluginSource
    {
        string Name { get; set; }
        Guid Id { get; set; }
        DllListing SelectedDll { get; set; }
        string Path { get; set; }
    }
}