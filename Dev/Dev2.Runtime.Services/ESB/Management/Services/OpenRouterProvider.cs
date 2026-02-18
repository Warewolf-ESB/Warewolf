using System;
using System.Collections.Generic;
using System.Text;
using Dev2.Common.Interfaces.Core;
using Newtonsoft.Json.Linq;

namespace Dev2.Runtime.ESB.Management.Services
{
    internal class OpenRouterProvider : IChatbotProvider
    {
        internal const string ProviderName = "OpenRouter";

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
