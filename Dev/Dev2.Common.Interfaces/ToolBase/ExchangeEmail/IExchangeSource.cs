using Dev2.Common.Interfaces.Core.DynamicServices;
using System;

namespace Dev2.Common.Interfaces.ToolBase.ExchangeEmail
{
    public interface IExchangeSource : IEquatable<IExchangeSource>
    {
        Guid ResourceID { get; set; }
        string AutoDiscoverUrl { get; set; }
        string ResourceName { get; set; }
        string UserName { get; set; }
        string Password { get; set; }
        enSourceType Type { get; set; }
        string ResourceType { get; set; }
        string Path { get; set; }
        int Timeout { get; set; }
    }
}