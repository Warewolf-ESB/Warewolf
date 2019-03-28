#pragma warning disable
﻿using Dev2.Common.Interfaces.Core.DynamicServices;
using System;

namespace Dev2.Common.Interfaces
{
    public interface IWcfServerSource : IEquatable<IWcfServerSource>
    {
        string Name { get; set; }
        string EndpointUrl { get; set; }
        string Path { get; set; }
        Guid Id { get; set; }
        string ResourceName { get; set; }
        Guid ResourceID { get; set; }

        enSourceType Type { get; set; }
        string ResourceType { get; set; }
    }
}