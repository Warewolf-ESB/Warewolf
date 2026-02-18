using System.Collections.Generic;
using Dev2.Common.Interfaces.Core;

namespace Warewolf.AI.Harness
{
    public class OpenAIProvider : IChatbotProvider
    {
        public const string ProviderName = "OpenAI";

        private readonly ChatbotApiService _serviceWrapper;

        public OpenAIProvider(ChatbotApiService serviceWrapper)
        {
            _serviceWrapper = serviceWrapper;
        }

        public string Send(ChatbotSourceDefinition source, List<object> messages)
        {
            return _serviceWrapper.SendToOpenAI_Public(source, messages);
        }
    }
}
