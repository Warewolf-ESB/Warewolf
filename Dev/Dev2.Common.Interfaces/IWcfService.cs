using System;
using System.Collections.Generic;
using Dev2.Common.Interfaces.DB;

namespace Dev2.Common.Interfaces
{
    public interface IWcfService : IEquatable<IWcfService>
    {
        string Name { get; set; }
        Guid Id { get; set; }
        IWcfServerSource Source { get; set; }
        IList<IServiceInput> Inputs { get; set; }
        IList<IServiceOutputMapping> OutputMappings { get; set; }
        string Path { get; set; }
        IWcfAction Action { get; set; }
    }
}
