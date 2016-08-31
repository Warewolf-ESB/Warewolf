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
using System.Linq;
using System.Text;
using Dev2.Common;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Common.Interfaces.DB;
using Dev2.Communication;
using Dev2.DynamicServices;
using Dev2.DynamicServices.Objects;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.Interfaces;
using Dev2.Runtime.ServiceModel;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Workspaces;
using Unlimited.Framework.Converters.Graph.Ouput;
using Warewolf.Core;

namespace Dev2.Runtime.ESB.Management.Services
{
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public class TestPluginService : IEsbManagementEndpoint
    {
        IResourceCatalog _rescat;
        IPluginServices _pluginServices;

        public StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            ExecuteMessage msg = new ExecuteMessage();
            Dev2JsonSerializer serializer = new Dev2JsonSerializer();
            try
            {

                Dev2Logger.Info("Test Plugin Service");
                StringBuilder resourceDefinition;

                values.TryGetValue("PluginService", out resourceDefinition);
                IPluginService src = serializer.Deserialize<IPluginService>(resourceDefinition);

                var methods = GetMethods(serializer, src.Source.Id, src.Action.FullName).First(a => a.Method == src.Action.Method);

                // ReSharper disable MaximumChainedReferences
                var parameters = src.Inputs == null ? new List<MethodParameter>() : src.Inputs.Select(a => new MethodParameter { EmptyToNull = a.EmptyIsNull, IsRequired = a.RequiredField, Name = a.Name, Value = a.Value, TypeName = a.TypeName }).ToList();
                // ReSharper restore MaximumChainedReferences
                var pluginsrc = ResourceCatalog.Instance.GetResource<PluginSource>(GlobalConstants.ServerWorkspaceID, src.Source.Id);
                var res = new PluginService
                {
                    Method = new ServiceMethod(src.Action.Method, src.Name, parameters, new OutputDescription(), new List<MethodOutput>(), "test"),
                    Namespace = src.Action.FullName,
                    ResourceName = src.Name,
                    ResourceID = src.Id,
                    Source = pluginsrc
                };

                string serializedResult;
                var result = PluginServices.Test(serializer.SerializeToBuilder(res).ToString(),out serializedResult);
                msg.HasError = false;
                msg.Message = serializer.SerializeToBuilder(new RecordsetListWrapper{Description = result.Description,RecordsetList = result,SerializedResult = serializedResult});
            }
            catch (Exception err)
            {
                msg.HasError = true;
                msg.Message = new StringBuilder(err.Message);
                Dev2Logger.Error(err);

            }

            return serializer.SerializeToBuilder(msg);
        }

        static IEnumerable<IPluginAction> GetMethods(Dev2JsonSerializer serializer, Guid srcId, string ns)
        {
            PluginServices services = new PluginServices();
            var src = ResourceCatalog.Instance.GetResource<PluginSource>(GlobalConstants.ServerWorkspaceID, srcId);
            //src.AssemblyName = ns.FullName;
            PluginService svc = new PluginService { Namespace = ns, Source = src };

            var methods = services.Methods(svc, Guid.Empty, Guid.Empty).Select(a => new PluginAction()
            {
                FullName = a.Name,
                Inputs = a.Parameters.Select(x => new ServiceInput(x.Name, x.DefaultValue ?? "") { Name = x.Name, EmptyIsNull = x.EmptyToNull, RequiredField = x.IsRequired, TypeName = x.TypeName } as IServiceInput).ToList(),
                Method = a.Name,
                Variables = a.Parameters.Select(x => new NameValue() { Name = x.Name + " (" + x.TypeName + ")", Value = "" } as INameValue).ToList(),
            } as IPluginAction
                ).ToList();
            return methods;
        }

        public DynamicService CreateServiceEntry()
        {
            DynamicService newDs = new DynamicService { Name = HandlesType(), DataListSpecification = new StringBuilder("<DataList><Roles ColumnIODirection=\"Input\"/><PluginService ColumnIODirection=\"Input\"/><WorkspaceID ColumnIODirection=\"Input\"/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>") };
            ServiceAction sa = new ServiceAction { Name = HandlesType(), ActionType = enActionType.InvokeManagementDynamicService, SourceMethod = HandlesType() };
            newDs.Actions.Add(sa);

            return newDs;
        }

        public IResourceCatalog ResourceCatalogue
        {
            get
            {
                return _rescat ?? ResourceCatalog.Instance;
            }
            set
            {
                _rescat = value;
            }
        }
        public IPluginServices PluginServices
        {
            get
            {
                return _pluginServices ?? new PluginServices();
            }
            set
            {
                _pluginServices = value;
            }
        }

        public string HandlesType()
        {
            return "TestPluginService";
        }
    }
}
