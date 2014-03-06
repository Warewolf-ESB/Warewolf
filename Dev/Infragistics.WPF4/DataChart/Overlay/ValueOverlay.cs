using System;
using System.Collections.Generic;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Data;
using System.ComponentModel;

namespace Infragistics.Controls.Charts
{
    /// <summary>
    /// Represents the class of the value overlay. The value overlay is a line or circle representing a value on an axis.
    /// </summary>
    public class ValueOverlay : Series, IHasCategoryModePreference
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="ValueOverlay"/> class.
        /// </summary>
        public ValueOverlay()
            : base()
        {
            this.DefaultStyleKey = typeof(ValueOverlay);
            
           

        }

        #endregion //Constructor

        #region Properties

        #region Public

        #region Axis

        /// <summary>
        /// Gets or sets the axis used by the value overlay.
        /// </summary>
        public Axis Axis
        {
            get { return (Axis)GetValue(AxisProperty); }
            set { SetValue(AxisProperty, value); }
        }

        private const string AxisPropertyName = "Axis";

        /// <summary>
        /// Identifies the Axis dependency property.
        /// </summary>
        public static readonly DependencyProperty AxisProperty =
            DependencyProperty.Register(AxisPropertyName, typeof(Axis), typeof(ValueOverlay),
            new PropertyMetadata(null, (sender, e) =>
                    {
                        (sender as ValueOverlay).RaisePropertyChanged(AxisPropertyName, e.OldValue, e.NewValue);
                    }));

        #endregion

        #region Value

