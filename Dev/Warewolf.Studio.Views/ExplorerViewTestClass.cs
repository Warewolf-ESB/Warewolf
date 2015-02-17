using System;
using System.Collections.Generic;
using System.Linq;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Studio.ViewModels;
using Infragistics.Controls.Menus;
using Moq;
using Warewolf.Studio.ViewModels;

namespace Warewolf.Studio.Views
{
    public class ExplorerViewTestClass
    {
        private readonly ExplorerView _explorerView;

        public ExplorerViewTestClass(ExplorerView explorerView)
        {
            _explorerView = explorerView;
        }

        XamDataTreeNode GetNode(string nodeName)
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
                _explorerView.ExplorerTree.ScrollNodeIntoView(foundFolder);
                var explorerItemViewModel = foundFolder.Data as IExplorerItemViewModel;
                var explorerViewModelBase = _explorerView.DataContext as ExplorerViewModelBase;
                if (explorerViewModelBase != null)
                {
                    explorerViewModelBase.SelectedItem = explorerItemViewModel;
                }

                return explorerItemViewModel;
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
            var foundFolder = VerifyItemExists(originalFolderName).Data as IExplorerItemViewModel;
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

            if (node != null)
            {
                var env = (node.Data as IEnvironmentViewModel);
                if (env != null)
                {
                    env.CreateFolderCommand.Execute(null);
                    var explorerItemViewModel = env.Children.FirstOrDefault(a => a.IsRenaming);
                    if (explorerItemViewModel != null)
                    {
                        explorerItemViewModel.ResourceName = folder;
                        explorerItemViewModel.IsRenaming = false;

                    }
                    else
                        throw new Exception("Folder was not found after adding");

                }

            }
            else
                throw new Exception("Server Not found in explorer");
        }

        //public void PerformFolderAdd(string folder, string server)
        //{
        //    var node = _explorerView.ExplorerTree.Nodes.FirstOrDefault(a => ((IEnvironmentViewModel)a.Data).DisplayName.Contains(server));
           
        //    if(node != null)
        //    {
        //        var env = (node.Data as IEnvironmentViewModel);
        //        if(env != null)
        //        {
        //            env.CreateFolderCommand.Execute(null);
        //            var explorerItemViewModel  = env.Children.FirstOrDefault(a => a.IsRenaming);
        //            if(explorerItemViewModel != null)
        //            {
        //                explorerItemViewModel.ResourceName = folder;
        //                explorerItemViewModel.IsRenaming = false;

        //            }
        //            else
        //            throw  new Exception("Folder was not found after adding");
                   
        //        }
                
