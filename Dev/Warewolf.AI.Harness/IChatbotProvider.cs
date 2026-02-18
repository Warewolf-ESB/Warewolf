using System.Collections.Generic;
using Dev2.Common.Interfaces.Core;

namespace Warewolf.AI.Harness
{
    public interface IChatbotProvider
    {
        string Send(ChatbotSourceDefinition source, List<object> messages);
    }
}
