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

        [JsonConverter(typeof (StringEnumConverter))]
        CompileMessageType MessageType { get; set; }

        [JsonConverter(typeof (StringEnumConverter))]
        ErrorType ErrorType { get; set; }

        string MessagePayload { get; set; }

        ICompileMessageTO Clone();

        IErrorInfo ToErrorInfo();

        FixType ToFixType();
    }
}