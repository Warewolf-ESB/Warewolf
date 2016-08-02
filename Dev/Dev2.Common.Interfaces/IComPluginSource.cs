using System;

namespace Dev2.Common.Interfaces
{
    public interface IComPluginSource : IEquatable<IComPluginSource>
    {
        string ResourceName { get; set; }
        Guid Id { get; set; }
        bool Is32Bit { get; set; }
        string ClsId { get; set; }
        IFileListing SelectedDll { get; set; }
        string ResourcePath { get; set; }
    }
}