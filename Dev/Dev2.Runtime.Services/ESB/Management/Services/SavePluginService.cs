using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dev2.Common;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Infrastructure;
using Dev2.Communication;
using Dev2.Converters.Graph.DataTable;
using Dev2.DynamicServices;
using Dev2.DynamicServices.Objects;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Workspaces;

namespace Dev2.Runtime.ESB.Management.Services
{
    public class SavePluginService : IEsbManagementEndpoint
    {
        IExplorerServerResourceRepository _serverExplorerRepository;
        IResourceCatalog _resourceCatalogue;

        public StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            ExecuteMessage msg = new ExecuteMessage();
            Dev2JsonSerializer serializer = new Dev2JsonSerializer();
            try
            {

                Dev2Logger.Log.Info("Save Plugin Service");
                StringBuilder resourceDefinition;


                values.TryGetValue("PluginService", out resourceDefinition);

                IPluginService serviceDef = serializer.Deserialize<IPluginService>(resourceDefinition);
                // ReSharper disable MaximumChainedReferences
                               // ReSharper restore MaximumChainedReferences
                var source = ResourceCatalogue.GetResource<PluginSource>(GlobalConstants.ServerWorkspaceID, serviceDef.Source.Id);
                var output = new List<MethodOutput>(serviceDef.OutputMappings.Select(a => new MethodOutput(a.Name, a.OutputName, "", false, a.RecordSetName, false, "", false, "", false)));
                var recset = new RecordsetList();
                var rec = new Recordset();
                rec.Fields.AddRange(new List<RecordsetField>(serviceDef.OutputMappings.Select(a => new RecordsetField { Name = a.Name, Alias = a.OutputName, RecordsetAlias = a.RecordSetName, Path = new DataTablePath(a.RecordSetName, a.Name) })));
                recset.Add(rec);
                var parameters = serviceDef.Inputs == null ? new List<MethodParameter>() : serviceDef.Inputs.Select(a => new MethodParameter { EmptyToNull = a.EmptyIsNull, IsRequired = a.RequiredField, Name = a.Name, Value = a.Value, Type = a.TypeName }).ToList();

                var res = new PluginService
                {
                    Method = new ServiceMethod(serviceDef.Name, serviceDef.Name, parameters, null, output, serviceDef.Action.Method),
                    ResourceName = serviceDef.Name,
                    ResourcePath = serviceDef.Path,
                    ResourceID = serviceDef.Id,
                    Source = source,
                    Recordsets = recset,
                    Namespace = serviceDef.Action.FullName,


                };
                IPluginService src = serializer.Deserialize<IPluginService>(resourceDefinition);

   

                // ReSharper disable MaximumChainedReferences
                              // ReSharper restore MaximumChainedReferences
                var pluginsrc = ResourceCatalog.Instance.GetResource<PluginSource>(GlobalConstants.ServerWorkspaceID, src.Source.Id);


                ResourceCatalog.Instance.SaveResource(GlobalConstants.ServerWorkspaceID, res);
                var explorerItem = ServerExplorerRepo.UpdateItem(res);

                msg.HasError = false;
            }
           
            catch (Exception err)
            {
                msg.HasError = true;
                msg.Message = new StringBuilder(err.Message);
                Dev2Logger.Log.Error(err);

            }

            return serializer.SerializeToBuilder(msg);
        }

        public DynamicService CreateServiceEntry()
        {
            DynamicService newDs = new DynamicService { Name = HandlesType(), DataListSpecification = new StringBuilder("<DataList><Roles ColumnIODirection=\"Input\"/><DbService ColumnIODirection=\"Input\"/><WorkspaceID ColumnIODirection=\"Input\"/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>") };
            ServiceAction sa = new ServiceAction { Name = HandlesType(), ActionType = Common.Interfaces.Core.DynamicServices.enActionType.InvokeManagementDynamicService, SourceMethod = HandlesType() };
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
                return _resourceCatalogue?? ResourceCatalog.Instance;
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