#pragma warning disable
﻿/*
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
using System.Linq;
using System.Text;
using Dev2.Common;
using Dev2.Common.Interfaces.Enums;
using Dev2.Common.Interfaces.Infrastructure;
using Dev2.Common.Interfaces.WebServices;
using Dev2.Communication;
using Dev2.DynamicServices;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Workspaces;


namespace Dev2.Runtime.ESB.Management.Services
{
    public class SaveWebService : EsbManagementEndpointBase
    {
        IExplorerServerResourceRepository _serverExplorerRepository;

        public override Guid GetResourceID(Dictionary<string, StringBuilder> requestArgs) => Guid.Empty;

        public override  AuthorizationContext GetAuthorizationContextForService() => AuthorizationContext.Contribute;

        public override  StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            var msg = new ExecuteMessage();
            var serializer = new Dev2JsonSerializer();
            try
            {
                Dev2Logger.Info("Save Resource Service", GlobalConstants.WarewolfInfo);


                values.TryGetValue("Webservice", out StringBuilder resourceDefinition);

                var src = serializer.Deserialize<IWebService>(resourceDefinition);
                
                var parameters = src.Inputs?.Select(a => new MethodParameter() { EmptyToNull = a.EmptyIsNull, IsRequired = a.RequiredField, Name = a.Name, Value = a.Value }).ToList() ?? new List<MethodParameter>();
                
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
                Dev2Logger.Error(err, GlobalConstants.WarewolfError);

            }

            return serializer.SerializeToBuilder(msg);
        }

        public IExplorerServerResourceRepository ServerExplorerRepo
        {
            get => _serverExplorerRepository ?? ServerExplorerRepository.Instance;
            set => _serverExplorerRepository = value;
        }

        public override  DynamicService CreateServiceEntry() => EsbManagementServiceEntry.CreateESBManagementServiceEntry(HandlesType(), "<DataList><Roles ColumnIODirection=\"Input\"/><DbService ColumnIODirection=\"Input\"/><WorkspaceID ColumnIODirection=\"Input\"/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>");

        public override  string HandlesType() => "SaveWebService";
    }
}
