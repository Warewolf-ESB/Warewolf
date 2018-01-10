﻿using System;
using System.Collections.Generic;
using System.Text;
using Dev2.Common;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core;
using Dev2.Common.Interfaces.Enums;
using Dev2.Common.Interfaces.Infrastructure;
using Dev2.Communication;
using Dev2.Data.ServiceModel;
using Dev2.DynamicServices;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.ServiceModel;
using Dev2.Workspaces;


namespace Dev2.Runtime.ESB.Management.Services
{
    public class SaveServerSource : IEsbManagementEndpoint
    {
        IExplorerServerResourceRepository _serverExplorerRepository;
        public Guid GetResourceID(Dictionary<string, StringBuilder> requestArgs) => Guid.Empty;

        public AuthorizationContext GetAuthorizationContextForService() => AuthorizationContext.Contribute;

        static int GetSpecifiedIndexOf(string str, char ch, int index)
        {
            var i = 0;
            var o = 1;
            while ((i = str.IndexOf(ch, i)) != -1)
            {
                if (o == index)
                {
                    return i;
                }

                o++;
                i++;
            }
            return 0;
        }

        public StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            var msg = new ExecuteMessage();
            var serializer = new Dev2JsonSerializer();
            try
            {
                Dev2Logger.Info("Save Resource Service",GlobalConstants.WarewolfInfo);

                values.TryGetValue("ServerSource", out StringBuilder resourceDefinition);

                IServerSource src = serializer.Deserialize<ServerSource>(resourceDefinition);
                var con = new Connection();

                var portIndex = GetSpecifiedIndexOf(src.Address, ':', 2);
                var port = src.Address.Substring(portIndex + 1);

                con.Address = src.Address;
                con.AuthenticationType = src.AuthenticationType;
                con.UserName = src.UserName;
                con.Password = src.Password;
                con.ResourceName = src.Name;
                con.ResourceID = src.ID;
                con.WebServerPort = int.Parse(port);
                var tester = new Connections();
                var res = tester.CanConnectToServer(con);
                if (res.IsValid)
                {
                    ResourceCatalog.Instance.SaveResource(GlobalConstants.ServerWorkspaceID, con, src.ResourcePath);
                    ServerExplorerRepo.UpdateItem(con);
                    msg.HasError = false;
                    msg.Message = new StringBuilder();
                }
                else
                {
                    msg.HasError = true;
                    msg.Message = new StringBuilder(res.ErrorMessage);
                }

            }
            catch (Exception err)
            {
                msg.HasError = true;
                msg.Message = new StringBuilder(err.Message);
                Dev2Logger.Error(err,GlobalConstants.WarewolfError);

            }

            return serializer.SerializeToBuilder(msg);
        }

        public IExplorerServerResourceRepository ServerExplorerRepo
        {
            get => _serverExplorerRepository ?? ServerExplorerRepository.Instance;
            set => _serverExplorerRepository = value;
        }

        public DynamicService CreateServiceEntry() => EsbManagementServiceEntry.CreateESBManagementServiceEntry(HandlesType(), "<DataList><Roles ColumnIODirection=\"Input\"/><ServerSource ColumnIODirection=\"Input\"/><WorkspaceID ColumnIODirection=\"Input\"/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>");

        public string HandlesType() => "SaveServerSourceService";
    }
}