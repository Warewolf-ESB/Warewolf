using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Diagnostics;
using System.Windows.Controls.Primitives;
using System.Collections.Generic;

using System.Linq;
using System.Windows.Data;
using Infragistics.Controls.Charts;
using System.Security;
using System.Reflection;





namespace Infragistics.Controls
{
    /// <summary>
    /// The platfrom specific view for a series viewer.
    /// </summary>
    public abstract class SeriesViewerView
    {
        /// <summary>
        /// Constructs a new instance of the SeriesViewerView.
        /// </summary>
        /// <param name="model">The SeriesViewer model to associate with the view.</param>
        public SeriesViewerView(SeriesViewer model)
            : base()
        {
            Debug.Assert(model != null, "View instantiated with null model.");

            Model = model;

            this.OverviewPlusDetailPane = new XamOverviewPlusDetailPane();
            this.OverviewPlusDetailPane.World = new Rect(0.0, 0.0, 1.0, 1.0);
            this.OverviewPlusDetailPane.Window = this.Model.ActualWindowRect;
            this.OverviewPlusDetailPane.SurfaceViewer = new SeriesViewerSurfaceViewer(this.Model, this);
            this.UpdateOverviewPlusDetailRects();

            this.OverviewPlusDetailPane.SetBinding(XamOverviewPlusDetailPane.VisibilityProperty, new Binding(SeriesViewer.OverviewPlusDetailPaneVisibilityPropertyName) { Source = this.Model });
            this.OverviewPlusDetailPane.SetBinding(XamOverviewPlusDetailPane.StyleProperty, new Binding(SeriesViewer.OverviewPlusDetailPaneStylePropertyName) { Source = this.Model });

            this.OverviewPlusDetailPane.SetBinding(XamOverviewPlusDetailPane.HorizontalAlignmentProperty, new Binding(SeriesViewer.OverviewPlusDetailPaneHorizontalAlignmentPropertyName) { Source = this.Model });
            this.OverviewPlusDetailPane.SetBinding(XamOverviewPlusDetailPane.VerticalAlignmentProperty, new Binding(SeriesViewer.OverviewPlusDetailPaneVerticalAlignmentPropertyName) { Source = this.Model });

            this.Model.GridAreaRectChanged += new RectChangedEventHandler(Model_GridAreaRectChanged);
            this.Model.ActualWindowRectChanged += new RectChangedEventHandler(Model_ActualWindowRectChanged);

            this.OverviewPlusDetailViewportHost = new OverviewPlusDetailViewportHost(this);

        }
        
        /// <summary>
        /// Removes an axis from the view.
        /// </summary>
        /// <param name="axis">The axis to remove.</param>
        public void RemoveAxis(Axis axis)
        {
            GridPanel.Children.Remove(axis);
        }

        /// <summary>
        /// Removes a label panel from the view. 
        /// </summary>
        /// <param name="axis">The axis for which to remove the label panel.</param>
        public void RemoveLabelPanel(Axis axis)
        {
            if (axis.LabelPanel != null
                && this.CentralArea != null
                && this.CentralArea.Children.Contains(axis.LabelPanel))
            {
                this.CentralArea.Children.Remove(axis.LabelPanel);
            }
        }

        /// <summary>
        /// Attaches and axis to the view.
        /// </summary>
        /// <param name="axis">The axis to attach.</param>
        public void AttachAxis(Axis axis)
        {
            axis.Detach();
            GridPanel.Children.Add(axis);
        }

        /// <summary>
        /// Adds a label panel to the view.
        /// </summary>
        /// <param name="axis">The axis for which to add the label panel.</param>
        public void AddLabelPanel(Axis axis)
        {
            if (axis.LabelPanel != null
                && this.CentralArea != null
                && !this.CentralArea.Children.Contains(axis.LabelPanel))
            {
                this.CentralArea.Children.Add(axis.LabelPanel);
            }
        }

        /// <summary>
        /// Cancels any pending mouse interaction with the plot area.
        /// </summary>
        public void CancelMouseInteractions()
        {
            PlotArea.ReleaseMouseCapture();
            CapturedMouse = false;
        }

        /// <summary>
        /// Returns if a series is attached to the view.
        /// </summary>
        /// <param name="series">The series in question.</param>
        /// <returns>True is the seris is attached.</returns>
        public bool SeriesAttached(Series series)
        {
            return SeriesPanel.Children.Contains(series);
        }

        /// <summary>
        /// Attaches a series to the view.
        /// </summary>
        /// <param name="series"></param>
        public void AttachSeries(Series series)
        {
            series.Detach();
            SeriesPanel.Children.Add(series);
        }

        /// <summary>
        /// Gets the panel in which series are layed out.
        /// </summary>
        protected Grid SeriesPanel
        {
            get { return seriesPanel; }
        }
        private readonly Grid seriesPanel = new Grid();
        internal virtual ChartAreaPanel CentralArea { get; set; }

        /// <summary>
        /// Gets the viewport of the plot area.
        /// </summary>
        public Rect PlotAreaViewport
        {
            get
            {
                return new Rect(0, 0, PlotArea.ActualWidth, PlotArea.ActualHeight);
            }
        }

