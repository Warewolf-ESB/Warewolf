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
using System.Diagnostics.CodeAnalysis;
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
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public class SaveResource : IEsbManagementEndpoint
    {
        public StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            try
            {

                Dev2Logger.Info("Save Resource Service");
                StringBuilder resourceDefinition;
              
                string workspaceIdString = string.Empty;
                StringBuilder savePathValue;
                values.TryGetValue("savePath", out savePathValue);
                if (savePathValue == null)
                {
                    throw new InvalidDataContractException("SavePath is missing");
                }
                values.TryGetValue("ResourceXml", out resourceDefinition);
                StringBuilder tmp;
                values.TryGetValue("WorkspaceID", out tmp);
                if (tmp != null)
                {
                    workspaceIdString = tmp.ToString();
                }
                Guid workspaceId;
                if (!Guid.TryParse(workspaceIdString, out workspaceId))
                {
                    workspaceId = theWorkspace.ID;
                }

                if (resourceDefinition == null || resourceDefinition.Length == 0)
                {
                    throw new InvalidDataContractException("ResourceXml is missing");
                }
                Dev2JsonSerializer serializer = new Dev2JsonSerializer();
                resourceDefinition = new StringBuilder( serializer.Deserialize<CompressedExecuteMessage>(resourceDefinition).GetDecompressedMessage());
                var res = new ExecuteMessage { HasError = false };

                List<DynamicServiceObjectBase> compiledResources = null;
                var errorMessage = Resources.CompilerMessage_BuildFailed + " " + DateTime.Now;
                try
                {
                    // Replace with proper object hydration and parsing ;)
                    compiledResources = new ServiceDefinitionLoader().GenerateServiceGraph(resourceDefinition);
                    if (compiledResources.Count == 0)
                    {
                        CompileMessageRepo.Instance.AddMessage(workspaceId, new List<ICompileMessageTO>
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
                catch (Exception err)
                {
                    Dev2Logger.Error(err);
                    CompileMessageRepo.Instance.AddMessage(workspaceId, new List<ICompileMessageTO>
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

                if (compiledResources != null)
                {
                    var saveResult = ResourceCatalog.Instance.SaveResource(workspaceId, resourceDefinition, savePathValue.ToString(), "Save");
                    res.SetMessage(saveResult.Message + " " + DateTime.Now);
                }

           
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
            DynamicService newDs = new DynamicService { Name = HandlesType(), DataListSpecification = new StringBuilder("<DataList><Roles ColumnIODirection=\"Input\"/><ResourceXml ColumnIODirection=\"Input\"/><WorkspaceID ColumnIODirection=\"Input\"/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>") };
            ServiceAction sa = new ServiceAction { Name = HandlesType(), ActionType = enActionType.InvokeManagementDynamicService, SourceMethod = HandlesType() };
            newDs.Actions.Add(sa);

            return newDs;
        }

        public string HandlesType()
        {
            return "SaveResourceService";
        }
    }
}
