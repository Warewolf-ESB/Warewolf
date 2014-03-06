using System;



using System.Linq;

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
using System.Runtime.CompilerServices;
using System.Diagnostics;

namespace Infragistics.Controls.Charts
{
    /// <summary>
    /// Represents a XamDataChart radius axis for polar and radial series.
    /// </summary>
    [WidgetModule("PolarChart")]
    [WidgetModule("RadialChart")]
    [WidgetIgnoreDepends("XamDataChart")]
    [WidgetIgnoreDepends("NumericAngleAxis")]
    [WidgetIgnoreDepends("CategoryAngleAxis")]
    public class NumericRadiusAxis : NumericAxisBase
    {
        internal override AxisView CreateView()
        {
            return new NumericRadiusAxisView(this);
        }
        internal override void OnViewCreated(AxisView view)
        {
            base.OnViewCreated(view);

            NumericRadiusView = (NumericRadiusAxisView)view;
        }
        internal NumericRadiusAxisView NumericRadiusView { get; set; }

        private PolarAxisRenderingManager renderingManager;

        #region Constructor and Initialisation
        /// <summary>
        /// Constructs a numeric radius axis.
        /// </summary>
        public NumericRadiusAxis()
        {
            ActualRadiusExtentScale = RadiusExtentScale;
            ActualInnerRadiusExtentScale = InnerRadiusExtentScale;
            renderingManager = new PolarAxisRenderingManager();
            Renderer = CreateRenderer();
        }       
        #endregion

        internal override AxisLabelPanelBase CreateLabelPanel()
        {
            return new RadialAxisLabelPanel();
        }

        internal bool Suppress { get; set; }

        private double ConvertToDouble(object x)
        {


#region Infragistics Source Cleanup (Region)





#endregion // Infragistics Source Cleanup (Region)

            return Convert.ToDouble(x);

        }

        private double _lastCrossing = double.NaN;

