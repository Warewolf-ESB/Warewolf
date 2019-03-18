#pragma warning disable CC0091, S1226, S100, CC0044, CC0045, CC0021, S1449, S1541, S1067, S3235, CC0015, S107, S2292, S1450, S105, CC0074, S1135, S101, S3776, CS0168, S2339, CC0031, S3240, CC0020, CS0108, S1694, S1481, CC0008, AD0001, S2328, S2696, S1643, CS0659, CS0067, S104, CC0030, CA2202, S3376, S1185, CS0219, S3253, S1066, CC0075, S3459, S1871, S1125, CS0649, S2737, S1858, CC0082, CC0001, S3241, S2223, S1301, CC0013, S2955, S1944, CS4014, S3052, S2674, S2344, S1939, S1210, CC0033, CC0002, S3458, S3254, S3220, S2197, S1905, S1699, S1659, S1155, CS0105, CC0019, S3626, S3604, S3440, S3256, S2692, S2345, S1109, FS0058, CS1998, CS0661, CS0660, CS0162, CC0089, CC0032, CC0011, CA1001
﻿using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Shapes;
using Dev2;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Studio.Controller;
using Dev2.Instrumentation;
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
            Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            Arrange(new Rect(new Size(ActualWidth, ActualHeight)));

            PreviewDragOver += DropPointOnDragEnter;
        }

        void UIElement_OnMouseMove(object sender, MouseEventArgs e)
        {
            if (sender is Grid grid && e.LeftButton == MouseButtonState.Pressed)
            {
                var dataContext = grid.DataContext as ToolDescriptorViewModel;
                if (dataContext?.ActivityType != null)
                {
                    DragDrop.DoDragDrop((DependencyObject)e.Source, dataContext.ActivityType, DragDropEffects.Copy);
                }
            }
        }

        void SelectAllText(object sender, RoutedEventArgs e)
        {
            var tb = sender as TextBox;
            tb?.SelectAll();
        }

        void SelectivelyIgnoreMouseButton(object sender, MouseButtonEventArgs e)
        {
            var imageSource = e.OriginalSource as FontAwesome.WPF.ImageAwesome;
            var rectSource = e.OriginalSource as Rectangle;
            if (imageSource == null && rectSource == null && sender is TextBox tb && !tb.IsKeyboardFocusWithin)
            {
                e.Handled = true;
                tb.Focus();
            }
        }

        void ToolGrid_OnMouseEnter(object sender, MouseEventArgs e)
        {
            if (sender is Grid grid)
            {
                var viewModel = grid.DataContext as ToolDescriptorViewModel;
                grid.ToolTip = viewModel?.Tool.ResourceToolTip;
            }

            var mainWindow = Application.Current.MainWindow;

            var isValid = mainWindow.FindName("Variables") is ContentPane variablesPane && !variablesPane.IsActivePane;
            isValid &= mainWindow.FindName("Explorer") is ContentPane explorerPane && !explorerPane.IsActivePane;
            isValid &= mainWindow.FindName("OutputPane") is ContentPane outputPane && !outputPane.IsActivePane;
            isValid &= mainWindow.FindName("DocumentHost") is ContentPane documentHostPane && !documentHostPane.IsActivePane;

            if (isValid)
            {
                var toolboxPane = mainWindow.FindName("Toolbox") as ContentPane;
                toolboxPane?.Activate();
            }
        }

        void DropPointOnDragEnter(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.None;
            e.Handled = true;
        }

        void ToolGrid_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Grid grid)
            {
                var viewModel = grid.DataContext as ToolDescriptorViewModel;
                if (e.ClickCount == 1)
                {
                    var toolboxViewModel = DataContext as ToolboxViewModel;
                    toolboxViewModel?.UpdateHelpDescriptor(viewModel?.Tool.ResourceHelpText);
                }
                else
                {
                    var _applicationTracker = CustomContainer.Get<IApplicationTracker>();
                    _applicationTracker?.TrackEvent(Studio.Resources.Languages.TrackEventToolbox.EventCategory, Studio.Resources.Languages.TrackEventToolbox.DoubleClick);
                    var popupController = CustomContainer.Get<IPopupController>();
                    popupController?.Show(Studio.Resources.Languages.Core.ToolboxPopupDescription, Studio.Resources.Languages.Core.ToolboxPopupHeader, MessageBoxButton.OK, MessageBoxImage.Information, "", false, false, true, false, false, false);
                }
            }
        }
    }
}