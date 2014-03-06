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
using Infragistics.Controls.Charts.Util;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Collections.Generic;


namespace Infragistics.Controls.Charts
{
    /// <summary>
    /// Base class for ranged category series with a category X-axis and a numeric Y-axis.
    /// </summary>
    public abstract class HorizontalRangeCategorySeries : RangeCategorySeries
    {
        #region XAxis Dependency Property
        /// <summary>
        /// Gets or sets the effective x-axis for the current CategorySeries object.
        /// <para>
        /// This is a dependency property.
        /// </para>
        /// </summary>
        public CategoryAxisBase XAxis
        {
            get
            {
                return (CategoryAxisBase)GetValue(XAxisProperty);
            }
            set
            {
                SetValue(XAxisProperty, value);
            }
        }

        internal const string XAxisPropertyName = "XAxis";

        /// <summary>
        /// Identifies the ActualXAxis dependency property.
        /// </summary>
        public static readonly DependencyProperty XAxisProperty = DependencyProperty.Register(XAxisPropertyName, typeof(CategoryAxisBase), typeof(HorizontalRangeCategorySeries),
            new PropertyMetadata(null, delegate(DependencyObject sender, DependencyPropertyChangedEventArgs e)
            {
                (sender as HorizontalRangeCategorySeries).RaisePropertyChanged(XAxisPropertyName, e.OldValue, e.NewValue);
            }));
        #endregion

        #region YAxis Depedency Property
        /// <summary>
        /// Gets or sets the effective y-axis for the current CategorySeries object.
        /// <para>
        /// This is a dependency property.
        /// </para>
        /// </summary>
        public NumericYAxis YAxis
        {
            get
            {
                return (NumericYAxis)GetValue(YAxisProperty);
            }
            set
            {
                SetValue(YAxisProperty, value);
            }
        }

        internal const string YAxisPropertyName = "YAxis";

