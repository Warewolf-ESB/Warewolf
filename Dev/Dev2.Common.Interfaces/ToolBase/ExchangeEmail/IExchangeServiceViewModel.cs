using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dev2.Common.Interfaces.DB;

namespace Dev2.Common.Interfaces.ToolBase.ExchangeEmail
{
    public interface IExchangeServiceViewModel
    {
        ISourceToolRegion<IExchangeSource> SourceRegion { get; set; }
        void SetDisplayName(string displayName);
        bool GenerateOutputsVisible { get; set; }
    }
}
