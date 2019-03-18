#pragma warning disable CC0091, S1226, S100, CC0044, CC0045, CC0021, S1449, S1541, S1067, S3235, CC0015, S107, S2292, S1450, S105, CC0074, S1135, S101, S3776, CS0168, S2339, CC0031, S3240, CC0020, CS0108, S1694, S1481, CC0008, AD0001, S2328, S2696, S1643, CS0659, CS0067, S104, CC0030, CA2202, S3376, S1185, CS0219, S3253, S1066, CC0075, S3459, S1871, S1125, CS0649, S2737, S1858, CC0082, CC0001, S3241, S2223, S1301, CC0013, S2955, S1944, CS4014, S3052, S2674, S2344, S1939, S1210, CC0033, CC0002, S3458, S3254, S3220, S2197, S1905, S1699, S1659, S1155, CS0105, CC0019, S3626, S3604, S3440, S3256, S2692, S2345, S1109, FS0058, CS1998, CS0661, CS0660, CS0162, CC0089, CC0032, CC0011, CA1001
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Dev2.Common;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Explorer;
using Dev2.Common.Interfaces.Runtime;
using Dev2.Common.Interfaces.Wrappers;
using Dev2.Explorer;
using Dev2.Runtime.Interfaces;
using Dev2.Runtime.Security;
using Dev2.Services.Security;


namespace Dev2.Runtime.Hosting
{
    public class ExplorerItemFactory : IExplorerItemFactory
    {
        readonly IAuthorizationService _authService;
        static readonly string RootName = Environment.MachineName;
        public ExplorerItemFactory(IResourceCatalog catalogue, IDirectory directory, IAuthorizationService authService)
        {
            _authService = authService;
            Catalogue = catalogue;
            Directory = directory;
        }

        public List<string> GetDuplicatedResourcesPaths()
        {
            var duplicateList = new List<string>();
            var resourceList = Catalogue.GetDuplicateResources();
            if (resourceList == null ||resourceList.Count <= 0)
            {
                return new List<string>();
            }

            foreach (var duplicateResource in resourceList)
            {
                var res = new StringBuilder();
                foreach (var path in duplicateResource.ResourcePath)
                {
                    res.AppendLine(path);
                }

                var format = string.Format("Resource: {0}"+ Environment.NewLine + "{1}", duplicateResource.ResourceName, res);
                duplicateList.Add(format);
            }
            return duplicateList;
        }

        public IExplorerItem CreateRootExplorerItem(string workSpacePath, Guid workSpaceId)
        {
            var resourceList = Catalogue.GetResourceList(workSpaceId);
            var rootNode = BuildStructureFromFilePathRoot(Directory, workSpacePath, BuildRoot());
            AddChildren(rootNode, resourceList, workSpaceId);
            return rootNode;
        }

        public IExplorerItem CreateRootExplorerItem(string type, string workSpacePath, Guid workSpaceId)
        {
            var resourceList = Catalogue.GetResourceList(workSpaceId);
            var rootNode = BuildStructureFromFilePathRoot(Directory, workSpacePath, BuildRoot());
            if (type == "Folder")
            {
                return rootNode;
            }
            AddChildren(rootNode, resourceList, type, workSpaceId);
            return rootNode;
        }
        void AddChildren(IExplorerItem rootNode, IEnumerable<IResource> resourceList, string type, Guid workSpaceId)
        {


            var children = resourceList.Where(a => GetResourceParent(a.GetResourcePath(workSpaceId)) == rootNode.ResourcePath && a.ResourceType == type.ToString());

            foreach (var node in rootNode.Children)
            {

                AddChildren(node, resourceList, type, workSpaceId);

            }
            foreach (var resource in children)
            {
                var childNode = CreateResourceItem(resource, workSpaceId);
                rootNode.Children.Add(childNode);
            }
        }

        string GetResourceParent(string resourcePath)
        {
            if (String.IsNullOrEmpty(resourcePath))
            {
                return "";
            }
            return resourcePath.Contains("\\") ? resourcePath.Substring(0, resourcePath.LastIndexOf("\\", StringComparison.Ordinal)) : "";
        }

        void AddChildren(IExplorerItem rootNode, IEnumerable<IResource> resourceList, Guid workSpaceId)
        {

            var children = resourceList.Where(a => GetResourceParent(a.GetResourcePath(workSpaceId)).Equals(rootNode.ResourcePath, StringComparison.InvariantCultureIgnoreCase));

            foreach (var node in rootNode.Children)
            {

                AddChildren(node, resourceList, workSpaceId);

            }
            foreach (var resource in children)
            {
                if (resource.ResourceType == "ReservedService")
                {
                    continue;
                }
                var childNode = CreateResourceItem(resource, workSpaceId);
                rootNode.Children.Add(childNode);
            }
        }

        public IExplorerItem CreateResourceItem(IResource resource, Guid workSpaceId)
        {
            var resourceId = resource.ResourceID;
            var childNode = new ServerExplorerItem(resource.ResourceName, resourceId, resource.ResourceType, null, _authService.GetResourcePermissions(resourceId), resource.GetResourcePath(workSpaceId))
            {
                IsService = resource.IsService,
                IsSource = resource.IsSource,
                IsFolder = resource.IsFolder,
                IsServer = resource.IsServer,
                IsResourceVersion = resource.IsResourceVersion,

            };
            return childNode;
        }


        IExplorerItem BuildStructureFromFilePathRoot(IDirectory directory, string path, IExplorerItem root)
        {

            root.Children = BuildStructureFromFilePath(directory, path, path);
            return root;
        }


        IList<IExplorerItem> BuildStructureFromFilePath(IDirectory directory, string path, string rootPath)
        {
            var firstGen =
                directory.GetDirectories(path)
                         .Where(a => !a.EndsWith("VersionControl"));

            IList<IExplorerItem> children = new List<IExplorerItem>();
            foreach (var resource in firstGen)
            {
                var resourcePath = resource.Replace(rootPath, "").Substring(1);

                var node = new ServerExplorerItem(new DirectoryInfo(resource).Name, Guid.NewGuid(), "Folder", null, _authService.GetResourcePermissions(Guid.Empty), resourcePath)
                {
                    IsFolder = true
                };
                children.Add(node);
                node.Children = BuildStructureFromFilePath(directory, resource, rootPath);
            }
            return children;
        }




        public IExplorerItem BuildRoot()
        {
            var serverExplorerItem = new ServerExplorerItem(RootName, Guid.Empty, "Server", new List<IExplorerItem>(), _authService.GetResourcePermissions(Guid.Empty), "")
            {
                ServerId = HostSecurityProvider.Instance.ServerID,
                WebserverUri = EnvironmentVariables.WebServerUri,
                IsServer = true
            };
            return serverExplorerItem;
        }
        public IResourceCatalog Catalogue { get; private set; }
        public IDirectory Directory { get; private set; }

    }

}