        /// <summary>
        /// Completes the mouse capture held on the plot area.
        /// </summary>
        public void CompleteMouseCapture()
        {
            if (CapturedMouse)
            {
                PlotArea.ReleaseMouseCapture();
                CapturedMouse = false;
            }
        }

        /// <summary>
        /// Called to create the visual layout for the view.
        /// </summary>
        public virtual void CreateLayout()
        {
            if (CentralArea == null)
            {
                CentralArea = Model.GetComponentsForView().CentralArea;
            }


            #region zoombars in the central area

            CentralArea.Children.Add(VerticalZoombar);
            VerticalZoombar.ZoomChanging += new EventHandler<ZoomChangeEventArgs>(Zoombar_ZoomChanging);
            VerticalZoombar.ZoomChanged += new EventHandler<ZoomChangedEventArgs>(Zoombar_ZoomChanged);

            CentralArea.Children.Add(HorizontalZoombar);
            HorizontalZoombar.ZoomChanging += new EventHandler<ZoomChangeEventArgs>(Zoombar_ZoomChanging);
            HorizontalZoombar.ZoomChanged += new EventHandler<ZoomChangedEventArgs>(Zoombar_ZoomChanged);

            #endregion zoombars in the central area



            // the grid area
            VisualInformationManager.SetIsMainGeometryVisual(PlotAreaBorder, true);
            CentralArea.Children.Add(PlotAreaBorder);

            PlotArea.SizeChanged += new SizeChangedEventHandler(PlotArea_SizeChanged);
            PlotAreaBorder.Child = PlotArea;

            #region PlotAreaBackgroundPresenter in PlotArea

            PlotAreaBackgroundPresenter.HorizontalAlignment = HorizontalAlignment.Stretch;
            PlotAreaBackgroundPresenter.VerticalAlignment = VerticalAlignment.Stretch;
            Canvas.SetZIndex(PlotAreaBackgroundPresenter, 0);

            PlotArea.Children.Add(PlotAreaBackgroundPresenter);

            #endregion PlotAreaBackgroundPresenter in PlotArea

            #region GridPanel in GridArea

            PlotArea.Children.Add(GridPanel);

            #endregion GridPanel in GridArea

            #region SeriesPanel in GridArea

            PlotArea.Children.Add(SeriesPanel);

            #endregion SeriesPanel in GridArea

            #region GridCanvas in GridPanel

            GridPanel.Children.Add(GridCanvas);
            Path strips = new Path() { Data = new GeometryGroup(), IsHitTestVisible = false, Stroke = null };
            Path majorLines = new Path() { Data = new GeometryGroup(), IsHitTestVisible = false };
            Path minorLines = new Path() { Data = new GeometryGroup(), IsHitTestVisible = false };
            GridCanvas.Children.Add(strips);
            GridCanvas.Children.Add(majorLines);
            GridCanvas.Children.Add(minorLines);

            #endregion GridCanvas in GridPanel

            //GridPanel.Visibility = GridMode == GridMode.None ? Visibility.Collapsed : Visibility.Visible;




#region Infragistics Source Cleanup (Region)





#endregion // Infragistics Source Cleanup (Region)


            #region Detail Canvas in GridArea

            Detail = new Canvas() { IsHitTestVisible = false };
            Canvas.SetZIndex(Detail, 4);
            PlotArea.Children.Add(Detail);

            // couple of children for the viewport

            {
                // preview strokePath

                previewPath = new Path() { IsHitTestVisible = false };
                Detail.Children.Add(previewPath);
            }

            {
                // drag strokePath

                DoubleCollection doubleCollection = new DoubleCollection();
                doubleCollection.Add(2.0);
                doubleCollection.Add(4.0);

                dragPath = new Path() { IsHitTestVisible = false, Stroke = new SolidColorBrush(Colors.Black), StrokeThickness = 0.75, Data = new RectangleGeometry() };
                dragPath.StrokeDashArray = doubleCollection;

                Detail.Children.Add(dragPath);
            }

            {
                // horizontal and vertical crosshairs

                horizontalCrosshair = new Line() { IsHitTestVisible = false };
                verticalCrosshair = new Line() { IsHitTestVisible = false };

                //Detail.Children.Add(horizontalCrosshair);
                //Detail.Children.Add(verticalCrosshair);



#region Infragistics Source Cleanup (Region)







#endregion // Infragistics Source Cleanup (Region)

            }

            #endregion Detail Canvas in GridArea

            #region Attach GridArea Events


            PlotAreaBorder.MouseEnter += new MouseEventHandler(PlotArea_MouseEnter);
            PlotAreaBorder.MouseLeave += new MouseEventHandler(PlotArea_MouseLeave);
            PlotAreaBorder.MouseMove += new MouseEventHandler(PlotArea_MouseMove);
            PlotAreaBorder.MouseWheel += new MouseWheelEventHandler(PlotArea_MouseWheel);
            PlotAreaBorder.MouseLeftButtonDown += new MouseButtonEventHandler(PlotArea_MouseLeftButtonDown);
            PlotAreaBorder.MouseLeftButtonUp += new MouseButtonEventHandler(PlotArea_MouseLeftButtonUp);
            Model.GetComponentsForView().SeriesViewer.KeyDown += new KeyEventHandler(Chart_KeyDown);

            //TouchUtil.ManipulationDelta+=new EventHandler<ManipulationDeltaEventArgs>(TouchUtil_ManipulationDelta);
            //TouchUtil.ManipulationCompleted+=new EventHandler<ManipulationCompletedEventArgs>(TouchUtil_ManipulationCompleted);


#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)


            #endregion Attach GridArea Events

            ToolTipPopup = new Popup()
            {
                DataContext = new DataContext(),
                Child = new ContentControl()
                {
                    IsHitTestVisible = false
                }
            };



#region Infragistics Source Cleanup (Region)







#endregion // Infragistics Source Cleanup (Region)



            if (SafeSetter.IsSafe)
            {
                ToolTipPopup.AllowsTransparency = true;

                try
                {
                    // [DN January 18, 2012 : 99043]
                    typeof(Popup).GetProperty("HitTestable", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetProperty).SetValue(this.ToolTipPopup, false, null);
                }
                catch (SecurityException) { }
                catch (MissingMethodException) { }

            }
            else
            {
                ToolTipPopup.PlacementTarget = Model;
            }


            #region OPD

            // you down with OPD?  yeah, you know me.

            this.CentralArea.Children.Add(this.OverviewPlusDetailPane);

            Action<object, PropertyChangedEventArgs<Rect>> onWindowChanged = (sender, e) => this.OnOverviewPlusDetailPaneWindowChanged(e.OldValue, e.NewValue);
            // the "ing" and "ed" events don't mean what you think they mean.  just handle them both and treat them equally...
            this.OverviewPlusDetailPane.WindowChanging += new EventHandler<PropertyChangedEventArgs<Rect>>(onWindowChanged);
            this.OverviewPlusDetailPane.WindowChanged += new EventHandler<PropertyChangedEventArgs<Rect>>(onWindowChanged);
            Canvas.SetZIndex(this.OverviewPlusDetailPane, 5);
            #endregion

        }

