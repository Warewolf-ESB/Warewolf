using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Shapes;
using Infragistics.Controls.Charts.Util;
using System.Runtime.CompilerServices;

namespace Infragistics.Controls.Charts
{
    /// <summary>
    /// Represents a XamDataChart bar series.
    /// </summary>
    public sealed class BarSeries : VerticalAnchoredCategorySeries, IIsCategoryBased, IBarSeries
    {
        /// <summary>
        /// Called to create the view for the series.
        /// </summary>
        /// <returns>The created view.</returns>
        protected override SeriesView CreateView()
        {
            return new BarSeriesView(this);
        }

        /// <summary>
        /// Called when the view for the series is created.
        /// </summary>
        /// <param name="view">The created view.</param>
        protected internal override void OnViewCreated(SeriesView view)
        {
            base.OnViewCreated(view);

            BarView = (BarSeriesView)view;
        }
        internal BarSeriesView BarView { get; set; }

        #region constructor and intialization
        /// <summary>
        /// Initializes a new instance of the BarSeries class. 
        /// </summary>
        public BarSeries()
        {
            DefaultStyleKey = typeof(BarSeries);
            FramePreparer = new BarFramePreparer(this, this.BarView, this, this, this.BarView.BucketCalculator);
        }

        internal override CategoryFramePreparer GetFramePreparer(CategorySeriesView view)
        {


            CategorySeriesView categoryView = view as CategorySeriesView;
            if (categoryView != null && categoryView == this.ThumbnailView)
            {
                return new BarFramePreparer(
                            this,
                            categoryView as ISupportsMarkers,
                            this.SeriesViewer.View.OverviewPlusDetailViewportHost,
                            this,
                            categoryView.BucketCalculator);
            }
            else


            {
                return this.FramePreparer;
            }
        }

        /// <summary>
        /// When overridden in a derived class, is invoked whenever application code or internal processes
        /// call ApplyTemplate.
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
        }

        #endregion

        #region RadiusX Dependency Property
        /// <summary>
        /// Gets or sets the x-radius of the ellipse that is used to round the corners of the bar.
        /// <para>
        /// This is a dependency property.
        /// </para>
        /// </summary> 
        [WidgetDefaultNumber(2.0)]
        public double RadiusX
        {
            get
            {
                return (double)GetValue(RadiusXProperty);
            }
            set
            {
                SetValue(RadiusXProperty, value);
            }
        }

        internal const string RadiusXPropertyName = "RadiusX";

        /// <summary>
        /// Identifies the RadiusX dependency property.
        /// </summary>
        public static readonly DependencyProperty RadiusXProperty = DependencyProperty.Register(RadiusXPropertyName, typeof(double), typeof(BarSeries),
            new PropertyMetadata(2.0, (sender, e) =>
            {
                (sender as BarSeries).RaisePropertyChanged(RadiusXPropertyName, e.OldValue, e.NewValue);
            }));
        #endregion

        #region RadiusY Dependency Property
        /// <summary>
        /// Gets or sets the y-radius of the ellipse that is used to round the corners of the bar.
        /// <para>
        /// This is a dependency property.
        /// </para>
        /// </summary>
        public double RadiusY
        {
            get
            {
                return (double)GetValue(RadiusYProperty);
            }
            set
            {
                SetValue(RadiusYProperty, value);
            }
        }

        internal const string RadiusYPropertyName = "RadiusY";

        /// <summary>
        /// Identifies the RadiusY dependency property.
        /// </summary>
        public static readonly DependencyProperty RadiusYProperty = DependencyProperty.Register(RadiusYPropertyName, typeof(double), typeof(BarSeries),
            new PropertyMetadata(2.0, (sender, e) =>
            {
                (sender as BarSeries).RaisePropertyChanged(RadiusYPropertyName, e.OldValue, e.NewValue);
            }));
        #endregion


#region Infragistics Source Cleanup (Region)


























































#endregion // Infragistics Source Cleanup (Region)

        #region Method overrides

