using System;

namespace Dev2.Common.Interfaces
{
    public interface IComPluginSource : IEquatable<IComPluginSource>
    {
        string Name { get; set; }
        Guid Id { get; set; }
        string ProgId { get; set; }
        string ClsId { get; set; }
        IFileListing SelectedDll { get; set; }
    }
}