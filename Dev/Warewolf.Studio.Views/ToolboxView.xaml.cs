using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Toolbox;
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

        #region Implementation of IToolboxView

        IEnumerable<IToolDescriptorViewModel> GetTools()
        {
            var collectionViewSource = Resources["ToolViewSource"] as CollectionViewSource;
            if (collectionViewSource != null)
            {
                var binding = BindingOperations.GetBindingExpression(collectionViewSource, CollectionViewSource.SourceProperty);
                if(binding != null)
                {
                    binding.UpdateTarget();
                }
                var listBoxCollection = collectionViewSource.Source;
                var tools = listBoxCollection as IEnumerable<IToolDescriptorViewModel>;
                return tools;
            }
            
            return null;
        }

        #endregion

        #region Implementation of IComponentConnector

        #endregion

        private Cursor _customCursor;

        void UIElement_OnDragEnter(object sender, DragEventArgs e)
        {
            var Source = e.Source;
            var originalSource = e.OriginalSource;
        }
    }
}