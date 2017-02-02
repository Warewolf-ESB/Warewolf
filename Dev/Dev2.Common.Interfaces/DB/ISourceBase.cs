

using System;

namespace Dev2.Common.Interfaces.DB
{
    public interface ISourceBase<T>
    {
        T Item { get; set; }
        bool HasChanged { get; }
        T ToModel();
        string Name { get; set; }
        void Save();
        Guid SelectedGuid { get; set; }
        void AfterSave(Guid environmentId, Guid resourceId);
    }
}