using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dev2.Common.Interfaces.Core.DynamicServices;

namespace Dev2.Common.Interfaces.ToolBase.ExchangeEmail
{
    public interface IExchangeSource : IEquatable<IExchangeSource>
    {
        Guid Id { get; set; }
        string Name { get; set; }
        string UserName { get; set; }
        string Password { get; set; }
        enSourceType Type { get; set; }
        string Path { get; set; }
    }
}
