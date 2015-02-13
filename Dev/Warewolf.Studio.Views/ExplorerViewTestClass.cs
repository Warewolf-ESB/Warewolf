using System;
using System.Collections.Generic;
using System.Linq;
using Dev2.AppResources.DependencyVisualization;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Studio.ViewModels;
using Infragistics.Controls.Menus;

namespace Warewolf.Studio.Views
{
    public class ExplorerViewTestClass
    {
        private readonly ExplorerView _explorerView;

        public ExplorerViewTestClass(ExplorerView explorerView)
        {
            _explorerView = explorerView;
        }

        public XamDataTreeNode GetNode(string nodeName)
        {
            var flattenTree = Descendants(_explorerView.ExplorerTree.Nodes[0]);
            var foundNode = flattenTree.FirstOrDefault(node =>
            {
                var explorerItem = node.Data as IExplorerItemViewModel;
                if (explorerItem != null)
                {
                    if (explorerItem.ResourceName.ToLowerInvariant()==nodeName.ToLowerInvariant())
                    {
                        return true;
                    }
                }
                return false;
            });
            return foundNode;
        }

        public IEnvironmentViewModel OpenEnvironmentNode(string nodeName)
        {
            var xamDataTreeNode = _explorerView.ExplorerTree.Nodes.FirstOrDefault(node =>
            {
                var explorerItem = node.Data as IEnvironmentViewModel;
                if (explorerItem != null)
                {
                    if (explorerItem.DisplayName.ToLowerInvariant().Contains(nodeName.ToLowerInvariant()))
                    {
                        return true;
                    }
                }
                return false;
            });
            if (xamDataTreeNode != null)
            {
                xamDataTreeNode.IsExpanded = true;
            }
            return xamDataTreeNode == null ? null : xamDataTreeNode.Data as IEnvironmentViewModel;
        }

        public List<IExplorerItemViewModel> GetFoldersVisible()
        {
            var folderItems = _explorerView.ExplorerTree.Nodes[0].Nodes.Where(node =>
            {
                var explorerItem = node.Data as IExplorerItemViewModel;
                if (explorerItem != null)
                {
                    if (explorerItem.ResourceType == ResourceType.Folder)
                    {
                        return true;
                    }
                }
                return false;
            }).Select(node => node.Data as IExplorerItemViewModel);
            return folderItems.ToList();
        }

        public IExplorerItemViewModel OpenFolderNode(string folderName)
        {
            var foundFolder = GetFolderXamDataTreeNode(folderName);
            if (foundFolder != null)
            {
                foundFolder.IsExpanded = true;
                return foundFolder.Data as IExplorerItemViewModel;
            }
            return null;
        }

        public int GetVisibleChildrenCount(string folderName)
        {
            var foundFolder = GetFolderXamDataTreeNode(folderName);
            if (foundFolder != null)
            {
                return foundFolder.Nodes[0].Manager.Nodes.Count;
            }
            return 0;
        }

        public void PerformFolderRename(string originalFolderName, string newFolderName)
        {
            var foundFolder = GetFolderXamDataTreeNode(originalFolderName).Data as IExplorerItemViewModel;
            if (foundFolder != null)
            {
                foundFolder.RenameCommand.Execute(null);
                foundFolder.ResourceName = newFolderName;
            }
        }

        private XamDataTreeNode GetFolderXamDataTreeNode(string folderName)
        {
            var flattenTree = Descendants(_explorerView.ExplorerTree.Nodes[0]);
            var foundFolder = flattenTree.FirstOrDefault(node =>
            {
                var explorerItem = node.Data as IExplorerItemViewModel;
                if (explorerItem != null)
                {
                    if (explorerItem.ResourceName.ToLowerInvariant().Contains(folderName.ToLowerInvariant()) &&
                        explorerItem.ResourceType == ResourceType.Folder)
                    {
                        return true;
                    }
                }
                return false;
            });
            return foundFolder;
        }

