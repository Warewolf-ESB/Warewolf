
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System;
using Dev2.Common.ExtMethods;
using Dev2.Common.Interfaces.Infrastructure.Providers.Errors;
using Dev2.Common.Interfaces.Infrastructure.SharedModels;
using Dev2.Providers.Errors;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Dev2.Data.ServiceModel.Messages
{
    /// <summary>
    /// Send compile time messages to the studio ;)
    /// </summary>
    [Serializable]
    // ReSharper disable InconsistentNaming
    public class CompileMessageTO : ICompileMessageTO
    {
        public Guid UniqueID { get; set; }
        public Guid ServiceID { get; set; }
        public Guid MessageID { get; set; }
        public Guid WorkspaceID { get; set; }
        public string ServiceName { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public CompileMessageType MessageType { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public ErrorType ErrorType { get; set; }

        // should be json or other sensable string data ;)
        public string MessagePayload { get; set; }

        public ICompileMessageTO Clone()
        {
            return new CompileMessageTO
            {
                UniqueID = UniqueID,
                WorkspaceID = WorkspaceID,
                ServiceName = ServiceName,
                ErrorType = ErrorType,
                MessageID = MessageID,
                ServiceID = ServiceID,
                MessageType = MessageType,
                MessagePayload = MessagePayload
            };
        }

        public IErrorInfo ToErrorInfo()
        {
            return new ErrorInfo
            {
                InstanceID = UniqueID,
                ErrorType = ErrorType,
                FixType = ToFixType(),
                Message = MessageType.GetDescription(),
                FixData = MessagePayload,
            };
        }

        public FixType ToFixType()
        {
            switch(MessageType)
            {
                case CompileMessageType.MappingChange:
                    return FixType.ReloadMapping;

                case CompileMessageType.MappingIsRequiredChanged:
                    return FixType.IsRequiredChanged;

                case CompileMessageType.ResourceDeleted:
                    break;

                case CompileMessageType.ResourceSaved:
                    break;
            }
            return FixType.None;
        }
    }
}
