﻿using System.Collections.Generic;
using System.Linq;
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

        public void EnterSearch(string searchTerm)
        {
            SearchTextBox.Text = searchTerm;
            BindingExpression be = SearchTextBox.GetBindingExpression(TextBox.TextProperty);
            if (be != null)
            {
                be.UpdateSource();
            }
        }

        public bool CheckToolIsVisible(string toolName)
        {
            var tools = GetTools();
            var tool = tools.FirstOrDefault(model => toolName.Contains(model.Tool.Name));
            return tool!=null;
        }

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

        public bool CheckAllToolsNotVisible()
        {
            var toolCount = GetToolCount();
            return toolCount == 0;
        }

        public void ClearFilter()
        {
            SearchTextBox.Text = string.Empty;
            BindingExpression be = SearchTextBox.GetBindingExpression(TextBox.TextProperty);
            if (be != null)
            {
                be.UpdateSource();
            }
        }

        public int GetToolCount()
        {
            var tools = GetTools();
            var toolCount = tools.Count();
            return toolCount;
        }

        #endregion

        #region Implementation of IComponentConnector

        /// <summary>
        /// Attaches events and names to compiled content. 
        /// </summary>
        /// <param name="connectionId">An identifier token to distinguish calls.</param><param name="target">The target to connect events and names to.</param>
        public void Connect(int connectionId, object target)
        {
        }

        #endregion

        private Cursor _customCursor;

        void UIElement_OnGiveFeedback(object sender, GiveFeedbackEventArgs e)
        {
            if (e.Effects == DragDropEffects.Copy)
            {
                if (_customCursor == null)
                    _customCursor = Application.Current.TryFindResource("CursorGrabbing") as Cursor;

                e.UseDefaultCursors = false;
                Mouse.SetCursor(_customCursor);
            }
            else
                e.UseDefaultCursors = true;

            e.Handled = true;
        }

        void UIElement_OnDragEnter(object sender, DragEventArgs e)
        {
            var Source = e.Source;
            var originalSource = e.OriginalSource;
        }

        void ToolGrid_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            Cursor grabCursor = Application.Current.TryFindResource("CursorGrabbing") as Cursor;
            Mouse.SetCursor(grabCursor);
        }
    }
}