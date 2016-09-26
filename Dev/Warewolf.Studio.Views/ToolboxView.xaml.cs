﻿using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Dev2.Common.Interfaces;
using Infragistics.Windows.DockManager;
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

        private void SelectAllText(object sender, RoutedEventArgs e)
        {
            TextBox tb = sender as TextBox;
            tb?.SelectAll();
        }

        private void SelectivelyIgnoreMouseButton(object sender, MouseButtonEventArgs e)
        {
            TextBox tb = sender as TextBox;
            if (tb != null)
            {
                if (!tb.IsKeyboardFocusWithin)
                {
                    e.Handled = true;
                    tb.Focus();
                }
            }
        }

        private void ToolGrid_OnMouseEnter(object sender, MouseEventArgs e)
        {
            var variablesPane = Application.Current.MainWindow.FindName("Variables") as ContentPane;
            var explorerPane = Application.Current.MainWindow.FindName("Explorer") as ContentPane;
            var outputPane = Application.Current.MainWindow.FindName("OutputPane") as ContentPane;
            var documentHostPane = Application.Current.MainWindow.FindName("DocumentHost") as ContentPane;

            if (variablesPane != null && !variablesPane.IsActivePane &&
                explorerPane != null && !explorerPane.IsActivePane &&
                outputPane != null && !outputPane.IsActivePane &&
                documentHostPane != null && !documentHostPane.IsActivePane)
            {
                var toolboxPane = Application.Current.MainWindow.FindName("Toolbox") as ContentPane;
                toolboxPane?.Activate();
            }
        }
    }
}