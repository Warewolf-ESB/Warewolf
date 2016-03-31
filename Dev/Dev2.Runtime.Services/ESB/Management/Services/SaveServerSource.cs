﻿using System;
using System.Collections.Generic;
using System.Text;
using Dev2.Common;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Common.Interfaces.Infrastructure;
using Dev2.Communication;
using Dev2.Data.ServiceModel;
using Dev2.DynamicServices;
using Dev2.DynamicServices.Objects;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.ServiceModel;
using Dev2.Workspaces;

namespace Dev2.Runtime.ESB.Management.Services
{
    /// <summary>
    /// Adds a resource
    /// </summary>
    public class SaveServerSource : IEsbManagementEndpoint
    {
        IExplorerServerResourceRepository _serverExplorerRepository;

        public static int GETSpecifiedIndexOf(string str, char ch, int index)
        {
            int i = 0, o = 1;
            while ((i = str.IndexOf(ch, i)) != -1)
            {
                if (o == index) return i;
                o++;
                i++;
            }
            return 0;
        }
        public StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            ExecuteMessage msg = new ExecuteMessage();
            Dev2JsonSerializer serializer = new Dev2JsonSerializer();
            try
            {

                Dev2Logger.Info("Save Resource Service");
                StringBuilder resourceDefinition;
                string workspaceIdString = string.Empty;

                values.TryGetValue("ServerSource", out resourceDefinition);

                IServerSource src = serializer.Deserialize<ServerSource>(resourceDefinition);
                Connection con = new Connection();

                int portIndex = GETSpecifiedIndexOf(src.Address, ':', 2);
                string port = src.Address.Substring(portIndex + 1);

                con.Address = src.Address;
                con.AuthenticationType = src.AuthenticationType;
                con.UserName = src.UserName;
                con.Password = src.Password;
                con.ResourceName = src.Name;
                con.ResourcePath = src.ResourcePath;
                con.ResourceID = src.ID;
                con.WebServerPort = int.Parse(port);
                Connections tester = new Connections();
                var res = tester.CanConnectToServer(con);
                if(res.IsValid)
                ResourceCatalog.Instance.SaveResource(GlobalConstants.ServerWorkspaceID,con);
                ServerExplorerRepo.UpdateItem(con);
                msg.HasError = false;
                msg.Message = new StringBuilder(res.IsValid ? "" : res.ErrorMessage);


            }
            catch (Exception err)
            {
                msg.HasError = true;
                msg.Message = new StringBuilder(err.Message);
                Dev2Logger.Error(err);

            }

            return serializer.SerializeToBuilder(msg);
        }
        public IExplorerServerResourceRepository ServerExplorerRepo
        {
            get { return _serverExplorerRepository ?? ServerExplorerRepository.Instance; }
            set { _serverExplorerRepository = value; }
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
            return "SaveServerSourceService";
        }
    }
}