using System.Collections.Generic;
using Dev2.Common.Interfaces.Core;

namespace Warewolf.AI.Harness
{
    public class OpenRouterProvider : IChatbotProvider
    {
        public const string ProviderName = "OpenRouter";

        private readonly ChatbotApiService _serviceWrapper;

        public OpenRouterProvider(ChatbotApiService serviceWrapper)
        {
            _serviceWrapper = serviceWrapper;
        }

        public string Send(ChatbotSourceDefinition source, List<object> messages)
        {
            return _serviceWrapper.SendToOpenRouter_Public(source, messages);
        }
    }
}
