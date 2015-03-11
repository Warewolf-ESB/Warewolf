using System.Windows;
using System.Windows.Input;
using System.Windows.Interactivity;
using Dev2.Common.Interfaces.Studio.ViewModels;
using Infragistics.Controls.Menus;

namespace Warewolf.Studio.Views
{
    public class RenameTreeNode : Behavior<FrameworkElement>
    {
        public XamDataTree Tree
        {
            get { return (XamDataTree)GetValue(TreeProperty); }
            set { SetValue(TreeProperty, value); }
        }
               
        public IExplorerItemViewModel DataContext
        {
            get { return (IExplorerItemViewModel)GetValue(DataContextProperty); }
            set { SetValue(DataContextProperty, value); }
        }

        public XamDataTreeNode Node
        {
            get { return (XamDataTreeNode)GetValue(NodeProperty); }
            set { SetValue(NodeProperty, value); }
        }

        public static readonly DependencyProperty NodeProperty =
           DependencyProperty.Register("Node", typeof(XamDataTreeNode), typeof(RenameTreeNode), new PropertyMetadata(PropertyChangedCallback));


        public static readonly DependencyProperty TreeProperty =
            DependencyProperty.Register("Tree", typeof(XamDataTree), typeof(RenameTreeNode), new PropertyMetadata(PropertyChangedCallback));
        
        public static readonly DependencyProperty DataContextProperty =
            DependencyProperty.Register("DataContext", typeof(IExplorerItemViewModel), typeof(RenameTreeNode), new PropertyMetadata(PropertyChangedCallback));

        private static void PropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {

            if (dependencyPropertyChangedEventArgs.Property == TreeProperty)
            {
                if (dependencyPropertyChangedEventArgs.NewValue != null)
                {
                    _tree = dependencyPropertyChangedEventArgs.NewValue as XamDataTree;
                }
            }
            if (dependencyPropertyChangedEventArgs.Property == DataContextProperty)
            {
                if (dependencyPropertyChangedEventArgs.NewValue != null)
                {
                    _dataContext = dependencyPropertyChangedEventArgs.NewValue as IExplorerItemViewModel;
                }
            }
            if (dependencyPropertyChangedEventArgs.Property == NodeProperty)
            {
                if (dependencyPropertyChangedEventArgs.NewValue != null)
                {
                    _node = dependencyPropertyChangedEventArgs.NewValue as XamDataTreeNode;
                }
            }
        }

        private static XamDataTree _tree;
        private static IExplorerItemViewModel _dataContext;
        private static XamDataTreeNode _node;

        protected override void OnAttached()
        {
            base.OnAttached();
            Mouse.AddMouseUpHandler(AssociatedObject,Handler);
        }

        private void Handler(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            if (mouseButtonEventArgs.ClickCount==1)
            {
                if (_tree != null && _dataContext!=null)
                {
                    _dataContext.IsRenaming = true;
                    if (_node != null)
                    {
                        _tree.EnterEditMode(_node);
                    }
                    else if (_tree.ActiveNode != null)
                    {
                        _tree.EnterEditMode(_tree.ActiveNode);
                    }
                    
                }
            }
        }


        protected override void OnDetaching()
        {
            Mouse.RemoveMouseUpHandler(AssociatedObject, Handler);
            base.OnDetaching();
        }
    }
}