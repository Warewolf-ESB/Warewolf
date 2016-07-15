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
                var dataContext = grid.DataContext as ToolDescriptorViewModel;
                if (dataContext?.ActivityType != null)
                {
                    DragDrop.DoDragDrop((DependencyObject)e.Source, dataContext.ActivityType, DragDropEffects.Copy);
                }
            }
        }

        void UIElement_OnDragEnter(object sender, DragEventArgs e)
        {
            
        }
    }
}