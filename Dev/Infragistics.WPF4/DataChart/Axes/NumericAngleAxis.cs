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
using System.Runtime.CompilerServices;

namespace Infragistics.Controls.Charts
{
    /// <summary>
    /// Represents a XamDataChart angle based axis for polar series.
    /// </summary>
    [WidgetModule("PolarChart")]
    [WidgetIgnoreDepends("XamDataChart")]
    public class NumericAngleAxis : NumericAxisBase, IAngleScaler
    {
        internal override AxisView CreateView()
        {
            return new NumericAngleAxisView(this);
        }
        internal override void OnViewCreated(AxisView view)
        {
            base.OnViewCreated(view);

            NumericAngleView = (NumericAngleAxisView)view;
        }
        internal NumericAngleAxisView NumericAngleView { get; set; }

        internal PolarAxisRenderingManager RenderingManager { get; set; }

        #region Constructor and Initialisation
        /// <summary>
        /// Constructs a numeric angle axis.
        /// </summary>
        public NumericAngleAxis()
        {
            RenderingManager = new PolarAxisRenderingManager();
            Renderer = CreateRenderer();
        }

        internal override AxisLabelPanelBase CreateLabelPanel()
        {
            AngleAxisLabelPanel panel = new AngleAxisLabelPanel();
            panel.GetPoint = (v) =>
                {
                    Rect windowRect = SeriesViewer != null ? SeriesViewer.ActualWindowRect : Rect.Empty;
                    Rect viewportRect = !windowRect.IsEmpty ? ViewportRect : Rect.Empty;
                    return GetLabelLocationPoint(v, new Point(0.5, 0.5), windowRect, viewportRect, LabelPanel.Extent);
                };
            return panel;
        }
        #endregion

        private double GetCrossingValue()
        {
            if (RadiusAxis == null)
            {
                return 0.0;
            }

            if (!HasCrossingValue())
            {
                return RadiusAxis.GetEffectiveMaximumLength();
            }
            else
            {



                return RadiusAxis.GetScaledValue(Convert.ToDouble(CrossingValue));

            }
        }

        private double Round10(double value)
        {



            return Math.Round(value, 10);

        }

        private bool _preventReentry = false;
        private double _lastCrossing = double.NaN;

