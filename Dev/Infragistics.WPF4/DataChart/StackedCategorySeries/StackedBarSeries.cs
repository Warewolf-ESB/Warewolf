using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Infragistics.Controls.Charts.Util;

namespace Infragistics.Controls.Charts
{
    /// <summary>
    /// Represents a XamDataChart stacked bar series.
    /// </summary>
    public class StackedBarSeries : VerticalStackedSeriesBase, IIsCategoryBased, IBarSeries
    {
        #region C'tor & Initialization
        /// <summary>
        /// Initializes a new instance of a StackedBarSeries class.
        /// </summary>
        public StackedBarSeries()
        {
            DefaultStyleKey = typeof(StackedBarSeries);
        }

        /// <summary>
        /// When overridden in a derived class, is invoked whenever application code or internal processes
        /// call ApplyTemplate.
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            RenderSeries(false);
        }
        #endregion

        #region View-related
        /// <summary>
        /// Called to create the view for the series.
        /// </summary>
        /// <returns>The created view.</returns>
        protected override SeriesView CreateView()
        {
            return new StackedBarSeriesView(this);
        }
        
        internal StackedBarSeriesView StackedBarView { get; set; }

        /// <summary>
        /// Called when the view has been created.
        /// </summary>
        /// <param name="view">The view class for the current series</param>
        protected internal override void OnViewCreated(SeriesView view)
        {
            base.OnViewCreated(view);
            StackedBarView = (StackedBarSeriesView)view;
        }
        #endregion