        /// <summary>
        /// Gets or sets the value of the overlay.
        /// </summary>
        /// <value>The value.</value>
        public double Value
        {
            get { return (double)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        private const string ValuePropertyName = "Value";

        /// <summary>
        /// Ideitifies the Value dependency property.
        /// </summary>
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register(ValuePropertyName, typeof(double), typeof(ValueOverlay),
            new PropertyMetadata(0.0, (sender, e) =>
                    {
                        (sender as ValueOverlay).RaisePropertyChanged(ValuePropertyName, e.OldValue, e.NewValue);
                    }));

        #endregion
        
        #endregion //Public

        #region Internal

        #region Transitions and Rendering
        internal double PreviousValue;
        internal double TransitionValue;
        internal double CurrentValue;
        #endregion //Transitions and Rendering

        #endregion //Internal

        #endregion //Properties

        #region Overrides

        #region GetItemIndex
        /// <summary>
        /// Gets the item item index associated with the specified world position
        /// </summary>
        /// <param name="world"></param>
        /// <returns>
        /// Item index or -1 if no item is assocated with the specified world position.
        /// </returns>
        protected override int GetItemIndex(Point world)
        {
            throw new NotImplementedException();
        }
        #endregion //GetItemIndex

        #region GetItem
        /// <summary>
        /// Gets the item that is the best match for the specified world coordinates.
        /// </summary>
        /// <param name="world">The world coordinates to use.</param>
        /// <returns>The item that is the best match.</returns>
        protected override object GetItem(Point world)
        {
            return this;
        }
        #endregion //GetItem

        #region ScrollIntoView
        /// <summary>
        /// Requests that the provided item should be brought into view if possible.
        /// </summary>
        /// <param name="item">The item to attempt to bring into view.</param>
        /// <returns></returns>
        public override bool ScrollIntoView(object item)
        {
            throw new NotImplementedException();
        }
        #endregion //ScrollIntoView

        #region GetRange
        /// <summary>
        /// Returns the range requirement of this series on the given axis.
        /// </summary>
        /// <param name="axis">The axis for which to provide the range requirement.</param>
        /// <returns>The axis range needed for this series to fully display, or null, if there is no range requirement.</returns>
        protected internal override AxisRange GetRange(Axis axis)
        {
            NumericAxisBase numericAxis = axis as NumericAxisBase;
            //NO, not correct.
            //if (numericAxis != null)
            //{
            //    if (numericAxis.ActualMinimumValue > this.Value)
            //    {
            //        return new AxisRange(this.Value, numericAxis.ActualMaximumValue);
            //    }
            //    else if(numericAxis.ActualMaximumValue < this.Value)
            //    {
            //        return new AxisRange(numericAxis.ActualMinimumValue, this.Value);
            //    }
            //    else
            //    {
            //        return new AxisRange(numericAxis.ActualMinimumValue, numericAxis.ActualMaximumValue);
            //    }
            //}
            //else
            //{
            var rangeValue = this.Value;
            if (double.IsNaN(rangeValue) ||
                double.IsPositiveInfinity(rangeValue) ||
                double.IsNegativeInfinity(rangeValue))
            {
                return null;
            }

            return new AxisRange(rangeValue, rangeValue);
            //}
        }
        #endregion GetRange

        #region RenderSeriesOverride
        /// <summary>
        /// Renders the series override.
        /// </summary>
        /// <param name="animate">if set to <c>true</c> [animate].</param>
        protected internal override void RenderSeriesOverride(bool animate)
        {
            this.ClearRendering(true, View);
            if(!ShouldRenderOverlay())
            {
                return;
            }

            if (ShouldAnimate(animate))
            {
                if (AnimationActive())
                {
                    double temp = PreviousValue;
                    PreviousValue = TransitionValue;
                    TransitionValue = temp;
                }
                else
                {
                    PreviousValue = CurrentValue;
                }
                CurrentValue = PrepareValue();
                StartupStoryBoard();
            }
            else
            {
                CurrentValue = PrepareValue();
                RenderValue(CurrentValue, this.ValueOverlayView);
            }
        }
        #endregion //RenderSeriesOverride

        #region PrepareValue
        internal double PrepareValue()
        {
            Rect windowRect, viewportRect;
            this.GetViewInfo(out viewportRect, out windowRect);
            ScalerParams sParams = new ScalerParams(windowRect, viewportRect, this.Axis.IsInverted);
            return this.Axis.GetScaledValue(this.Value, sParams);
        }
        #endregion

        #region RenderValue
        internal void RenderValue(double scaledValue, ValueOverlayView view)
        {
            if (this.Axis == null)
            {
                return;
            }
            CategoryAxisBase categoryAxis = this.Axis as CategoryAxisBase;
            if (categoryAxis != null)
            {
                // if this is missing the scaledValue is set to 0.5 when having 1 point
                if (this.Value > categoryAxis.ItemsCount - 1)
                {
                    return;
                }

                if (categoryAxis.CategoryMode == CategoryMode.Mode2)
                {
                    double offset = 0.5 * categoryAxis.GetCategorySize(view.WindowRect, view.Viewport);
                    bool isInverted = false;

                    if (categoryAxis is CategoryYAxis && !categoryAxis.IsInverted)
                        isInverted = true;

                    if (categoryAxis is CategoryXAxis && categoryAxis.IsInverted)
                        isInverted = true;

                    if (isInverted) offset = -offset;

                    scaledValue += offset;
                }
            }

            NumericAxisBase numericAxis = this.Axis as NumericAxisBase;
            if (numericAxis != null)
            {
                if (this.Value < numericAxis.ActualMinimumValue || this.Value > numericAxis.ActualMaximumValue)
                {
                    return;
                }

                if (numericAxis.IsReallyLogarithmic && this.Value < 0)
                {
                    return;
                }
            }

            switch (this.Axis.Orientation)
            {
                case AxisOrientation.Horizontal:
                    RenderHorizontalAxisOverlay(scaledValue, view);
                    break;
                case AxisOrientation.Vertical:
                    RenderVerticalAxisOverlay(scaledValue, view);
                    break;
                case AxisOrientation.Angular:
                    RenderAngularAxisOverlay(scaledValue, view);
                    break;
                case AxisOrientation.Radial:
                    RenderRadialAxisOverlay(scaledValue, view);
                    break;
            }
        }
        #endregion //RenderValue

        #region ValidateSeries
        /// <summary>
        /// Checks if the series is valid to be rendered.
        /// </summary>
        /// <param name="viewportRect">The current viewport, a rectangle with bounds equivalent to the screen size of the series.</param>
        /// <param name="windowRect">The current window, a rectangle bounded between 0 and 1 representing the pan and zoom position.</param>
        /// <param name="view">The SeriesView in context.</param>
        /// <returns>True if the series is valid to be rendered, otherwise false.</returns>
        protected internal override bool ValidateSeries(Rect viewportRect, Rect windowRect, SeriesView view)
        {
            bool isValid = true;

            isValid = base.ValidateSeries(viewportRect, windowRect, view);

            if (!view.HasSurface()
                || windowRect.IsEmpty
                || viewportRect.IsEmpty
                || Axis == null
                || Axis.SeriesViewer == null)
            {
                isValid = false;
            }

            return isValid;
        }
        #endregion //ValidateSeries

        #region WindowRectChangedOverride
        /// <summary>
        /// When overridden in a derived class, is invoked whenever the series window rectangle
        /// is changed.
        /// </summary>
        /// <param name="oldWindowRect">Old window rectangle in normalised world coordinates.</param>
        /// <param name="newWindowRect">New window rectangle in normalised world coordinates.</param>
        protected override void WindowRectChangedOverride(Rect oldWindowRect, Rect newWindowRect)
        {
            this.RenderSeries(false);
        }
        #endregion //WindowRectChangedOverride

        #region ViewportRectChangedOverride
        /// <summary>
        /// When overridden in a derived class, is invoked whenever the series viewport rectangle
        /// is changed.
        /// </summary>
        /// <param name="oldViewportRect">Old viewport rectangle in device coordinates.</param>
        /// <param name="newViewportRect">New viewport rectangle in device coordinates.</param>
        protected override void ViewportRectChangedOverride(Rect oldViewportRect, Rect newViewportRect)
        {
            this.RenderSeries(false);
        }
        #endregion //ViewportRectChangedOverride

        #region DataUpdatedOverride
        /// <summary>
        /// When overridden in a derived class, DataChangedOverride is called whenever a change is made to
        /// the series data.
        /// </summary>
        /// <param name="action">The action performed on the data</param>
        /// <param name="position">The index of the first item involved in the update.</param>
        /// <param name="count">The number of items in the update.</param>
        /// <param name="propertyName">The name of the updated property.</param>
        protected override void DataUpdatedOverride(FastItemsSourceEventAction action, int position, int count, string propertyName)
        {
            throw new NotImplementedException();
        }
        #endregion //DataUpdatedOverride

        #region PropertyUpdatedOverride
        /// <summary>
        /// When overridden in a derived class, gets called whenever a property value is updated
        /// on the series or owning chart. Gives the series a chance to respond to the various property updates.
        /// </summary>
        /// <param name="sender">The object being updated.</param>
        /// <param name="propertyName">The name of the property being updated.</param>
        /// <param name="oldValue">The old value of the property.</param>
        /// <param name="newValue">The new value of the property.</param>
        protected override void PropertyUpdatedOverride(object sender, string propertyName, object oldValue, object newValue)
        {
            base.PropertyUpdatedOverride(sender, propertyName, oldValue, newValue);
            switch (propertyName)
            {
                case AxisPropertyName:
                    if (oldValue != null)
                    {
                        (oldValue as Axis).DeregisterSeries(this);
                    }

                    if (newValue != null)
                    {
                        (newValue as Axis).RegisterSeries(this);
                    }

                    this.RenderSeries(false);
                    this.NotifyThumbnailAppearanceChanged();
                    break;
                case TransitionProgressPropertyName:
                    TransitionValue = PreviousValue + TransitionProgress * (CurrentValue - PreviousValue);

                    if (ClearAndAbortIfInvalid(View))
                    {
                        return;
                    }

                    if (TransitionProgress == 1.0)
                    {
                        RenderValue(CurrentValue, this.ValueOverlayView);
                    }
                    else
                    {
                        RenderValue(TransitionValue, this.ValueOverlayView);
                    }

                    break;
                case ValuePropertyName:
                    NumericAxisBase numericAxis = this.Axis as NumericAxisBase;
                    if (numericAxis != null)
                    {
                        numericAxis.UpdateRange();
                    }
                    this.RenderSeries(true);
                    this.NotifyThumbnailDataChanged();
                    break;
            }
            base.PropertyUpdatedOverride(sender, propertyName, oldValue, newValue);
        }
        #endregion //PropertyUpdatedOverride

        #region Axes
        /// <summary>
        /// Invalidates the axes that use this overlay.
        /// </summary>
        protected internal override void InvalidateAxes()
        {
            base.InvalidateAxes();
            if (this.Axis != null)
            {
                this.Axis.RenderAxis(false);
            }
        }
        #endregion //Axes

        #region ClearRendering
        /// <summary>
        /// Clears the rendering for the series.
        /// </summary>
        /// <param name="wipeClean">True if the cached visuals should also be cleared.</param>
        /// <param name="view">The SeriesView in context.</param>
        protected internal override void ClearRendering(bool wipeClean, SeriesView view)
        {
            base.ClearRendering(wipeClean, view);

            var overlayView = (ValueOverlayView)view;
            overlayView.ClearRendering();
        }
        #endregion //ClearRendering

        #endregion //Overrides

        #region Private methods

        private void RenderHorizontalAxisOverlay(double scaledValue, ValueOverlayView view)
        {
            LineGeometry lineGeometry = new LineGeometry();

            lineGeometry.StartPoint = new Point(scaledValue, view.Viewport.Bottom);
            lineGeometry.EndPoint = new Point(scaledValue, view.Viewport.Top);
            
            view.SetPathData(lineGeometry);
        }

        private void RenderVerticalAxisOverlay(double scaledValue, ValueOverlayView view)
        {
            LineGeometry lineGeometry = new LineGeometry();

            lineGeometry.StartPoint = new Point(view.Viewport.Left, scaledValue);
            lineGeometry.EndPoint = new Point(view.Viewport.Right, scaledValue);

            view.SetPathData(lineGeometry);
        }

        private void RenderAngularAxisOverlay(double scaledValue, ValueOverlayView view)
        {
            if (this.Axis is NumericAngleAxis)
            {
                NumericAngleAxis numericAngleAxis = this.Axis as NumericAngleAxis;
                if (numericAngleAxis.RadiusAxis == null)
                {
                    return;
                }

                if ((numericAngleAxis.MinimumValue != double.NaN && numericAngleAxis.MinimumValue > this.Value) ||
                    (numericAngleAxis.MaximumValue != double.NaN && numericAngleAxis.MaximumValue < this.Value))
                {
                    return;
                }
                 
                PolarAxisRenderingParameters renderParams = numericAngleAxis.CreateRenderingParams(view.Viewport, view.WindowRect) as PolarAxisRenderingParameters;
                if (renderParams == null)
                {
                    return;
                }

                double angle = scaledValue;

                RenderAngularAxisOverlay(angle, renderParams.Center, renderParams.MinLength, renderParams.MaxLength, view);
            }
            else if (this.Axis is CategoryAngleAxis)
            {
                CategoryAngleAxis categoryAngleAxis = this.Axis as CategoryAngleAxis;
                RadialAxisRenderingParameters renderParams = categoryAngleAxis.Renderer.CreateRenderingParams(view.Viewport, view.WindowRect) as RadialAxisRenderingParameters;
                if (renderParams == null)
                {
                    return;
                }

                double angle = scaledValue;

                RenderAngularAxisOverlay(angle, renderParams.Center, renderParams.MinLength, renderParams.MaxLength, view);

            }

            
        }

        private void RenderAngularAxisOverlay(double angle, Point center, double minLength, double maxLength, ValueOverlayView view)
        {
            double cosX = Math.Cos(angle);
            double sinX = Math.Sin(angle);

            //determine the x and y of the start and end extent of the line.
            double startX = center.X + cosX * minLength;
            double startY = center.Y + sinX * minLength;
            double endX = center.X + cosX * maxLength;
            double endY = center.Y + sinX * maxLength;

            startX = ViewportUtils.TransformXToViewport(startX, view.WindowRect, view.Viewport);
            startY = ViewportUtils.TransformYToViewport(startY, view.WindowRect, view.Viewport);
            endX = ViewportUtils.TransformXToViewport(endX, view.WindowRect, view.Viewport);
            endY = ViewportUtils.TransformYToViewport(endY, view.WindowRect, view.Viewport);

            LineGeometry lineGeometry = new LineGeometry();

            lineGeometry.StartPoint = new Point(startX, startY);
            lineGeometry.EndPoint = new Point(endX, endY);

            view.SetPathData(lineGeometry);
        }

        private void RenderRadialAxisOverlay(double scaledValue, ValueOverlayView view)
        {
            NumericRadiusAxis radiusAxis = this.Axis as NumericRadiusAxis;
            PolarAxisRenderingParameters renderParams = radiusAxis.CreateRenderingParams(view.Viewport, view.WindowRect) as PolarAxisRenderingParameters;

            double radius = scaledValue;

            PathGeometry pathGeometry = new PathGeometry();

            double radiusX = ViewportUtils.TransformXToViewportLength(radius, view.WindowRect, view.Viewport);
            double radiusY = ViewportUtils.TransformYToViewportLength(radius, view.WindowRect, view.Viewport);

            if (radiusX <= 0 || radiusY <= 0)
                return;

            double centerX = ViewportUtils.TransformXToViewport(renderParams.Center.X, view.WindowRect, view.Viewport);
            double centerY = ViewportUtils.TransformYToViewport(renderParams.Center.Y, view.WindowRect, view.Viewport);

            if (renderParams.MaxAngle - renderParams.MinAngle < Math.PI &&
                renderParams.MaxAngle - renderParams.MinAngle > 0)
            {
                Point startPoint = new Point(
                    ViewportUtils.TransformXToViewport(renderParams.Center.X +
                        radius * Math.Cos(renderParams.MinAngle), view.WindowRect, view.Viewport),
                    ViewportUtils.TransformYToViewport(renderParams.Center.Y +
                        radius * Math.Sin(renderParams.MinAngle), view.WindowRect, view.Viewport));

                Point endPoint = new Point(
                    ViewportUtils.TransformXToViewport(renderParams.Center.X +
                        radius * Math.Cos(renderParams.MaxAngle), view.WindowRect, view.Viewport),
                    ViewportUtils.TransformYToViewport(renderParams.Center.Y +
                        radius * Math.Sin(renderParams.MaxAngle), view.WindowRect, view.Viewport));

                PathFigure pf = new PathFigure();
                pf.StartPoint = startPoint;
                pf.IsClosed = false;
                pf.Segments.Add(new ArcSegment()
                {
                    IsLargeArc = false,
                    Point = endPoint,
                    Size = new Size(radiusX, radiusY),
                    SweepDirection = SweepDirection.Clockwise
                });
                pathGeometry.Figures.Add(pf);
            }
            else
            {
                PathFigure pf = new PathFigure();
                pf.StartPoint = new Point(centerX, centerY - radiusY);
                pf.IsClosed = true;
                pf.Segments.Add(new ArcSegment()
                {
                    IsLargeArc = false,
                    Point = new Point(centerX, centerY + radiusY),
                    Size = new Size(radiusX, radiusY),
                    SweepDirection = SweepDirection.Clockwise
                });
                pf.Segments.Add(new ArcSegment()
                {
                    IsLargeArc = false,
                    Point = new Point(centerX, centerY - radiusY),
                    Size = new Size(radiusX, radiusY),
                    SweepDirection = SweepDirection.Clockwise
                });

                pathGeometry.Figures.Add(pf);
            }

            view.SetPathData(pathGeometry);
        }

        bool ShouldRenderOverlay()
        {
            if (this.Axis == null ||
                this.Axis.SeriesViewer == null ||
                double.IsNaN(this.Value) || this.Visibility == Visibility.Collapsed)
            {
                return false;
            }
            CategoryAxisBase categoryAxis = this.Axis as CategoryAxisBase;
            if (categoryAxis != null && categoryAxis.ItemsCount == 0)
            {
                return false;
            }

            return true;
        }

        #endregion

        #region IHasCategoryModePreference

        /// <summary>
        /// Gets the preferred category mode.
        /// </summary>
        /// <param name="axis">The axis.</param>
        /// <returns></returns>
        CategoryMode IHasCategoryModePreference.PreferredCategoryMode(CategoryAxisBase axis)
        {
            return CategoryMode.Mode0;
        }

        /// <summary>
        /// Gets the category axis.
        /// </summary>
        /// <value>The category axis.</value>
        CategoryAxisBase IHasCategoryAxis.CategoryAxis
        {
            get
            {
                return this.Axis as CategoryAxisBase;
            }
        }

        #endregion //IHasCategoryModePreference
        /// <summary>
        /// Called to create the view for the series.
        /// </summary>
        /// <returns>The created view.</returns>
        protected override SeriesView CreateView()
        {
            return new ValueOverlayView(this);
        }
        /// <summary>
        /// Called when the view for the series has been created.
        /// </summary>
        /// <param name="view">The created view.</param>
        protected internal override void OnViewCreated(SeriesView view)
        {
            base.OnViewCreated(view);
            this.ValueOverlayView = view as ValueOverlayView;
        }
        private ValueOverlayView ValueOverlayView { get; set; }
        /// <summary>
        /// Called when the series needs to render a thumbnail view.
        /// </summary>
        /// <param name="viewportRect">The viewport to use.</param>
        /// <param name="surface">The surface to attach the view to.</param>
        protected internal override void RenderThumbnail(Rect viewportRect, RenderSurface surface)
        {
            base.RenderThumbnail(viewportRect, surface);

            if (!ThumbnailDirty)
            {
                View.PrepSurface(surface);
                return;
            }

            this.ThumbnailView.PrepSurface(surface);

            if (ClearAndAbortIfInvalid(ThumbnailView) || !this.ShouldRenderOverlay())
            {
                return;
            }

            double scaledValue = this.Axis.GetScaledValue(this.Value, new ScalerParams(this.ThumbnailView.WindowRect, viewportRect, this.Axis.IsInverted));
            this.RenderValue(scaledValue, this.ThumbnailView as ValueOverlayView);

            ThumbnailDirty = false;
        }
    }
    internal class ValueOverlayView : SeriesView
    {
        internal ValueOverlayView(ValueOverlay model)
            : base(model)
        {
            this.Path = new Path();
        }
        private Path Path { get; set; }
        public override void AttachUI(Canvas rootCanvas)
        {
            base.AttachUI(rootCanvas);
            this.Path.SetBinding(Shape.StrokeProperty, new Binding(Series.ActualBrushPropertyName) { Source = this.Model });
            this.Path.SetBinding(Shape.StrokeThicknessProperty, new Binding(Series.ThicknessPropertyName) { Source = this.Model });
            this.Path.SetBinding(Shape.StrokeDashCapProperty, new Binding(Series.DashCapPropertyName) { Source = this.Model });
            this.Path.SetBinding(Shape.StrokeDashArrayProperty, new Binding(Series.DashArrayPropertyName)
            {
                Source = this.Model,
                Converter = new DoubleCollectionDuplicator()
            });
            this.Path.SetBinding(Shape.StrokeLineJoinProperty, new Binding(Series.LineJoinPropertyName) { Source = this.Model });
            this.Path.SetBinding(Shape.StrokeMiterLimitProperty, new Binding(Series.MiterLimitPropertyName) { Source = this.Model });
            this.Path.SetBinding(Shape.StrokeStartLineCapProperty, new Binding(Series.StartCapPropertyName) { Source = this.Model });
            this.Path.SetBinding(Shape.StrokeEndLineCapProperty, new Binding(Series.EndCapPropertyName) { Source = this.Model });
            this.Path.SetBinding(Shape.StrokeLineJoinProperty, new Binding(Series.LineJoinPropertyName) { Source = this.Model });
            if (rootCanvas != null && rootCanvas.Children != null && !rootCanvas.Children.Contains(this.Path))
            {
                rootCanvas.Children.Add(this.Path);
            }
        }
        internal void SetPathData(Geometry data)
        {
            this.Path.Data = data;
        }
        internal void ClearRendering()
        {
            this.Path.Data = null;
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