        internal override CategoryMode PreferredCategoryMode(CategoryAxisBase axis)
        {
            return CategoryMode.Mode2;
        }
        /// <summary>
        /// Clears the rendering for the series.
        /// </summary>
        /// <param name="wipeClean">True if the cached visuals should also be cleared.</param>
        /// <param name="view">The SeriesView in context.</param>
        protected internal override void ClearRendering(bool wipeClean, SeriesView view)
        {
            base.ClearRendering(wipeClean, view);
            BarSeriesView barView = view as BarSeriesView;
            if (wipeClean && barView != null && barView.Columns != null)
            {
                barView.Columns.Count = 0;
            }
        }
        /// <summary>
        /// Returns the mode 2 index to use for the series.
        /// </summary>
        /// <returns>The mode 2 index.</returns>
        protected internal override int GetMode2Index()
        {
            int result = 0;
            foreach (Series currentSeries in this.SeriesViewer.Series)
            {
                if (currentSeries == this)
                {
                    return result;
                }
                IBarSeries currentCategorySeries = currentSeries as IBarSeries;
                if (currentCategorySeries != null && currentCategorySeries.YAxis == YAxis && currentCategorySeries.GetPreferredCategoryMode() == CategoryMode.Mode2)
                {
                    result++;
                }
            }
            Debug.Assert(false, "CategorySeries.GetMode2Index failed to find series");
            return -1;
        }

        internal override double GetWorldZeroValue(CategorySeriesView view)
        {
            double value = 0.0;

            Rect windowRect = view.WindowRect;
            Rect viewportRect = view.Viewport;
            ScalerParams xParams = new ScalerParams(windowRect, viewportRect, XAxis.IsInverted);

            if (!windowRect.IsEmpty && !viewportRect.IsEmpty && XAxis != null)
            {
                value = XAxis.GetScaledValue(XAxis.ReferenceValue, xParams);
            }

            return value;
        }



#region Infragistics Source Cleanup (Region)












#endregion // Infragistics Source Cleanup (Region)



        /// <summary>
        /// Returns the range requirement of this series on the given axis.
        /// </summary>
        /// <param name="axis">The axis for which to provide the range requirement.</param>
        /// <returns>The axis range needed for this series to fully display, or null, if there is no range requirement.</returns>
        protected internal override AxisRange GetRange(Axis axis)
        {
            if (ValueColumn == null || ValueColumn.Count == 0)
            {
                return null;
            }

            if (axis == YAxis)
            {
                return new AxisRange(0, ValueColumn.Count - 1);
            }

            if (axis == XAxis)
            {
                return new AxisRange(ValueColumn.Minimum, ValueColumn.Maximum);
            }

            return null;
        }

        /// <summary>
        /// Scrolls the series to display the item for the specified data item.
        /// </summary>
        /// <remarks>
        /// The series is scrolled by the minimum amount required to place the specified data item within
        /// the central 80% of the visible axis.
        /// </remarks>
        /// <param name="item">The data item (item) to scroll to.</param>
        /// <returns>True if the specified item could be displayed.</returns>
        public override bool ScrollIntoView(object item)
        {
            Rect windowRect = this.View.WindowRect;
            Rect viewportRect = this.View.Viewport;
            Rect unitRect = new Rect(0, 0, 1, 1);
            int index =
                !windowRect.IsEmpty
                && !viewportRect.IsEmpty
                && FastItemsSource != null ? FastItemsSource[item] : -1;
            ScalerParams xParams = new ScalerParams(unitRect, unitRect, XAxis.IsInverted);
            ScalerParams yParams = new ScalerParams(unitRect, unitRect, YAxis.IsInverted);
            double cy = YAxis != null ? YAxis.GetScaledValue(index, yParams) : double.NaN;
            double offset = YAxis != null ? FramePreparer.GetOffset(YAxis, unitRect, unitRect) : 0.0;
            cy += offset;
            double cx =
                XAxis != null
                && ValueColumn != null
                && index < ValueColumn.Count ? XAxis.GetScaledValue(ValueColumn[index], xParams) : double.NaN;

            if (!double.IsNaN(cx))
            {
                if (cx < windowRect.Left + 0.1 * windowRect.Width)
                {
                    cx = cx + 0.4 * windowRect.Width;
                    windowRect.X = cx - 0.5 * windowRect.Width;
                }

                if (cx > windowRect.Right - 0.1 * windowRect.Width)
                {
                    cx = cx - 0.4 * windowRect.Width;
                    windowRect.X = cx - 0.5 * windowRect.Width;
                }
            }

            if (!double.IsNaN(cy))
            {
                if (cy < windowRect.Top + 0.1 * windowRect.Height)
                {
                    cy = cy + 0.4 * windowRect.Height;
                    windowRect.Y = cy - 0.5 * windowRect.Height;
                }

                if (cy > windowRect.Bottom - 0.1 * windowRect.Height)
                {
                    cy = cy - 0.4 * windowRect.Height;
                    windowRect.Y = cy - 0.5 * windowRect.Height;
                }
            }

            if (SyncLink != null)
            {
                SyncLink.WindowNotify(SeriesViewer, windowRect);
            }

            return index >= 0;
        }

