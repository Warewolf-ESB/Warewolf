/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
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
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Communication;
using Dev2.Diagnostics.Debug;
using Dev2.DynamicServices;
using Dev2.DynamicServices.Objects;
using Dev2.Workspaces;
using Warewolf.Resource.Errors;

namespace Dev2.Runtime.ESB.Management.Services
{
    public class FetchRemoteDebugMessages : DefaultEsbManagementEndpoint
    {
        public override StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            try
            {         
                Dev2Logger.Info("Fetch Remote Debug Messages", GlobalConstants.WarewolfInfo);
                string invokerId = null;
                Dev2JsonSerializer serializer = new Dev2JsonSerializer();

                    values.TryGetValue("InvokerID", out StringBuilder tmp);
                    if (tmp != null)
                {
                    invokerId = tmp.ToString();
                }

                if(string.IsNullOrEmpty(invokerId))
                {
                    throw new InvalidDataContractException(ErrorResource.NullServiceIDOrWorkspaceID);
                }

                    // RemoteDebugMessageRepo
                    Guid.TryParse(invokerId, out Guid iGuid);

                    if (iGuid != Guid.Empty)
                {
                    var items = RemoteDebugMessageRepo.Instance.FetchDebugItems(iGuid);

                    return serializer.SerializeToBuilder(items);
                }

                return new StringBuilder();
            }
            catch (Exception err)
            {
                Dev2Logger.Error("Fetch Remote Debug Messages Error", err, GlobalConstants.WarewolfError);
                throw;
            }
        }

        public override DynamicService CreateServiceEntry()
        {
            var serviceEntry = new DynamicService
            {
                Name = HandlesType(),
                DataListSpecification = new StringBuilder("<DataList><InvokerID ColumnIODirection=\"Input\"/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>")
            };
            using (ServiceAction serviceAction = new ServiceAction
            {
                Name = HandlesType(),
                SourceMethod = HandlesType(),
                ActionType = enActionType.InvokeManagementDynamicService
            })
            {
                serviceEntry.Actions.Add(serviceAction);
                return serviceEntry;
            }
        }

        public override string HandlesType()
        {
            return "FetchRemoteDebugMessagesService";
        }
    }
}