        /// <summary>
        /// Creates the rendering provider for the axis.
        /// </summary>
        /// <returns>The axis renderer.</returns>
        internal override NumericAxisRenderer CreateRenderer()
        {
            NumericAxisRenderer renderer = base.CreateRenderer();

            renderer.LabelManager.FloatPanelAction = (crossing) =>
            {
                if ((this.LabelSettings == null || this.LabelSettings.Visibility == Visibility.Visible) && this.AngleAxis != null)
                {
                    if ((this.LabelSettings == null || (this.LabelSettings.ActualLocation == AxisLabelsLocation.InsideTop || this.LabelSettings.ActualLocation == AxisLabelsLocation.InsideBottom))
                        && _lastCrossing != crossing)
                    {
                        _lastCrossing = crossing;
                        this.LabelPanel.CrossingValue = crossing;
                        this.SeriesViewer.InvalidatePanels();
                    }
                }
            };

            renderer.Line = (p, g, value) =>
                {
                    PolarAxisRenderingParameters r = p as PolarAxisRenderingParameters;
                    renderingManager.ConcentricLine(g, value,
                        r.ViewportRect,
                        r.WindowRect,
                        r.Center, r.MinAngle, r.MaxAngle);
                };

            renderer.Strip = (p, g, start, end) =>
                {
                    if (start == end)
                    {
                        return;
                    }
                    PolarAxisRenderingParameters r = p as PolarAxisRenderingParameters;
                    renderingManager.ConcentricStrip(g, start,
                        end, r.ViewportRect,
                        r.WindowRect, r.Center, r.MinAngle, r.MaxAngle);
                };

            renderer.Scaling = (p, unscaled) =>
                {
                    return GetScaledValue(unscaled);
                };

            renderer.ShouldRenderLines = (p, value) =>
                {
                    PolarAxisRenderingParameters r = p as PolarAxisRenderingParameters;
                    return value <= r.EffectiveMaximum;
                };

            renderer.ShouldRenderContent = (p, value) =>
                {
                    PolarAxisRenderingParameters r = p as PolarAxisRenderingParameters;
                    return value <= r.EffectiveMaximum;
                };

            renderer.AxisLine = (p) =>
                {
                    //double effectiveMaximum = GetEffectiveMaximumLength(p);
                    PolarAxisRenderingParameters r = p as PolarAxisRenderingParameters;
                    renderingManager.RadialLine(p.AxisGeometry, r.CrossingAngleRadians, p.ViewportRect,
                        p.WindowRect, r.MinLength,
                        r.MaxLength,
                        r.Center);
                };

            renderer.DetermineCrossingValue = (p) =>
                {
                    p.CrossingValue = this.LabelSettings == null || (this.LabelSettings.ActualLocation == AxisLabelsLocation.InsideTop || this.LabelSettings.ActualLocation == AxisLabelsLocation.OutsideTop)
                    ? p.ViewportRect.Top : p.ViewportRect.Bottom;

                    PolarAxisRenderingParameters r = p as PolarAxisRenderingParameters;
                    r.CrossingAngleRadians = (ConvertToDouble(CrossingValue) * Math.PI) / 180.0;

                    if (this.LabelSettings == null || (this.LabelSettings.ActualLocation == AxisLabelsLocation.InsideTop || this.LabelSettings.ActualLocation == AxisLabelsLocation.InsideBottom))
                    {
                        r.CrossingValue = ViewportUtils.TransformYToViewport(0.5, r.WindowRect, r.ViewportRect) - p.ViewportRect.Top;
                        
                        RadialAxisLabelPanel panel = this.LabelPanel as RadialAxisLabelPanel;
                        if (panel != null)
                        {
                            double yVal = 0;
                            if (LabelSettings != null && LabelSettings.ActualLocation == AxisLabelsLocation.InsideTop)
                            {
                                yVal = 1;
                            }
                            panel.RotationCenter =
                                new Point(
                                    ViewportUtils.TransformXToViewport
                                    (0.5, r.WindowRect, r.ViewportRect),
                                    yVal);
                            panel.CrossingAngle = r.CrossingAngleRadians;
                        }
                    }
                };

            renderer.ShouldRenderLabel = (p, v, last) =>
                {
                    PolarAxisRenderingParameters r = p as PolarAxisRenderingParameters;

                    if (AngleAxis == null)
                    {
                        return false;
                    }

                    if (v > r.EffectiveMaximum)
                    {
                        return false;
                    }

                    bool embedded = false;
                    embedded = LabelSettings == null || (LabelSettings.ActualLocation == AxisLabelsLocation.InsideTop || LabelSettings.ActualLocation == AxisLabelsLocation.InsideBottom);
                    double panelAngle = 0.0;
                    if (embedded)
                    {
                        panelAngle = CrossingValueRadians;
                    }

                    double x = r.Center.X +
                        v * Math.Cos(panelAngle);
                    double y = r.Center.Y +
                        v * Math.Sin(panelAngle);

                    x = ViewportUtils.TransformXToViewport(x,
                        r.WindowRect, r.ViewportRect);
                    y = ViewportUtils.TransformYToViewport(y,
                        r.WindowRect, r.ViewportRect);


                    if (x <= p.ViewportRect.Right &&
                        x >= p.ViewportRect.Left &&
                        ((y <= p.ViewportRect.Bottom &&
                        y >= p.ViewportRect.Top) || !embedded))
                    {
                        return true;
                    }
                    return false;
                };

            renderer.GetLabelLocation = (p, value) =>
                {
                    PolarAxisRenderingParameters r = 
                        p as PolarAxisRenderingParameters;

                    return new LabelPosition(
                        ViewportUtils.TransformXToViewport(
                        r.Center.X + value,
                        r.WindowRect, r.ViewportRect));
                };

            renderer.SnapMajorValue = (p, value, i, interval) =>
            {
                if (value < p.ActualMinimumValue)
                {
                    return p.ActualMinimumValue;
                }
                else if (value > p.ActualMaximumValue)
                {
                    return p.ActualMaximumValue;
                }

                return value;
            };

            return renderer;
        }

        /// <summary>
        /// Gets the window length for the provided radius.
        /// </summary>
        /// <param name="unscaledValue">The radius for which to get the window length.</param>
        /// <param name="p">Scaler parameters</param>
        /// <returns></returns>
        public override double GetScaledValue(double unscaledValue, ScalerParams p)
        {
            return GetScaledValue(unscaledValue);
        }


