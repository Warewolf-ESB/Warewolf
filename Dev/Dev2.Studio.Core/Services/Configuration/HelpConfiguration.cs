using System;
using Dev2.Collections;
using Newtonsoft.Json;

namespace Dev2.Services.Configuration
{
    public class HelpConfiguration
    {
        public HelpConfiguration()
        {
            IsCollapsed = new ConcurrentDictionarySafe<Type, bool>();
        }

        [JsonConverter(typeof(ConcurrentDictionarySafeConverter<Type, bool>))]
        public ConcurrentDictionarySafe<Type, bool> IsCollapsed { get; private set; }
    }
}