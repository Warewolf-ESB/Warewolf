using Dev2.Common.Interfaces.Core.DynamicServices;
using System;

namespace Dev2.Common.Interfaces
{
    public interface IWcfSource : IEquatable<IWcfSource>
    {
        string ResourceName { get; set; }
        Guid ResourceID { get; set; }
        string EndpointUrl { get; set; }
        string ContactName { get; set; }
        string Name { get; set; }
        string Path { get; set; }
        Guid Id { get; set; }
        enSourceType Type { get; set; }
        string ResourceType { get; set; }
    }
}