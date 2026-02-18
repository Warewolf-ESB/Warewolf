using System.Collections.Generic;
using Dev2.Common.Interfaces.Core;

namespace Dev2.Runtime.ESB.Management.Services
{
    internal interface IChatbotProvider
    {
        string Send(ChatbotSourceDefinition source, List<object> messages);
    }
}
