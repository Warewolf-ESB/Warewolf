using System;
using System.Collections.Generic;
using Dev2.Common.Interfaces.DB;

namespace Dev2.Common.Interfaces
{
    public class ComPluginServiceDefinition : IComPluginService
    {
        public string Name { get; set; }
        public Guid Id { get; set; }
        public IComPluginSource Source { get; set; }
        public IList<IServiceInput> Inputs { get; set; }
        public IList<IServiceOutputMapping> OutputMappings { get; set; }
        public string Path { get; set; }
        public IPluginAction Action { get; set; }
    }
}