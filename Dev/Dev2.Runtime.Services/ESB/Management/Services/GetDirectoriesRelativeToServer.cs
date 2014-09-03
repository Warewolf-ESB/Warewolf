using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using Dev2.Common;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Explorer;
using Dev2.Common.Interfaces.Infrastructure;
using Dev2.Communication;
using Dev2.DynamicServices;
using Dev2.DynamicServices.Objects;
using Dev2.Runtime.Hosting;
using Dev2.Workspaces;

namespace Dev2.Runtime.ESB.Management.Services
{
    public class GetDirectoriesRelativeToServer : IEsbManagementEndpoint
    {
        IExplorerServerResourceRepository _serverExplorerRepository;

        #region Implementation of ISpookyLoadable<string>

        public string HandlesType()
        {
            return "GetDirectoriesRelativeToServerService";
        }

        #endregion

        #region Implementation of IEsbManagementEndpoint

        /// <summary>
        /// Executes the service
        /// </summary>
        /// <param name="values">The values.</param>
        /// <param name="theWorkspace">The workspace.</param>
        /// <returns></returns>
        public StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            try
            {

          
            string directory = null;
            StringBuilder result = new StringBuilder();
            if(values == null)
            {
                throw new InvalidDataContractException("No parameter values provided.");
            }
            StringBuilder tmp;
            values.TryGetValue("Directory", out tmp);
            if(tmp != null)
            {
                directory = tmp.ToString();
            }
            if(String.IsNullOrEmpty(directory))
            {
                throw new InvalidDataContractException("No value provided for Directory parameter.");
            }
            Dev2Logger.Log.Info("Get Directories Relative to Server. "+directory);
            result.Append("<JSON>");
            var explorerItem = ServerExplorerRepo.Load(ResourceType.Folder, string.Empty);
            var jsonTreeNode = new JsonTreeNode(explorerItem);
            var serializer = new Dev2JsonSerializer();
            var directoryInfoAsJson = serializer.Serialize(jsonTreeNode);
            result.Append(directoryInfoAsJson);
            result.Append("</JSON>");
            return result;
            }
            catch (Exception e)
            {
                Dev2Logger.Log.Error(e);
                throw;
            }
        }

        public IExplorerServerResourceRepository ServerExplorerRepo
        {
            get { return _serverExplorerRepository ?? ServerExplorerRepository.Instance; }
            set { _serverExplorerRepository = value; }
        }
        /// <summary>
        /// Creates the service entry.
        /// </summary>
        /// <returns></returns>
        public DynamicService CreateServiceEntry()
        {
            var ds = new DynamicService
            {
                Name = HandlesType(),
                DataListSpecification = "<DataList><Directory ColumnIODirection=\"Input\"/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>"
            };

            var sa = new ServiceAction
            {
                Name = HandlesType(),
                ActionType = enActionType.InvokeManagementDynamicService,
                SourceMethod = HandlesType()
            };

            ds.Actions.Add(sa);

            return ds;
        }

        #endregion
    }

    internal class JsonTreeNode
    {
        public JsonTreeNode(IExplorerItem explorerItem)
        {
            if(explorerItem.ResourceType == ResourceType.Server)
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

        // ReSharper disable InconsistentNaming
        public string title { get; set; }
        public bool isFolder { get; set; }
        public string key { get; set; }
        public bool isLazy { get; set; }
        public List<JsonTreeNode> children { get; set; }
    }
}