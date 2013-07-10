using System;
using Dev2.Studio.Core.ErrorHandling;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Dev2.Data.ServiceModel.Messages
{
    /// <summary>
    /// Send compile time messages to the studio ;)
    /// </summary>
    [Serializable]
    public class CompileMessageTO
    {
        public Guid UniqueID { get; set; }

        public Guid ServiceID { get; set; }

        public Guid MessageID { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public CompileMessageType MessageType { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public enErrorType ErrorType { get; set; }

        // should be json or other sensable string data ;)
        public string MessagePayload { get; set; }


        public CompileMessageTO Clone()
        {
            return new CompileMessageTO()
            {
                UniqueID = this.UniqueID,
                ErrorType = this.ErrorType,
                MessageID = this.MessageID,
                ServiceID = this.ServiceID,
                MessageType = this.MessageType,
                MessagePayload = this.MessagePayload
            };
        }
    }
}
