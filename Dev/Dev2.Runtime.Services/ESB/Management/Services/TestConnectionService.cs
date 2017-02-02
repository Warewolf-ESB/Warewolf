
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
    using System.Text;
    using Dev2.Common;
    using Dev2.Common.Interfaces;
    using Dev2.Common.Interfaces.Core;
    using Dev2.Common.Interfaces.Core.DynamicServices;
    using Dev2.Communication;
    using Dev2.Data.ServiceModel;
    using Dev2.DynamicServices;
    using Dev2.DynamicServices.Objects;
    using Dev2.Runtime.ServiceModel;
    using Dev2.Services.Security;
    using Dev2.Workspaces;

    namespace Dev2.Runtime.ESB.Management.Services
    {
    /// <summary>
    /// Adds a resource
    /// </summary>
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public class TestConnectionService : IEsbManagementEndpoint
    { 
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

                    Dev2Logger.Info("Test connection Service");
                    StringBuilder resourceDefinition;
                    string workspaceIdString = string.Empty;

                    values.TryGetValue("ServerSource", out resourceDefinition);

                    IServerSource src = serializer.Deserialize<ServerSource>(resourceDefinition);
                    Connections tester = new Connections();

                    var result = tester.CanConnectToServer(new Connection
                    {
                        Address = src.Address,
                        AuthenticationType = src.AuthenticationType,
                        UserName = src.UserName,
                        Password = src.Password
                    });

                    msg.HasError = false;
                    msg.Message = new StringBuilder(result.IsValid ? "" : result.ErrorMessage);
                    msg.HasError = !result.IsValid;
                  
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
                DynamicService newDs = new DynamicService { Name = HandlesType(), DataListSpecification = new StringBuilder("<DataList><Roles ColumnIODirection=\"Input\"/><ServerSource ColumnIODirection=\"Input\"/><WorkspaceID ColumnIODirection=\"Input\"/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>") };
                ServiceAction sa = new ServiceAction { Name = HandlesType(), ActionType = enActionType.InvokeManagementDynamicService, SourceMethod = HandlesType() };
                newDs.Actions.Add(sa);

                return newDs;
            }

            public string HandlesType()
            {
                return "TestConnectionService";
            }
        }
    }

