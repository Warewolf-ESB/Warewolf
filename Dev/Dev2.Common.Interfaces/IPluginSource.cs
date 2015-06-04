using System;
using System.Dynamic;

namespace Dev2.Common.Interfaces
{
    public interface IPluginSource
    {
        string Name { get; set; }
        Guid Id { get; set; }
        IDllListing SelectedDll { get; set; }
        string Path { get; set; }
    }
}