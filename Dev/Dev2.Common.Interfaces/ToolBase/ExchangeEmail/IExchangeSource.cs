using System;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Common.Interfaces.Data;

namespace Dev2.Common.Interfaces.ToolBase.ExchangeEmail
{
    public interface IExchangeSource : IEquatable<IExchangeSource>
    {
        Guid ResourceID { get; set; }
        string AutoDiscoverUrl { get; set; }
        string Name { get; set; }
        string UserName { get; set; }
        string Password { get; set; }
        enSourceType Type { get; set; }
        ResourceType ResourceType { get; set; }
        string Path { get; set; }
        int Timeout { get; set; }
    }
}