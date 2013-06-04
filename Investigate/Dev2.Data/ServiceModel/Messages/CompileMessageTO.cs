using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Dev2.Data.ServiceModel.Messages
{
    /// <summary>
    /// Send compile time messages to the studio ;)
    /// </summary>
    public class CompileMessageTO
    {
        public Guid MessageID { get; set; }

        public Guid ServiceID { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public CompileMessageType MessageType { get; set; }

        // should be json or other sensable string data ;)
        public string MessagePayload { get; set; }

    }
}
