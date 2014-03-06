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
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using Infragistics.Controls.Charts.VisualData;

namespace Infragistics.Controls.Charts
{
    /// <summary>
    /// Represents the platform specific portion of a series.
    /// </summary>
    public class SeriesView : IProvidesViewport
    {
        /// <summary>
        /// Gets or sets the model for the series.
        /// </summary>
        protected virtual Series Model { get; set; }

        /// <summary>
        /// Gets or sets the calculator to use for the viewport calculations.
        /// </summary>
        protected internal ViewportCalculator ViewportCalculator { get; set; }
        internal SeriesToolTipManager ChartToolTipManager { get; set; }

        /// <summary>
        /// Constructs a SeriesView.
        /// </summary>
        /// <param name="model">The model to associate with the view.</param>
        public SeriesView(Series model)
        {
            Model = model;
        }

        /// <summary>
        /// Called to initialize the view.
        /// </summary>
        public virtual void OnInit()
        {
            ChartToolTipManager = new SeriesToolTipManager(Model);
            ViewportCalculator = new AxisBasedViewportCalculator();

            if (!IsThumbnailView)
            {
                VisualStateManager.GoToState(VisualStateTarget, Series.NormalVisualStateName, false);
                Model.Loaded += new RoutedEventHandler(Series_Loaded);
                Model.Unloaded += new RoutedEventHandler(Series_Unloaded);

            
                Model.MouseEnter += OnMouseEnter;
                Model.MouseLeave += OnMouseLeave;
                Model.MouseMove += OnMouseMove;
                Model.MouseLeftButtonDown += OnMouseLeftButtonDown;
                Model.MouseLeftButtonUp += OnMouseLeftButtonUp;
                Model.LostMouseCapture += OnLostMouseCapture;

                Model.MouseRightButtonDown += OnMouseRightButtonDown;
                Model.MouseRightButtonUp += OnMouseRightButtonUp;

            }
        }

        private void Series_Loaded(object sender, RoutedEventArgs e)
        {
            Model.OnSeriesAttached();
        }

        private void Series_Unloaded(object sender, RoutedEventArgs e)
        {
            Model.OnSeriesDetached();

        }

        /// <summary>
        /// Gets the root visual for the series.
        /// </summary>
        protected Canvas RootCanvas { get; set; }

        /// <summary>
        /// Called when the template for the control has been provided.
        /// </summary>
        protected internal virtual void OnTemplateProvided()
        {
            this.AttachUI(Model.GetSeriesComponentsForView().RootCanvas);
        }

        /// <summary>
        /// Called to attach ui of the view to the appropriate canvas.
        /// </summary>
        /// <param name="rootCanvas">The canvas to which to attach the view visuals.</param>
        public virtual void AttachUI(Canvas rootCanvas)
        {
            Canvas oldCanvas = RootCanvas;
            RootCanvas = rootCanvas;
            RootCanvas.SizeChanged += RootCanvas_SizeChanged;
            if (oldCanvas != null)
            {
                oldCanvas.SizeChanged -= RootCanvas_SizeChanged;
                oldCanvas.TransferChildrenTo(RootCanvas);
            }
            if (MarkerCanvas == null)
            {
                MarkerCanvas = new Canvas();
            }
            if (!RootCanvas.Children.Contains(MarkerCanvas)
                && !Model.UseParentMarkerCanvas()
                && MarkerCanvas.Parent == null)
            {
                RootCanvas.Children.Add(MarkerCanvas);
            }
            Canvas.SetZIndex(MarkerCanvas, 1000);

            if (IsThumbnailView)
            {
                this.RootCanvas.SetBinding(UIElement.OpacityProperty, new Binding("Opacity") { Source = this.Model });
            }
            else
            {
                Model.DoUpdateIndexedProperties();
            }
        }



        /// <summary>
        /// Gets the parent canvas for the current series object's markerItems.
        /// </summary>
        internal protected Canvas MarkerCanvas { get; private set; }

        internal void SetMarkerCanvas(Canvas canvas)
        {
            MarkerCanvas = canvas;
        }

        void RootCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            OnViewportChanged(new Rect(new Point(0, 0), e.PreviousSize), new Rect(new Point(0, 0), e.NewSize));
        }
        private void OnViewportChanged(Rect oldRect, Rect newRect)
        {
            Model.OnViewportChanged(oldRect, newRect);
        }

        internal Series VisualStateTarget
        {
            get
            {
                return Model.GetSeriesComponentsForView().Series;
            }
        }


