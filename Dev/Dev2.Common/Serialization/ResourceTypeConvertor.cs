using System;
using System.Collections.Generic;
using Dev2.Common.Interfaces.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Dev2.Common.Serialization
{
   public class ResourceTypeConvertor : JsonConverter
    {

        static readonly Dictionary<int, ResourceType> LookupValues = new Dictionary<int, ResourceType>
        { { 0, ResourceType.Unknown },
            { 1, ResourceType.WorkflowService },
            { 2, ResourceType.DbService },
            { 3, ResourceType.Version },
            { 4, ResourceType.PluginService },
            { 8, ResourceType.WebService},
            { 16, ResourceType.DbSource},
            { 32, ResourceType.PluginSource},
            { 64, ResourceType.WebSource },
            { 128,  ResourceType.EmailSource},
            { 256,  ResourceType.ServerSource},
            { 512,  ResourceType.Folder},
            { 1024,   ResourceType.Server},
            { 2048,   ResourceType.ReservedService },
            { 3069,    ResourceType.Message }
        };

        public override bool CanConvert(Type objectType)
        {
            return typeof(ResourceType).IsAssignableFrom(objectType);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var enumValue = reader.Value.ToString();
            int res;
            if(int.TryParse( enumValue, out res))
            {
                return LookupValues[res];
            }
            return Enum.Parse(typeof(ResourceType), enumValue);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            new StringEnumConverter().WriteJson(writer,value,serializer);
        }
    }
}
