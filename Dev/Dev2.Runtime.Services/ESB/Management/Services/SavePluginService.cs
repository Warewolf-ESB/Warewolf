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
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Common.Interfaces.Infrastructure;
using Dev2.Communication;
using Dev2.Converters.Graph.DataTable;
using Dev2.DynamicServices;
using Dev2.DynamicServices.Objects;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.Interfaces;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Services.Security;
using Dev2.Workspaces;
// ReSharper disable MemberCanBePrivate.Global

namespace Dev2.Runtime.ESB.Management.Services
{
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public class SavePluginService : IEsbManagementEndpoint
    {
        IExplorerServerResourceRepository _serverExplorerRepository;
        IResourceCatalog _resourceCatalogue;


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
                Dev2Logger.Info("Save Plugin Service");
                StringBuilder resourceDefinition;


                values.TryGetValue("PluginService", out resourceDefinition);

                IPluginService serviceDef = serializer.Deserialize<IPluginService>(resourceDefinition);
                // ReSharper disable MaximumChainedReferences
                // ReSharper restore MaximumChainedReferences
                var source = ResourceCatalogue.GetResource<PluginSource>(GlobalConstants.ServerWorkspaceID, serviceDef.Source.Id);
                var output = new List<MethodOutput>(serviceDef.OutputMappings.Select(a => new MethodOutput(a.MappedFrom, a.MappedTo, "", false, a.RecordSetName, false, "", false, "", false)));
                var recset = new RecordsetList();
                var rec = new Recordset();
                rec.Fields.AddRange(new List<RecordsetField>(serviceDef.OutputMappings.Select(a => new RecordsetField { Name = a.MappedFrom, Alias = a.MappedTo, RecordsetAlias = a.RecordSetName, Path = new DataTablePath(a.RecordSetName, a.MappedFrom) })));
                recset.Add(rec);
                var parameters = serviceDef.Inputs?.Select(a => new MethodParameter { EmptyToNull = a.EmptyIsNull, IsRequired = a.RequiredField, Name = a.Name, Value = a.Value, TypeName = a.TypeName }).ToList() ?? new List<MethodParameter>();
                var constructorParams = serviceDef.Constructor.Inputs.Select(input => new ConstructorParameter() { Name = input.Name, Value = input.Value, TypeName = input.TypeName, EmptyToNull = input.EmptyToNull, IsRequired = input.IsRequired, ShortTypeName = Type.GetType(input.TypeName)?.FullName });
                var res = new PluginService
                {
                    Constructor = new ServiceConstructor(serviceDef.Constructor.ConstructorName, constructorParams, null),
                    Method = new ServiceMethod(serviceDef.Name, serviceDef.Name, parameters, null, output, serviceDef.Action.Method),
                    ResourceName = serviceDef.Name,
                    ResourceID = serviceDef.Id,
                    Source = source,
                    Recordsets = recset,
                    Namespace = serviceDef.Action.FullName
                };

                ResourceCatalog.Instance.SaveResource(GlobalConstants.ServerWorkspaceID, res, serviceDef.Path);
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
            DynamicService newDs = new DynamicService { Name = HandlesType(), DataListSpecification = new StringBuilder("<DataList><Roles ColumnIODirection=\"Input\"/><PluginService ColumnIODirection=\"Input\"/><WorkspaceID ColumnIODirection=\"Input\"/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>") };
            ServiceAction sa = new ServiceAction { Name = HandlesType(), ActionType = enActionType.InvokeManagementDynamicService, SourceMethod = HandlesType() };
            newDs.Actions.Add(sa);

            return newDs;
        }
        public IExplorerServerResourceRepository ServerExplorerRepo
        {
            get { return _serverExplorerRepository ?? ServerExplorerRepository.Instance; }
            set { _serverExplorerRepository = value; }
        }
        public IResourceCatalog ResourceCatalogue
        {
            get
            {
                return _resourceCatalogue ?? ResourceCatalog.Instance;
            }
            set
            {
                _resourceCatalogue = value;
            }
        }

        public string HandlesType()
        {
            return "SavePluginService";
        }
    }
}