        /// <summary>
        /// Creates the renderer for this axis.
        /// </summary>
        /// <returns>The renderer object.</returns>
        internal override NumericAxisRenderer CreateRenderer()
        {
            NumericAxisRenderer renderer = base.CreateRenderer();

            renderer.LabelManager.FloatPanelAction = (crossing) =>
            {
                if ((this.LabelSettings == null || this.LabelSettings.Visibility == Visibility.Visible) 
                    && this.RadiusAxis != null
                    && _lastCrossing != crossing)
                {
                    XamDataChart dataChart = this.SeriesViewer as XamDataChart;
                    if (dataChart == null) return;
                    _lastCrossing = crossing;
                    this.LabelPanel.CrossingValue = crossing;
                    dataChart.InvalidatePanels();

                    foreach (var axis in dataChart.Axes)
                    {
                        if (axis != this &&
                            axis.LabelPanel is AngleAxisLabelPanel)
                        {
                            axis.LabelPanel.InvalidateArrange();
                        }
                    }

                }
            };

            renderer.DetermineCrossingValue =
                (p) =>
                {
                    p.CrossingValue = GetCrossingValue();
                };

            renderer.AxisLine =
                (p) =>
                {
                    PolarAxisRenderingParameters r = p as PolarAxisRenderingParameters;
                    if (r.CurrentRangeInfo == r.RangeInfos[0])
                    {
                        RenderingManager.ConcentricLine(p.AxisGeometry, p.CrossingValue,
                            p.ViewportRect, p.WindowRect, r.Center, r.MinAngle, r.MaxAngle);
                    }
                };

            renderer.Line = (p, g, value) =>
                {
                    PolarAxisRenderingParameters r = p as PolarAxisRenderingParameters;
                    RenderingManager.RadialLine(g, value,
                        p.ViewportRect, p.WindowRect,
                        r.MinLength, r.MaxLength, r.Center);
                };

            renderer.Strip = (p, g, start, end) =>
                {
                    PolarAxisRenderingParameters r = p as PolarAxisRenderingParameters;
                    RenderingManager.RadialStrip(g, start,
                        end, r.ViewportRect,
                        p.WindowRect, r.MinLength,
                        r.MaxLength, r.Center);
                };

            renderer.CreateRenderingParams = (viewportRect, windowRect) =>
                {
                    PolarAxisRenderingParameters r =
                        CreateRenderingParams(viewportRect, windowRect)
                        as PolarAxisRenderingParameters;

                    return r;
                };

            renderer.OnRendering = () =>
                {
                    if (!_preventReentry)
                    {
                        _preventReentry = true;
                        RadiusAxis.UpdateRange();
                        _preventReentry = false;
                    }
                };

            renderer.Scaling = (p, unscaled) =>
                {
                    return GetScaledAngle(unscaled);
                };

            renderer.ShouldRender = (viewport, window) =>
                {
                    return !window.IsEmpty
                        && !viewport.IsEmpty
                        && RadiusAxis != null;
                };

            renderer.ShouldRenderLines = (p, value) =>
                {
                    if (Round10(value - _startAngleOffsetRadians) < 0)
                    {
                        return false;
                    }

                    if (Round10(value - _startAngleOffsetRadians - (2.0 * Math.PI)) > 0)
                    {
                        return false;
                    }

                    return true;
                };

            renderer.ShouldRenderLabel = (p, value, last) =>
                {
                    PolarAxisRenderingParameters r = p
                        as PolarAxisRenderingParameters;

                    Point endPoint = GetLabelLocationPoint(
                        GetScaledAngle(r.ActualMaximumValue),
                        r.Center,
                        p.WindowRect,
                        p.ViewportRect, 0);

                    //shouldnt exclude labels based on extent at this
                    //point because the auto-extent may not have been 
                    //calculated yet.
                    Point labelPoint = GetLabelLocationPoint(value,
                        r.Center,
                        p.WindowRect,
                        p.ViewportRect, 0);

                    if (last &&
                        MathUtil.Hypot(endPoint.X - labelPoint.X, endPoint.Y - labelPoint.Y) < 2.0)
                    {
                        return false;
                    }

                    if (labelPoint.X < p.ViewportRect.Right &&
                        labelPoint.X >= p.ViewportRect.Left &&
                        labelPoint.Y < p.ViewportRect.Bottom &&
                        labelPoint.Y >= p.ViewportRect.Top)
                    {
                        return true;
                    }
                    return false;
                };

            renderer.SnapMajorValue = (p, value, i, interval) =>
            {
                if (value < p.ActualMinimumValue &&
                    p.TickmarkValues is LogarithmicTickmarkValues)
                {
                    return p.ActualMinimumValue;
                }
                else if (value > p.ActualMaximumValue && (p.TickmarkValues is LogarithmicTickmarkValues || p.HasUserMax))
                {
                    return p.ActualMaximumValue;
                }

                return value;
            };

            return renderer;
        }

        private Point GetLabelLocationPoint(double angleValue,
            Point center,
            Rect windowRect,
            Rect viewportRect,
            double extent)
        {
            double crossingValue = GetCrossingValue();

            double extentValue =
                ViewportUtils.TransformXFromViewportLength(
                extent,
                windowRect, viewportRect);

            if (LabelSettings != null && (LabelSettings.ActualLocation == AxisLabelsLocation.InsideBottom || LabelSettings.ActualLocation == AxisLabelsLocation.OutsideBottom))
            {
                extentValue *= -1;
            }

            double x = center.X +
                (crossingValue + extentValue) * Math.Cos(angleValue);
            double y = center.Y +
                (crossingValue + extentValue) * Math.Sin(angleValue);

            x = ViewportUtils.TransformXToViewport(x,
                windowRect, viewportRect);
            y = ViewportUtils.TransformYToViewport(y,
                windowRect, viewportRect);

            return new Point(x, y);
        }

        /// <summary>
        /// Gets the scaled angle value in radians from the raw axis value.
        /// </summary>
        /// <param name="unscaledValue">The raw axis value.</param>
        /// <param name="p">Scaler parameters</param>
        /// <returns></returns>
        public override double GetScaledValue(double unscaledValue, ScalerParams p)
        {
            return GetScaledAngle(unscaledValue);
        }

        internal double GetScaledAngle(double unscaledValue, bool isLogarithmic, bool isInverted)
        {
            double scaledValue = 0;

            if (isLogarithmic)
            {
                scaledValue = (Math.Log(unscaledValue) - logActualMinimumValue) /
                    (logActualMaximumValue - logActualMinimumValue);
            }
            else
            {
                scaledValue = (unscaledValue - ActualMinimumValue) /
                    (ActualMaximumValue - ActualMinimumValue);
            }

            if (isInverted)
            {
                scaledValue = 1.0 - scaledValue;
            }

            //scale to radians (2.0 * pi) = 360 degrees.
            //this will need to get more sophisticated if the users want to
            //specify the min and max extent angles on the circle that the chart should plot on.
            return (scaledValue * 2.0 * Math.PI) + _startAngleOffsetRadians;
        }

