using System.Collections.Generic;
using Dev2.Common.Interfaces.Core;

namespace Warewolf.AI.Harness
{
    public class GeminiProvider : IChatbotProvider
    {
        public const string ProviderName = "Google Gemini";

        private readonly ChatbotApiService _serviceWrapper;

        public GeminiProvider(ChatbotApiService serviceWrapper)
        {
            _serviceWrapper = serviceWrapper;
        }

        public string Send(ChatbotSourceDefinition source, List<object> messages)
        {
            return _serviceWrapper.SendToGemini_Public(source, messages);
        }
    }
}
