using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dev2.Common;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Common.Interfaces.Infrastructure;
using Dev2.Common.Interfaces.WebServices;
using Dev2.Communication;
using Dev2.Converters.Graph.DataTable;
using Dev2.DynamicServices;
using Dev2.DynamicServices.Objects;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Workspaces;
using Unlimited.Framework.Converters.Graph.Ouput;

namespace Dev2.Runtime.ESB.Management.Services
{
    public class SaveWebService : IEsbManagementEndpoint
    {
        IExplorerServerResourceRepository _serverExplorerRepository;

        public StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            ExecuteMessage msg = new ExecuteMessage();
            Dev2JsonSerializer serializer = new Dev2JsonSerializer();
            try
            {

                Dev2Logger.Log.Info("Save Resource Service");
                StringBuilder resourceDefinition;


                values.TryGetValue("Webservice", out resourceDefinition);

                var src = serializer.Deserialize<IWebService>(resourceDefinition);
                // ReSharper disable MaximumChainedReferences
                var parameters = src.Inputs == null ? new List<MethodParameter>() : src.Inputs.Select(a => new MethodParameter() { EmptyToNull = a.EmptyIsNull, IsRequired = a.RequiredField, Name = a.Name, Value = a.Value }).ToList();
                // ReSharper restore MaximumChainedReferences
                var source = ResourceCatalog.Instance.GetResource<WebSource>(GlobalConstants.ServerWorkspaceID, src.Source.Id);
                var output = new List<MethodOutput>(src.OutputMappings.Select(a => new MethodOutput(a.Name, a.OutputName, "", false, a.RecordSetName, false, "", false, "", false)));
                var recset = new Recordset();
                recset.Fields.AddRange( new List<RecordsetField>(src.OutputMappings.Select(a=> new RecordsetField{Name = a.Name, Alias = a.OutputName,RecordsetAlias = a.RecordSetName, Path = new DataTablePath( a.RecordSetName,a.Name)})));
                var serviceOutputMapping = src.OutputMappings.FirstOrDefault(mapping => !string.IsNullOrEmpty(mapping.RecordSetName));
                if(serviceOutputMapping != null)
                {
                    recset.Name = serviceOutputMapping.RecordSetName;
                }
                var recordsetList = new RecordsetList{recset};
                var res = new WebService
                {
                    Method = new ServiceMethod(src.Name, src.Name, parameters, new OutputDescription(), output, "test"),
                    RequestUrl = string.Concat(src.SourceUrl, src.RequestUrl),
                    ResourceName = src.Name,
                    ResourcePath = src.Path,
                    ResourceID = src.Id,
                    RequestBody = src.PostData,
                    Recordsets = recordsetList,
                    Source = source,
                    Headers = src.Headers                    
                };
                ResourceCatalog.Instance.SaveResource(GlobalConstants.ServerWorkspaceID, res);
                ServerExplorerRepo.UpdateItem(res);

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