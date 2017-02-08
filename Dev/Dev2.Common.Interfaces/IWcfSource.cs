using Dev2.Common.Interfaces.Core.DynamicServices;
using System;
using Dev2.Common.Interfaces.Data;

namespace Dev2.Common.Interfaces
{
    public interface IWcfSource : IEquatable<IWcfSource> , IResource
    {
        string EndpointUrl { get; set; }
        string ContactName { get; set; }
        string Name { get; set; }
        string Path { get; set; }
        Guid Id { get; set; }
        enSourceType Type { get; set; }

    }
}