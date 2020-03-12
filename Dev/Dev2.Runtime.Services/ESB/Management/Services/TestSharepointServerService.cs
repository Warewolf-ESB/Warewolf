#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using Dev2.Common;
using Dev2.Common.Interfaces.Enums;
using Dev2.Communication;
using Dev2.Data.ServiceModel;
using Dev2.DynamicServices;
using Dev2.Workspaces;
using Warewolf.Resource.Errors;

namespace Dev2.Runtime.ESB.Management.Services
{
    public class TestSharepointServerService : EsbManagementEndpointBase
    {
        public override Guid GetResourceID(Dictionary<string, StringBuilder> requestArgs) => Guid.Empty;

        public override AuthorizationContext GetAuthorizationContextForService() => AuthorizationContext.Contribute;
       
        public override StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            if(values == null)
            {
                throw new InvalidDataContractException(ErrorResource.NoParameter);
            }
            string serializedSource = null;
            var msg = new ExecuteMessage();
            values.TryGetValue("SharepointServer", out StringBuilder tmp);
            if(tmp != null)
            {
                serializedSource = tmp.ToString();
            }
            
            var serializer = new Dev2JsonSerializer();

            if (string.IsNullOrEmpty(serializedSource))
            {
                var res = new ExecuteMessage { HasError = true };
                res.SetMessage(ErrorResource.NoSharepointServerSet);
                Dev2Logger.Debug(ErrorResource.NoSharepointServerSet, GlobalConstants.WarewolfDebug);
                return serializer.SerializeToBuilder(res);
            }
            try
            {
                msg.HasError = false;
                var sharepointSource = serializer.Deserialize<SharepointSource>(serializedSource);
                var result = sharepointSource.TestConnection();

                if (result.Contains("Failed"))
                {
                    msg.HasError = true;
                }
                msg.Message = serializer.SerializeToBuilder(result);

                var sharepointSourceTo = new SharepointSourceTo
                {
                    TestMessage = result,
                    IsSharepointOnline = sharepointSource.IsSharepointOnline
                };
                return serializer.SerializeToBuilder(sharepointSourceTo);

            }
            catch(Exception ex)
            {
                Dev2Logger.Error(ex, GlobalConstants.WarewolfError);
                msg.Message = serializer.SerializeToBuilder(ex.Message);
            }
            return serializer.SerializeToBuilder(msg);
        }

        public override DynamicService CreateServiceEntry() => EsbManagementServiceEntry.CreateESBManagementServiceEntry(HandlesType(), "<DataList><Database ColumnIODirection=\"Input\"/><TableName ColumnIODirection=\"Input\"/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>");

        public override string HandlesType() => "TestSharepointServerService";
    }
}