        /// <summary>
        /// Gets the scaled angle value in radians based on the raw input.
        /// </summary>
        /// <param name="unscaledValue">The raw input value.</param>
        /// <returns>The scaled angle value.</returns>
        public double GetScaledAngle(double unscaledValue)
        {
            return GetScaledAngle(unscaledValue, IsReallyLogarithmic, IsInvertedCached);
        }

        /// <summary>
        /// Gets the raw axis value back from the angle that would be used on the chart.
        /// </summary>
        /// <param name="scaledValue">The chart angle value.</param>
        /// <returns>The raw axis value.</returns>
        public double GetUnscaledAngle(double scaledValue)
        {
            double unscaledValue = (scaledValue - _startAngleOffsetRadians) / (2.0 * Math.PI);

            if (IsInverted)
            {
                unscaledValue = 1.0 - unscaledValue;
            }

            if (IsReallyLogarithmic)
            {
                return Math.Exp(
                    unscaledValue * (logActualMaximumValue - logActualMinimumValue)
                    + logActualMinimumValue);
            }
            else
            {
                return ActualMinimumValue + unscaledValue * (ActualMaximumValue - ActualMinimumValue);
            }
        }

        //#region RadiusAxis Dependency Property
        ///// <summary>
        ///// Links to the paired Radius Axis for this angle axis.
        ///// <para>This is a dependency property.</para>
        ///// </summary>
        ///// <remarks>
        ///// The Angle axis must always be paired with a radius axis for it to perform its functions.
        ///// The usual way of specifying this pairing is with a binding.
        ///// </remarks>
        //public NumericRadiusAxis RadiusAxis
        //{
        //    get
        //    {
        //        return (NumericRadiusAxis)GetValue(RadiusAxisProperty);
        //    }
        //    set
        //    {
        //        SetValue(RadiusAxisProperty, value);
        //    }
        //}

        //internal const string RadiusAxisPropertyName = "RadiusAxis";

        ///// <summary>
        ///// Identifies the RadiusAxis dependency property.
        ///// </summary>
        //public static readonly DependencyProperty RadiusAxisProperty =
        //    DependencyProperty.Register(RadiusAxisPropertyName, typeof(NumericRadiusAxis),
        //    typeof(NumericAngleAxis),
        //    new PropertyMetadata(null, (sender, e) =>
        //    {
        //        (sender as NumericAngleAxis).RaisePropertyChanged(RadiusAxisPropertyName, e.OldValue, e.NewValue);
        //    }));
        //#endregion

        #region StartAngleOffset Dependency Property
        /// <summary>
        /// Indicates the angle in degress that the chart's 0th angle should be offset.
        /// <para>This is a dependency property.</para>
        /// </summary>
        [WidgetDefaultNumber(0.0)]
        public double StartAngleOffset
        {
            get
            {
                return (double)GetValue(StartAngleOffsetProperty);
            }
            set
            {
                SetValue(StartAngleOffsetProperty, value);
            }
        }

        private double _startAngleOffsetRadians = 0;

        internal const string StartAngleOffsetPropertyName = "StartAngleOffset";

        /// <summary>
        /// Identifies the StartAngleOffset dependency property.
        /// </summary>
        public static readonly DependencyProperty StartAngleOffsetProperty =
            DependencyProperty.Register(StartAngleOffsetPropertyName, typeof(double),
            typeof(NumericAngleAxis),
            new PropertyMetadata(0.0, (sender, e) =>
            {
                (sender as NumericAngleAxis).RaisePropertyChanged(StartAngleOffsetPropertyName, e.OldValue, e.NewValue);
            }));
        #endregion

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
            base.PropertyUpdatedOverride(sender, propertyName, oldValue, newValue);
            XamDataChart dataChart = this.SeriesViewer as XamDataChart;
            switch (propertyName)
            {
                case CrossingAxisPropertyName:
                    NumericRadiusAxis radiusAxis = newValue as NumericRadiusAxis;
                    OnRadiusAxisChanged(radiusAxis);
                    if (radiusAxis != null)
                    {
                        radiusAxis.OnAngleAxisChanged(this);
                    }
                    RenderAxis(false);
                    break;
                case StartAngleOffsetPropertyName:
                    _startAngleOffsetRadians = StartAngleOffset;
                    while (_startAngleOffsetRadians < 0)
                    {
                        _startAngleOffsetRadians += 360;
                    }
                    while (_startAngleOffsetRadians >= 360)
                    {
                        _startAngleOffsetRadians -= 360;
                    }
                    _startAngleOffsetRadians = (StartAngleOffset * Math.PI) / 180;
                    RenderAxis(false);

                    foreach (Series series in DirectSeries())
                    {
                        series.RenderSeries(false);
                    }
                    break;
                case LabelPropertyName:
                    if (dataChart != null)
                    {
                        foreach (var axis in dataChart.Axes)
                        {
                            axis.RenderAxis();
                        }
                    }
                    break;
                case CrossingValuePropertyName:
                    if (dataChart != null)
                    {
                        foreach (var axis in dataChart.Axes)
                        {
                            if (axis is NumericAngleAxis ||
                                axis is CategoryAngleAxis)
                            {
                                axis.RenderAxis();
                            }
                        }
                    }
                    break;
                case LabelSettingsPropertyName:
                    Renderer = CreateRenderer();
                    ForcePanelRefloat();
                    MustInvalidateLabels = true;
                    RenderAxis(false);
                    break;
            }
        }

