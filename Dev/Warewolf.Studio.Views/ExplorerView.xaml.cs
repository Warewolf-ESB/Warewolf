using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Dev2.Common.Interfaces.Studio.ViewModels;
using Infragistics.Controls.Menus;
using Warewolf.Studio.Core.View_Interfaces;
using System.Windows.Controls.Primitives;
using Dev2.Common.Interfaces.Data;

namespace Warewolf.Studio.Views
{
	/// <summary>
	/// Interaction logic for ExplorerView.xaml
	/// </summary>
	public partial class ExplorerView : IExplorerView
	{
		public ExplorerView()
		{
			InitializeComponent();
		}

        private void ScrollBar_Loaded(object sender, RoutedEventArgs e)
        {
            if ((sender as ScrollBar).Orientation == System.Windows.Controls.Orientation.Horizontal)
            {
                ExplorerTree.Tag = sender;                
            }
        }

	    public IEnvironmentViewModel GetEnvironmentNode(string nodeName)
	    {
            var xamDataTreeNode = ExplorerTree.Nodes.FirstOrDefault(node =>
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
	        return xamDataTreeNode == null ? null : xamDataTreeNode.Data as IEnvironmentViewModel;
	    }

	    public List<IExplorerItemViewModel> GetFoldersVisible()
	    {
	        var folderItems = ExplorerTree.Nodes.Where(node =>
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

	    void ExplorerTree_OnNodeDragDrop(object sender, TreeDropEventArgs e)
	    {
            
            var node = e.DragDropEventArgs.Data as XamDataTreeNode;

            if (node != null)
            {
                var dropped = e.DragDropEventArgs.DropTarget as XamDataTreeNodeControl;
                if (dropped != null)
                {
                    var destination = dropped.Node.Data as IExplorerItemViewModel;
                    var source = node.Data as IExplorerItemViewModel;
                    if (source != null && destination != null)
                    {
                        if (!destination.CanDrop || !source.CanDrag)
                        {
                            e.Handled = true;
                            return;
                        }
                        if (!source.Move(destination))
                        {
                            e.Handled = true;
                        }
                    }
                }

            }
            e.Handled = true;
            
	    }

        void UIElement_OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
	    {
            Keyboard.Focus((IInputElement)sender);
	    }
	}
}