        internal void GoToMouseOverState()
        {
            VisualStateManager.GoToState(VisualStateTarget, Series.MouseOverVisualStateName, true);
        }


        internal void GoToNormalState()
        {
            VisualStateManager.GoToState(VisualStateTarget, Series.NormalVisualStateName, true);
        }

        internal Popup ToolTipPopup
        {
            get
            {
                var series = Model.GetSeriesComponentsForView().Series;
                if (series != null && series.SeriesViewer != null)
                {
                    return series.SeriesViewer.ToolTipPopup;
                }
                return null;
            }
        }

        internal void HideTooltip()
        {
            Popup toolTipPopup = ToolTipPopup;
            ContentControl toolTipControl = toolTipPopup != null ? toolTipPopup.Child as ContentControl : null;

            if (toolTipControl != null)
            {
                toolTipPopup.IsOpen = false;
            }
        }

        internal StringFormatter ToolTipFormatter = new StringFormatter();
        internal void UpdateToolTipValue(object toolTip)
        {
            ToolTipFormatter = toolTip is string ? new StringFormatter() { FormatString = toolTip as string } : null;

            ContentControl toolTipControl = ToolTipPopup != null ? ToolTipPopup.Child as ContentControl : null;

            if (toolTipControl != null && (ToolTipPopup.DataContext as DataContext).Series ==
                Model.GetSeriesComponentsForView().Series)
            {
                if (toolTip is string)
                {
                    toolTipControl.Content = ToolTipFormatter.Format(ToolTipPopup.DataContext as DataContext, null);
                }

                if (toolTip is UIElement)
                {
                    toolTipControl.Content = toolTip as UIElement;
                }
            }


#region Infragistics Source Cleanup (Region)












#endregion // Infragistics Source Cleanup (Region)

        }

        internal bool Ready()
        {
            return RootCanvas != null;
        }

        private SeriesComponentsFromView _seriesComponentsFromView = new SeriesComponentsFromView();
        internal SeriesComponentsFromView GetSeriesComponentsFromView()
        {
            _seriesComponentsFromView.MarkerCanvas = MarkerCanvas;
            _seriesComponentsFromView.ToolTipFormatter = ToolTipFormatter;
            return _seriesComponentsFromView;
        }

        internal void OnSeriesDetached()
        {
            this.ClearInheritanceParent(Model.ActualBrush);
        }

        #region Mouse event overrides

        /// <summary>
        /// Called when the mouse enters the control.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event arguments.</param>
        protected void OnMouseEnter(object sender, MouseEventArgs e)
        {
            Point pt = e.GetPosition(Model);
            object data = e;
            Model.OnMouseEnter(pt, e.OriginalSource, data);
        }

        /// <summary>
        /// Called when the mouse moves over the control.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event arguments.</param>
        protected void OnMouseMove(object sender, MouseEventArgs e)
        {
            Point point = e.GetPosition(Model);
            object data = e;
            Model.OnMouseMove(point, e.OriginalSource, data);
        }

        /// <summary>
        /// Called when the mouse leaves the control.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event arguments.</param>
        protected void OnMouseLeave(object sender, MouseEventArgs e)
        {
            Point pt = e.GetPosition(Model);
            object data = e;
            Model.OnMouseLeave(pt, e.OriginalSource, data);
        }

        private bool CapturedMouse { get; set; }

