using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Dev2.Common.Interfaces;
using Infragistics.DragDrop;
using Warewolf.Studio.ViewModels.ToolBox;

namespace Warewolf.Studio.Views
{
    /// <summary>
    /// Interaction logic for ToolboxView.xaml
    /// </summary>
    public partial class ToolboxView : IToolboxView
    {
        public ToolboxView()
        {
            InitializeComponent();
        }

        #region Implementation of IWarewolfView

        #endregion

        private void DragSource_OnDragStart(object sender, DragDropStartEventArgs e)
        {
            if (e != null)
            {
            }
        }

        private void FrameworkElement_OnLoaded(object sender, RoutedEventArgs e)
        {
//            DragSource source = DragDropManager.GetDragSource(sender as DependencyObject);
//            if (source != null)
//            {
//                var grid = source.AssociatedObject as Grid;
//                if (grid != null)
//                {
//                    var dataContext = grid.DataContext;
//                    if (dataContext != null)
//                    {
//                        BindingOperations.SetBinding(source,
//                            DragSource.DataObjectProperty,
//                            new Binding
//                            {
//                                RelativeSource = new RelativeSource(RelativeSourceMode.Self),
//                                Path = new PropertyPath("AssociatedObject.DataContext.ActivityType")
//                            });
//                    }
//                }
//
//
//            }
        }

        private void UIElement_OnMouseMove(object sender, MouseEventArgs e)
        {
            var grid = sender as Grid;
            if (grid != null && e.LeftButton == MouseButtonState.Pressed)
            {
                var dataContext = grid.DataContext as ToolDescriptorViewModel;
                if (dataContext != null)
                {
                    DragDrop.DoDragDrop(this, dataContext.ActivityType, DragDropEffects.All);
                }
            }
        }
    }

}