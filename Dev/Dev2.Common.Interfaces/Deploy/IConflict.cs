using System;

namespace Dev2.Common.Interfaces.Deploy
{
    public interface IConflict
    {
        Guid Id { get; }
        string SourceName { get; }
        string DestinationName { get; }
    }
}