        /// <summary>
        /// Called wehn the left mouse button is depressed.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event arguments.</param>
        protected void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (Model.CaptureMouse())
            {
                CapturedMouse = true;
            }
            Point pt = e.GetPosition(Model);
            object data = e;
            Model.OnLeftButtonDown(pt, e.OriginalSource, data);
        }

        /// <summary>
        /// Called when the left mouse button is released.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event arguments.</param>
        protected void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (CapturedMouse)
            {
                Model.ReleaseMouseCapture();
                CapturedMouse = false;
            }
            Point pt = e.GetPosition(Model);
            object data = e;
            Model.OnMouseLeftButtonUp(pt, e.OriginalSource, data);
        }

        /// <summary>
        /// Called when the mouse capture has been lost.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event arguments.</param>
        protected void OnLostMouseCapture(object sender, MouseEventArgs e)
        {
            if (CapturedMouse)
            {
                CapturedMouse = false;
                object data = e;
                Point pt = e.GetPosition(Model);

                if (Model.SeriesViewer == null || !Model.SeriesViewer.View.StealingCapture)

                {
                    Model.OnLostMouseCapture(pt, e.OriginalSource, data);
                }
            }
        }


        /// <summary>
        /// Called when the right mouse button is pressed.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event arguments.</param>
        protected void OnMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            Point pt = e.GetPosition(Model);
            object data = e;
            Model.OnRightButtonDown(pt, e.OriginalSource, data);

        }

        /// <summary>
        /// Overridden in derived classes when they want to respond to the right mouse button being released.
        /// </summary>
        /// <param name="sender">The source of this mouse event.</param>
        /// <param name="e">The mouse event arguments.</param>
        protected void OnMouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            Point pt = e.GetPosition(Model);
            object data = e;
            Model.OnRightButtonUp(pt, e.OriginalSource, data);
        }


        #endregion

        private void ClearInheritanceParent(DependencyObject problemObject)
        {
            new Dummy().Nothing = Model.ActualBrush;
        }

        private class Dummy : DependencyObject
        {
            internal static readonly DependencyProperty DummyProperty = DependencyProperty.Register("Dummy", typeof(object), typeof(Dummy), null);
            internal object Nothing
            {
                get
                {
                    return this.GetValue(DummyProperty);
                }
                set
                {
                    this.SetValue(DummyProperty, value);
                }
            }
        }

        internal DataContext GetDataContextFromSender(object sender)
        {
            FrameworkElement frameworkElement = sender as FrameworkElement;
            DataContext dataContext = frameworkElement != null ? frameworkElement.DataContext as DataContext : null;
            return dataContext;
        }

        internal void ResetActualBrush()
        {
            Model.ClearValue(Series.ActualBrushProperty);
        }

        internal void BindActualToUserBrush()
        {
            Model.SetBinding(Series.ActualBrushProperty, new Binding(Series.BrushPropertyName) { Source = Model });
        }

        internal void ResetActualOutlineBrush()
        {
            Model.ClearValue(Series.ActualOutlineProperty);
        }

        internal void BindActualToUserOutline()
        {
            Model.SetBinding(Series.ActualOutlineProperty, new Binding(Series.OutlinePropertyName) { Source = Model });
        }



#region Infragistics Source Cleanup (Region)











#endregion // Infragistics Source Cleanup (Region)


        internal void UpdateToolTip(Point pt, object item, object data)
        {
            ChartToolTipManager.UpdateToolTip(pt, item, data);
        }

        /// <summary>
        /// Gets the viewport for the series.
        /// </summary>
        protected internal Rect Viewport { get; set; }

        /// <summary>
        /// Gets the current zoom label for the series.
        /// </summary>
        protected internal virtual Rect WindowRect
        {
            get
            {
                if (this.IsThumbnailView)
                {
                    return XamDataChart.StandardRect;
                }
                else
                {
                    return this.Model.SeriesViewer != null ? this.Model.SeriesViewer.ActualWindowRect : Rect.Empty;
                }
            }
        }
        internal bool IsThumbnailView { get; set; }

        internal void DetachFromChart(SeriesViewer oldSeriesViewer)
        {
            
        }

        internal void AttachToChart(SeriesViewer newSeriesViewer)
        {
           
        }

        private Canvas _thumbnailCanvas = null;

        internal virtual void PrepSurface(RenderSurface surface)
        {
            if (_thumbnailCanvas == null)
            {
                _thumbnailCanvas = new Canvas();
                Model.ThumbnailView.AttachUI(_thumbnailCanvas);
            }
            if (surface.Surface == null)
            {
                surface.Surface = new Canvas();
            }
            _thumbnailCanvas.Detach();
            surface.Surface.Children.Add(_thumbnailCanvas);
        }
        
        /// <summary>
        /// Exports visual information about the series for use by external tools and functionality.
        /// </summary>
        /// <param name="svd">The data container.</param>
        protected internal virtual void ExportViewShapes(SeriesVisualData svd)
        {

        }

        internal void OnLegendItemVisibilityChanged()
        {

        }

        internal void OnTitlePropertyChanged()
        {

        }

        /// <summary>
        /// Gets the view info for the series.
        /// </summary>
        /// <param name="viewportRect">The viewport to use.</param>
        /// <param name="windowRect">The window to use.</param>
        public void GetViewInfo(out Rect viewportRect, out Rect windowRect)
        {
            viewportRect = this.Viewport;
            windowRect = this.WindowRect;
        }
        internal void SetZIndex(int value)
        {
            Canvas.SetZIndex(this.RootCanvas, value);
        }

        internal bool HasSurface()
        {
            return RootCanvas != null;
        }
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