        internal double GetScaledValue(double unscaledValue, bool isLogarithmic, bool isInverted, double radiusExtentScale, double innerRadiusExtentScale)
        {
            double scaledValue = 0;

            if (isLogarithmic)
            {
                if (unscaledValue <= 0)
                {
                    scaledValue = (Math.Log(ActualMinimumValue) - logActualMinimumValue) / (logActualMaximumValue - logActualMinimumValue);
                }
                else
                {
                    scaledValue = (Math.Log(unscaledValue) - logActualMinimumValue) / (logActualMaximumValue - logActualMinimumValue);
                }
            }
            else
            {
                scaledValue = (unscaledValue - ActualMinimumValue) / (ActualMaximumValue - ActualMinimumValue);
            }

            if (isInverted)
            {
                scaledValue = 1.0 - scaledValue;
            }

            scaledValue = innerRadiusExtentScale + (scaledValue * (radiusExtentScale - innerRadiusExtentScale));
            // a radius should only take up, at most, half of the worldspace.
            scaledValue /= 2.0;

            return scaledValue;
        }

        /// <summary>
        /// Returns a world coordinates radius length (0 - 0.5) from a raw axis value.
        /// </summary>
        /// <param name="unscaledValue">The raw axis value.</param>
        /// <returns>The world coordinates radius value.</returns>
        public double GetScaledValue(double unscaledValue)
        {
            return GetScaledValue(unscaledValue, IsReallyLogarithmic, IsInvertedCached, ActualRadiusExtentScale, ActualInnerRadiusExtentScale);
        }

        /// <summary>
        /// Returns a raw axis value from the world coordinates radius length provided.
        /// </summary>
        /// <param name="scaledValue">The scaled world coordinates radius length.</param>
        /// <returns>The raw axis value.</returns>
        public double GetUnscaledValue(double scaledValue)
        {
            double unscaledValue = scaledValue * 2.0;

            unscaledValue = (unscaledValue - ActualInnerRadiusExtentScale) / (ActualRadiusExtentScale - ActualInnerRadiusExtentScale);

            if (IsInverted)
            {
                unscaledValue = 1.0 - unscaledValue;
            }

            if (IsReallyLogarithmic)
            {
                return Math.Exp(unscaledValue * (logActualMaximumValue - logActualMinimumValue) + logActualMinimumValue); 
            }
            else
            {
                return ActualMinimumValue + unscaledValue * (ActualMaximumValue - ActualMinimumValue);
            }
        }

        //#region AngleAxis Dependency Property
        ///// <summary>
        ///// Links to the paired Angle Axis for this radius axis.
        ///// <para>This is a dependency property.</para>
        ///// </summary>
        ///// <remarks>
        ///// The Radius axis must always be paired with an angle axis for it to perform its functions.
        ///// The usual way of specifying this pairing is with a binding.
        ///// </remarks>
        //public NumericAngleAxis AngleAxis
        //{
        //    get
        //    {
        //        return (NumericAngleAxis)GetValue(AngleAxisProperty);
        //    }
        //    set
        //    {
        //        SetValue(AngleAxisProperty, value);
        //    }
        //}

        //internal const string AngleAxisPropertyName = "AngleAxis";

        ///// <summary>
        ///// Identifies the AngleAxis dependency property.
        ///// </summary>
        //public static readonly DependencyProperty AngleAxisProperty = DependencyProperty.Register(AngleAxisPropertyName, typeof(NumericAngleAxis), typeof(NumericRadiusAxis),
        //    new PropertyMetadata(null, (sender, e) =>
        //    {
        //        (sender as NumericRadiusAxis).RaisePropertyChanged(AngleAxisPropertyName, e.OldValue, e.NewValue);
        //    }));
        //#endregion

        #region RadiusExtentScale Dependency Property
        /// <summary>
        /// Defines the percentage of the maximum radius extent to use as the maximum radius. Should be 
        /// a value between 0.0 and 1.0.
        /// <para>This is a dependency property.</para>
        /// </summary>
        [WidgetDefaultNumber(0.75)]
        public double RadiusExtentScale
        {
            get
            {
                return (double)GetValue(RadiusExtentScaleProperty);
            }
            set
            {
                SetValue(RadiusExtentScaleProperty, value);
            }
        }

        internal const string RadiusExtentScalePropertyName = "RadiusExtentScale";

