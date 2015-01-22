using System.Windows;
using System.Windows.Annotations;
using System.Windows.Controls;
using Dev2.Common.Interfaces.Studio.ViewModels;
using Infragistics.Controls.Menus;
using Warewolf.Studio.Core.View_Interfaces;

namespace Warewolf.Studio.Views
{
	/// <summary>
	/// Interaction logic for ExplorerView.xaml
	/// </summary>
	public partial class ExplorerView : UserControl, IExplorerView
	{
		public ExplorerView()
		{
			InitializeComponent();
		}

	    void ExplorerTree_OnNodeDragDrop(object sender, TreeDropEventArgs e)
	    {
            var node = e.DragDropEventArgs.Data as XamDataTreeNode;

	        if(node != null)
	        {
                var dropped = e.DragDropEventArgs.DropTarget as Infragistics.Controls.Menus.XamDataTreeNodeControl;
	            if(dropped != null)
	            {
	                var destination = dropped.Node.Data as IExplorerItemViewModel;
                    var source = node.Data as IExplorerItemViewModel;
                    if(source!=null&&destination!= null)
                    {
                        source.Move(destination);
                    }
	            }
	           
	        }
	    }
	}
}