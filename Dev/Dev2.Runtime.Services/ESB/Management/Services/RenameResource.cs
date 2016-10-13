/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2016 by Warewolf Ltd <alpha@warewolf.io>
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
using Warewolf.Resource.Errors;

namespace Dev2.Runtime.ESB.Management.Services
{
    /// <summary>
    /// Adds a resource
    /// </summary>
    public class RenameResource : IEsbManagementEndpoint
    {
        private readonly IAuthorizer _authorizer;
        private IAuthorizer Authorizer => _authorizer ?? new SecuredContributeManagementEndpoint();

        public RenameResource(IAuthorizer authorizer)
        {
            _authorizer = authorizer;
        }

        // ReSharper disable once MemberCanBeInternal
        public RenameResource()
        {

        }

        public StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            try
            {


                var res = new ExecuteMessage {HasError = false};

                string resourceId = null;
                string newName = null;
                string resourcePath = null;
                if(values == null)
                {
                    throw new InvalidDataContractException(ErrorResource.NoParameter);
                }

                StringBuilder tmp;
                values.TryGetValue("ResourceID", out tmp);
                if (tmp != null)
                {
                    resourceId = tmp.ToString();
                }
              
                values.TryGetValue("NewName", out tmp);
                if(tmp != null)
                {
                    newName = tmp.ToString();
                }
                values.TryGetValue("resourcePath", out tmp);
                if(tmp != null)
                {
                    resourcePath = tmp.ToString();
                }

                if(resourceId == null)
                {
                    throw new InvalidDataContractException(string.Format(ErrorResource.NoValueProvidedForParameter, "ResourceID"));
                }
                if(string.IsNullOrEmpty(newName))
                {
                    throw new InvalidDataContractException(ErrorResource.NoParameter);
                }

                Guid id;
                Guid.TryParse(resourceId, out id);
                Authorizer.RunPermissions(id);
                Dev2Logger.Info($"Rename Resource. ResourceId:{resourceId} NewName:{newName}");
                var saveToWorkSpaceResult = ResourceCatalog.Instance.RenameResource(theWorkspace.ID, id, newName, resourcePath);
                if (saveToWorkSpaceResult.Status == ExecStatus.Success)
                {
                    var saveToLocalServerResult = ResourceCatalog.Instance.RenameResource(Guid.Empty, id, newName, resourcePath);
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

                Dev2JsonSerializer serializer = new Dev2JsonSerializer();
                return serializer.SerializeToBuilder(res);

            }
            catch (Exception err)
            {
                Dev2Logger.Error(err);
                throw;
            }
        }

        public DynamicService CreateServiceEntry()
        {
            DynamicService newDs = new DynamicService { Name = HandlesType(), DataListSpecification = new StringBuilder("<DataList><ResourceID ColumnIODirection=\"Input\"/><NewName ColumnIODirection=\"Input\"/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>") };
            ServiceAction sa = new ServiceAction { Name = HandlesType(), ActionType = enActionType.InvokeManagementDynamicService, SourceMethod = HandlesType() };
            newDs.Actions.Add(sa);

            return newDs;
        }

        public string HandlesType()
        {
            return "RenameResourceService";
        }
    }
}
