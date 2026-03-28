using System;
using System.Runtime.Serialization;
using Dev2.Common.Interfaces;

namespace Dev2
{
    [DataContract]
    public class DeletedFileMetadata : IDeletedFileMetadata
    {
        [DataMember]
        public bool IsDeleted { get; set; }
        [DataMember]
        public Guid ResourceId { get; set; }
        [DataMember]
        public bool ShowDependencies { get; set; }
        [DataMember]
        public bool ApplyToAll { get; set; }
        [DataMember]
        public bool DeleteAnyway { get; set; }
    }
}