        /// <summary>
        /// Represents the keyboard modifiers that are currently held down.
        /// </summary>
        public ModifierKeys CurrentModifiers { get { return Keyboard.Modifiers; } }

        /// <summary>
        /// Focuses the chart element.
        /// </summary>
        public void FocusChart()
        {
            Model.GetComponentsForView().SeriesViewer.Focus();
        }


        public bool StealingCapture { get; set; }


        /// <summary>
        /// Makes the plot area capture the mouse.
        /// </summary>
        public void PlotAreaCaptureMouse()
        {






            StealingCapture = true;
            if (Mouse.Capture(this.PlotArea, CaptureMode.SubTree))
            {
                CapturedMouse = true;
            }
            StealingCapture = false;

        }
        private SeriesViewerComponentsFromView _componentsFromView = new SeriesViewerComponentsFromView();
        internal SeriesViewerComponentsFromView GetComponentsFromView()
        {

            _componentsFromView.HorizontalZoombar = HorizontalZoombar;
            _componentsFromView.VerticalZoombar = VerticalZoombar;

            _componentsFromView.PlotAreaBorder = PlotAreaBorder;
            _componentsFromView.ToolTipPopup = ToolTipPopup;


            _componentsFromView.OverviewPlusDetailPane = OverviewPlusDetailPane;


            return _componentsFromView;
        }

