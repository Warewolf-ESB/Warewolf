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
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using Dev2.Common;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Common.Interfaces.Infrastructure;
using Dev2.Common.Interfaces.WebServices;
using Dev2.Communication;
using Dev2.DynamicServices;
using Dev2.DynamicServices.Objects;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Services.Security;
using Dev2.Workspaces;
// ReSharper disable MemberCanBePrivate.Global

namespace Dev2.Runtime.ESB.Management.Services
{
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public class SaveWebService : IEsbManagementEndpoint
    {
        IExplorerServerResourceRepository _serverExplorerRepository;


        public Guid GetResourceID(Dictionary<string, StringBuilder> requestArgs)
        {
            return Guid.Empty;
        }

        public AuthorizationContext GetAuthorizationContextForService()
        {
            return AuthorizationContext.Contribute;
        }

        public StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            ExecuteMessage msg = new ExecuteMessage();
            Dev2JsonSerializer serializer = new Dev2JsonSerializer();
            try
            {
                Dev2Logger.Info("Save Resource Service");
                StringBuilder resourceDefinition;


                values.TryGetValue("Webservice", out resourceDefinition);

                var src = serializer.Deserialize<IWebService>(resourceDefinition);
                // ReSharper disable MaximumChainedReferences
                var parameters = src.Inputs?.Select(a => new MethodParameter() { EmptyToNull = a.EmptyIsNull, IsRequired = a.RequiredField, Name = a.Name, Value = a.Value }).ToList() ?? new List<MethodParameter>();
                // ReSharper restore MaximumChainedReferences
                var source = ResourceCatalog.Instance.GetResource<WebSource>(GlobalConstants.ServerWorkspaceID, src.Source.Id);
                var output = new List<MethodOutput>(src.OutputMappings.Select(a => new MethodOutput(a.MappedFrom, a.MappedTo, "", false, a.RecordSetName, false, "", false, "", false)));
                var recset = new Recordset();
                recset.Fields.AddRange(new List<RecordsetField>(src.OutputMappings.Select(a => new RecordsetField { Name = a.MappedFrom, Alias = a.MappedTo, RecordsetAlias = a.RecordSetName })));
                var serviceOutputMapping = src.OutputMappings.FirstOrDefault(mapping => !string.IsNullOrEmpty(mapping.RecordSetName));
                if (serviceOutputMapping != null)
                {
                    recset.Name = serviceOutputMapping.RecordSetName;
                }
                var recordsetList = new RecordsetList { recset };
                var res = new WebService
                {
                    Method = new ServiceMethod(),
                    RequestUrl = string.Concat(src.SourceUrl, src.RequestUrl),
                    ResourceName = src.Name,
                    ResourceID = src.Id,
                    RequestBody = src.PostData,
                    Recordsets = recordsetList,
                    Source = source,
                    Headers = src.Headers,
                    RequestMethod = src.Method,
                    RequestResponse = src.Response
     
                };
                res.Method.Name = src.Name;
                res.Method.Parameters = parameters;
                res.Method.Outputs = output;
                res.Method.OutputDescription = res.GetOutputDescription();
                ResourceCatalog.Instance.SaveResource(GlobalConstants.ServerWorkspaceID, res, src.Path);
                ServerExplorerRepo.UpdateItem(res);

                msg.HasError = false;
            }

            catch (Exception err)
            {
                msg.HasError = true;
                msg.Message = new StringBuilder(err.Message);
                Dev2Logger.Error(err);

            }

            return serializer.SerializeToBuilder(msg);
        }

        public DynamicService CreateServiceEntry()
        {
            DynamicService newDs = new DynamicService { Name = HandlesType(), DataListSpecification = new StringBuilder("<DataList><Roles ColumnIODirection=\"Input\"/><DbService ColumnIODirection=\"Input\"/><WorkspaceID ColumnIODirection=\"Input\"/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>") };
            ServiceAction sa = new ServiceAction { Name = HandlesType(), ActionType = enActionType.InvokeManagementDynamicService, SourceMethod = HandlesType() };
            newDs.Actions.Add(sa);

            return newDs;
        }
        public IExplorerServerResourceRepository ServerExplorerRepo
        {
            get { return _serverExplorerRepository ?? ServerExplorerRepository.Instance; }
            set { _serverExplorerRepository = value; }
        }
        public string HandlesType()
        {
            return "SaveWebService";
        }
    }
}
