﻿
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2016 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Data;
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
    public class TestDbService : IEsbManagementEndpoint
    {
        public StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            ExecuteMessage msg = new ExecuteMessage();
            Dev2JsonSerializer serializer = new Dev2JsonSerializer();
            try
            {

                Dev2Logger.Info("Test DB Connection Service");
                StringBuilder resourceDefinition;

                values.TryGetValue("DbService", out resourceDefinition);

                IDatabaseService src = serializer.Deserialize<IDatabaseService>(resourceDefinition);
                // ReSharper disable MaximumChainedReferences
                var parameters = src.Inputs == null ? new List<MethodParameter>() : src.Inputs.Select(a => new MethodParameter() { EmptyToNull = a.EmptyIsNull, IsRequired = a.RequiredField, Name = a.Name, Value = a.Value }).ToList();
                // ReSharper restore MaximumChainedReferences
                var source = ResourceCatalog.Instance.GetResource<DbSource>(GlobalConstants.ServerWorkspaceID, src.Source.Id);
                if (source == null)
                {
                    source = new DbSource();
                    source.DatabaseName = src.Source.DbName;
                    source.ResourceID = src.Source.Id;
                    source.ServerType = src.Source.Type;
                    source.ResourceType = Common.Interfaces.Data.ResourceType.DbSource;
                }
                
                var res = new DbService
                {
                    Method = new ServiceMethod(src.Name, src.Name, parameters, new OutputDescription(), new List<MethodOutput>(), src.Action.Name),
                    ResourceName = src.Name,
                    ResourcePath = src.Path,
                    ResourceID = src.Id
                   ,
                    Source = source


                };

                ServiceModel.Services services = new ServiceModel.Services();
                var output = services.DbTest(res, GlobalConstants.ServerWorkspaceID, Guid.Empty);
                if(output.HasErrors)
                {
                    msg.HasError = true;
                    var errorMessage = output.ErrorMessage;
                    msg.Message = new StringBuilder(errorMessage);
                    Dev2Logger.Error(errorMessage);
                }
                else
                {
                    var result = ToDataTable(output);
                    msg.HasError = false;
                    msg.Message = serializer.SerializeToBuilder(result);
                }
            }
            catch (Exception err)
            {
                msg.HasError = true;
                msg.Message = new StringBuilder(err.Message);
                Dev2Logger.Error(err);

            }

            return serializer.SerializeToBuilder(msg);
        }

        DataTable ToDataTable(Recordset output)
        {
            DataTable dt = new DataTable(output.Name);

            foreach (var recordsetField in output.Fields)
            {
                dt.Columns.Add(new DataColumn(recordsetField.Name));
            }

            foreach(var row in output.Records)
            {
                var data = new List<object>();
                data.AddRange(row.Cells.Select(a => a.Value));
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
