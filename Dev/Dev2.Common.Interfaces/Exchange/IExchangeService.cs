using System;
using System.Collections.Generic;
using Dev2.Common.Interfaces.DB;
using Dev2.Common.Interfaces.ToolBase.ExchangeEmail;

namespace Dev2.Common.Interfaces.Exchange
{
    public interface IExchangeService : IEquatable<IExchangeService>
    {
        string Name { get; set; }
        string Path { get; set; }
        IExchangeSource Source { get; set; }
        IList<IServiceInput> Inputs { get; set; }
        IList<IServiceOutputMapping> OutputMappings { get; set; }
        Guid Id { get; set; }
    }
}