        /// <summary>
        /// Identifies the ActualYAxis dependency property.
        /// </summary>
        public static readonly DependencyProperty YAxisProperty = DependencyProperty.Register(YAxisPropertyName, typeof(NumericYAxis), typeof(HorizontalRangeCategorySeries),
            new PropertyMetadata(null, delegate(DependencyObject sender, DependencyPropertyChangedEventArgs e)
            {
                (sender as HorizontalRangeCategorySeries).RaisePropertyChanged(YAxisPropertyName, e.OldValue, e.NewValue);
            }));
        #endregion
        internal override Axis GetXAxis()
        {
            return this.XAxis;
        }
        internal override Axis GetYAxis()
        {
            return this.YAxis;
        }
        internal override bool UpdateNumericAxisRange()
        {
            return this.YAxis != null && this.YAxis.UpdateRange();
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
                    DeregisterForAxis(oldValue as Axis);
                    RegisterForAxis(newValue as Axis);
                    this.CategoryView.BucketCalculator.CalculateBuckets(Resolution);
                    RenderSeries(false);
                    this.NotifyThumbnailAppearanceChanged();
                    break;

                case YAxisPropertyName:
                    DeregisterForAxis(oldValue as Axis);
                    RegisterForAxis(newValue as Axis);
                    this.CategoryView.BucketCalculator.CalculateBuckets(Resolution);
                    this.UpdateNumericAxisRange();
                    RenderSeries(false);
                    this.NotifyThumbnailAppearanceChanged();
                    break;
            }
        }
    }

    /// <summary>
    /// Represents the base class for all XamDataChart ranged category/value series.
    /// </summary>
    [WidgetModuleParent("RangeCategoryChart")]
    public abstract class RangeCategorySeries : CategorySeries, IIsCategoryBased, IHasHighLowValueCategory
    {
        /// <summary>
        /// Called to create the view for the series.
        /// </summary>
        /// <returns>The created view.</returns>
        protected override SeriesView CreateView()
        {
            return new RangeCategorySeriesView(this);
        }

        /// <summary>
        /// Called when the view for the series is created.
        /// </summary>
        /// <param name="view">The created view.</param>
        protected internal override void OnViewCreated(SeriesView view)
        {
            base.OnViewCreated(view);

            RangeView = (RangeCategorySeriesView)view;
        }
        internal RangeCategorySeriesView RangeView { get; set; }

        #region Constructor and Initialisation
        /// <summary>
        /// Initializes a default, empty RangeCategorySeries object.
        /// </summary>
        internal protected RangeCategorySeries()
        {
            FramePreparer = new RangeCategoryFramePreparer(this, this.RangeView, this, this, this.RangeView.BucketCalculator);
        }

        internal RangeCategoryFramePreparer FramePreparer { get; set; }

        #endregion

        #region LowMemberPath Dependency Property
        /// <summary>
        /// Gets or sets the value mapping property for the current series object.
        /// <para>
        /// This is a dependency property.
        /// </para>
        /// </summary>
        public string LowMemberPath
        {
            get
            {
                return (string)GetValue(LowMemberPathProperty);
            }
            set
            {
                SetValue(LowMemberPathProperty, value);
            }
        }

        internal const string LowMemberPathPropertyName = "LowMemberPath";

        /// <summary>
        /// Identifies the LowMemberPath dependency property.
        /// </summary>
        public static readonly DependencyProperty LowMemberPathProperty = DependencyProperty.Register(LowMemberPathPropertyName, typeof(string), typeof(RangeCategorySeries), new PropertyMetadata(null, delegate(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            (sender as RangeCategorySeries).RaisePropertyChanged(LowMemberPathPropertyName, e.OldValue, e.NewValue);
        }));

        /// <summary>
        /// Gets the column representing the Low values for the data source.
        /// </summary>
        protected internal IFastItemColumn<double> LowColumn
        {
            get { return lowColumn; }
            private set
            {
                if (lowColumn != value)
                {
                    IFastItemColumn<double> oldLowColumn = lowColumn;

                    lowColumn = value;
                    RaisePropertyChanged(LowColumnPropertyName, oldLowColumn, lowColumn);
                }
            }
        }
        private IFastItemColumn<double> lowColumn;
        internal const string LowColumnPropertyName = "LowColumn";
        #endregion

        #region HighMemberPath Dependency Property
        /// <summary>
        /// Gets or sets the value mapping property for the current series object.
        /// <para>
        /// This is a dependency property.
        /// </para>
        /// </summary>
        public string HighMemberPath
        {
            get
            {
                return (string)GetValue(HighMemberPathProperty);
            }
            set
            {
                SetValue(HighMemberPathProperty, value);
            }
        }

        internal const string HighMemberPathPropertyName = "HighMemberPath";

        /// <summary>
        /// Identifies the HighMemberPath dependency property.
        /// </summary>
        public static readonly DependencyProperty HighMemberPathProperty = DependencyProperty.Register(HighMemberPathPropertyName, typeof(string), typeof(RangeCategorySeries), new PropertyMetadata(null, delegate(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            (sender as RangeCategorySeries).RaisePropertyChanged(HighMemberPathPropertyName, e.OldValue, e.NewValue);
        }));

        /// <summary>
        /// Gets the column representing the High values of the data source.
        /// </summary>
        protected internal IFastItemColumn<double> HighColumn
        {
            get { return highColumn; }
            private set
            {
                if (highColumn != value)
                {
                    IFastItemColumn<double> oldHighColumn = highColumn;

                    highColumn = value;
                    RaisePropertyChanged(HighColumnPropertyName, oldHighColumn, highColumn);
                }
            }
        }
        private IFastItemColumn<double> highColumn;
        internal const string HighColumnPropertyName = "HighColumn";
        #endregion

        internal override void RenderFrame(CategoryFrame frame, CategorySeriesView view)
        {
            base.RenderFrame(frame, view);
            CategoryMarkerManager.RasterizeMarkers(this, frame.Markers, view.Markers, UseLightweightMarkers);
        }



#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

        /// <summary>
        /// Renders into a floating polygon
        /// </summary>
        /// <param name="polyline0"></param>
        /// <param name="polygon01"></param>
        /// <param name="polyline1"></param>
        /// <param name="count">Number of points</param>
        /// <param name="x0">Bottom points x coordinate indexed left to right</param>
        /// <param name="y0">Bottom points y coordinate indexed left to right</param>
        /// <param name="x1">Top points x coordinate indexed right to left</param>
        /// <param name="y1">Top points y coordinate indexed right to left</param>        
        protected void RasterizePolygon(Polyline polyline0, Polygon polygon01, Polyline polyline1,
                                                int count,
                                                Func<int, double> x0, Func<int, double> y0,
                                                Func<int, double> x1, Func<int, double> y1)
        {
            RangeView.RasterizePolygon(polyline0, polygon01, polyline1, count, x0, y0, x1, y1);
        }


        #region CategorySeries Implementation

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
            Rect windowRect = View != null ? View.WindowRect : Rect.Empty;
            Rect viewportRect = View != null ? View.Viewport : Rect.Empty;
            int index = !windowRect.IsEmpty && !viewportRect.IsEmpty && FastItemsSource != null ? FastItemsSource.IndexOf(item) : -1;
            
            Axis xAxis = this.GetXAxis();
           
            double cx;
            if (xAxis != null)
            {
                Rect unitRect = new Rect(0, 0, 1, 1);
                ScalerParams xParams = new ScalerParams(unitRect, unitRect, xAxis.IsInverted);
                cx = xAxis.GetScaledValue(index, xParams);
            }
            else
            {
                cx = double.NaN;
            }
            double offset = xAxis != null ? FramePreparer.GetOffset(xAxis as ICategoryScaler, new Rect(0, 0, 1, 1), new Rect(0, 0, 1, 1)) : 0.0;
            cx += offset;

            if (index >= 0 && windowRect != null && viewportRect != null)
            {
                if (!double.IsNaN(cx))
                {
                    if (cx < windowRect.Left + 0.1 * windowRect.Width)
                    {
                        cx = cx + 0.4 * windowRect.Width;
                    }

                    if (cx > windowRect.Right - 0.1 * windowRect.Width)
                    {
                        cx = cx - 0.4 * windowRect.Width;
                    }

                    windowRect.X = cx - 0.5 * windowRect.Width;
                }

                Axis yAxis = this.GetYAxis();

                if (yAxis != null && HighColumn != null && index < HighColumn.Count)
                {
                    ScalerParams yParams = new ScalerParams(windowRect, viewportRect, yAxis.IsInverted);
                    double high = yAxis.GetScaledValue(HighColumn[index], yParams);
                    double low = yAxis.GetScaledValue(LowColumn[index], yParams);
                    if (!double.IsNaN(high) && !double.IsNaN(low))
                    {
                        double height = Math.Abs(low - high);
                        if (windowRect.Height < height)
                        {
                            windowRect.Height = height;
                            windowRect.Y = Math.Min(low, high);
                        }
                        else
                        {
                            if (low < windowRect.Top + 0.1 * windowRect.Height)
                            {
                                low = low + 0.4 * windowRect.Height;
                            }

                            if (low > windowRect.Bottom - 0.1 * windowRect.Height)
                            {
                                low = low - 0.4 * windowRect.Height;
                            }

                            windowRect.Y = low - 0.5 * windowRect.Height;
                        }
                    }
                }

                if (SyncLink != null)
                {
                    SyncLink.WindowNotify(SeriesViewer, windowRect);
                }
            }

            return index >= 0;
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
                case Series.FastItemsSourcePropertyName:
                    if (oldValue as FastItemsSource != null)
                    {
                        (oldValue as FastItemsSource).DeregisterColumn(LowColumn);
                        (oldValue as FastItemsSource).DeregisterColumn(HighColumn);
                        LowColumn = null;
                        HighColumn = null;
                    }

                    if (newValue as FastItemsSource != null)
                    {
                        LowColumn = RegisterDoubleColumn(LowMemberPath);
                        HighColumn = RegisterDoubleColumn(HighMemberPath);
                    }
                    if (!this.UpdateNumericAxisRange())
                    {
                        this.CategoryView.BucketCalculator.CalculateBuckets(Resolution);
                        RenderSeries(false);
                    }

                    break;

                case LowMemberPathPropertyName:
                    if (FastItemsSource != null)
                    {
                        FastItemsSource.DeregisterColumn(LowColumn);
                        LowColumn = RegisterDoubleColumn(LowMemberPath);
                    }
                    break;

                case LowColumnPropertyName:
                    if (!this.UpdateNumericAxisRange())
                    {
                        this.CategoryView.BucketCalculator.CalculateBuckets(Resolution);
                        RenderSeries(false);
                    }
                    break;

                case HighMemberPathPropertyName:
                    if (FastItemsSource != null)
                    {
                        FastItemsSource.DeregisterColumn(HighColumn);
                        HighColumn = RegisterDoubleColumn(HighMemberPath);
                    }
                    break;

                case HighColumnPropertyName:
                    if (!this.UpdateNumericAxisRange())
                    {
                        this.CategoryView.BucketCalculator.CalculateBuckets(Resolution);
                        RenderSeries(false);
                    }
                    break;
            }
        }

        /// <summary>
        /// Returns the range requirement of this series on the given axis.
        /// </summary>
        /// <param name="axis">The axis for which to provide the range requirement.</param>
        /// <returns>The axis range needed for this series to fully display, or null, if there is no range requirement.</returns>
        protected internal override AxisRange GetRange(Axis axis)
        {
            if (LowColumn == null || LowColumn.Count == 0
                || HighColumn == null || HighColumn.Count == 0)
            {
                return null;
            }

            if (axis == this.GetXAxis())
            {
                int max = Math.Min(LowColumn.Count, HighColumn.Count);

                return new AxisRange(0, max - 1);
            }

            if (axis == this.GetYAxis())
            {
                double min = Math.Min(LowColumn.Minimum, HighColumn.Minimum);
                double max = Math.Max(LowColumn.Maximum, HighColumn.Maximum);

                return new AxisRange(Math.Min(min, max), Math.Max(min, max));
            }

            return null;
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
            if (GetXAxis() != null && GetXAxis() is ISortingAxis)
            {
                (GetXAxis() as ISortingAxis).NotifyDataChanged();
            }

            switch (action)
            {
                case FastItemsSourceEventAction.Change:
                    if (propertyName == LowMemberPath || propertyName == HighMemberPath)
                    {
                        if (!this.UpdateNumericAxisRange())
                        {
                            RenderSeries(true);
                        }
                    }
                    break;

                case FastItemsSourceEventAction.Insert:
                    this.CategoryView.BucketCalculator.CalculateBuckets(Resolution);

                    if (!this.UpdateNumericAxisRange())
                    {
                        RenderSeries(true);
                    }
                    break;

                case FastItemsSourceEventAction.Remove:
                    this.CategoryView.BucketCalculator.CalculateBuckets(Resolution);

                    if (!this.UpdateNumericAxisRange())
                    {
                        RenderSeries(true);
                    }
                    break;

                case FastItemsSourceEventAction.Replace:
                    if (LowMemberPath != null && HighMemberPath != null && this.CategoryView.BucketCalculator.BucketSize > 0
                        && !this.UpdateNumericAxisRange())
                    {
                        RenderSeries(true);
                    }
                    break;

                case FastItemsSourceEventAction.Reset:
                    this.CategoryView.BucketCalculator.CalculateBuckets(Resolution);

                    if (!this.UpdateNumericAxisRange())
                    {
                        RenderSeries(true);
                    }
                    break;
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
            bool isValid = base.ValidateSeries(viewportRect, windowRect, view);

            if (LowColumn == null || LowColumn.Count == 0 ||
                HighColumn == null || HighColumn.Count == 0)
            {
                isValid = false;
            }
            return isValid;
        }

        internal override void PrepareFrame(CategoryFrame frame, CategorySeriesView view)
        {
            FramePreparer.PrepareFrame(frame, view);
        }

        #endregion


        CategoryMode IIsCategoryBased.CurrentCategoryMode
        {
            get { return PreferredCategoryMode(this.GetXAxis() as CategoryAxisBase); }
        }
        ICategoryScaler IIsCategoryBased.Scaler
        {
            get { return this.GetXAxis() as ICategoryScaler; }
        }

        IScaler IIsCategoryBased.YScaler
        {
            get { return this.GetYAxis() as IScaler; }
        }


        IBucketizer IIsCategoryBased.Bucketizer
        {
            get { return this.CategoryView.BucketCalculator; }
        }

        int IIsCategoryBased.CurrentMode2Index
        {
            get { return GetMode2Index(); }
        }

        System.Collections.Generic.IList<double> IHasHighLowValueCategory.HighColumn
        {
            get { return HighColumn; }
        }

        System.Collections.Generic.IList<double> IHasHighLowValueCategory.LowColumn
        {
            get { return LowColumn; }
        }

        IDetectsCollisions IIsCategoryBased.ProvideCollisionDetector()
        {
            return new CollisionAvoider();
        }

        /// <summary>
        /// Renders the thumbnail for the OPD pane.
        /// </summary>
        /// <param name="viewportRect">The viewport to use.</param>
        /// <param name="surface">The render target.</param>
        protected internal override void RenderThumbnail(Rect viewportRect, RenderSurface surface)
        {
            var dirty = ThumbnailDirty;

            base.RenderThumbnail(viewportRect, surface);


            if (!dirty)
            {
                View.PrepSurface(surface);
                return;
            }

            View.PrepSurface(surface);
            if (ClearAndAbortIfInvalid(ThumbnailView))
            {
                return;
            }

            RangeCategoryFramePreparer framePreparer = new RangeCategoryFramePreparer(
                this,
                this.ThumbnailView as ISupportsMarkers,
                this.SeriesViewer.View.OverviewPlusDetailViewportHost,
                this,
                ((CategorySeriesView)ThumbnailView).BucketCalculator);

            this.ThumbnailFrame = new CategoryFrame(3);
            framePreparer.PrepareFrame(this.ThumbnailFrame, this.ThumbnailView);

            this.RenderFrame(this.ThumbnailFrame, (CategorySeriesView)ThumbnailView);

            ThumbnailDirty = false;


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