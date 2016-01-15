using System;

namespace Dev2.Common.Interfaces
{
    public interface IPluginSource:IEquatable<IPluginSource>    
    {
        string Name { get; set; }
        Guid Id { get; set; }
        IFileListing SelectedDll { get; set; }
        string Path { get; set; }
    }
}
