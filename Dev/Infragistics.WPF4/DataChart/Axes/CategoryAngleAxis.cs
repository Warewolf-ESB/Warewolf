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
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Infragistics.Controls.Charts
{
    /// <summary>
    /// Represents a XamDataChart category angle axis. Useful for displaying radial categories.
    /// </summary>
    [WidgetModule("RadialChart")]
    [WidgetIgnoreDepends("XamDataChart")]
    public class CategoryAngleAxis : CategoryAxisBase, IAngleScaler
    {
        internal override AxisView CreateView()
        {
            return new CategoryAngleAxisView(this);
        }
        internal override void OnViewCreated(AxisView view)
        {
            base.OnViewCreated(view);

            CategoryAngleView = (CategoryAngleAxisView)view;
        }
        internal CategoryAngleAxisView CategoryAngleView { get; set; }

        private PolarAxisRenderingManager renderingManager;

        #region Constructor and Initalisation
        /// <summary>
        /// Initializes a new CategoryAngleAxis instance.
        /// </summary>
        public CategoryAngleAxis():base()
        {
            Renderer = CreateRenderer();
            renderingManager = new PolarAxisRenderingManager();
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

        internal CategoryAxisRenderer Renderer { get; set; }

        private bool _preventReentry = false;

        private double Round10(double value)
        {



            return Math.Round(value, 10);

        }

        private double _lastCrossing = double.NaN;

        private CategoryAxisRenderer CreateRenderer()
        {
            var labelManager = new AxisLabelManager()
            {
                Axis = this,
                LabelPositions = this.LabelPositions,
                LabelDataContext = this.LabelDataContext,
                TargetPanel = this.LabelPanel
            };

            if (this.LabelSettings != null)
            {
                this.LabelSettings.Axis = this;
            }

            var renderer = new CategoryAxisRenderer(labelManager);

            renderer.Clear = () =>
            {
                GeometryCollection axisGeometry = View.GetAxisLinesGeometry();
                GeometryCollection stripsGeometry = View.GetStripsGeometry();
                GeometryCollection majorGeometry = View.GetMajorLinesGeometry();
                GeometryCollection minorGeometry = View.GetMinorLinesGeometry();

                UpdateLineVisibility();

                ClearMarks(axisGeometry);
                ClearMarks(stripsGeometry);
                ClearMarks(majorGeometry);
                ClearMarks(minorGeometry);
            };

            renderer.ShouldRender = (viewport, window) =>
            {
                return !window.IsEmpty
                        && !viewport.IsEmpty
                        && RadiusAxis != null;
            };

            renderer.CreateRenderingParams = (viewport, window) =>
            {
                return CreateRenderingParams(viewport, window);
            };

            renderer.OnRendering =
                () =>
                {
                    if (!_preventReentry)
                    {
                        _preventReentry = true;
                        RadiusAxis.UpdateRange();
                        _preventReentry = false;
                    }
                };

            renderer.GetLabelForItem = (item) =>
            {
                int index = (int)item;
                if (index > FastItemsSource.Count - 1)
                {
                    index -= FastItemsSource.Count;
                }
                object dataItem = this.FastItemsSource[index];
                return GetLabel(dataItem);
            };

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

            renderer.Line = (p, g, value) =>
            {
                RadialAxisRenderingParameters r = p as RadialAxisRenderingParameters;
                renderingManager.RadialLine(g, value,
                    p.ViewportRect, p.WindowRect,
                    r.MinLength, r.MaxLength, r.Center);
            };

            renderer.Strip = (p, g, start, end) =>
            {
                RadialAxisRenderingParameters r = p as RadialAxisRenderingParameters;
                renderingManager.RadialStrip(g, start,
                    end, r.ViewportRect,
                    p.WindowRect, r.MinLength,
                    r.MaxLength, r.Center);
            };

            renderer.Scaling = (p, unscaled) =>
            {
                return GetScaledAngle(unscaled);
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

            renderer.AxisLine = (p) =>
            {
                RadialAxisRenderingParameters r = p as RadialAxisRenderingParameters;
                if (r.CurrentRangeInfo == r.RangeInfos[0])
                {
                    renderingManager.ConcentricLine(
                        p.AxisGeometry, p.CrossingValue,
                        p.ViewportRect, p.WindowRect, 
                        r.Center, r.MinAngle, r.MaxAngle);
                }
            };

            renderer.DetermineCrossingValue = (p) =>
            {
                p.CrossingValue = GetCrossingValue();
            };

            renderer.ShouldRenderLabel = (p, value, last) =>
            {
                RadialAxisRenderingParameters r = p
                        as RadialAxisRenderingParameters;
                if (last)
                {
                    return false;
                }

                Point labelPoint = GetLabelLocationPoint(value,
                    r.Center,
                    p.WindowRect,
                    p.ViewportRect, 0);

                if (labelPoint.X < p.ViewportRect.Right &&
                    labelPoint.X >= p.ViewportRect.Left &&
                    labelPoint.Y < p.ViewportRect.Bottom &&
                    labelPoint.Y >= p.ViewportRect.Top)
                {
                    return true;
                }
                return false;
            };

            renderer.AdjustMajorValue =
                (p, value, i, interval) =>
                {
                    ScalerParams sParams = new ScalerParams(p.WindowRect, p.ViewportRect, IsInverted);
                    double categoryValue = value;
                    if (this.CategoryMode != CategoryMode.Mode0)
                    {
                        double unscaledValue = (i * interval) + 1;
                        unscaledValue = Math.Min(unscaledValue, this.ItemsCount);

                        double nextCategoryValue = 
                            GetScaledValue(unscaledValue, sParams);

                        categoryValue = (value + nextCategoryValue) / 2;
                    }
                    return categoryValue;
                };

            renderer.GetGroupCenter = GetGroupCenter;
            renderer.GetUnscaledGroupCenter = GetUnscaledGroupCenter;

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

        private AxisRenderingParametersBase CreateRenderingParams(Rect viewportRect, Rect windowRect)
        {
            if (FastItemsSource == null)
            {
                return null;
            }

            var renderingParams = new RadialAxisRenderingParameters();
            int max = FastItemsSource.Count - 1;

            GeometryCollection axisGeometry = View.GetAxisLinesGeometry();
            GeometryCollection stripsGeometry = View.GetStripsGeometry();
            GeometryCollection majorGeometry = View.GetMajorLinesGeometry();
            GeometryCollection minorGeometry = View.GetMinorLinesGeometry();

            renderingParams.AxisGeometry = axisGeometry;
            renderingParams.Strips = stripsGeometry;
            renderingParams.Major = majorGeometry;
            renderingParams.Minor = minorGeometry;
            renderingParams.ActualMaximumValue = max;
            renderingParams.ActualMinimumValue = 0;
            renderingParams.HasUserMax = false;
            renderingParams.ViewportRect = viewportRect;
            renderingParams.WindowRect = windowRect;
            renderingParams.HasUserInterval = HasUserInterval();
            renderingParams.Interval = Interval;
            renderingParams.Label = Label;
//#if TINYCLR
//            //if someone specifies a formatting delegate that will format the labels.
//            if (Label == null && FormatLabel != null)
//            {
//                renderingParams.Label = "Format";
//            }
//#endif

            //Determine the closest and furthest visible radius in the viewing window.
            //These will constrain which gridlines we need to draw.
            double closestRadius = renderingManager.GetClosestRadiusValue(windowRect);
            double furthestRadius = renderingManager.GetFurthestRadiusValue(windowRect);

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
            renderingManager.DetermineView(
                windowRect,
                renderingParams,
                0,
                FastItemsSource.Count,
                IsInverted,
                GetUnscaledAngle,
                resolution);

            Point center = new Point(0.5, 0.5);

            renderingParams.Center = center;
            renderingParams.MaxLength = maxLen;
            renderingParams.MinLength = minLen;
            renderingParams.EffectiveMaximum = effectiveMaximum;

            renderingParams.Count = FastItemsSource.Count;
            renderingParams.CategoryMode = CategoryMode;
            renderingParams.WrapAround = true;

            renderingParams.IsInverted = IsInverted;
            renderingParams.Mode2GroupCount = Mode2GroupCount;

            renderingParams.TickmarkValues = new CategoryTickmarkValues();
            renderingParams.ShouldRenderMinorLines = ShouldRenderMinorLines;
            

            return renderingParams;
        }

        void IAngleScaler.GetMinMaxAngle(Rect windowRect, out double visibleMinimum, out double visibleMaximum)
        {
            GetMinMaxAngle(windowRect, out visibleMinimum, out visibleMaximum);
        }

        internal void GetMinMaxAngle(Rect windowRect, out double visibleMinimum, out double visibleMaximum)
        {
            renderingManager.GetMinMaxAngle(
                windowRect,
                out visibleMinimum,
                out visibleMaximum);
        }


        /// <summary>
        /// When overridden in a derived class, is invoked whenever application code or internal processes
        /// call ApplyTemplate.
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            RenderAxis(false);
        }
        #endregion

        private NumericRadiusAxis _radiusAxis;

        /// <summary>
        /// The radius axis for this angle axis.
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

        #region StartAngleOffset Dependency Property
        /// <summary>
        /// Indicates the angle in degress that the chart's 0th angle should be offset.
        /// <para>This is a dependency property.</para>
        /// </summary>
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
            typeof(CategoryAngleAxis),
            new PropertyMetadata(0.0, (sender, e) =>
            {
                (sender as CategoryAngleAxis).RaisePropertyChanged(StartAngleOffsetPropertyName, e.OldValue, e.NewValue);
            }));
        #endregion

        internal override double GetCategorySize(Rect windowRect, Rect viewportRect)
        {
            return 2 * Math.PI / ItemsCount;
        }
        internal override double GetGroupSize(Rect windowRect, Rect viewportRect)
        {
            double gap = !double.IsNaN(Gap) ? MathUtil.Clamp(Gap, 0.0, 1.0) : 0.0;
            double overlap = 0.0;
            
            if (!double.IsNaN(Overlap))
            {
                overlap = Math.Min(Overlap, 1);
            }
            double categorySpace = 1.0 - 0.5 * gap;

            return GetCategorySize(windowRect, viewportRect) * categorySpace / (Mode2GroupCount - (Mode2GroupCount - 1) * overlap);
        }
        internal override double GetGroupCenter(int groupIndex, Rect windowRect, Rect viewportRect)
        {
            double groupCenter = 0.5;

            if (Mode2GroupCount > 1)
            {
                double gap = !double.IsNaN(Gap) ? MathUtil.Clamp(Gap, 0.0, 1.0) : 0.0;
                double overlap = 0.0;
                if (!double.IsNaN(Overlap))
                {
                    overlap = Math.Min(Overlap, 1);
                }
                double categorySpace = 1.0 - 0.5 * gap;
                double groupWidth = categorySpace / (Mode2GroupCount - (Mode2GroupCount - 1) * overlap);
                double groupSep = (categorySpace - groupWidth) / (Mode2GroupCount - 1);

                groupCenter = 0.25 * gap + 0.5 * groupWidth + groupIndex * groupSep;
            }

            return GetCategorySize(windowRect, viewportRect) * groupCenter;
        }

        internal double GetUnscaledGroupCenter(int groupIndex)
        {
            double groupCenter = 0.5;

            if (Mode2GroupCount > 1)
            {
                double gap = !double.IsNaN(Gap) ? MathUtil.Clamp(Gap, 0.0, 1.0) : 0.0;
                double overlap = 0.0;
                if (!double.IsNaN(Overlap))
                {
                    overlap = Math.Min(Overlap, 1);
                }
                double categorySpace = 1.0 - 0.5 * gap;
                double groupWidth = categorySpace / (Mode2GroupCount - (Mode2GroupCount - 1) * overlap);
                double groupSep = (categorySpace - groupWidth) / (Mode2GroupCount - 1);

                groupCenter = 0.25 * gap + 0.5 * groupWidth + groupIndex * groupSep;
            }

            return groupCenter;
        }

        /// <summary>
        /// Renders the visuals for the current axis.
        /// </summary>
        /// <param name="animate">True if the changes should be animated.</param>
        protected override void RenderAxisOverride(bool animate)
        {
            Rect windowRect = SeriesViewer != null ? SeriesViewer.ActualWindowRect : Rect.Empty;
            Rect viewportRect = !windowRect.IsEmpty ? ViewportRect : Rect.Empty;

            Renderer.Render(animate, viewportRect, windowRect);
        }

        internal double GetMinimumViewable(Rect viewportRect, Rect windowRect)
        {
            double minAngle;
            double maxAngle;
            renderingManager.GetMinMaxAngle(windowRect, out minAngle, out maxAngle);

            if (minAngle == 0)
            {
                if (IsInverted)
                {
                    return ItemsCount;
                }
                else
                {
                    return 0;
                }
            }

            double value = GetUnscaledAngle(minAngle);
            if (value < 0 ||
               value > ItemsCount)
            {
                value = GetUnscaledAngle(minAngle + Math.PI * 2.0);
            }
            return value;
        }

        internal double GetMaximumViewable(Rect viewportRect, Rect windowRect)
        {
            double minAngle;
            double maxAngle;
            renderingManager.GetMinMaxAngle(windowRect, out minAngle, out maxAngle);

            if (maxAngle > Math.PI * 2.0)
            {
                maxAngle = maxAngle - Math.PI * 2.0;
            }

            if (maxAngle == Math.PI * 2.0)
            {
                if (IsInverted)
                {
                    return 0;
                }
                else
                {
                    return ItemsCount;
                }
            }

            double value = GetUnscaledAngle(maxAngle);
            if (value < 0 ||
                value > ItemsCount)
            {
                value = GetUnscaledAngle(maxAngle + Math.PI * 2.0);
            }
            return value;
        }

        /// <summary>
        /// Gets the scaled angle in radians from the raw axis value.
        /// </summary>
        /// <param name="unscaledAngle">The raw axis value.</param>
        /// <returns>The scaled angle in radians.</returns>
        public double GetScaledAngle(double unscaledAngle)
        {
            int itemsCount = ItemsCount;

            double scaledValue = itemsCount >= 2 ? (unscaledAngle) / (double)(itemsCount) :
                                itemsCount == 1 ? 0.5 :
                                double.NaN;

            if (IsInvertedCached)
            {
                scaledValue = 1.0 - scaledValue;
            }

            return (scaledValue * 2.0 * Math.PI) + _startAngleOffsetRadians;
        }

        /// <summary>
        /// Gets the raw axis value from the scaled angle in radians.
        /// </summary>
        /// <param name="scaledAngle"></param>
        /// <returns></returns>
        public double GetUnscaledAngle(double scaledAngle)
        {
            double unscaledValue = (scaledAngle - _startAngleOffsetRadians) / (2.0 * Math.PI);

            if (IsInverted)
            {
                unscaledValue = 1.0 - unscaledValue;
            }

            return unscaledValue * (ItemsCount);
        }

        /// <summary>
        /// Gets the scaled angle in radians from the raw axis value.
        /// </summary>
        /// <param name="unscaledValue">The raw axis value.</param>
        /// <param name="p">Scaler parameters</param>
        /// <returns></returns>
        public override double GetScaledValue(double unscaledValue, ScalerParams p)
        {
            return ((IAngleScaler)this).GetScaledAngle(unscaledValue);
        }


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
                    MustInvalidateLabels = true;
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
                    _startAngleOffsetRadians = (_startAngleOffsetRadians * Math.PI) / 180;
                    MustInvalidateLabels = true;
                    RenderAxis(false);





                    foreach (var s in Series)
                    {

                        s.RenderSeries(false);
                    }
                    break;
                case LabelPropertyName:
                    if (dataChart != null)
                    {
                        foreach (var axis in dataChart.Axes)
                        {
                            axis.MustInvalidateLabels = true;
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
                                axis.MustInvalidateLabels = true;
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

        internal void OnRadiusAxisChanged(NumericRadiusAxis radiusAxis)
        {
            RadiusAxis = radiusAxis;
        }

        private const string IntervalPropertyName = "Interval";
        /// <summary>
        /// Identifies the Interval dependency property.
        /// </summary>
        public static readonly DependencyProperty IntervalProperty = DependencyProperty.Register(IntervalPropertyName, typeof(double), typeof(CategoryAngleAxis), new PropertyMetadata(double.NaN,
            (sender, e) =>
            {
                (sender as CategoryAngleAxis).RaisePropertyChanged(IntervalPropertyName, e.OldValue, e.NewValue);
                (sender as CategoryAngleAxis).RenderAxis(false);
            }));

        /// <summary>
        /// Gets or sets the frequency of displayed labels.
        /// </summary>
        /// <remarks>
        /// The set value is a factor that determines which labels will be hidden. For example, an interval of 2 will display every other label.
        /// </remarks>
        public double Interval
        {
            get
            {
                return (double)this.GetValue(IntervalProperty);
            }
            set
            {
                this.SetValue(IntervalProperty, value);
            }
        }

        /// <summary>
        /// Determines if the axis has a user-defined interval.
        /// </summary>
        /// <returns></returns>
        protected bool HasUserInterval()
        {



            return this.ReadLocalValue(IntervalProperty) != DependencyProperty.UnsetValue;

        }

        /// <summary>
        /// Gets the axis orientation.
        /// </summary>
        protected internal override AxisOrientation Orientation
        {
            get { return AxisOrientation.Angular; }
        }

        internal override bool UpdateRangeOverride()
        {
            if (FastItemsSource == null)
            {
                return false;
            }

            int max = FastItemsSource.Count;

            if (max != ActualMaximum)
            {
                AxisRangeChangedEventArgs ea = new AxisRangeChangedEventArgs(1, 1, ActualMaximum, max);
                ActualMaximum = max;
                RaiseRangeChanged(ea);
                return true;
            }

            return false;
        }


        private int _actualMaximum = 1;
        internal int ActualMaximum
        {
            get
            {
                return _actualMaximum;
            }
            set
            {
                _actualMaximum = value;
            }
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