        /// <summary>
        /// Identifies the RadiusExtentScale dependency property.
        /// </summary>
        public static readonly DependencyProperty RadiusExtentScaleProperty = DependencyProperty.Register(RadiusExtentScalePropertyName, typeof(double), typeof(NumericRadiusAxis),
            new PropertyMetadata(0.75, (sender, e) =>
            {
                (sender as NumericRadiusAxis).RaisePropertyChanged(RadiusExtentScalePropertyName, e.OldValue, e.NewValue);
            }));
        #endregion

        internal double ActualRadiusExtentScale { set; get; }

        #region InnerRadiusExtentScale Dependency Property
        /// <summary>
        /// Defines the percentage of the maximum radius extent to leave blank at the center of the chart. Should be 
        /// a value between 0.0 and 1.0.
        /// <para>This is a dependency property.</para>
        /// </summary>
        [WidgetDefaultNumber(0.0)]
        public double InnerRadiusExtentScale
        {
            get
            {
                return (double)GetValue(InnerRadiusExtentScaleProperty);
            }
            set
            {
                SetValue(InnerRadiusExtentScaleProperty, value);
            }
        }

        internal const string InnerRadiusExtentScalePropertyName = "InnerRadiusExtentScale";

        /// <summary>
        /// Identifies the RadiusExtentScale dependency property.
        /// </summary>
        public static readonly DependencyProperty InnerRadiusExtentScaleProperty = DependencyProperty.Register(InnerRadiusExtentScalePropertyName, typeof(double), typeof(NumericRadiusAxis),
            new PropertyMetadata(0.0, (sender, e) =>
            {
                (sender as NumericRadiusAxis).RaisePropertyChanged(InnerRadiusExtentScalePropertyName, e.OldValue, e.NewValue);
            }));
        #endregion

        internal double ActualInnerRadiusExtentScale { set; get; }

        /// <summary>
        /// Handles property updated events.
        /// </summary>
        /// <param name="sender">The sender of the updated event.</param>
        /// <param name="propertyName">The name of the property that was updated.</param>
        /// <param name="oldValue">The old value of the property.</param>
        /// <param name="newValue">The new value of the property.</param>
        protected override void PropertyUpdatedOverride(object sender, string propertyName,
            object oldValue, object newValue)
        {
            switch (propertyName)
            {
                case CrossingValuePropertyName:
                    CrossingValueRadians = ConvertToDouble(CrossingValue) * Math.PI / 180;
                    break;
            }

            base.PropertyUpdatedOverride(sender, propertyName, oldValue, newValue);

            switch (propertyName)
            {
                case RadiusExtentScalePropertyName:
                    ActualRadiusExtentScale = RadiusExtentScale;
                    if (ActualRadiusExtentScale < 0.0)
                    {
                        ActualRadiusExtentScale = 0.1;
                    }
                    if (ActualRadiusExtentScale > 1.0)
                    {
                        ActualRadiusExtentScale = 1.0;
                    }

                    if (ActualInnerRadiusExtentScale >= ActualRadiusExtentScale)
                    {
                        ActualInnerRadiusExtentScale = ActualRadiusExtentScale - .01;

                        if (ActualInnerRadiusExtentScale < 0)
                        {
                            ActualInnerRadiusExtentScale = 0;
                            ActualRadiusExtentScale = .01;
                        }
                    }

                    MustInvalidateLabels = true;
                    RenderAxis(false);
                    if (AngleAxis != null)
                    {
                        AngleAxis.RenderAxis();

                        foreach (Series s in AngleAxis.Series)
                        {
                            if (s is ValueOverlay)
                            {
                                s.RenderSeries(false);
                            }
                        }

                    }

                    foreach (Series s in DirectSeries())
                    {
                        s.RenderSeries(false);
                    }
                    break;
                case InnerRadiusExtentScalePropertyName:
                    ActualInnerRadiusExtentScale = InnerRadiusExtentScale;
                    if (ActualInnerRadiusExtentScale < 0.0)
                    {
                        ActualInnerRadiusExtentScale = 0.1;
                    }
                    if (ActualInnerRadiusExtentScale > 1.0)
                    {
                        ActualInnerRadiusExtentScale = 1.0;
                    }

                    if (ActualInnerRadiusExtentScale >= ActualRadiusExtentScale)
                    {
                        ActualInnerRadiusExtentScale = ActualRadiusExtentScale - .01;
                        if (ActualInnerRadiusExtentScale < 0)
                        {
                            ActualInnerRadiusExtentScale = 0;
                            ActualRadiusExtentScale = .01;
                        }
                    }

                    MustInvalidateLabels = true;
                    RenderAxis(false);
                    if (AngleAxis != null)
                    {
                        AngleAxis.RenderAxis();

                        foreach (Series s in AngleAxis.Series)
                        {
                            if (s is ValueOverlay)
                            {
                                s.RenderSeries(false);
                            }
                        }

                    }

                    foreach (Series s in DirectSeries())
                    {
                        s.ThumbnailDirty = true;
                        s.RenderSeries(false);
                    }
                    if (this.SeriesViewer != null)
                    {
                        this.SeriesViewer.NotifyThumbnailAppearanceChanged();
                    }
                    break;
                case CrossingAxisPropertyName:
                    NumericAngleAxis angleAxis = newValue as NumericAngleAxis;
                    CategoryAngleAxis catAxis = newValue as CategoryAngleAxis;
                    if (angleAxis == null && catAxis == null)
                    {
                        OnAngleAxisChanged(null);
                    }
                    if (angleAxis != null)
                    {
                        OnAngleAxisChanged(angleAxis);
                        angleAxis.OnRadiusAxisChanged(this);
                    }
                    if (catAxis != null)
                    {
                        OnAngleAxisChanged(catAxis);
                        catAxis.OnRadiusAxisChanged(this);
                    }
                    MustInvalidateLabels = true;
                    RenderAxis(false);
                    break;
                case IsInvertedPropertyName:
                    if (AngleAxis != null)
                    {
                        AngleAxis.MustInvalidateLabels = true;
                        AngleAxis.RenderAxis(false);
                    }
                    break;
            }
        }

