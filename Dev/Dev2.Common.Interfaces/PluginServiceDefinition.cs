using System;
using System.Collections.Generic;
using Dev2.Common.Interfaces.DB;

namespace Dev2.Common.Interfaces
{
    public class PluginServiceDefinition:IPluginService
    {
        #region Implementation of IPluginService

        public string Name { get; set; }
        public Guid Id { get; set; }
        public IPluginSource Source { get; set; }
        public IList<IServiceInput> Inputs { get; set; }
        public IList<IServiceOutputMapping> OutputMappings { get; set; }
        public string Path { get; set; }
        public IPluginAction Action { get; set; }

        #endregion
    }
}