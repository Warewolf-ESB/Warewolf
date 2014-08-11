using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using Dev2.Studio.AppResources.ExtensionMethods;

// ReSharper disable CheckNamespace
namespace Dev2.CustomControls
// ReSharper restore CheckNamespace
{
    [TemplatePart(Name = PART_Thumb, Type = typeof(Thumb))]
    public class SimpleGridSplitter : Control
    {
        #region fields
// ReSharper disable InconsistentNaming
        private const string PART_Thumb = "Thumb";
// ReSharper restore InconsistentNaming
        private Thumb _thumb;
        private Grid _containingGrid;
        #endregion

        #region ctor and overrides
        public SimpleGridSplitter()
        {
            DefaultStyleKey = typeof(SimpleGridSplitter);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _thumb = GetTemplateChild(PART_Thumb) as Thumb;

            if(_thumb != null)
            {
                _thumb.DragDelta += ThumbDragDelta;
                _containingGrid = Parent as Grid;

                if(_containingGrid == null)
                    throw new InvalidOperationException("Gridsplitter only works in grid");
            }

            SetCursor();
        }
        #endregion

        #region dependency properties
        public Orientation Orientation
        {
            get { return (Orientation)GetValue(OrientationProperty); }
            set { SetValue(OrientationProperty, value); }
        }

        public static readonly DependencyProperty OrientationProperty =
            DependencyProperty.Register("Orientation", typeof(Orientation), typeof(SimpleGridSplitter), new PropertyMetadata(Orientation.Vertical, OrientationChangedCallback));

        private static void OrientationChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var gridSplitter = (SimpleGridSplitter)dependencyObject;
            gridSplitter.SetCursor();
        }
        #endregion

        #region private methods
        private void ThumbDragDelta(object sender, DragDeltaEventArgs e)
        {

            switch(Orientation)
            {
                case Orientation.Horizontal:
                    ResizeColumns(e.HorizontalChange);
                    break;
                case Orientation.Vertical:
                    ResizeRows(e.VerticalChange);
                    break;
            }
        }

        private void SetCursor()
        {
            switch(Orientation)
            {
                case Orientation.Horizontal:
                    if(_thumb != null) _thumb.Cursor = Cursors.ScrollWE;
                    break;
                case Orientation.Vertical:
                    if(_thumb != null) _thumb.Cursor = Cursors.ScrollNS;
                    break;
            }
        }

        private void ResizeRows(double delta)
        {
            //if no change just return
            if(delta.CompareTo(0D) == 0) return;

            //Only works when the grid has at least 3 rows
            var containingGridRows = _containingGrid.RowDefinitions.Count;
            if(containingGridRows < 3) return;

            var row = Grid.GetRow(this);

            if(row < 1) return; //Do nothing if in top row - introduce ResizeBehavior
            if(row == containingGridRows - 1) return; //Do nothing if in last row - introduce ResizeBehavior

            var upperRow = row - 1;
            var lowerRow = row + 1;

            bool isUpperContentMaxed;
            bool isLowerContentMaxed;

            var upperScrollViewer = GetScrollViewer(_containingGrid, upperRow);
            var lowerScrollViewer = GetScrollViewer(_containingGrid, lowerRow);

            if(upperScrollViewer != null)
                isUpperContentMaxed = upperScrollViewer.ExtentHeight.CompareTo(upperScrollViewer.ViewportHeight) == 0;
            else
            {
                var upperScrollBar = GetScrollBar(_containingGrid, upperRow);
                isUpperContentMaxed = upperScrollBar.Visibility == Visibility.Collapsed;
            }

            if(lowerScrollViewer != null)
                isLowerContentMaxed = lowerScrollViewer.ExtentHeight.CompareTo(lowerScrollViewer.ViewportHeight) == 0;
            else
            {
                var lowerScrollBar = GetScrollBar(_containingGrid, lowerRow);
                isLowerContentMaxed = lowerScrollBar.Visibility == Visibility.Collapsed;
            }

            if(delta < 0 && isLowerContentMaxed) return;
            if(delta > 0 && isUpperContentMaxed) return;

            var upperRowDefinition = _containingGrid.RowDefinitions[upperRow];
            var lowerRowDefinition = _containingGrid.RowDefinitions[lowerRow];

            var upperRowActualHeight = upperRowDefinition.ActualHeight;
            var lowerRowActualHeight = lowerRowDefinition.ActualHeight;

            var upperRowMaxHeight = upperRowDefinition.MaxHeight;
            var lowerRowMaxHeight = lowerRowDefinition.MaxHeight;
            var upperRowMinHeight = upperRowDefinition.MinHeight;
            var lowerRowMinHeight = lowerRowDefinition.MinHeight;

            delta = AdjustDelta(upperRowActualHeight, upperRowMinHeight, upperRowMaxHeight, 1, delta);
            delta = AdjustDelta(lowerRowActualHeight, lowerRowMinHeight, lowerRowMaxHeight, -1, delta);

            var newUpperRowActualHeight = upperRowActualHeight + delta;
            var newLowerRowActualHeight = lowerRowActualHeight - delta;
            var requestedActualHeight = newUpperRowActualHeight + newLowerRowActualHeight;

            _containingGrid.BeginInit();

            var upperRatio = newUpperRowActualHeight / requestedActualHeight;
            var lowerRatio = newLowerRowActualHeight / requestedActualHeight;
            upperRowDefinition.Height = new GridLength(upperRatio, GridUnitType.Star);
            lowerRowDefinition.Height = new GridLength(lowerRatio, GridUnitType.Star);

            _containingGrid.EndInit();
        }

        private ScrollViewer GetScrollViewer(Grid containingGrid, int row)
        {
            var children = containingGrid.Children.Cast<UIElement>()
                                         .Where(ue => Grid.GetRow(ue) == row)
                                         .ToList();
            return children.FirstOrDefault(ue => ue.GetType() == typeof(ScrollViewer)) as ScrollViewer;
        }

        private ScrollBar GetScrollBar(Grid containingGrid, int row)
        {
            var grid = containingGrid.Children.Cast<UIElement>()
                                     .FirstOrDefault(ue => Grid.GetRow(ue) == row);

            var datagrid = grid.Descendents().OfType<DataGrid>().FirstOrDefault();

            var scrollBars = datagrid.Descendents().OfType<ScrollBar>().Where(sb => sb.Name == "PART_VerticalScrollBar");
            return scrollBars.FirstOrDefault();
        }

        private double AdjustDelta(double actualHeight, double minHeight, double maxHeight, int multiplier, double delta)
        {
            if(actualHeight + multiplier * delta > maxHeight)
                delta = multiplier * Math.Max(0, maxHeight - actualHeight);

            if(actualHeight + multiplier * delta < minHeight)
                delta = multiplier * Math.Min(0, minHeight - actualHeight);

            return delta;
        }

        // ReSharper disable UnusedParameter.Local
        private void ResizeColumns(double horizontalChange)
        // ReSharper restore UnusedParameter.Local
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
