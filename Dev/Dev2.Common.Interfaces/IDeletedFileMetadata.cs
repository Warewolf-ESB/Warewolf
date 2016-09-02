using System;

namespace Dev2.Common.Interfaces
{
    public interface IDeletedFileMetadata
    {
        bool IsDeleted { get; set; }
        Guid ResourceId { get; set; }
        bool ShowDependencies { get; set; }
    }
}