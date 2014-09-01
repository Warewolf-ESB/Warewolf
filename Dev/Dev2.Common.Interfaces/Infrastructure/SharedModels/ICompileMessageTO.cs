using System;
using Dev2.Common.Interfaces.Infrastructure.Providers.Errors;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Dev2.Common.Interfaces.Infrastructure.SharedModels
{
    public interface ICompileMessageTO
    {
        Guid UniqueID { get; set; }
        Guid ServiceID { get; set; }
        Guid MessageID { get; set; }
        Guid WorkspaceID { get; set; }
        string ServiceName { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        CompileMessageType MessageType { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        ErrorType ErrorType { get; set; }
        string MessagePayload { get; set; }

        ICompileMessageTO Clone();

        IErrorInfo ToErrorInfo();

        FixType ToFixType();
    }
}