        #region Public Properties
        #region RadiusX Dependency Property
        /// <summary>
        /// Gets or sets the x-radius of the ellipse that is used to round the corners of the bar.
        /// <para>
        /// This is a dependency property.
        /// </para>
        /// </summary>  
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
        public static readonly DependencyProperty RadiusXProperty = DependencyProperty.Register(RadiusXPropertyName, typeof(double), typeof(StackedBarSeries),
            new PropertyMetadata(2.0, (sender, e) =>
            {
                (sender as StackedBarSeries).RaisePropertyChanged(RadiusXPropertyName, e.OldValue, e.NewValue);
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
        public static readonly DependencyProperty RadiusYProperty = DependencyProperty.Register(RadiusYPropertyName, typeof(double), typeof(StackedBarSeries),
            new PropertyMetadata(2.0, (sender, e) =>
            {
                (sender as StackedBarSeries).RaisePropertyChanged(RadiusYPropertyName, e.OldValue, e.NewValue);
            }));
        #endregion


#region Infragistics Source Cleanup (Region)


























































#endregion // Infragistics Source Cleanup (Region)

        #endregion

        #region Member Overrides

        internal override CategorySeriesView GetSeriesView()
        {
            return StackedBarView;
        }

        internal override CategoryMode PreferredCategoryMode(CategoryAxisBase axis)
        {
            return CategoryMode.Mode2;
        }

        /// <summary>
        /// Returns CategoryMode2 index of the current series.
        /// </summary>
        /// <returns>CategoryMode2 index</returns>
        protected internal override int GetMode2Index()
        {
            int result = 0;
            foreach (Series currentSeries in SeriesViewer.Series)
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

        /// <summary>
        /// Returns the scaled zero value based on the axis reference value.
        /// </summary>
        internal override double GetScaledWorldZeroValue()
        {
            double value = 0.0;

            Rect windowRect = SeriesViewer != null ? SeriesViewer.ActualWindowRect : Rect.Empty;
            Rect viewportRect = View.Viewport;
            ScalerParams xParams = new ScalerParams(windowRect, viewportRect, XAxis.IsInverted);

            if (!windowRect.IsEmpty && !viewportRect.IsEmpty && XAxis != null)
            {
                value = XAxis.GetScaledValue(XAxis.ReferenceValue, xParams);
            }

            return value;
        }

        /// <summary>
        /// Returns the unscaled zero value based on the axis reference value.
        /// </summary>
        internal override double GetUnscaledWorldZeroValue()
        {
            if (XAxis != null)
            {
                return XAxis.ReferenceValue;
            }

            return 0.0;
        }

        /// <summary>
        /// Returns the range requirement of this series on the given axis.
        /// </summary>
        /// <param name="axis">The axis for which to provide the range requirement.</param>
        /// <returns>The axis range needed for this series to fully display, or null, if there is no range requirement.</returns>
        protected internal override AxisRange GetRange(Axis axis)
        {
            if (ItemsSource == null)
            {
                return null;
            }

            if (axis == YAxis)
            {
                return new AxisRange(0, FastItemsSource.Count - 1);
            }

            if (axis == XAxis)
            {
                PrepareData();
                return new AxisRange(Minimum, Maximum);
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
            int index = FastItemsSource != null ? FastItemsSource[item] : -1;
            Rect windowRect = SeriesViewer != null ? SeriesViewer.ActualWindowRect : Rect.Empty;
            Rect viewportRect = View.Viewport;
            Rect unitRect = new Rect(0, 0, 1, 1);
            ScalerParams xParams = new ScalerParams(unitRect, unitRect, XAxis.IsInverted);
            ScalerParams yParams = new ScalerParams(unitRect, unitRect, YAxis.IsInverted);
            if (index >= 0 && windowRect != null && viewportRect != null)
            {
                if (YAxis != null)
                {
                    double cy = YAxis.GetScaledValue(index, yParams);

                    if (cy < windowRect.Top + 0.1 * windowRect.Height)
                    {
                        cy = cy + 0.4 * windowRect.Height;
                    }

                    if (cy > windowRect.Bottom - 0.1 * windowRect.Height)
                    {
                        cy = cy - 0.4 * windowRect.Height;
                    }

                    windowRect.Y = cy - 0.5 * windowRect.Height;
                }

                if (XAxis != null && Highs != null && index < Highs.Length)
                {
                    double high = this.XAxis.GetScaledValue(Highs[index], xParams);
                    double low = this.XAxis.GetScaledValue(Lows[index], xParams);
                    if (!double.IsNaN(high) && !double.IsNaN(low))
                    {
                        double width = Math.Abs(low - high);
                        if (windowRect.Width < width)
                        {
                            windowRect.Width = width;
                            windowRect.X = Math.Min(low, high);
                        }
                        else
                        {
                            if (low < windowRect.Left + 0.1 * windowRect.Width)
                            {
                                low = low + 0.4 * windowRect.Width;
                            }

                            if (low > windowRect.Right - 0.1 * windowRect.Width)
                            {
                                low = low - 0.4 * windowRect.Width;
                            }

                            windowRect.X = low - 0.5 * windowRect.Width;
                        }
                    }
                }

                SyncLink.WindowNotify(SeriesViewer, windowRect);
            }

            return index >= 0;
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
            var categoryView = (CategorySeriesView)view;

            if (!view.HasSurface()
                || windowRect.IsEmpty
                || viewportRect.IsEmpty
                || YAxis == null
                || YAxis.ItemsSource == null
                || XAxis == null
                || FastItemsSource == null
                || XAxis.SeriesViewer == null
                || YAxis.SeriesViewer == null
                || XAxis.ActualMinimumValue == XAxis.ActualMaximumValue)
            {
                categoryView.BucketCalculator.BucketSize = 0;
                isValid = false;
            }

            return isValid;
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
                    this.CategoryView.BucketCalculator.CalculateBuckets(Resolution);
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
                    if (this.CategoryView.BucketCalculator.BucketSize > 0)
                    {
                        RenderSeries(true);
                    }
                    break;

                case FastItemsSourceEventAction.Change:
                    if (XAxis != null && !XAxis.UpdateRange())
                    {
                        RenderSeries(true);
                    }
                    break;
            }
        }

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
            if (YAxis != null && !windowRect.IsEmpty && !viewportRect.IsEmpty)
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

        internal override void UpdateAxisRanges()
        {
            if (XAxis != null)
                XAxis.UpdateRange(true);

            if (YAxis != null)
                YAxis.UpdateRange(true);
        }

        internal override int GetFragmentItemIndex(Point world)
        {
            return GetItemIndex(world);
        }

        internal override bool ValidateFragmentSeries(AnchoredCategorySeries series, Rect viewportRect, Rect windowRect, SeriesView view)
        {
            bool isValid = true;

            if (!view.HasSurface()
                || windowRect.IsEmpty
                || viewportRect.IsEmpty
                || YAxis == null
                || YAxis.ItemsSource == null
                || XAxis == null
                || FastItemsSource == null
                || XAxis.SeriesViewer == null
                || YAxis.SeriesViewer == null)
            {
                isValid = false;
            }

            var categoryView = (CategorySeriesView)view;
            if (series.ValueColumn == null || series.ValueColumn.Count == 0 || categoryView.BucketCalculator.BucketSize < 1)
            {
                isValid = false;
            }
            return isValid;
        }

        internal override void RenderFragment(AnchoredCategorySeries series, CategoryFrame frame, CategorySeriesView view)
        {
            BarFragment barSeries = series as BarFragment;
            ColumnFragmentView fragmentView = view as ColumnFragmentView;
            if (!ValidateSeries(view.Viewport, view.WindowRect, view) || barSeries == null || fragmentView == null)
            {
                return;
            }

            double groupWidth = YAxis.GetGroupSize(view.WindowRect, view.Viewport);
            
            if (double.IsNaN(groupWidth) || double.IsInfinity(groupWidth))
            {
                barSeries.ColumnFragmentView.Columns.Count = 0;
                return;
            }

            int counter = 0;

            foreach (var bucket in frame.Buckets)
            {
                //avoid trying to render rectangles with invalid dimensions, skip to the next one.
                if (double.IsInfinity(bucket[0]) || double.IsNaN(bucket[0])
                    || double.IsInfinity(bucket[1]) || double.IsInfinity(bucket[2])
                    || double.IsNaN(bucket[1]) || double.IsNaN(bucket[2]))
                {
                    continue;
                }

                double top = bucket[0] - 0.5 * groupWidth;
                double right = bucket[1];
                double left = bucket[2];

                //to avoid a glitch when most of the bar renders outside the viewport
                //we clip the rectangle. 100px should be sufficient.
                left = Math.Max(left, -100);
                right = Math.Min(right, view.Viewport.Width + 100);

                Rectangle column = fragmentView.Columns[counter];
                column.Height = groupWidth;
                column.Width = Math.Abs(right - left);
                column.RenderTransform = new TranslateTransform() { X = Math.Min(right, left), Y = top };

                counter++;
            }

            fragmentView.Columns.Count = counter;
        }
        #endregion

        #region Property Updates
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
                case RadiusXPropertyName:
                case RadiusYPropertyName:
                    foreach (var series in ActualSeries)
                    {
                        series.UpdateRadiusX();
                        series.UpdateRadiusY();
                    }
                    RenderSeries(false);
                    break;

                case SyncLinkPropertyName:
                    if (XAxis != null) XAxis.UpdateRange();
                    break;

                case CategorySeries.FastItemsSourcePropertyName:
                    if (XAxis != null && !XAxis.UpdateRange())
                    {
                        StackedBarView.BucketCalculator.CalculateBuckets(Resolution);
                    }
                    RenderSeries(false);
                    break;

                case SeriesViewerPropertyName:
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

                    this.StackedBarView.BucketCalculator.CalculateBuckets(Resolution);
                    RenderSeries(false);

                    if (XAxis != null) XAxis.UpdateRange();
                    break;
            }
        }

        #endregion

        #region IBarSeries implementation
        CategoryMode IBarSeries.GetPreferredCategoryMode()
        {
            return PreferredCategoryMode(YAxis);
        }
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