        /// <summary>
        /// Hides the drag path.
        /// </summary>
        public void HideDragPath()
        {
            dragPath.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// Makes the drag path visible.
        /// </summary>
        internal void ShowDragPath()
        {
            dragPath.Visibility = Visibility.Visible;
        }
        /// <summary>
        /// Sets the PlotArea's cursor to null, which effectively sets it to the system default cursor.
        /// </summary>
        public void SetDefaultCursor()
        {
            PlotArea.Cursor = null;
        }

        /// <summary>
        /// Gets the target for visual state transitions.
        /// </summary>
        protected virtual Control VisualStateTarget
        {
            get
            {
                return Model.GetComponentsForView().SeriesViewer;
            }
        }

        /// <summary>
        /// Transitions the view to the idle state.
        /// </summary>
        public void GoToIdleState()
        {
            var target = Model.GetComponentsForView().SeriesViewer;
            VisualStateManager.GoToState(VisualStateTarget, SeriesViewer.IdleVisualStateName, true);
        }

        /// <summary>
        /// Sets the hand cursor for the view.
        /// </summary>
        public void SetHandCursor()
        {
            PlotArea.Cursor = Cursors.Hand;
        }

        /// <summary>
        /// Hides the tooltip.
        /// </summary>
        public void HideTooltip()
        {
            ToolTipPopup.IsOpen = false;
        }

        /// <summary>
        /// Transitions to the dragging visual state.
        /// </summary>
        public void GoToDraggingVisualState()
        {
            VisualStateManager.GoToState(VisualStateTarget, SeriesViewer.DraggingVisualStateName, true);
        }

        /// <summary>
        /// Transitions to the panning visual state.
        /// </summary>
        public void GoToPanningVisualState()
        {
            VisualStateManager.GoToState(VisualStateTarget, SeriesViewer.PanningVisualStateName, true);
        }

        /// <summary>
        /// Updates the path for the drag rectangle.
        /// </summary>
        /// <param name="rect">The new rectangle to user.</param>
        public void UpdateDragPath(Rect rect)
        {
            (dragPath.Data as RectangleGeometry).Rect = rect;
        }

        /// <summary>
        /// Updates the positioning of the vertical crosshair.
        /// </summary>
        /// <param name="x1">The first x coordinate for the crosshair.</param>
        /// <param name="y1">The first y coordinate for the crosshair.</param>
        /// <param name="x2">The second x coordinate for the crosshair.</param>
        /// <param name="y2">The second y coordinate for the crosshair.</param>
        public void UpdateVerticalCrosshair(double x1, double y1, double x2, double y2)
        {
            verticalCrosshair.X1 = x1;
            verticalCrosshair.Y1 = y1;
            verticalCrosshair.X2 = x2;
            verticalCrosshair.Y2 = y2;
        }

        /// <summary>
        /// Makes the vertical crosshair visible.
        /// </summary>
        public void ShowVerticalCrosshair()
        {
            if (!Detail.Children.Contains(verticalCrosshair))
            {
                Detail.Children.Add(verticalCrosshair);
            }
            verticalCrosshair.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Hides the vertical crosshair.
        /// </summary>
        public void HideVerticalCrosshair()
        {
            if (verticalCrosshair != null && verticalCrosshair.Visibility == Visibility.Visible)
            {
                verticalCrosshair.Visibility = Visibility.Collapsed;
            }
        }

        /// <summary>
        /// Moves the info box.
        /// </summary>
        /// <param name="x"></param>
        public void UpdateInfoBoxXTranslation(double x)
        {




        }

        /// <summary>
        /// Updates the position of the horizontal crosshair.
        /// </summary>
        /// <param name="x1">The first x coordinate for the crosshair.</param>
        /// <param name="y1">The first y coordinate for the crosshair.</param>
        /// <param name="x2">The second x coordinate for the crosshair.</param>
        /// <param name="y2">The second y coordinate for the crosshair.</param>
        public void UpdateHorizontalCrosshair(double x1, double y1, double x2, double y2)
        {
            horizontalCrosshair.X1 = x1;
            horizontalCrosshair.Y1 = y1;
            horizontalCrosshair.X2 = x2;
            horizontalCrosshair.Y2 = y2;
        }

        /// <summary>
        /// Makes the horizontal crosshair visible.
        /// </summary>
        public void ShowHorizontalCrosshair()
        {
            if (!Detail.Children.Contains(horizontalCrosshair))
            {
                Detail.Children.Add(horizontalCrosshair);
            }
            horizontalCrosshair.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Hides the horizontal crosshair.
        /// </summary>
        public void HideHorizontalCrosshair()
        {
            if (horizontalCrosshair != null && horizontalCrosshair.Visibility == Visibility.Visible)
            {
                horizontalCrosshair.Visibility = Visibility.Collapsed;
            }
        }



#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

        /// <summary>
        /// Hides the preivew path.
        /// </summary>
        public void HidePreviewPath()
        {
            previewPath.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// Upates the preview path.
        /// </summary>
        /// <param name="viewport">The viewport rectangle.</param>
        /// <param name="previewRect">The preview rectangle.</param>
        public void UpdatePreviewPath(Rect viewport, Rect previewRect)
        {
            GeometryGroup geometryGroup = new GeometryGroup();

            geometryGroup.Children.Add(new RectangleGeometry() { Rect = viewport });
            geometryGroup.Children.Add(new RectangleGeometry() { Rect = previewRect });

            previewPath.Data = geometryGroup;
        }

        /// <summary>
        /// Makes the preview path visible.
        /// </summary>
        public void ShowPreviewPath()
        {
            previewPath.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Called to initialize the view.
        /// </summary>
        public void OnInit()
        {
            var components = Model.GetComponentsForView();
            var chart = components.SeriesViewer;
            chart.Loaded += new RoutedEventHandler(chart_Loaded);
            chart.Unloaded += new RoutedEventHandler(chart_Unloaded);
        }

        void chart_Unloaded(object sender, RoutedEventArgs e)
        {
            Model.OnDetachedFromUI();
        }

        void chart_Loaded(object sender, RoutedEventArgs e)
        {
            Model.OnAttachedToUI();
        }
        /// <summary>
        /// Boolean indicating whether or not zoombars are supported by this SeriesViewer.
        /// </summary>
        /// <returns>True.</returns>
        protected virtual bool ZoombarsSupported()
        {
            return true;
        }

        /// <summary>
        /// Called when the template is provided.
        /// </summary>
        public virtual void OnTemplateProvided()
        {
            var target = Model.GetComponentsForView().SeriesViewer;

            if (ZoombarsSupported())
            {
                VerticalZoombar.SetBinding(Control.StyleProperty, new Binding(SeriesViewer.ZoombarStylePropertyName) { Source = target });
                VerticalZoombar.SetBinding(Control.VisibilityProperty, new Binding(SeriesViewer.VerticalZoombarVisibilityPropertyName) { Source = target });

                HorizontalZoombar.SetBinding(Control.StyleProperty, new Binding(SeriesViewer.ZoombarStylePropertyName) { Source = target });
                HorizontalZoombar.SetBinding(Control.VisibilityProperty, new Binding(SeriesViewer.HorizontalZoombarVisibilityPropertyName) { Source = target });
            }
            else
            {
                if (VerticalZoombar != null)
                {
                    VerticalZoombar.Visibility = Visibility.Collapsed;
                }
                if (HorizontalZoombar != null)
                {
                    HorizontalZoombar.Visibility = Visibility.Collapsed;
                }
            }

            previewPath.SetBinding(Shape.StyleProperty, new Binding(SeriesViewer.PreviewPathStylePropertyName) { Source = target });

            PlotAreaBorder.SetBinding(Border.BorderBrushProperty, new Binding(SeriesViewer.PlotAreaBorderBrushPropertyName) { Source = target });
            PlotAreaBorder.SetBinding(Border.BorderThicknessProperty, new Binding(SeriesViewer.PlotAreaBorderThicknessPropertyName) { Source = target });
            PlotAreaBorder.SetBinding(Border.BackgroundProperty, new Binding(SeriesViewer.PlotAreaBackgroundPropertyName) { Source = target });

            horizontalCrosshair.SetBinding(Shape.StyleProperty, new Binding(SeriesViewer.CrosshairLineStylePropertyName) { Source = target });
            verticalCrosshair.SetBinding(Shape.StyleProperty, new Binding(SeriesViewer.CrosshairLineStylePropertyName) { Source = target });


#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)





            ContentControl toolTipControl = ToolTipPopup.Child as ContentControl;
            toolTipControl.SetBinding(ContentControl.StyleProperty,
                new Binding(SeriesViewer.ToolTipStylePropertyName) { Source = target });


            VisualStateManager.GoToState(VisualStateTarget, SeriesViewer.IdleVisualStateName, false);
        }

        internal XamOverviewPlusDetailPane OverviewPlusDetailPane { get; private set; }


        private void OnOverviewPlusDetailPaneWindowChanged(Rect oldRect, Rect newRect)
        {
            // adjust rect based on horizontal zoomableness and vertical zoomableness
            Rect newWindowRect = SeriesViewerSurfaceViewer.ChangeRect(oldRect, newRect, this.Model.HorizontalZoomable, this.Model.VerticalZoomable, this.OverviewPlusDetailPane.World);

            
            if (newWindowRect.IsEmpty)
            {
                // the window has escaped the world.  don't allow this...
                this.OverviewPlusDetailPane.Window = oldRect;
                return;
            }
            else if (newWindowRect != newRect)
            {
                this.OverviewPlusDetailPane.Window = newWindowRect;
                // the event should be raised again by the OPD.  but it isn't, so i'm just gonna invoke this method again.
                this.OnOverviewPlusDetailPaneWindowChanged(oldRect, newWindowRect);
            }
            else
            {
                double aspect = this.Model.ViewportRect.Width / this.Model.ViewportRect.Height;
                newWindowRect = new Rect(newWindowRect.Left / aspect, newWindowRect.Top, newWindowRect.Width / aspect, newWindowRect.Height);
                this.Model.WindowRect = newWindowRect;
            }
        }

        /// <summary>
        /// Called wehn the OPD rects need to be updated.
        /// </summary>
        public void UpdateOverviewPlusDetailRects()
        {
            if (this.Model.OverviewPlusDetailPaneVisibility != Visibility.Visible)
            {
                return;
            }

            double aspect = 1;

            if (this.Model.ViewportRect.IsEmpty == false)
            {
                aspect = this.Model.ViewportRect.Width / this.Model.ViewportRect.Height;
            }

            var oldRect = this.OverviewPlusDetailPane.World;
            var newRect = new Rect(0.0, 0.0, aspect, 1.0);
            this.OverviewPlusDetailPane.World = newRect;
            if (oldRect.Width != newRect.Width ||
                oldRect.Height != newRect.Height ||
                oldRect.X != newRect.X ||
                oldRect.Y != newRect.Y)
            {
                ((SeriesViewerSurfaceViewer)OverviewPlusDetailPane.SurfaceViewer).IsDirty = true;
            }
            this.OverviewPlusDetailPane.Window = new Rect(this.Model.ActualWindowRect.Left * aspect, this.Model.ActualWindowRect.Top, this.Model.ActualWindowRect.Width * aspect, this.Model.ActualWindowRect.Height);
        }

        private void Model_ActualWindowRectChanged(object sender, RectChangedEventArgs e)
        {
            this.UpdateOverviewPlusDetailRects();
        }
        
        private void Model_GridAreaRectChanged(object sender, RectChangedEventArgs e)
        {
            this.UpdateOverviewPlusDetailRects();

        }
        internal OverviewPlusDetailViewportHost OverviewPlusDetailViewportHost { get; private set; }

        private double _lastWidth;
        private double _lastHeight;
        internal void GetThumbnail(double width, double height, RenderSurface surface)
        {
            bool sizeChanged = false;
            if (width != _lastWidth || height != _lastHeight)
            {
                _lastHeight = height;
                _lastWidth = width;
                sizeChanged = true;
            }

            foreach (Series s in this.Model.Series)
            {
                if (sizeChanged)
                {
                    s.ThumbnailDirty = true;
                }
                s.RenderThumbnail(new Rect(0.0, 0.0, width, height), surface);
            }
        }

        /// <summary>
        /// Sets or gets the model.
        /// </summary>
        protected virtual SeriesViewer Model { get; set; }



        /// <summary>
        /// The current Chart object's horizontal Zoombar
        /// </summary>
        public XamZoombar HorizontalZoombar
        {
            get { return _horizontalZoombar; }
        }

        private readonly XamZoombar _horizontalZoombar = new XamZoombar() { Orientation = Orientation.Horizontal, Height = 16 };

        /// <summary>
        /// The current Chart object's vertical Zoombar
        /// </summary>
        public XamZoombar VerticalZoombar
        {
            get
            {
                return _verticalZoombar;
            }
        }

        private readonly XamZoombar _verticalZoombar = new XamZoombar() { Orientation = Orientation.Vertical, Width = 16 };


        /// <summary>
        /// The current Chart object's grid area border.
        /// </summary>
        internal Border PlotAreaBorder
        {
            get { return plotAreaBorder; }
        }

        private readonly Border plotAreaBorder = new Border();

        /// <summary>
        /// The current Chart object's plot area.
        /// </summary>
        internal Grid PlotArea
        {
            get { return plotArea; }
        }

        private readonly Grid plotArea = new Grid();

        /// <summary>
        /// Gets the background content presenter.
        /// </summary>
        protected ContentPresenter PlotAreaBackgroundPresenter
        {
            get { return plotAreaBackgroundPresenter; }
        }

        private ContentPresenter plotAreaBackgroundPresenter = new ContentPresenter();



        /// <summary>
        /// The canvas containing preview path, drag path, crosshairs.
        /// </summary>
        internal Canvas Detail { get; set; }

        private Rect previewRect = Rect.Empty;
        private Path previewPath;

        /// <summary>
        /// Path used to display the user's drag rectangle. The strokePath is used to display the
        /// precise rectangle dragged by the user which may or may not correspond to the preview
        /// area or anything else.
        /// </summary>
        private Path dragPath;

        /// <summary>
        /// Line used to display the horizontal crosshair.
        /// </summary>
        private Line horizontalCrosshair;

        /// <summary>
        /// Line used to display the vertical crosshair.
        /// </summary>
        private Line verticalCrosshair;


        #region ToolTipPopup (Internal)

        internal Popup ToolTipPopup
        {
            get { return toolTipPopup; }
            private set
            {
                toolTipPopup = value;


#region Infragistics Source Cleanup (Region)





#endregion // Infragistics Source Cleanup (Region)

            }
        }

        private Popup toolTipPopup;

        #endregion ToolTipPopup (Internal)

        /// <summary>
        /// Called when a series should be removed.
        /// </summary>
        /// <param name="series">The series to remove.</param>
        public void RemoveSeries(Series series)
        {
            SeriesPanel.Children.Remove(series);
        }



#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)


        /// <summary>
        /// Listener for the grid area's mouse enter event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PlotArea_MouseEnter(object sender, MouseEventArgs e)
        {
            Point pt = e.GetPosition(PlotArea);

            Model.OnMouseEnter(pt);
        }

        /// <summary>
        /// Listener for the grid area's mouse leave event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PlotArea_MouseLeave(object sender, MouseEventArgs e)
        {
            Point pt = e.GetPosition(PlotArea);

            Model.OnMouseLeave(pt);
        }

        private bool CapturedMouse { get; set; }

        /// <summary>
        /// Listener for the grid area's left button down event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PlotArea_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.ContactStarted(e);
        }

        /// <summary>
        /// Listener for the grid area's mouse move event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PlotArea_MouseMove(object sender, MouseEventArgs e)
        {
            this.ContactMoved(e);
        }

        /// <summary>
        /// Listener for the grid area's left button up event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PlotArea_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            this.ContactCompleted(e);
        }

        private void PlotArea_MouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            Point pt = e.GetPosition(PlotArea);

            e.Handled = Model.OnMouseWheel(pt, e.Delta / 1200.0);
        }

        /// <summary>
        /// Listener for the chart area's key down event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Chart_KeyDown(object sender, KeyEventArgs e)
        {
            Key key = e.Key;
            bool handled = Model.OnKeyDown(key);
            if (handled)
            {
                e.Handled = true;
            }
        }

        /// <summary>
        /// Listener for the grid area's size changed event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PlotArea_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Rect oldGridAreaRect = new Rect(new Point(0, 0), e.PreviousSize);
            Rect newGridAreaRect = new Rect(new Point(0, 0), e.NewSize);

            PlotArea.Clip = new RectangleGeometry() { Rect = newGridAreaRect };

            Model.ProcessPlotAreaSizeChanged(oldGridAreaRect, newGridAreaRect);
        }



#region Infragistics Source Cleanup (Region)







































































































#endregion // Infragistics Source Cleanup (Region)


        private void ContactStarted(EventArgs eventArgs)
        {




            var e = (MouseButtonEventArgs)eventArgs;
            bool isFinger = false;

            var pt = e.GetPosition(PlotArea);

            Model.OnContactStarted(pt, isFinger);
        }

        private void ContactMoved(EventArgs eventArgs)
        {




            var e = (MouseEventArgs)eventArgs;
            bool isFinger = false;

            var pt = e.GetPosition(PlotArea);

            Model.OnContactMoved(pt, isFinger);
        }

        private void ContactCompleted(EventArgs eventArgs)
        {




            var e = (MouseButtonEventArgs)eventArgs;
            bool isFinger = false;

            var pt = e.GetPosition(PlotArea);

            Model.OnContactCompleted(pt, isFinger);
        }




        internal void ResetWindowRect()
        {
            Model.ClearValue(SeriesViewer.WindowRectProperty);
        }


        #region Zoombar Feedback

        /// <summary>
        /// Sentinel used to indicate that events from either of the zoombars should be ignored. This
        /// is required when the zoombar is configured in code.
        /// </summary>
        private bool ignoreZoombar;

        /// <summary>
        /// Listener for the Zoombar ZoomChanged events from both the horizontal and vertical Zoombars.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Zoombar_ZoomChanged(object sender, ZoomChangedEventArgs e)
        {
            if (!ignoreZoombar)
            {
                Rect rc = this.ProcessZoombarChange(sender, e.NewRange);

                Model.OnZoomChanged(rc);
            }
        }

        /// <summary>
        /// Listener for the Zoombar ZoomChanging events from both the horizontal and vertical Zoombars.
        /// </summary>
        private void Zoombar_ZoomChanging(object sender, ZoomChangeEventArgs e)
        {
            if (!ignoreZoombar)
            {
                if (sender == HorizontalZoombar && !Model.HorizontalZoomable)
                {
                    e.Cancel = true;
                    return;
                }

                if (sender == VerticalZoombar && !Model.VerticalZoomable)
                {
                    e.Cancel = true;
                    return;
                }

                Rect rc = this.ProcessZoombarChange(sender, e.NewRange);
                Model.OnZoomChanging(rc);
            }
        }

        private Rect ProcessZoombarChange(object sender, Range zoombarRange)
        {
            Rect rc = Model.ActualWindowRect;

            if (sender == HorizontalZoombar)
            {
                double left = (zoombarRange.Minimum - HorizontalZoombar.Minimum) / HorizontalZoombar.Maximum - HorizontalZoombar.Minimum;
                double right = (zoombarRange.Maximum - HorizontalZoombar.Minimum) / HorizontalZoombar.Maximum - HorizontalZoombar.Minimum;

                rc.X = left;
                rc.Width = right - left;
            }

            if (sender == VerticalZoombar)
            {
                double bottom = (zoombarRange.Maximum - VerticalZoombar.Minimum) / VerticalZoombar.Maximum - VerticalZoombar.Minimum;
                double top = (zoombarRange.Minimum - VerticalZoombar.Minimum) / VerticalZoombar.Maximum - VerticalZoombar.Minimum;

                rc.Y = top;
                rc.Height = bottom - top;
            }
            return rc;
        }

        #endregion Zoombar Feedback






        /// <summary>
        /// Called when the zoombars should be updated with new zoom values.
        /// </summary>
        /// <param name="windowRect">The new window rectangle.</param>
        public void UpdateZoombars(Rect windowRect)
        {
            ignoreZoombar = true;

            if (HorizontalZoombar != null)
            {
                double minimum = (windowRect.Left + HorizontalZoombar.Minimum) * HorizontalZoombar.Maximum + HorizontalZoombar.Minimum;
                double maximum = (windowRect.Right + HorizontalZoombar.Minimum) * HorizontalZoombar.Maximum + HorizontalZoombar.Minimum;

                HorizontalZoombar.Range = new Range() { Minimum = minimum, Maximum = maximum };
                double difference = maximum - minimum;
                if (double.IsNaN(difference))
                {
                    HorizontalZoombar.SmallChange =
                        HorizontalZoombar.LargeChange = 0.0;
                }
                else
                {
                    HorizontalZoombar.SmallChange = 0.1 * difference;
                    HorizontalZoombar.LargeChange = difference;
                }
            }

            if (VerticalZoombar != null)
            {
                double minimum = (windowRect.Top + VerticalZoombar.Minimum) * VerticalZoombar.Maximum + VerticalZoombar.Minimum;
                double maximum = (windowRect.Bottom + VerticalZoombar.Minimum) * VerticalZoombar.Maximum + VerticalZoombar.Minimum;
                VerticalZoombar.Range = new Range() { Minimum = minimum, Maximum = maximum };
                double difference = maximum - minimum;
                if (double.IsNaN(difference))
                {
                    VerticalZoombar.SmallChange = 
                        VerticalZoombar.LargeChange = 0.0;
                }
                else
                {
                    VerticalZoombar.SmallChange = 0.1 * (maximum - minimum);
                    VerticalZoombar.LargeChange = maximum - minimum;
                }
            }

            ignoreZoombar = false;
        }








        internal static IEnumerable<Series> GetAllSeries(Series series)
        {

            var allSeries = series.SeriesViewer.Series.Concat(
                from s in series.SeriesViewer.Series.OfType<StackedSeriesBase>()
                from ss in s.StackedSeriesManager.SeriesVisual
                select ss as Series);
            return allSeries;
        }


        /// <summary>
        /// Gets the panel associated with the grid elements.
        /// </summary>
        protected Grid GridPanel
        {
            get { return gridPanel; }
        }

        private readonly Grid gridPanel = new Grid();

        internal Canvas GridCanvas
        {
            get { return gridCanvas; }
        }

        private readonly Canvas gridCanvas = new Canvas();

        internal void OverviewPlusDetailPaneVisibilityChanged()
        {

        }

        internal void ViewportChanged(Rect viewport)
        {

        }

        internal void EnsurePanelsArranged()
        {
            CentralArea.InvalidateArrange();
            CentralArea.UpdateLayout();
        }

        internal void OnPlotAreaBackgroundChanged(Brush plotAreaBackground)
        {

        }

        internal void CheckInteractionCompleted(Point pt)
        {
//#if WPF
//            if (Model.State != InteractionState.None)
//            {
//                if (Mouse.LeftButton == MouseButtonState.Released)
//                {
//                    Model.OnContactCompleted(pt);
//                }
//            }
//#endif
        }

        public bool UseDeltaZoom { get; set; }
    }

