using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Studio.ViewModels;
using Infragistics.Controls.Menus;
using Warewolf.Studio.Core.View_Interfaces;

namespace Warewolf.Studio.Views
{
	/// <summary>
	/// Interaction logic for ExplorerView.xaml
	/// </summary>
	public partial class ExplorerView : IExplorerView
	{
	    private readonly ExplorerViewTestClass _explorerViewTestClass;
	    Grid _blackoutGrid;

	    public ExplorerView()
	    {
	        InitializeComponent();
	        _explorerViewTestClass = new ExplorerViewTestClass(this);
            ExplorerTree.ActiveNodeChanged+=ExplorerTreeOnActiveNodeChanged;
	    }

	    private void ExplorerTreeOnActiveNodeChanged(object sender, ActiveNodeChangedEventArgs activeNodeChangedEventArgs)
	    {
	        if (activeNodeChangedEventArgs.NewActiveTreeNode==null)
	        {
	            activeNodeChangedEventArgs.Cancel = true;
	        }
	        if (activeNodeChangedEventArgs.NewActiveTreeNode != null)
	        {
	            var explorerItemViewModel = activeNodeChangedEventArgs.NewActiveTreeNode.Data as IExplorerItemViewModel;
	            if (explorerItemViewModel != null)
	            {
	                explorerItemViewModel.ItemSelectedCommand.Execute(null);
	            }
	        }
	    }

	    public ExplorerViewTestClass ExplorerViewTestClass
	    {
	        get { return _explorerViewTestClass; }
	    }


	    private void ScrollBar_Loaded(object sender, RoutedEventArgs e)
        {
	        var scrollBar = sender as ScrollBar;
	        if (scrollBar != null && scrollBar.Orientation == Orientation.Horizontal)
            {
                ExplorerTree.Tag = sender;                
            }
        }

        public IEnvironmentViewModel OpenEnvironmentNode(string nodeName)
        {
            return _explorerViewTestClass.OpenEnvironmentNode(nodeName);
        }

	    public List<IExplorerItemViewModel> GetFoldersVisible()
	    {
	        return _explorerViewTestClass.GetFoldersVisible();
	    }

	    public IExplorerItemViewModel OpenFolderNode(string folderName)
	    {
	        return _explorerViewTestClass.OpenFolderNode(folderName);
	    }

	    public int GetVisibleChildrenCount(string folderName)
	    {
	        return _explorerViewTestClass.GetVisibleChildrenCount(folderName);
	    }

	    public void PerformFolderRename(string originalFolderName, string newFolderName)
	    {
	        _explorerViewTestClass.PerformFolderRename(originalFolderName, newFolderName);
	    }

	    public void PerformSearch(string searchTerm)
	    {
	        SearchTextBox.Text = searchTerm;
            BindingExpression be = SearchTextBox.GetBindingExpression(TextBox.TextProperty);
	        if(be != null)
	        {
            be.UpdateSource();
	        }
	    }

	    public void Blur()
	    {
        

            if (Content != null)
            {
                //Effect = new BlurEffect(){Radius = 10};
                //Background = new SolidColorBrush(Colors.Black);
                Overlay.Visibility = Visibility.Visible;
                Overlay.Opacity = 0.75;
          
            }
	    }

	    public void UnBlur()
	    {
            RemoveVisualChild(_blackoutGrid);
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
	}
}