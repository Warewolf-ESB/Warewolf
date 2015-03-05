using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using Dev2.Common;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Common.Interfaces.DB;
using Dev2.Communication;
using Dev2.DynamicServices;
using Dev2.DynamicServices.Objects;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Workspaces;
using Unlimited.Framework.Converters.Graph.Ouput;

namespace Dev2.Runtime.ESB.Management.Services
{
    // ReSharper disable UnusedMember.Global
    public class TestDbService : IEsbManagementEndpoint
        // ReSharper restore UnusedMember.Global
    {
        public StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            ExecuteMessage msg = new ExecuteMessage();
            Dev2JsonSerializer serializer = new Dev2JsonSerializer();
            try
            {

                Dev2Logger.Log.Info("Test DB Connection Service");
                StringBuilder resourceDefinition;

                values.TryGetValue("DbService", out resourceDefinition);

                IDatabaseService src = serializer.Deserialize<IDatabaseService>(resourceDefinition);
                // ReSharper disable MaximumChainedReferences
                var parameters = src.Inputs==null?new List<MethodParameter>(): src.Inputs.Select(a => new MethodParameter() { EmptyToNull = a.EmptyIsNull, IsRequired = a.RequiredField, Name = a.Name, Value = a.Value }).ToList();
                // ReSharper restore MaximumChainedReferences
                var source = ResourceCatalog.Instance.GetResource<DbSource>(GlobalConstants.ServerWorkspaceID, src.Source.Id);
                var res = new DbService
                {
                    Method = new ServiceMethod(src.Name, src.Name, parameters, new OutputDescription(), new List<MethodOutput>(), src.Action.Name),
                    ResourceName = src.Name,
                    ResourcePath = src.Path,
                    ResourceID = src.Id
                   ,Source = source


                };
                
                ServiceModel.Services services = new ServiceModel.Services();
                var output = services.DbTest(res, GlobalConstants.ServerWorkspaceID, Guid.Empty);
                output.Name = src.Action.Name;
                var result = ToDataTable(output);
                result.TableName = src.Action.Name;
                msg.HasError = false;
                msg.Message = serializer.SerializeToBuilder(result);
            }
            catch (Exception err)
            {
                msg.HasError = true;
                msg.Message = new StringBuilder(err.Message);
                Dev2Logger.Log.Error(err);

            }

            return serializer.SerializeToBuilder(msg);
        }

        DataTable ToDataTable(Recordset output)
        {
            DataTable dt = new DataTable("TestResults");
            dt.Columns.Add("Record Name");

            foreach(var recordsetField in output.Fields)
            {
                dt.Columns.Add(new DataColumn(recordsetField.Name));
            }
            
            for(int i = 0; i < output.Records.Count; i++)
            {
                var row = output.Records[i];

                List<object> data = new List<object>(){output.Name+"("+ i.ToString(CultureInfo.InvariantCulture)+")"};
                data.AddRange( row.Cells.Select(a => a.Value));
                dt.Rows.Add(data.ToArray());
            }
            return dt;
        }

        public DynamicService CreateServiceEntry()
        {
            DynamicService newDs = new DynamicService { Name = HandlesType(), DataListSpecification = new StringBuilder("<DataList><Roles ColumnIODirection=\"Input\"/><DbSource ColumnIODirection=\"Input\"/><WorkspaceID ColumnIODirection=\"Input\"/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>") };
            ServiceAction sa = new ServiceAction { Name = HandlesType(), ActionType = enActionType.InvokeManagementDynamicService, SourceMethod = HandlesType() };
            newDs.Actions.Add(sa);

            return newDs;
        }

        public string HandlesType()
        {
            return "TestDbService";
        }
    }
}