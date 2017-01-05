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
using Dev2.Communication;
using Dev2.DynamicServices;
using Dev2.DynamicServices.Objects;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.Interfaces;
using Dev2.Runtime.ServiceModel;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Services.Security;
using Dev2.Workspaces;
using Unlimited.Framework.Converters.Graph.Ouput;
// ReSharper disable MemberCanBePrivate.Global

namespace Dev2.Runtime.ESB.Management.Services
{
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public class TestPluginService : IEsbManagementEndpoint
    {
        IResourceCatalog _rescat;
        IPluginServices _pluginServices;



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

                Dev2Logger.Info("Test Plugin Service");
                StringBuilder resourceDefinition;

                values.TryGetValue("PluginService", out resourceDefinition);
                IPluginService src = serializer.Deserialize<IPluginService>(resourceDefinition);


                // ReSharper disable MaximumChainedReferences
                var parameters = src.Inputs?.Select(a => new MethodParameter { EmptyToNull = a.EmptyIsNull, IsRequired = a.RequiredField, Name = a.Name, Value = a.Value, TypeName = a.TypeName }).ToList() ?? new List<MethodParameter>();
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
