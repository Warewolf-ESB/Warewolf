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
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using Dev2.Common;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Common.Interfaces.Hosting;
using Dev2.Communication;
using Dev2.DynamicServices;
using Dev2.DynamicServices.Objects;
using Dev2.Runtime.Hosting;
using Dev2.Workspaces;

namespace Dev2.Runtime.ESB.Management.Services
{
    /// <summary>
    ///     Adds a resource
    /// </summary>
    public class RenameResource : IEsbManagementEndpoint
    {
        public StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            try
            {
                var res = new ExecuteMessage {HasError = false};

                string resourceId = null;
                string newName = null;
                if (values == null)
                {
                    throw new InvalidDataContractException("No parameter values provided.");
                }

                StringBuilder tmp;
                values.TryGetValue("ResourceID", out tmp);
                if (tmp != null)
                {
                    resourceId = tmp.ToString();
                }
                values.TryGetValue("NewName", out tmp);
                if (tmp != null)
                {
                    newName = tmp.ToString();
                }

                if (resourceId == null)
                {
                    throw new InvalidDataContractException("No value provided for ResourceID parameter.");
                }
                if (String.IsNullOrEmpty(newName))
                {
                    throw new InvalidDataContractException("No value provided for NewName parameter.");
                }

                Guid id;
                Guid.TryParse(resourceId, out id);
                Dev2Logger.Log.Info(String.Format("Rename Resource. ResourceId:{0} NewName:{1}", resourceId, newName));
                ResourceCatalogResult saveToWorkSpaceResult = ResourceCatalog.Instance.RenameResource(theWorkspace.ID,
                    id, newName);
                if (saveToWorkSpaceResult.Status == ExecStatus.Success)
                {
                    ResourceCatalogResult saveToLocalServerResult = ResourceCatalog.Instance.RenameResource(Guid.Empty,
                        id, newName);
                    if (saveToLocalServerResult.Status == ExecStatus.Success)
                    {
                        res.SetMessage(saveToLocalServerResult.Message);
                    }
                }
                else
                {
                    res.HasError = true;
                }

                res.SetMessage(saveToWorkSpaceResult.Message);

                var serializer = new Dev2JsonSerializer();
                return serializer.SerializeToBuilder(res);
            }
            catch (Exception err)
            {
                Dev2Logger.Log.Error(err);
                throw;
            }
        }

        public DynamicService CreateServiceEntry()
        {
            var newDs = new DynamicService
            {
                Name = HandlesType(),
                DataListSpecification =
                    new StringBuilder(
                        "<DataList><ResourceID ColumnIODirection=\"Input\"/><NewName ColumnIODirection=\"Input\"/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>")
            };
            var sa = new ServiceAction
            {
                Name = HandlesType(),
                ActionType = enActionType.InvokeManagementDynamicService,
                SourceMethod = HandlesType()
            };
            newDs.Actions.Add(sa);

            return newDs;
        }

        public string HandlesType()
        {
            return "RenameResourceService";
        }
    }
}