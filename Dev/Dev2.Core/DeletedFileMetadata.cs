using System;
using Dev2.Common.Interfaces;

namespace Dev2
{
    [Serializable]
    public class DeletedFileMetadata : IDeletedFileMetadata
    {
        public bool IsDeleted { get; set; }
        public Guid ResourceId { get; set; }
        public bool ShowDependencies { get; set; }
        public bool ApplyToAll { get; set; }
        public bool DeleteAnyway { get; set; }
    }
}