        /// <summary>
        /// Gets or sets the axis crossing value in radians.
        /// </summary>
        protected double CrossingValueRadians { get; set; }

        /// <summary>
        /// Handles axis changed event.
        /// </summary>
        /// <param name="angleAxis">source axis</param>
        protected internal virtual void OnAngleAxisChanged(Axis angleAxis)
        {
            AngleAxis = angleAxis;
        }

        private Axis _angleAxis;

        /// <summary>
        /// Gets or sets the reference to the angle axis.
        /// </summary>
        protected internal Axis AngleAxis
        {
            get 
            {
                if (_angleAxis != null)
                {
                    return _angleAxis;
                }
                XamDataChart dataChart = this.SeriesViewer as XamDataChart;
                if (dataChart != null)
                {
                    return dataChart.Axes.Where(
                        (a) => a is NumericAngleAxis ||
                        a is CategoryAngleAxis).FirstOrDefault();
                }
                return _angleAxis;
            }
            set { _angleAxis = value; }
        }

        ///// <summary>
        ///// Sets up the strips for this type of axis.
        ///// </summary>
        ///// <remarks>
        ///// The concentric strips require using the strop brush as a stroke not a fill.
        ///// </remarks>
        //protected override void SetupStrips()
        //{
        //    RootCanvas.Children.Add(Strips);
        //    Strips.SetBinding(Shape.StrokeProperty, new Binding(StripPropertyName) { Source = this });
        //}

        internal override NumericAxisRenderingParameters CreateRenderingParamsInstance()
        {
            return new PolarAxisRenderingParameters();
        }

