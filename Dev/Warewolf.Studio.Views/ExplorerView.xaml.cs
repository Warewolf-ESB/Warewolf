using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Effects;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Studio.ViewModels;
using Infragistics.Controls.Menus;

namespace Warewolf.Studio.Views
{
	/// <summary>
	/// Interaction logic for ExplorerView.xaml
	/// </summary>
	public partial class ExplorerView : IExplorerView
	{
	    readonly ExplorerViewTestClass _explorerViewTestClass;
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
            return ExplorerViewTestClass.OpenEnvironmentNode(nodeName);
        }

	    public List<IExplorerItemViewModel> GetFoldersVisible()
	    {
	        return ExplorerViewTestClass.GetFoldersVisible();
	    }

	    public IExplorerItemViewModel OpenFolderNode(string folderName)
	    {
	        return ExplorerViewTestClass.OpenFolderNode(folderName);
	    }

	    public int GetVisibleChildrenCount(string folderName)
	    {
	        return ExplorerViewTestClass.GetVisibleChildrenCount(folderName);
	    }

	    public void PerformFolderRename(string originalFolderName, string newFolderName)
	    {
	        ExplorerViewTestClass.PerformFolderRename(originalFolderName, newFolderName);
	    }

	    public void PerformSearch(string searchTerm)
	    {
	        SearchTextBox.Text = searchTerm;
            BindingExpression be = SearchTextBox.GetBindingExpression(TextBox.TextProperty);
	        if (be != null)
	        {
            be.UpdateSource();
	        }
	    }

	    public void AddNewFolder(string folder, string server)
	    {
            ExplorerViewTestClass.PerformFolderAdd(server, folder);
	    }

	    public void VerifyItemExists(string path)
	    {
            ExplorerViewTestClass.VerifyItemExists(path);
	    }

	    public void DeletePath(string path)
	    {
            ExplorerViewTestClass.DeletePath(path);
	    }

	    public void AddNewFolderFromPath(string path)
	    {
            ExplorerViewTestClass.PerformFolderAdd(path);
	    }

	    public void AddNewResource(string path, string itemType)
	    {
            ExplorerViewTestClass.PerformItemAdd(path,itemType);
	    }

	    public void AddResources(int resourceNumber, string path, string type)
	    {
            ExplorerViewTestClass.AddChildren(resourceNumber, path,type);
	    }

	    public int GetResourcesVisible(string path)
	    {
            return ExplorerViewTestClass.GetFoldersResourcesVisible(path);
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



	 

        public static T FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            //get parent item
            DependencyObject parentObject = VisualTreeHelper.GetParent(child);

            //we've reached the end of the tree
            if (parentObject == null) return null;

            //check if the parent matches the type we're looking for
            T parent = parentObject as T;
            if (parent != null)
                return parent;
            return FindParent<T>(parentObject);
        }
	}
}