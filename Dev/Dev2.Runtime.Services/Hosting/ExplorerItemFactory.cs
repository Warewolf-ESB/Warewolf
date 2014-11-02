/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
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
using Dev2.Common;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Explorer;
using Dev2.Common.Interfaces.Runtime;
using Dev2.Common.Interfaces.Wrappers;
using Dev2.Explorer;
using Dev2.Runtime.Security;
using Dev2.Services.Security;

namespace Dev2.Runtime.Hosting
{
    public class ExplorerItemFactory : IExplorerItemFactory
    {
        private static readonly string RootName = Environment.MachineName;
        private readonly IAuthorizationService _authService;

        public ExplorerItemFactory(IResourceCatalog catalogue, IDirectory directory, IAuthorizationService authService)
        {
            _authService = authService;
            Catalogue = catalogue;
            Directory = directory;
        }

        public IResourceCatalog Catalogue { get; private set; }
        public IDirectory Directory { get; private set; }

        public IExplorerItem CreateRootExplorerItem(string workSpacePath, Guid workSpaceId)
        {
            IList<IResource> resourceList = Catalogue.GetResourceList(workSpaceId);
            IExplorerItem rootNode = BuildStructureFromFilePathRoot(Directory, workSpacePath, BuildRoot());
            AddChildren(rootNode, resourceList);
            return rootNode;
        }

        public IExplorerItem CreateRootExplorerItem(ResourceType type, string workSpacePath, Guid workSpaceId)
        {
            IList<IResource> resourceList = Catalogue.GetResourceList(workSpaceId);
            IExplorerItem rootNode = BuildStructureFromFilePathRoot(Directory, workSpacePath, BuildRoot());
            if (type == ResourceType.Folder)
            {
                return rootNode;
            }
            AddChildren(rootNode, resourceList, type);
            return rootNode;
        }

        private void AddChildren(IExplorerItem rootNode, IEnumerable<IResource> resourceList, ResourceType type)
        {
            // ReSharper disable PossibleMultipleEnumeration

            IEnumerable<IResource> children =
                resourceList.Where(
                    a => GetResourceParent(a.ResourcePath) == rootNode.ResourcePath && a.ResourceType == type);
            // ReSharper restore PossibleMultipleEnumeration
            foreach (IExplorerItem node in rootNode.Children)
            {
                // ReSharper disable PossibleMultipleEnumeration
                AddChildren(node, resourceList, type);
                // ReSharper restore PossibleMultipleEnumeration
            }
            foreach (IResource resource in children)
            {
                ServerExplorerItem childNode = CreateResourceItem(resource);
                rootNode.Children.Add(childNode);
            }
        }

        private string GetResourceParent(string resourcePath)
        {
            if (String.IsNullOrEmpty(resourcePath))
            {
                return "";
            }
            return resourcePath.Contains("\\")
                ? resourcePath.Substring(0, resourcePath.LastIndexOf("\\", StringComparison.Ordinal))
                : "";
        }

        private void AddChildren(IExplorerItem rootNode, IEnumerable<IResource> resourceList)
        {
            // ReSharper disable PossibleMultipleEnumeration
            IEnumerable<IResource> children =
                resourceList.Where(
                    a =>
                        GetResourceParent(a.ResourcePath)
                            .Equals(rootNode.ResourcePath, StringComparison.InvariantCultureIgnoreCase));
            // ReSharper restore PossibleMultipleEnumeration
            foreach (IExplorerItem node in rootNode.Children)
            {
                // ReSharper disable PossibleMultipleEnumeration
                AddChildren(node, resourceList);
                // ReSharper restore PossibleMultipleEnumeration
            }
            foreach (IResource resource in children)
            {
                if (resource.ResourceType == ResourceType.ReservedService)
                {
                    continue;
                }
                ServerExplorerItem childNode = CreateResourceItem(resource);
                rootNode.Children.Add(childNode);
            }
        }

        public ServerExplorerItem CreateResourceItem(IResource resource)
        {
            Guid resourceId = resource.ResourceID;
            var childNode = new ServerExplorerItem(resource.ResourceName, resourceId,
                resource.ResourceType == ResourceType.Server ? ResourceType.ServerSource : resource.ResourceType, null,
                _authService.GetResourcePermissions(resourceId), resource.ResourcePath);
            return childNode;
        }


        private IExplorerItem BuildStructureFromFilePathRoot(IDirectory directory, string path, IExplorerItem root)
        {
            root.Children = BuildStructureFromFilePath(directory, path, path);
            return root;
        }


        private IList<IExplorerItem> BuildStructureFromFilePath(IDirectory directory, string path, string rootPath)
        {
            IEnumerable<string> firstGen =
                directory.GetDirectories(path)
                    .Where(a => !a.EndsWith("VersionControl"));

            IList<IExplorerItem> children = new List<IExplorerItem>();
            foreach (string resource in firstGen)
            {
                string resourcePath = resource.Replace(rootPath, "").Substring(1);

                var node = new ServerExplorerItem(new DirectoryInfo(resource).Name, Guid.NewGuid(), ResourceType.Folder,
                    null, _authService.GetResourcePermissions(Guid.Empty), resourcePath);
                children.Add(node);
                node.Children = BuildStructureFromFilePath(directory, resource, rootPath);
            }
            return children;
        }


        public IExplorerItem BuildRoot()
        {
            var serverExplorerItem = new ServerExplorerItem(RootName, Guid.Empty, ResourceType.Server,
                new List<IExplorerItem>(), _authService.GetResourcePermissions(Guid.Empty), "")
            {
                ServerId = HostSecurityProvider.Instance.ServerID,
                WebserverUri = EnvironmentVariables.WebServerUri
            };
            return serverExplorerItem;
        }

        public bool IsChild(string parent, string maybeChild)
        {
            return parent == maybeChild;
        }
    }
}