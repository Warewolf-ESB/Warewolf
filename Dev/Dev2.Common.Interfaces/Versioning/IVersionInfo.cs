using System;

namespace Dev2.Common.Interfaces.Versioning
{
    public interface IVersionInfo
    {
        DateTime DateTimeStamp { get; set; }
        string Reason { get; set; }
        string User { get; set; }
        string VersionNumber { get; set; }
        Guid ResourceId { get; set; }
        Guid VersionId { get; set; }
    }
}