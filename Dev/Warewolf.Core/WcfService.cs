using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.DB;
using System;
using System.Collections.Generic;

namespace Warewolf.Core
{
    public class WcfService : IWcfService
    {
        public string Name { get; set; }
        public Guid Id { get; set; }
        public IWcfServerSource Source { get; set; }
        public IList<IServiceInput> Inputs { get; set; }
        public IList<IServiceOutputMapping> OutputMappings { get; set; }
        public string Path { get; set; }
        public IWcfAction Action { get; set; }
    }
}