        internal override void RenderFrame(CategoryFrame frame, CategorySeriesView view)
        {
            base.RenderFrame(frame, view);

            // this is the brain-dead version. one bar per bucket

            List<float[]> buckets = frame.Buckets;

            if (!view.HasSurface())
            {
                return;
            }

            // GetViewInfo(out viewportRect, out windowRect);
            Rect windowRect = view.WindowRect;
            Rect viewportRect = view.Viewport;
            ScalerParams xParams = new ScalerParams(windowRect, viewportRect, XAxis.IsInverted);

            NumericXAxis xscale = XAxis;
            //CategoryYAxis yscale = YAxis;

            double zero = xscale.GetScaledValue(xscale.ReferenceValue, xParams);
            double groupWidth = YAxis.GetGroupSize(windowRect, viewportRect);

            BarSeriesView barView = view as BarSeriesView;

            if (double.IsNaN(groupWidth) || double.IsInfinity(groupWidth) || double.IsNaN(zero))
            {
                barView.Columns.Count = 0;
                return;
            }

            for (int i = 0; i < buckets.Count; ++i)
            {
                double top = buckets[i][0] - 0.5 * groupWidth;
                //double bottom = top + groupWidth;
                double right = buckets[i][1];
                double left = zero;

                //to avoid a glitch when most of the bar renders outside the viewport
                //we clip the rectangle. 100px should be sufficient.
                left = Math.Max(left, -100);
                right = Math.Min(right, viewportRect.Width + 100);

                Rectangle column = barView.Columns[i];
                column.Height = groupWidth;
                column.Width = Math.Abs(right - left);
                barView.SetColumnPosition(column, Math.Min(right, left), top);
            }

            barView.Columns.Count = buckets.Count;
        }

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
            switch (action)
            {
                case FastItemsSourceEventAction.Reset:
                case FastItemsSourceEventAction.Insert:
                case FastItemsSourceEventAction.Remove:
                    this.AnchoredView.BucketCalculator.CalculateBuckets(Resolution);
                    break;
            }