        private void ForcePanelRefloat()
        {
            _lastCrossing = double.NaN;
        }

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
            double closestRadius = RenderingManager.GetClosestRadiusValue(windowRect);
            double furthestRadius = RenderingManager.GetFurthestRadiusValue(windowRect);

            //Don't draw for a radius longer than the maximum radius permitted by the axis scale.
            double maxRadius = .5 * RadiusAxis.ActualRadiusExtentScale;
            double minRadius = .5 * RadiusAxis.ActualInnerRadiusExtentScale;
            //furthestRadius = Math.Min(furthestRadius, maxRadius);

            //Determine the extents of the spokes we will draw, note that this still results in
            //portions of some of the spokes being drawn offscreen. If this becomes a problem in
            //the future, those paths will have to be converted into paths that describe only the 
            //on screen portion of the spoke. But this keeps offscreen drawing to a healthy minimum.
            double minLen = closestRadius;
            double maxLen = furthestRadius;

            double effectiveMaximum = RadiusAxis.GetEffectiveMaximumLength();

            if (double.IsNaN(effectiveMaximum) ||
               double.IsInfinity(effectiveMaximum))
            {
                return null;
            }

            if (maxLen >= maxRadius)
            {
                maxLen = effectiveMaximum;
            }
            if (minLen < minRadius)
            {
                minLen = minRadius;
            }

            double resolution = viewportRect.Width;

            //Get the axis values for the min and max angle that are visible in the current
            //view into the chart.
            RenderingManager.DetermineView(
                windowRect,
                renderingParams,
                ActualMinimumValue,
                ActualMaximumValue,
                IsInverted,
                GetUnscaledAngle,
                resolution);

            Point center = new Point(0.5, 0.5);

            renderingParams.Center = center;
            renderingParams.MaxLength = maxLen;
            renderingParams.MinLength = minLen;
            renderingParams.EffectiveMaximum = effectiveMaximum;

            return renderingParams;
        }

        internal void GetMinMaxAngle(Rect windowRect, out double minAngle, out double maxAngle)
        {
            RenderingManager.GetMinMaxAngle(
                windowRect,
                out minAngle,
                out maxAngle);
        }

        void IAngleScaler.GetMinMaxAngle(Rect windowRect, out double minAngle, out double maxAngle)
        {
            GetMinMaxAngle(windowRect, out minAngle, out maxAngle);
        }

        /// <summary>
        /// Performs the rendering of the axis.
        /// </summary>
        /// <param name="animate">Indicates whether the rendering should be animated.</param>
        protected override void RenderAxisOverride(bool animate)
        {
            Rect windowRect = SeriesViewer != null ? SeriesViewer.ActualWindowRect : Rect.Empty;
            Rect viewportRect = !windowRect.IsEmpty ? ViewportRect : Rect.Empty;

            Renderer.Render(animate, viewportRect, windowRect);
        }

        private NumericRadiusAxis _radiusAxis;

        /// <summary>
        /// Gets or sets the radius axis reference.
        /// </summary>
        protected internal NumericRadiusAxis RadiusAxis
        {
            get
            {
                if (_radiusAxis != null)
                {
                    return _radiusAxis;
                }
                XamDataChart dataChart = this.SeriesViewer as XamDataChart;
                if (dataChart != null)
                {
                    return dataChart.Axes.OfType<NumericRadiusAxis>().FirstOrDefault();
                }
                return _radiusAxis;
            }
            set { _radiusAxis = value; }
        }

        internal void OnRadiusAxisChanged(NumericRadiusAxis numericRadiusAxis)
        {
            RadiusAxis = numericRadiusAxis;
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

        /// <summary>
        /// Gets the axis orientation.
        /// </summary>
        protected internal override AxisOrientation Orientation
        {
            get { return AxisOrientation.Angular; }
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