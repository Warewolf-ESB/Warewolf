using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Dev2.Common.Interfaces;
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

        private void UIElement_OnMouseMove(object sender, MouseEventArgs e)
        {
            var grid = sender as Grid;
            if (grid != null && e.LeftButton == MouseButtonState.Pressed)
            {
                Mouse.SetCursor(_customCursor);
                var dataContext = grid.DataContext as ToolDescriptorViewModel;
                if (dataContext != null &&
                    dataContext.ActivityType != null)
                {
                    DragDrop.DoDragDrop((DependencyObject)e.Source, dataContext.ActivityType, DragDropEffects.Copy);
                }
            }
        }


        private Cursor _customCursor;

        void UIElement_OnDragEnter(object sender, DragEventArgs e)
        {
            var Source = e.Source;
            var originalSource = e.OriginalSource;
        }
    }
}