            switch (action)
            {
                case FastItemsSourceEventAction.Reset:
                    if (XAxis != null && !XAxis.UpdateRange())
                    {
                        RenderSeries(true);
                    }

                    break;

                case FastItemsSourceEventAction.Insert:
                    if (XAxis != null && !XAxis.UpdateRange())
                    {
                        RenderSeries(true);
                    }
                    break;

                case FastItemsSourceEventAction.Remove:
                    if (XAxis != null && !XAxis.UpdateRange())
                    {
                        RenderSeries(true);
                    }
                    break;

                case FastItemsSourceEventAction.Replace:
                    if (ValueMemberPath != null && this.AnchoredView.BucketCalculator.BucketSize > 0)
                    {
                        RenderSeries(true);
                    }
                    break;

                case FastItemsSourceEventAction.Change:
                    if (propertyName == ValueMemberPath)
                    {
                        if (XAxis != null && !XAxis.UpdateRange())
                        {
                            RenderSeries(true);
                        }
                    }

                    break;
            }
        }

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
                case XAxisPropertyName:
                    if (oldValue != null)
                    {
                        (oldValue as Axis).DeregisterSeries(this);
                    }

                    if (newValue != null)
                    {
                        (newValue as Axis).RegisterSeries(this);
                    }

                    this.AnchoredView.BucketCalculator.CalculateBuckets(Resolution);

                    if (XAxis != null && XAxis.UpdateRange())
                    {
                        RenderSeries(false);
                    }
                    break;

                case YAxisPropertyName:
                    if (oldValue != null)
                    {
                        (oldValue as Axis).DeregisterSeries(this);
                    }

                    if (newValue != null)
                    {
                        (newValue as Axis).RegisterSeries(this);
                    }

                    this.AnchoredView.TrendLineManager = CategoryTrendLineManagerBase.SelectManager(
                       this.AnchoredView.TrendLineManager, YAxis, RootCanvas, this);

                    this.AnchoredView.BucketCalculator.CalculateBuckets(Resolution);
                    RenderSeries(false);

                    break;

                case Series.FastItemsSourcePropertyName:
                    if (XAxis != null && !XAxis.UpdateRange())
                    {
                        this.AnchoredView.BucketCalculator.CalculateBuckets(Resolution);
                        RenderSeries(false);
                    }

                    break;

                case ValueColumnPropertyName:
                    if (XAxis != null && !XAxis.UpdateRange())
                    {
                        this.AnchoredView.BucketCalculator.CalculateBuckets(Resolution);
                        RenderSeries(false);
                    }
                    break;

                case Series.SeriesViewerPropertyName:
                    //dont stay registered with axes while not in chart
                    if (oldValue != null && newValue == null)
                    {
                        DeregisterForAxis(XAxis);
                        DeregisterForAxis(YAxis);
                    }
                    if (oldValue == null && newValue != null)
                    {
                        RegisterForAxis(XAxis);
                        RegisterForAxis(YAxis);
                    }

                    this.AnchoredView.BucketCalculator.CalculateBuckets(Resolution);
                    RenderSeries(false);
                    break;
            }
        }

        #endregion

        #region Data and Buckets

        /// <summary>
        /// Gets the index of the item that resides at the provided world coordinates.
        /// </summary>
        /// <param name="world">The world coordinates of the requested item.</param>
        /// <returns>The requested item's index.</returns>
        protected override int GetItemIndex(Point world)
        {
            Rect windowRect = SeriesViewer != null ? SeriesViewer.ActualWindowRect : Rect.Empty;
            Rect viewportRect = View.Viewport;

            int rowIndex = -1;
            if (this.YAxis != null && !windowRect.IsEmpty && !viewportRect.IsEmpty)
            {
                double top = YAxis.GetUnscaledValue(viewportRect.Top, windowRect, viewportRect, YAxis.CategoryMode);
                double bottom = YAxis.GetUnscaledValue(viewportRect.Bottom, windowRect, viewportRect, YAxis.CategoryMode);

                double windowY = (world.Y - windowRect.Top) / windowRect.Height;
                double bucket = top + (windowY * (bottom - top));
                if (YAxis.CategoryMode != CategoryMode.Mode0)
                {
                    bucket -= .5;
                }
                int bucketNumber = (int)Math.Round(bucket);

                //the row index is the bucket number at this point. It doesn't depend on the bucket size, because 
                //GetUnscaledValue uses total items count
                //rowIndex = bucketNumber * BucketSize;

                rowIndex = bucketNumber;
            }
            return rowIndex;
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


        #endregion

        #region IBarSeries implementation
        CategoryMode IBarSeries.GetPreferredCategoryMode()
        {
            return PreferredCategoryMode(YAxis);
        }



#region Infragistics Source Cleanup (Region)





















#endregion // Infragistics Source Cleanup (Region)


        #endregion
        
        #region IScaler overrides
        CategoryMode IIsCategoryBased.CurrentCategoryMode
        {
            get { return PreferredCategoryMode(YAxis); }
        }

        ICategoryScaler IIsCategoryBased.Scaler
        {
            get { return YAxis; }
        }

        IScaler IIsCategoryBased.YScaler
        {
            get { return XAxis; }
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