    internal class OverviewPlusDetailViewportHost : IProvidesViewport
    {
        internal OverviewPlusDetailViewportHost(SeriesViewerView view)
        {
            if (view == null)
            {
                throw new ArgumentNullException("view");
            }
            this.View = view;
        }
        internal SeriesViewerView View { get; private set; }
        #region IProvidesViewport Members

        public void GetViewInfo(out Rect viewportRect, out Rect windowRect)
        {
            viewportRect = this.View.OverviewPlusDetailPane.PreviewViewportdRect;
            windowRect = SeriesViewer.StandardRect;
        }

        #endregion
    }

}

#region Copyright (c) 2001-2012 Infragistics, Inc. All Rights Reserved
/* ---------------------------------------------------------------------*
*                           Infragistics, Inc.                          *
*              Copyright (c) 2001-2012 All Rights reserved               *
*                                                                       *
*                                                                       *
* This file and its contents are protected by United States and         *
* International copyright laws.  Unauthorized reproduction and/or       *
* distribution of all or any portion of the code contained herein       *
* is strictly prohibited and will result in severe civil and criminal   *
* penalties.  Any violations of this copyright will be prosecuted       *
* to the fullest extent possible under law.                             *
*                                                                       *
* THE SOURCE CODE CONTAINED HEREIN AND IN RELATED FILES IS PROVIDED     *
* TO THE REGISTERED DEVELOPER FOR THE PURPOSES OF EDUCATION AND         *
* TROUBLESHOOTING. UNDER NO CIRCUMSTANCES MAY ANY PORTION OF THE SOURCE *
* CODE BE DISTRIBUTED, DISCLOSED OR OTHERWISE MADE AVAILABLE TO ANY     *
* THIRD PARTY WITHOUT THE EXPRESS WRITTEN CONSENT OF INFRAGISTICS, INC. *
*                                                                       *
* UNDER NO CIRCUMSTANCES MAY THE SOURCE CODE BE USED IN WHOLE OR IN     *
* PART, AS THE BASIS FOR CREATING A PRODUCT THAT PROVIDES THE SAME, OR  *
* SUBSTANTIALLY THE SAME, FUNCTIONALITY AS ANY INFRAGISTICS PRODUCT.    *
*                                                                       *
* THE REGISTERED DEVELOPER ACKNOWLEDGES THAT THIS SOURCE CODE           *
* CONTAINS VALUABLE AND PROPRIETARY TRADE SECRETS OF INFRAGISTICS,      *
* INC.  THE REGISTERED DEVELOPER AGREES TO EXPEND EVERY EFFORT TO       *
* INSURE ITS CONFIDENTIALITY.                                           *
*                                                                       *
* THE END USER LICENSE AGREEMENT (EULA) ACCOMPANYING THE PRODUCT        *
* PERMITS THE REGISTERED DEVELOPER TO REDISTRIBUTE THE PRODUCT IN       *
* EXECUTABLE FORM ONLY IN SUPPORT OF APPLICATIONS WRITTEN USING         *
* THE PRODUCT.  IT DOES NOT PROVIDE ANY RIGHTS REGARDING THE            *
* SOURCE CODE CONTAINED HEREIN.                                         *
*                                                                       *
* THIS COPYRIGHT NOTICE MAY NOT BE REMOVED FROM THIS FILE.              *
* --------------------------------------------------------------------- *
*/
#endregion Copyright (c) 2001-2012 Infragistics, Inc. All Rights Reserved