        /// <summary>
        /// Creates the parameters for the rendering.
        /// </summary>
        /// <param name="viewportRect">The viewport rectangle.</param>
        /// <param name="windowRect">The window rectangle.</param>
        /// <returns>The rendering parameters.</returns>
        internal override NumericAxisRenderingParameters CreateRenderingParams(Rect viewportRect, Rect windowRect)
        {
            PolarAxisRenderingParameters renderingParams =
                base.CreateRenderingParams(viewportRect, windowRect)
                as PolarAxisRenderingParameters;

            //Determine the closest and furthest visible radius in the viewing window.
            //These will constrain which gridlines we need to draw.
            double closestRadius = renderingManager.GetClosestRadiusValue(windowRect);
            double furthestRadius = renderingManager.GetFurthestRadiusValue(windowRect);

            //Don't draw for a radius longer than the maximum radius permitted by the axis scale.
            double maxRadius = .5 * ActualRadiusExtentScale;
            double minRadius = .5 * ActualInnerRadiusExtentScale;

            double visibleMinimum, visibleMaximum;
            if (windowRect == SeriesViewer.StandardRect) // todo: use a better condition to detect when this should happen
            {
                visibleMaximum = this.ActualMaximumValue;
                visibleMinimum = this.ActualMinimumValue;
            }
            else
            {
                visibleMaximum = Math.Min(furthestRadius, maxRadius);
                visibleMinimum = GetUnscaledValue(closestRadius);
                visibleMaximum = GetUnscaledValue(visibleMaximum);

                SnapVisibleExtents(viewportRect, windowRect, ref visibleMinimum, ref visibleMaximum);
            }
            ////in this case, we are not over the chart, and no concentric lines are visible.
            //if (visibleMinimum > visibleMaximum)
            //{
            //    return null;
            //}

            //The center in chart area coordinates for drawing circles around.
            //note that many of the circles will draw partially offscreen and be clipped.
            //if this becomes an issue in the future, they will have to be converted into paths
            //that represent only the visible portion of the ellipses.
            Point center = new Point(0.5, 0.5);

            //Determine the extents of the spokes we will draw, note that this still results in
            //portions of some of the spokes being drawn offscreen. If this becomes a problem in
            //the future, those paths will have to be converted into paths that describe only the 
            //on screen portion of the spoke. But this keeps offscreen drawing to a healthy minimum.
            double minLen = closestRadius;
            double maxLen = furthestRadius;

            //The resolution value to use for the snappers. Here we want it to be proportional to the
            //value we are using for the radius which is half the min dimension of the viewport.
            double resolution = Math.Min(viewportRect.Width, viewportRect.Height) *
                (ActualRadiusExtentScale - ActualInnerRadiusExtentScale) / 
                2.0;
            renderingParams.Center = center;

            double trueMaxLen = Math.Max(maxLen, minLen);
            double trueMinLen = Math.Min(minLen, maxLen);

            renderingParams.MaxLength = trueMaxLen;
            renderingParams.MinLength = trueMinLen;

            double trueVisibleMinimum = Math.Min(visibleMinimum, visibleMaximum);
            double trueVisibleMaximum = Math.Max(visibleMinimum, visibleMaximum);

            if (trueVisibleMinimum < ActualMinimumValue)
            {
                trueVisibleMinimum = ActualMinimumValue;
            }

            if (trueVisibleMaximum > ActualMaximumValue)
            {
                trueVisibleMaximum = ActualMaximumValue;
            }

            renderingParams.RangeInfos.Add(
                new RangeInfo()
                {
                    VisibleMinimum = trueVisibleMinimum,
                    VisibleMaximum = trueVisibleMaximum,
                    Resolution = resolution
                });

            
#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)


            IAngleScaler angleAxis = AngleAxis as IAngleScaler;
            if (angleAxis != null)
            {
                double minAngle;
                double maxAngle;
                angleAxis.GetMinMaxAngle(windowRect, out minAngle, out maxAngle);

                renderingParams.MinAngle = Math.Min(minAngle, maxAngle);
                renderingParams.MaxAngle = Math.Max(minAngle, maxAngle);
            }

            double effectiveMaximum = GetEffectiveMaximumLength();
            if (maxLen >= maxRadius)
            {
                maxLen = effectiveMaximum;
            }
            if (minLen < minRadius)
            {
                minLen = minRadius;
            }
            renderingParams.MinLength = minLen;
            renderingParams.MaxLength = maxLen;
            renderingParams.EffectiveMaximum = effectiveMaximum;

            renderingParams.TickmarkValues = this.ActualTickmarkValues;
            LinearTickmarkValues linearTicks = renderingParams.TickmarkValues as LinearTickmarkValues;
            if (linearTicks != null)
            {
                linearTicks.MinTicks = GetMinTicks(
                    center,
                    renderingParams.MinLength,
                    renderingParams.MaxLength,
                    windowRect,
                    viewportRect);
            }
            return renderingParams;
        }

