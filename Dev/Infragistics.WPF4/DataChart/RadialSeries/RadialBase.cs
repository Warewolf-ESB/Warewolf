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
using System.ComponentModel;
using System.Collections.Generic;
using Infragistics.Controls.Charts.Util;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Infragistics.Controls.Charts
{
    internal class RadialFrame : CategoryFrame
    {
        public RadialFrame(int count)
            : base(count)
        {
        }

    }

    /// <summary>
    /// Represents the base class for all XamDataChart radial series
    /// </summary>
    [WidgetModuleParent("RadialChart")]
    public abstract class RadialBase : MarkerSeries, IHasCategoryModePreference
    {
        /// <summary>
        /// Called when the view for the series is created.
        /// </summary>
        /// <param name="view">The created view.</param>
        protected internal override void OnViewCreated(SeriesView view)
        {
            base.OnViewCreated(view);

            RadialView = (RadialBaseView)view;
        }
        internal RadialBaseView RadialView { get; set; }

        #region Constructor and Initialisation
        /// <summary>
        /// Initializes a new instance of the RadialBase class. 
        /// </summary>
        internal RadialBase()
        {
            //ViewportCalculator = new PolarAxisBasedViewportCalculator();
            SeriesRenderer = new SeriesRenderer<RadialFrame, RadialBaseView>(
                PrepareFrame,
                RenderFrame, AnimationActive, StartAnimation,
                (f) => this.RadialView.BucketCalculator.CalculateBuckets(Resolution));            
        }
        #endregion

        CategoryMode IHasCategoryModePreference.PreferredCategoryMode(CategoryAxisBase axis)
        {
            return PreferredCategoryMode(axis);
        }

        CategoryAxisBase IHasCategoryAxis.CategoryAxis
        {
            get { return AngleAxis; }
        }

        internal SeriesRenderer<RadialFrame, RadialBaseView> SeriesRenderer { get; set; }

        #region AngleAxis Dependency Property
        /// <summary>
        /// Gets the effective angle axis for the current series object.
        /// <para>
        /// This is a dependency property.
        /// </para>
        /// </summary>
        public CategoryAngleAxis AngleAxis
        {
            get
            {
                return (CategoryAngleAxis)GetValue(AngleAxisProperty);
            }
            set
            {
                SetValue(AngleAxisProperty, value);
            }
        }

        internal const string AngleAxisPropertyName = "AngleAxis";

        /// <summary>
        /// Identifies the ActualAngleAxis dependency property.
        /// </summary>
        public static readonly DependencyProperty AngleAxisProperty = DependencyProperty.Register(AngleAxisPropertyName, typeof(CategoryAngleAxis), typeof(RadialBase),
            new PropertyMetadata(null, delegate(DependencyObject sender, DependencyPropertyChangedEventArgs e)
            {
                (sender as RadialBase).RaisePropertyChanged(AngleAxisPropertyName, e.OldValue, e.NewValue);
            }));
        #endregion

        #region ValueAxis Dependency Property
        /// <summary>
        /// Gets the effective value axis for the current series object.
        /// <para>
        /// This is a dependency property.
        /// </para>
        /// </summary>
        public NumericRadiusAxis ValueAxis
        {
            get
            {
                return (NumericRadiusAxis)GetValue(ValueAxisProperty);
            }
            set
            {
                SetValue(ValueAxisProperty, value);
            }
        }

        internal const string ValueAxisPropertyName = "ValueAxis";

        /// <summary>
        /// Identifies the ValueAxis dependency property.
        /// </summary>
        public static readonly DependencyProperty ValueAxisProperty = DependencyProperty.Register(ValueAxisPropertyName, typeof(NumericRadiusAxis), typeof(RadialBase),
            new PropertyMetadata(null, delegate(DependencyObject sender, DependencyPropertyChangedEventArgs e)
            {
                (sender as RadialBase).RaisePropertyChanged(ValueAxisPropertyName, e.OldValue, e.NewValue);
            }));
        #endregion

        #region ClipSeriesToBounds Dependency Property
        /// <summary>
        /// Gets or sets whether to clip the series to the bounds.
        /// <para>
        /// This is a dependency property.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Setting this to true can effect performance.
        /// </remarks>
        [WidgetDefaultBoolean(false)]
        public bool ClipSeriesToBounds
        {
            get
            {
                return (bool)GetValue(ClipSeriesToBoundsProperty);
            }
            set
            {
                SetValue(ClipSeriesToBoundsProperty, value);
            }
        }

        internal const string ClipSeriesToBoundsPropertyName = "ClipSeriesToBounds";

        /// <summary>
        /// Identifies the ClipSeriesToBounds dependency property.
        /// </summary>
        public static readonly DependencyProperty ClipSeriesToBoundsProperty = DependencyProperty.Register(ClipSeriesToBoundsPropertyName, typeof(bool), typeof(RadialBase),
            new PropertyMetadata(false, (sender, e) =>
            {
                (sender as RadialBase).RaisePropertyChanged(ClipSeriesToBoundsPropertyName, e.OldValue, e.NewValue);
            }));
        #endregion

        /// <summary>
        /// Overridden in derived classes when they want to respond to the chart's window changing.
        /// </summary>
        /// <param name="oldWindowRect">The old window rectangle of the chart.</param>
        /// <param name="newWindowRect">The new window rectangle of the chart.</param>
        protected override void WindowRectChangedOverride(Rect oldWindowRect, Rect newWindowRect)
        {
            this.RadialView.BucketCalculator.CalculateBuckets(Resolution);
            RenderSeries(false);
        }

        /// <summary>
        /// Overridden in derived classes when they want to respond to the viewport changing.
        /// </summary>
        /// <param name="oldViewportRect">The old viewport rectangle.</param>
        /// <param name="newViewportRect">The new viewport rectangle.</param>
        protected override void ViewportRectChangedOverride(Rect oldViewportRect, Rect newViewportRect)
        {
            this.RadialView.BucketCalculator.CalculateBuckets(Resolution);
            RenderSeries(false);
        }

        internal abstract CategoryMode PreferredCategoryMode(CategoryAxisBase axis);

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
                case SeriesViewerPropertyName:
                    //dont stay registered with axes while not in chart
                    if (oldValue != null && newValue == null)
                    {
                        if (AngleAxis != null)
                        {
                            AngleAxis.DeregisterSeries(this);
                        }
                        if (ValueAxis != null)
                        {
                            ValueAxis.DeregisterSeries(this);
                        }
                    }
                    if (oldValue == null && newValue != null)
                    {
                        if (AngleAxis != null)
                        {
                            AngleAxis.RegisterSeries(this);
                        }
                        if (ValueAxis != null)
                        {
                            ValueAxis.RegisterSeries(this);
                        }
                    }
                    this.RadialView.BucketCalculator.CalculateBuckets(Resolution);
                    RenderSeries(false);
                    break;

                case AngleAxisPropertyName:
                    if (AngleAxis != null && ValueAxis != null)
                    {
                        _axes = new RadialAxes(ValueAxis, AngleAxis);
                    }

                    if (oldValue != null)
                    {
                        (oldValue as Axis).DeregisterSeries(this);
                    }

                    if (newValue != null)
                    {
                        (newValue as Axis).RegisterSeries(this);
                    }
                    this.RadialView.BucketCalculator.CalculateBuckets(Resolution);
                    RenderSeries(false);
                    break;

                case ValueAxisPropertyName:
                    if (AngleAxis != null && ValueAxis != null)
                    {
                        _axes = new RadialAxes(ValueAxis, AngleAxis);
                    }

                    if (oldValue != null)
                    {
                        (oldValue as Axis).DeregisterSeries(this);
                    }

                    if (newValue != null)
                    {
                        (newValue as Axis).RegisterSeries(this);
                    }
                    this.RadialView.BucketCalculator.CalculateBuckets(Resolution);
                    if (ValueAxis == null ||
                        !ValueAxis.UpdateRange())
                    {
                        RenderSeries(false);
                    }

                    break;

                case Series.SyncLinkPropertyName:
                    if (SyncLink != null && SeriesViewer != null)
                    {
                        this.RadialView.BucketCalculator.CalculateBuckets(Resolution);
                        RenderSeries(false);
                    }

                    break;

                case TransitionProgressPropertyName:
                    TransitionFrame.Interpolate((float)TransitionProgress, PreviousFrame, CurrentFrame);

                    if (ClearAndAbortIfInvalid(View))
                    {
                        return;
                    }

                    if (TransitionProgress == 1.0)
                    {
                        RenderFrame(CurrentFrame, RadialView);
                    }
                    else
                    {
                        RenderFrame(TransitionFrame, RadialView);
                    }

                    break;
                case ClipSeriesToBoundsPropertyName:
                    RenderSeries(false);
                    this.NotifyThumbnailAppearanceChanged();
                    break;
                case Series.VisibilityProxyPropertyName:
                    if ((Visibility)oldValue != Visibility.Visible && (Visibility)newValue == Visibility.Visible)
                    {
                        this.RadialView.BucketCalculator.CalculateBuckets(this.Resolution);
                    }
                    break;
            }
        }

        /// <summary>
        /// Gets the item that is the best match for the specified world coordinates.
        /// </summary>
        /// <param name="world">The world coordinates to use.</param>
        /// <returns>The item that is the best match.</returns>
        protected override object GetItem(Point world)
        {
            int index = GetItemIndex(world);
            return index >= 0 && FastItemsSource != null && index < FastItemsSource.Count ? FastItemsSource[index] : null;
        }

        /// <summary>
        /// Get the index of the item near the provided world coordinates.
        /// </summary>
        /// <param name="world">The world coordinates for which to getch the item index.</param>
        /// <returns>The index of the item near the coordinates.</returns>
        protected override int GetItemIndex(Point world)
        {
            Rect windowRect = View.WindowRect;
            Rect viewportRect = View.Viewport;

            int rowIndex = -1;
            if (this.AngleAxis != null && !windowRect.IsEmpty && !viewportRect.IsEmpty
                && _axes != null)
            {
                double angle = _axes.GetAngleTo(world);
                if (angle < 0)
                {
                    angle += Math.PI * 2.0;
                }
                if (angle > Math.PI * 2.0)
                {
                    angle -= Math.PI * 2.0;
                }

                double unscaled = AngleAxis.GetUnscaledAngle(angle);
                if (this.AngleAxis.CategoryMode != CategoryMode.Mode0)
                {
                    unscaled -= .5;
                }
                int index = (int)Math.Round(unscaled);
                rowIndex = index;
            }
            return rowIndex;
        }

        /// <summary>
        /// Scrolls the specified item into the view.
        /// </summary>
        /// <param name="item">The item to scroll into view.</param>
        /// <returns>True if the item has been scrolled into view.</returns>
        public override bool ScrollIntoView(object item)
        {
            return false;
        }

        internal RadialFrame PreviousFrame = new RadialFrame(3);
        internal RadialFrame TransitionFrame = new RadialFrame(3);
        internal RadialFrame CurrentFrame = new RadialFrame(3);

        internal abstract void PrepareFrame(RadialFrame radialFrame, RadialBaseView view);

        internal abstract void RenderFrame(RadialFrame radialFrame, RadialBaseView view);

        internal RadialAxes _axes;

        /// <summary>
        /// Invalidates the axes associatede with the series.
        /// </summary>
        protected internal override void InvalidateAxes()
        {
            base.InvalidateAxes();
            if (this.AngleAxis != null)
            {
                this.AngleAxis.RenderAxis(false);
            }
            if (this.ValueAxis != null)
            {
                this.ValueAxis.RenderAxis(false);
            }
        }

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
            var radialView = (RadialBaseView)view;

            if (!base.ValidateSeries(viewportRect, windowRect, view) || !view.HasSurface()
                || windowRect.IsEmpty
                || viewportRect.IsEmpty
                || AngleAxis == null
                || AngleAxis.ItemsSource == null
                || ValueAxis == null
                || _axes == null
                || FastItemsSource == null
                || AngleAxis.SeriesViewer == null
                || ValueAxis.SeriesViewer == null
                || ValueAxis.ActualMinimumValue == ValueAxis.ActualMaximumValue)
            {
              
                radialView.BucketCalculator.BucketSize = 0;
                isValid = false;
            }

            return isValid;
        }

        /// <summary>
        /// Called to render the series.
        /// </summary>
        /// <param name="animate">True if the change should be animated.</param>
        protected internal override void RenderSeriesOverride(bool animate)
        {
            Rect windowRect;
            Rect viewportRect;

            GetViewInfo(out viewportRect, out windowRect);
            if (!ValidateSeries(viewportRect, windowRect, View))
            {
                ClearRendering(true, View);
                return;
            }

            SeriesRenderingArguments args =
                new SeriesRenderingArguments(this, viewportRect, windowRect, animate);



#region Infragistics Source Cleanup (Region)





#endregion // Infragistics Source Cleanup (Region)

            SeriesRenderer.Render(args, ref PreviousFrame, ref CurrentFrame, ref TransitionFrame, this.RadialView);

        }

        internal int GetMode2Index()
        {
            int result = 0;
            foreach (Series currentSeries in this.SeriesViewer.Series)
            {
                if (currentSeries == this)
                {
                    return result;
                }
                RadialBase currentRadialSeries = currentSeries as RadialBase;
                if (currentRadialSeries != null 
                    && currentRadialSeries.AngleAxis == this.AngleAxis 
                    && currentRadialSeries.PreferredCategoryMode(currentRadialSeries.AngleAxis) == CategoryMode.Mode2)
                {
                    result++;
                }
            }
            Debug.Assert(false, "RadialBase.GetMode2Index failed to find series");
            return -1;
        }
    }

    internal abstract class RadialBucketCalculator : IBucketizer
    {
        protected RadialBaseView View { get; private set; }
        internal RadialBucketCalculator(RadialBaseView view)
        {
            this.View = view;
        }
        /// <summary>
        /// The first visible bucket of values.
        /// </summary>
        protected internal int FirstBucket { get; private set; }

        /// <summary>
        /// The last visible bucket of values.
        /// </summary>
        protected internal int LastBucket { get; private set; }

        /// <summary>
        /// The size of the visible buckets of values.
        /// </summary>
        protected internal int BucketSize { get; set; }

        public virtual float[] GetBucket(int index)
        {
            throw new NotImplementedException();
        }

        public float GetErrorBucket(int index, IFastItemColumn<double> column)
        {
            return float.NaN;
        }

        public void GetBucketInfo(out int firstBucket, out int lastBucket, out int bucketSize, out double resolution)
        {
            firstBucket = this.FirstBucket;
            lastBucket = this.LastBucket;
            bucketSize = this.BucketSize;
            resolution = this.View.RadialModel.Resolution;
        }
        /// <summary>
        /// Caculates which buckets are visible and their size.
        /// </summary>
        /// <param name="resolution">The current resolution of the chart.</param>
        protected internal void CalculateBuckets(double resolution)
        {
            Rect windowRect = View.WindowRect;
            Rect viewportRect = View.Viewport;

            CategoryAngleAxis angleAxis = this.View.RadialModel.AngleAxis;

            if (windowRect.IsEmpty || viewportRect.IsEmpty || angleAxis == null || this.View.RadialModel.FastItemsSource == null || this.View.RadialModel.FastItemsSource.Count == 0)
            {
                BucketSize = 0;
                return;
            }


            double x0 = Math.Floor(angleAxis.GetMinimumViewable(viewportRect, windowRect));
            double x1 = Math.Ceiling(angleAxis.GetMaximumViewable(viewportRect, windowRect));

            if (angleAxis.IsInverted)
            {
                x1 = Math.Ceiling(angleAxis.GetMinimumViewable(viewportRect, windowRect));
                x0 = Math.Floor(angleAxis.GetMaximumViewable(viewportRect, windowRect));
            }
            if (x1 < x0)
            {
                x1 = angleAxis.ItemsCount + x1;
            }
            NumericRadiusAxis valueAxis = this.View.RadialModel.ValueAxis;
            double extentScale = valueAxis != null ? valueAxis.ActualRadiusExtentScale : .75;
            double circum = Math.Min(viewportRect.Width, viewportRect.Height) *
                .5 * (extentScale) *
                2.0 * Math.PI;

            double c = Math.Floor((x1 - x0 + 1.0) * resolution / circum);     // the number of rows per bucket

            BucketSize = (int)Math.Max(1.0, c);
            FirstBucket = (int)Math.Max(0.0, Math.Floor(x0 / BucketSize) - 1.0);            // last invisible bucket
            LastBucket = (int)Math.Ceiling(x1 / BucketSize);                                // first invisible bucket
        }



#region Infragistics Source Cleanup (Region)





#endregion // Infragistics Source Cleanup (Region)

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