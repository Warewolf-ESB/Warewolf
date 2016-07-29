using Dev2.Common.Interfaces.DB;
using Dev2.Common.Interfaces.Exchange;
using Dev2.Common.Interfaces.ToolBase.ExchangeEmail;
using System;
using System.Collections.Generic;

namespace Warewolf.Core
{
    public class ExchangeService : IExchangeService, IEquatable<ExchangeService>
    {
        public bool Equals(IExchangeService other)
        {
            return Equals(other as ExchangeService);
        }

        public string Name { get; set; }
        public string Path { get; set; }
        public IExchangeSource Source { get; set; }
        public IList<IServiceInput> Inputs { get; set; }
        public IList<IServiceOutputMapping> OutputMappings { get; set; }
        public Guid Id { get; set; }

        public bool Equals(ExchangeService other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }
            if (ReferenceEquals(this, other))
            {
                return true;
            }
            return Equals(Source, other.Source);
        }
    }
}