        //    }
        //    else
        //    throw  new Exception("Server Not found in explorer");
        //}

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
                var toSearch = path.Substring(0, path.IndexOf("/", StringComparison.Ordinal));
                var childnode = _explorerView.ExplorerTree.Nodes.FirstOrDefault(a => ((IEnvironmentViewModel)a.Data).DisplayName.Contains(toSearch));
                if (path.Length > 1 + path.IndexOf("/", StringComparison.Ordinal))
                {
                   return VerifyItemExists(path.Substring(1 + path.IndexOf("/", StringComparison.Ordinal)),  childnode);
                }
                throw new Exception("Invalid path");
            }
        }

        XamDataTreeNode VerifyItemExists(string path, XamDataTreeNode node)
        {
            if(!path.Contains("/"))
            {

                  return GetNodeWithName(path,node);
                   
                
            }
            var toSearch = path.Substring(0, path.IndexOf("/", StringComparison.Ordinal));
            var childnode = GetNodeWithName(toSearch,node);
            if(path.Length > 1 + path.IndexOf("/", StringComparison.Ordinal))
            {
                return  VerifyItemExists(path.Substring(1 + path.IndexOf("/", StringComparison.Ordinal)), childnode);
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
                var node = VerifyItemExists(path.Substring(0, path.LastIndexOf("/", StringComparison.Ordinal)));
                var env = node.Data as IExplorerTreeItem;
                if (env != null)
                {
                   
                    env.CreateFolderCommand.Execute(null);
                    var explorerItemViewModel = env.Children.FirstOrDefault(a => a.IsRenaming);
                    if (explorerItemViewModel != null)
                    {
                        explorerItemViewModel.ResourceName = path.Substring(1+ path.LastIndexOf("/", StringComparison.Ordinal));
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

        public void PerformItemAdd(string path)
        {
             if(path.Contains("/"))
            {
                var node = VerifyItemExists(path.Substring(0, path.LastIndexOf("/", StringComparison.Ordinal)));
                if(node == null)
                    throw  new Exception("Invalid path");

                var item = (node.Data as ExplorerItemViewModel);
                if(item != null)
                {
                    item.AddChild(new ExplorerItemViewModel(item.ShellViewModel,item.Server,new Mock<IExplorerHelpDescriptorBuilder>().Object,item){ ResourceName = path.Substring(1+ path.LastIndexOf("/", StringComparison.Ordinal))});
                }
            }
             else
             {
                 throw new Exception("must have a path of form server/resource");
             }
        }



        public void AddChildren(int resourceNumber, string path, string type,string name="Resource ")
        {
            var resourceType = (ResourceType)Enum.Parse(typeof(ResourceType), type);
            if (path.Contains("/"))
            {
                var node = VerifyItemExists(path);
                var item = (node.Data as ExplorerItemViewModel);
              
                for(int i = 0; i < resourceNumber; i++)
                {
                    if(item != null)
                    {
                        item.AddChild(new ExplorerItemViewModel(item.ShellViewModel, item.Server, new Mock<IExplorerHelpDescriptorBuilder>().Object, item) { ResourceName = name + i, ResourceType = resourceType });
                    }
                }
            }
            else
            {
                var node = VerifyItemExists(path);
                var item = (node.Data as EnvironmentViewModel);

                for (int i = 0; i < resourceNumber; i++)
                {
                    if(item != null)
                    {
                        item.AddChild(new ExplorerItemViewModel(item.ShellViewModel, item.Server, new Mock<IExplorerHelpDescriptorBuilder>().Object, null) { ResourceName = name + i, ResourceType = resourceType });
                    }
                }
            }
        }

        public int GetFoldersResourcesVisible(string path)
        {
            var node = VerifyItemExists(path);
            return node.Nodes.Select(child => (child.Data as ExplorerItemViewModel)).Count(childitem => childitem != null && childitem.ResourceType == ResourceType.WorkflowService);
        }

        public void ShowVersionHistory(string path)
        {
            var node = VerifyItemExists(path);
            var explorerItemViewModel    = node.Data as IExplorerItemViewModel;
            if(explorerItemViewModel != null)
            {
                explorerItemViewModel.ShowVersionHistory.Execute(null);
            }
        }

        public ICollection<IExplorerItemViewModel> CreateChildNodes(int count, string path)
        {

      
                var node = VerifyItemExists(path);
                var item = (node.Data as ExplorerItemViewModel);
                var items = new List<IExplorerItemViewModel>();
                const string Name = "Resource ";
                for (int i = 0; i < count; i++)
                {
                    if (item != null)
                    {
                        items.Add(new ExplorerItemViewModel(item.ShellViewModel, item.Server, new Mock<IExplorerHelpDescriptorBuilder>().Object, item) { ResourceName = Name + i, ResourceType = ResourceType.Version });
                    }
                }
                return items;
          
        }

        public void PerformVersionRollback(string versionPath)
        {
            var node = VerifyItemExists(versionPath.Substring(0,versionPath.LastIndexOf("/", StringComparison.Ordinal)));
            
            var explorerItemViewModel = node.Data as IExplorerItemViewModel;
            if(explorerItemViewModel != null)
            {
                var child = explorerItemViewModel.Children.FirstOrDefault(a => a.ResourceName.Contains(versionPath.Substring(1+versionPath.LastIndexOf("/", StringComparison.Ordinal))));
                if(child != null)
                {
                    child.RollbackCommand.Execute(null);
                }
            }
        }

        public void PerformVersionDelete(string versionPath)
        {
            var node = VerifyItemExists(versionPath.Substring(0, versionPath.LastIndexOf("/", StringComparison.Ordinal)));
            var explorerItemViewModel = node.Data as IExplorerItemViewModel;
            if (explorerItemViewModel != null)
            {
               explorerItemViewModel.DeleteVersionCommand.Execute(null);
            }
        }

        public void VerifyContextMenu(string option, string visibility, string path)
        {

        }




       
    }
}