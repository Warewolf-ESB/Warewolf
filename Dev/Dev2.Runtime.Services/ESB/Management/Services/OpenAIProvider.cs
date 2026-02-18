using System;
using System.Collections.Generic;
using Dev2.Common.Interfaces.Core;
using Newtonsoft.Json.Linq;

namespace Dev2.Runtime.ESB.Management.Services
{
    internal class OpenAIProvider : IChatbotProvider
    {
        internal const string ProviderName = "OpenAI";

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