        private void SnapVisibleExtents(Rect viewportRect, Rect windowRect, ref double visibleMinimum, ref double visibleMaximum)
        {
            Point center = new Point(.5, .5);
            double extent = 0.0;
            if (IsInverted)
            {
                extent = GetScaledValue(this.ActualMinimumValue);
            }
            else
            {
                extent = GetScaledValue(this.ActualMaximumValue);
            }

            double crossingValue = 0.0;
            if (CrossingValue != null)
            {
                crossingValue = CrossingValueRadians;
            }

            double x = center.X +
                extent * Math.Cos(crossingValue);
            double y = center.Y +
                extent * Math.Sin(crossingValue);

            center.X = ViewportUtils.TransformXToViewport(center.X, windowRect, viewportRect);
            center.Y = ViewportUtils.TransformYToViewport(center.Y, windowRect, viewportRect);
            x = ViewportUtils.TransformXToViewport(x, windowRect, viewportRect);
            y = ViewportUtils.TransformYToViewport(y, windowRect, viewportRect);

            if (x >= viewportRect.Left && x <= viewportRect.Right &&
                y >= viewportRect.Top && y <= viewportRect.Bottom)
            {
                if (IsInverted)
                {
                    visibleMaximum = ActualMinimumValue;
                    //Debug.WriteLine("snapping visibleMinimum");
                }
                else
                {
                    visibleMaximum = ActualMaximumValue;
                    //Debug.WriteLine("snapping visibleMaximum");
                }
            }
            
            if (center.X >= viewportRect.Left && center.X <= viewportRect.Right &&
               center.Y >= viewportRect.Top && center.Y <= viewportRect.Bottom)
            {
                if (IsInverted)
                {
                    visibleMinimum = ActualMaximumValue;
                    //Debug.WriteLine("snapping visibleMaximum");
                }
                else
                {
                    visibleMinimum = ActualMinimumValue;
                    //Debug.WriteLine("snapping visibleMinimum");
                }
            }

        }

        private int GetMinTicks(Point center, double minLen, double maxLen, Rect windowRect, Rect viewportRect)
        {
            double radViewportLength = ViewportUtils.TransformXToViewportLength(maxLen - minLen, windowRect, viewportRect);
            double viewportRatio = radViewportLength / Math.Min(viewportRect.Width, viewportRect.Height);

            if (viewportRatio > .7)
            {
                return 10;
            }

            return 5;
        }

        internal double GetEffectiveMaximumLength()
        {
            double value = 0.0;
            if (!IsInverted)
            {
                value = GetScaledValue(ActualMaximumValue);
            }
            else
            {
                value = GetScaledValue(ActualMinimumValue);
            }

            return value;
        }

        internal override void OnRangeChanged(AxisRangeChangedEventArgs ea)
        {
            if (AngleAxis != null)
            {
                AngleAxis.RenderAxis();
            }
        }

        /// <summary>
        /// Renders the axis.
        /// </summary>
        /// <param name="animate">Whether or not to use animation</param>
        protected override void RenderAxisOverride(bool animate)
        {
            Rect windowRect = SeriesViewer != null ? SeriesViewer.ActualWindowRect : Rect.Empty;
            Rect viewportRect = !windowRect.IsEmpty ? ViewportRect : Rect.Empty;

            Renderer.Render(animate, viewportRect, windowRect);
        }

        /// <summary>
        /// Overridden in derived classes if they want to respond to the viewport of the axis changing.
        /// </summary>
        /// <param name="oldRect">The old viewport rectangle.</param>
        /// <param name="newRect">The new viewport rectangle.</param>
        protected override void ViewportChangedOverride(Rect oldRect, Rect newRect)
        {
            base.ViewportChangedOverride(oldRect, newRect);
            if (newRect.Height != oldRect.Height || newRect.Width != oldRect.Width)
            {
                this.UpdateRange();
            }
        }

        internal void DefineClipRegion(GeometryGroup geom, Rect viewportRect, Rect windowRect)
        {
            var renderingParams = CreateRenderingParams(viewportRect, windowRect) as IPolarRadialRenderingParameters;
            if (renderingParams == null)
            {
                return;
            }

            renderingManager.ConcentricStrip(geom.Children,
                renderingParams.MinLength,
                renderingParams.MaxLength,
                viewportRect,
                windowRect,
                renderingParams.Center,
                renderingParams.MinAngle,
                renderingParams.MaxAngle);
        }

        /// <summary>
        /// Gets the axis orientation.
        /// </summary>
        protected internal override AxisOrientation Orientation
        {
            get { return AxisOrientation.Radial; }
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