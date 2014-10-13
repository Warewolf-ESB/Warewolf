
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
using Dev2.Common.Interfaces.Infrastructure.Providers.Errors;
using Dev2.Common.Interfaces.Infrastructure.SharedModels;
using Dev2.Communication;
using Dev2.Data.ServiceModel.Messages;
using Dev2.DynamicServices;
using Dev2.DynamicServices.Objects;
using Dev2.DynamicServices.Objects.Base;
using Dev2.Runtime.Hosting;
using Dev2.Workspaces;

namespace Dev2.Runtime.ESB.Management.Services
{
    /// <summary>
    /// Adds a resource
    /// </summary>
    public class SaveResource : IEsbManagementEndpoint
    {
        public StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            try
            {

            Dev2Logger.Log.Info("Save Resource Service");
            StringBuilder resourceDefinition;
            string workspaceIDString = string.Empty;

            values.TryGetValue("ResourceXml", out resourceDefinition);
            StringBuilder tmp;
            values.TryGetValue("WorkspaceID", out tmp);
            if(tmp != null)
            {
                workspaceIDString = tmp.ToString();
            }
            Guid workspaceID;
            if(!Guid.TryParse(workspaceIDString, out workspaceID))
            {
                workspaceID = theWorkspace.ID;
            }

            if(resourceDefinition == null || resourceDefinition.Length == 0)
            {
                throw new InvalidDataContractException("Roles or ResourceXml is missing");
            }

            var res = new ExecuteMessage { HasError = false };

            List<DynamicServiceObjectBase> compiledResources = null;
            var errorMessage = Resources.CompilerMessage_BuildFailed + " " + DateTime.Now;
            try
            {
                // Replace with proper object hydration and parsing ;)
                compiledResources = new ServiceDefinitionLoader().GenerateServiceGraph(resourceDefinition);
                if(compiledResources.Count == 0)
                {
                    CompileMessageRepo.Instance.AddMessage(workspaceID, new List<ICompileMessageTO>
                    {
                        new CompileMessageTO
                        {
                            ErrorType = ErrorType.Warning,
                            MessageID = Guid.NewGuid(),
                            MessagePayload = errorMessage
                        }
                    });

                    res.SetMessage(Resources.CompilerMessage_BuildFailed + " " + DateTime.Now);
                }
            }
            catch(Exception err)
            {
                Dev2Logger.Log.Error(err);
                CompileMessageRepo.Instance.AddMessage(workspaceID, new List<ICompileMessageTO>
                {
                    new CompileMessageTO
                    {
                        ErrorType = ErrorType.Warning,
                        MessageID = Guid.NewGuid(),
                        MessagePayload = errorMessage                            
                    }
                });

                res.SetMessage(errorMessage);
            }

            if(compiledResources != null)
            {
                var saveResult = ResourceCatalog.Instance.SaveResource(workspaceID, resourceDefinition,null,"Save","");
                res.SetMessage(saveResult.Message + " " + DateTime.Now);
            }

            Dev2JsonSerializer serializer = new Dev2JsonSerializer();
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
            DynamicService newDs = new DynamicService { Name = HandlesType(), DataListSpecification = new StringBuilder("<DataList><Roles ColumnIODirection=\"Input\"/><ResourceXml ColumnIODirection=\"Input\"/><WorkspaceID ColumnIODirection=\"Input\"/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>") };
            ServiceAction sa = new ServiceAction();
            sa.Name = HandlesType();
            sa.ActionType = enActionType.InvokeManagementDynamicService;
            sa.SourceMethod = HandlesType();
            newDs.Actions.Add(sa);

            return newDs;
        }

        public string HandlesType()
        {
            return "SaveResourceService";
        }
    }
}