        private IEnumerable<XamDataTreeNode> Descendants(XamDataTreeNode root)
        {
            var nodes = new Stack<XamDataTreeNode>(new[] { root });
            while (nodes.Any())
            {
                XamDataTreeNode node = nodes.Pop();
                yield return node;
                foreach (var n in node.Nodes) nodes.Push(n);
            }
        }

        public void PerformFolderAdd(string folder, string server)
        {
            var node = _explorerView.ExplorerTree.Nodes.FirstOrDefault(a => ((IEnvironmentViewModel)a.Data).DisplayName.Contains(server));
           
            if(node != null)
            {
                var env = (node.Data as IEnvironmentViewModel);
                if(env != null)
                {
                    env.CreateFolderCommand.Execute(null);
                    var explorerItemViewModel  = env.Children.FirstOrDefault(a => a.IsRenaming);
                    if(explorerItemViewModel != null)
                    {
                        explorerItemViewModel.ResourceName = folder;
                        explorerItemViewModel.IsRenaming = false;

                    }
                    else
                    throw  new Exception("Folder was not found after adding");
                   
                }
                
            }
            else
            throw  new Exception("Server Not found in explorer");
        }

        public XamDataTreeNode VerifyItemExists(string path)
        {
            if(!path.Contains("/"))
            {

               var childnode = _explorerView.ExplorerTree.Nodes.FirstOrDefault(a => ((IEnvironmentViewModel)a.Data).DisplayName.Contains(path));
            
                if (childnode == null)
                    throw new Exception("Folder or environment not found. Name" + path);
                return childnode;
   
            }
            else
            {
                var toSearch = path.Substring(0, path.IndexOf("/", System.StringComparison.Ordinal));
                var childnode = _explorerView.ExplorerTree.Nodes.FirstOrDefault(a => ((IEnvironmentViewModel)a.Data).DisplayName.Contains(toSearch));
                if (path.Length > 1 + path.IndexOf("/", System.StringComparison.Ordinal))
                {
                   return VerifyItemExists(path.Substring(1 + path.IndexOf("/", System.StringComparison.Ordinal)),  childnode);
                }
                throw new Exception("Invalid path");
            }
        }

        public XamDataTreeNode VerifyItemExists(string path, XamDataTreeNode node)
        {
            if(!path.Contains("/"))
            {

                  return GetNodeWithName(path,node);
                   
                
            }
            var toSearch = path.Substring(0, path.IndexOf("/", System.StringComparison.Ordinal));
            var childnode = GetNodeWithName(toSearch,node);
            if(path.Length > 1 + path.IndexOf("/", System.StringComparison.Ordinal))
            {
                return  VerifyItemExists(path.Substring(1 + path.IndexOf("/", System.StringComparison.Ordinal)), childnode);
            }
            throw  new Exception("Invalid path");
        }

        XamDataTreeNode GetNodeWithName(string path,XamDataTreeNode node)
        {
            var found = node.Nodes.FirstOrDefault(a => ((IExplorerItemViewModel)a.Data).ResourceName.Equals(path));
            if (found == null)
            {
                throw new Exception("Folder or environment not found. Name" + path);
            }
            return found;
        }

        public void DeletePath(string path)
        {
            var node = VerifyItemExists(path);
            if(node!= null)
            {
                ((IExplorerItemViewModel)node.Data).DeleteCommand.Execute(null);
            }
        }

        internal void PerformFolderAdd(string path)
        {
            if(path.Contains("/"))
            {
                var node = VerifyItemExists(path.Substring(0, path.LastIndexOf("/", System.StringComparison.Ordinal)));
                var env = node.Data as IExplorerTreeItem;
                if (env != null)
                {
                   
                    env.CreateFolderCommand.Execute(null);
                    var explorerItemViewModel = env.Children.FirstOrDefault(a => a.IsRenaming);
                    if (explorerItemViewModel != null)
                    {
                        explorerItemViewModel.ResourceName = path.Substring(1+ path.LastIndexOf("/", System.StringComparison.Ordinal));
                        explorerItemViewModel.IsRenaming = false;

                    }
                    else
                        throw new Exception("Folder was not found after adding");

                }
                else
                {
                    throw new Exception("Path requires server and sub folder");
                }
            }
        }
    }
}