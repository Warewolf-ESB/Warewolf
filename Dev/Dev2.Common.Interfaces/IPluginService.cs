using System;
using System.Collections.Generic;
using Dev2.Common.Interfaces.DB;

namespace Dev2.Common.Interfaces
{
    public interface IPluginService
    {
        string Name { get; set; }
        Guid Id { get; set; }
        IPluginSource Source { get; set; }
        IList<IServiceInput> Inputs { get; set; }
        IList<IServiceOutputMapping> OutputMappings { get; set; }
        string Path { get; set; }
        IPluginAction Action { get; set; }


    }
}