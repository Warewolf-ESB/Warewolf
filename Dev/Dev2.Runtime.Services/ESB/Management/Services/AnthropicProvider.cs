using System.Collections.Generic;
using Dev2.Common.Interfaces.Core;

namespace Dev2.Runtime.ESB.Management.Services
{
    internal class AnthropicProvider : IChatbotProvider
    {
        internal const string ProviderName = "Anthropic";

        private readonly ChatbotApiService _serviceWrapper;

        public AnthropicProvider(ChatbotApiService serviceWrapper)
        {
            _serviceWrapper = serviceWrapper;
        }

        public string Send(ChatbotSourceDefinition source, List<object> messages)
        {
            return _serviceWrapper.SendToAnthropic_Public(source, messages);
        }
    }
}
