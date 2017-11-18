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
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using Dev2.Common;
using Dev2.Common.Interfaces.Explorer;
using Dev2.Common.Interfaces.Infrastructure;
using Dev2.Communication;
using Dev2.DynamicServices;
using Dev2.Runtime.Hosting;
using Dev2.Workspaces;
using Warewolf.Resource.Errors;


namespace Dev2.Runtime.ESB.Management.Services
{
    public class GetDirectoriesRelativeToServer : DefaultEsbManagementEndpoint
    {
        IExplorerServerResourceRepository _serverExplorerRepository;
        
        #region Implementation of DefaultEsbManagementEndpoint

        public override StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            try
            {

          
            string directory = null;
            StringBuilder result = new StringBuilder();
            if(values == null)
            {
                throw new InvalidDataContractException(ErrorResource.NoParameter);
            }
                values.TryGetValue("Directory", out StringBuilder tmp);
                if (tmp != null)
            {
                directory = tmp.ToString();
            }
            if(string.IsNullOrEmpty(directory))
            {
                throw new InvalidDataContractException(ErrorResource.DirectoryIsRequired);
            }
            Dev2Logger.Info("Get Directories Relative to Server. "+directory, GlobalConstants.WarewolfInfo);
            result.Append("<JSON>");
            var explorerItem = ServerExplorerRepo.Load("Folder", string.Empty);
            var jsonTreeNode = new JsonTreeNode(explorerItem);
            var serializer = new Dev2JsonSerializer();
            var directoryInfoAsJson = serializer.Serialize(jsonTreeNode);
            result.Append(directoryInfoAsJson);
            result.Append("</JSON>");
            return result;
            }
            catch (Exception e)
            {
                Dev2Logger.Error(e, GlobalConstants.WarewolfError);
                throw;
            }
        }

        public IExplorerServerResourceRepository ServerExplorerRepo
        {
            get { return _serverExplorerRepository ?? ServerExplorerRepository.Instance; }
            set { _serverExplorerRepository = value; }
        }

        #endregion

        public override DynamicService CreateServiceEntry() => EsbManagementServiceEntry.CreateESBManagementServiceEntry(HandlesType(), "<DataList><Directory ColumnIODirection=\"Input\"/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>");

        public override string HandlesType() => "GetDirectoriesRelativeToServerService";
    }

    internal class JsonTreeNode
    {
        public JsonTreeNode(IExplorerItem explorerItem)
        {
            if(explorerItem.ResourceType == "Server")
            {
                title = "Root";
                key = "root";
            }
            else
            {
                title = explorerItem.DisplayName;
                string name = Regex.Replace(explorerItem.ResourcePath.Replace(EnvironmentVariables.ApplicationPath + "\\", ""), @"\\", @"\\");
                key = name;
            }
            isFolder = true;
            isLazy = false;
            children = new List<JsonTreeNode>();
            foreach(var child in explorerItem.Children)
            {
                children.Add(new JsonTreeNode(child));
            }
        }

        
        public string title { get; set; }
        public bool isFolder { get; set; }
        public string key { get; set; }
        public bool isLazy { get; set; }
        public List<JsonTreeNode> children { get; set; }
    }
}
