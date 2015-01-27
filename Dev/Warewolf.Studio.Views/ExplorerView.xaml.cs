using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
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
		public ExplorerView()
		{
			InitializeComponent();
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
            
	    }

        private void ScrollBar_Loaded(object sender, RoutedEventArgs e)
        {
            var scrollBar = sender as ScrollBar;
            if (scrollBar != null && scrollBar.Orientation == Orientation.Horizontal)
            {
                scrollBar.MinHeight = 0;
                scrollBar.Height = 0;
            }
        }

	    void UIElement_OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
	    {
            Keyboard.Focus((IInputElement)sender);
	    }

	    void ScrollBarOnScroll(object sender, ScrollEventArgs e)
	    {
            ExplorerTree.Width = ExplorerTree.Width + 1;
            ExplorerTree.Width = ExplorerTree.Width - 1;
	    }
	}

    public static class FocusExtension
    {
        public static bool GetIsFocused(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsFocusedProperty);
        }


        public static void SetIsFocused(DependencyObject obj, bool value)
        {
            obj.SetValue(IsFocusedProperty, value);
        }


        public static readonly DependencyProperty IsFocusedProperty =
            DependencyProperty.RegisterAttached(
             "IsFocused", typeof(bool), typeof(FocusExtension),
             new UIPropertyMetadata(false, OnIsFocusedPropertyChanged));


        private static void OnIsFocusedPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            //var uie = (UIElement)d;
            //if ((bool)e.NewValue)
            //{
            //    uie.Focus(); // Don't care about false values.
            //    Keyboard.Focus(uie);
            //}
        }
    }
}