using System.Collections.Generic;
using System.Linq;
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
    }
}