
    /*
    *  Warewolf - The Easy Service Bus
    *  Copyright 2015 by Warewolf Ltd <alpha@warewolf.io>
    *  Licensed under GNU Affero General Public License 3.0 or later. 
    *  Some rights reserved.
    *  Visit our website for more information <http://warewolf.io/>
    *  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
    *  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
    */

    using System;
    using System.Collections.Generic;
    using System.Text;
    using Dev2.Common;
    using Dev2.Common.Interfaces.Core;
    using Dev2.Common.Interfaces.Core.DynamicServices;
    using Dev2.Common.Interfaces.ServerDialogue;
    using Dev2.Communication;
    using Dev2.Data.ServiceModel;
    using Dev2.DynamicServices;
    using Dev2.DynamicServices.Objects;
    using Dev2.Runtime.ServiceModel;
    using Dev2.Workspaces;

    namespace Dev2.Runtime.ESB.Management.Services
    {
        /// <summary>
        /// Adds a resource
        /// </summary>
        public class TestConnectionService : IEsbManagementEndpoint
        {
            public StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
            {
                ExecuteMessage msg = new ExecuteMessage();
                 Dev2JsonSerializer serializer = new Dev2JsonSerializer();
                try
                {

                    Dev2Logger.Log.Info("Save Resource Service");
                    StringBuilder resourceDefinition;
                    string workspaceIdString = string.Empty;

                    values.TryGetValue("ServerSource", out resourceDefinition);

                    IServerSource src = serializer.Deserialize<ServerSource>(resourceDefinition);
                    Connection con = new Connection();
                    con.Address = src.Address;
                    con.AuthenticationType = src.AuthenticationType;
                    con.UserName = src.UserName;
                    con.Password = src.Password;
                    Connections tester = new Connections();
                    var res = tester.CanConnectToServer(con);
                    msg.HasError = false;
                    msg.Message = new StringBuilder( res.IsValid ? "" : res.ErrorMessage);

                  
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

