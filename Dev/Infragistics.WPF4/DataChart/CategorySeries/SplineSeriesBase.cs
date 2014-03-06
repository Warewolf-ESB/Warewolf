using System;
using System.ComponentModel;
using System.Windows;
using Infragistics.Controls.Charts.Util;
using System.Windows.Media;



using System.Linq;


using System.Runtime.CompilerServices;

namespace Infragistics.Controls.Charts
{
    /// <summary>
    ///  Represents the base class for all XamDataChart spline series.
    /// </summary>
    public abstract class SplineSeriesBase : HorizontalAnchoredCategorySeries
    {
        /// <summary>
        /// Called to create the view for the series.
        /// </summary>
        /// <returns>The created view.</returns>
        protected override SeriesView CreateView()
        {
            return new SplineSeriesBaseView(this);
        }

        /// <summary>
        /// Called when the view for the series is created.
        /// </summary>
        /// <param name="view">The created view.</param>
        protected internal override void OnViewCreated(SeriesView view)
        {
            base.OnViewCreated(view);

            SplineBaseView = (SplineSeriesBaseView)view;
        }
        internal SplineSeriesBaseView SplineBaseView { get; set; }

        /// <summary>
        /// Represents the spline fit data.
        /// </summary>
        protected double[] UColumn { get; set; }

        internal float ConvertToSingle(double value)
        {

            return Convert.ToSingle(value);



        }

        internal void PrepareDateTimeFrame(CategoryFrame frame, Rect windowRect, Rect viewportRect, CategoryAxisBase xaxis, NumericYAxis yaxis, CategorySeriesView view)
        {
            ISortingAxis sortingXAxis = xaxis as ISortingAxis;

            if (sortingXAxis == null)
            {
                return;
            }

            ScalerParams xParams = new ScalerParams(windowRect, viewportRect, xaxis.IsInverted);
            ScalerParams yParams = new ScalerParams(windowRect, viewportRect, yaxis.IsInverted);

            bool markers = this.ShouldDisplayMarkers();
            int markerCount = 0;

            double offset = GetOffset(xaxis, windowRect, viewportRect);

            Func<int, double> xv = (i) => i;
            Func<int, double> yv = (i) => ValueColumn[sortingXAxis.SortedIndices[i]];
            int lastBucket = this.CategoryView.BucketCalculator.LastBucket;
            int firstBucket = this.CategoryView.BucketCalculator.FirstBucket;
            int n = (int)Math.Ceiling(viewportRect.Width / (lastBucket - firstBucket));
            CollisionAvoider collisionAvoider = new CollisionAvoider();

            int bucketSize = this.CategoryView.BucketCalculator.BucketSize;

            if (bucketSize <= 0 || (firstBucket <= 0 && lastBucket <= 0))
            {
                CategoryView.Markers.Count = markerCount;
                return;
            }

            for (int i =  firstBucket; i < lastBucket + 1; ++i)
            {
                float[] bucket = null;
                int itemIndex = i * bucketSize;
                
                if (sortingXAxis != null && sortingXAxis.SortedIndices != null &&
                        itemIndex >= 0 && itemIndex < sortingXAxis.SortedIndices.Count)
                {
                    itemIndex = sortingXAxis.SortedIndices[itemIndex];
                }

                if (i >= (ValueColumn.Count - 1))
                {
                    //the bucket for the last value does not get added to the frame, therefore there is no marker at the end of the spline.
                    //add the last marker here.
                    if (markers &&
                        this.PrepareMarker(frame, frame.Buckets.Last<float[]>(), collisionAvoider,
                                           Math.Min(itemIndex, this.FastItemsSource.Count - 1), markerCount, view))
                    {
                        ++markerCount;
                    }

                    break;
                }

                double x1 = xv(i);
                double y1 = yv(i);
                double x2 = xv(i + 1);
                double y2 = yv(i + 1);
                double h = x2 - x1;
                double u1 = UColumn[i];
                double u2 = UColumn[i + 1];

                double unscaledValue = sortingXAxis.GetUnscaledValueAt(sortingXAxis.SortedIndices[i]);
                double firstPointX = xaxis.GetScaledValue(unscaledValue, xParams) + offset;

                double firstPointY = yaxis.GetScaledValue(y1, yParams);
                frame.Buckets.Add(new float[] {(float) firstPointX, (float) firstPointY, (float) firstPointY});

                for (int j = 1; j < n; ++j)
                {
                    double pp = ((double)j) / ((double)n);

                    double x = x1 + h * pp;
                    double a = (x2 - x)/h;
                    double b = (x - x1)/h;
                    double y = a * y1 + b * y2 + ((a*a*a - a)*u1 + (b*b*b - b)*u2)*(h*h)/6.0;

                    //calculate the intermediate x values
                    double unscaledValueFirst = sortingXAxis.GetUnscaledValueAt(sortingXAxis.SortedIndices[i]);
                    double unscaledValueNext = sortingXAxis.GetUnscaledValueAt(sortingXAxis.SortedIndices[i + 1]);

                    double currentUnscaledValue = unscaledValueFirst + (unscaledValueNext - unscaledValueFirst) * pp;
                    x = xaxis.GetScaledValue(currentUnscaledValue, xParams) + offset;
                    y = yaxis.GetScaledValue(y, yParams);

                    frame.Buckets.Add(new float[] {(float) x, (float) y, (float) y});
                }

                if (markers)
                {
                    //bucket = GetBucket(i);

                    //bucket[0] = (float) xaxis.GetScaledValue(bucket[0], windowRect, viewportRect);
                    //bucket[1] = (float) yaxis.GetScaledValue(bucket[1], windowRect, viewportRect);
                    //bucket[2] = (float) yaxis.GetScaledValue(bucket[2], windowRect, viewportRect);
                    bucket = new float[] { (float)firstPointX, (float)firstPointY, (float)firstPointY };
                }

                if (markers &&
                    this.PrepareMarker(frame, bucket, collisionAvoider,
                                       Math.Min(itemIndex, this.FastItemsSource.Count - 1), markerCount, view))
                {
                    ++markerCount;
                }
            }

            this.CategoryView.Markers.Count = markerCount;
        }

        internal virtual bool PrepareMarker(
            CategoryFrame frame,
            float[] bucket,
            CollisionAvoider collisionAvoider,
            int itemIndex, int markerCount, CategorySeriesView view)
        {
            double x = bucket[0];
            double y = bucket[1];

            Rect markerRect = new Rect(x - 5, y - 5, 11, 11);

            if (!double.IsNaN(x) && !double.IsNaN(y) && collisionAvoider.TryAdd(markerRect))
            {
                frame.Markers.Add(new Point(x, y));

                //MarkersHost.UpdateMarkerTemplate(markerCount, itemIndex);
                Marker marker = view.Markers[markerCount];
                (marker.Content as DataContext).Item = this.FastItemsSource[itemIndex];

                return true;
            }
            return false;
        }

        internal override void PrepareFrame(CategoryFrame frame, CategorySeriesView view)
        {
            base.PrepareFrame(frame, view);

            //first, let's examine the frame created by the frame preparer.
            //handle cases where frame.buckets is empty or has one bucket
            //We don't need to bother with custom spline logic if there are fewer than 2 buckets.
            if (frame.Buckets.Count <= 1)
            {
                return;
            }

            if (view.BucketCalculator.BucketSize == 0)
            {
                return;
            }

            // bucketize using UColumn instead of ValueColumn

            Rect windowRect = view.WindowRect;
            Rect viewportRect = view.Viewport;
            // GetViewInfo(out viewportRect, out windowRect);
            CategoryAxisBase xaxis = XAxis;
            NumericYAxis yaxis = YAxis;
            ScalerParams xParams = new ScalerParams(windowRect, viewportRect, xaxis.IsInverted);
            ScalerParams yParams = new ScalerParams(windowRect, viewportRect, yaxis.IsInverted);

            frame.Buckets.Clear();
            frame.Markers.Clear();

            bool markers = this.ShouldDisplayMarkers();
            int markerCount = 0;

            ISortingAxis sortingXAxis = XAxis as ISortingAxis;
            if (sortingXAxis != null && sortingXAxis.SortedIndices.Count != FastItemsSource.Count)
            {
                //mismatch in series and axis data sources.
                return;
            }

            double offset = GetOffset(xaxis, windowRect, viewportRect);

            Func<int, double> xv = (i) => i;
            Func<int, double> yv = (i) => ValueColumn[i];

            int bucketSize = view.BucketCalculator.BucketSize;
            if ((UColumn == null || UColumn.Length != ValueColumn.Count) && bucketSize == 1)
            {
                double endPointsFirstDerivative = this.SplineType == SplineType.Natural ? double.NaN : 0.0;
                if (XAxis != null && XAxis is ISortingAxis && (XAxis as ISortingAxis).SortedIndices != null)
                {
                    SafeSortedReadOnlyDoubleCollection sorted =
                        new SafeSortedReadOnlyDoubleCollection(ValueColumn, (XAxis as ISortingAxis).SortedIndices);
                    yv = (i) => sorted[i];
                }
                UColumn = Numeric.SafeCubicSplineFit(ValueColumn.Count, xv, yv, endPointsFirstDerivative, endPointsFirstDerivative);
            }
            int lastBucket = view.BucketCalculator.LastBucket;
            int firstBucket = view.BucketCalculator.FirstBucket;
            int n = (int)Math.Ceiling(viewportRect.Width / (lastBucket - firstBucket));
            CollisionAvoider collisionAvoider = new CollisionAvoider();

            #region bucketize data and markers
            if (sortingXAxis != null)
            {
                PrepareDateTimeFrame(frame, windowRect, viewportRect, xaxis, yaxis, view);
                return;
            }




            for (int i = firstBucket; i <= lastBucket; ++i)
            {
                float[] bucket = null;

                if (bucketSize == 1)
                {
                    // if an axis of larger range is being used our bucketsize == 1
                    // spline should stop short when it runs out of values.
                    if (i >= (ValueColumn.Count - 1))
                    {
                        //the bucket for the last value does not get added to the frame, therefore there is no marker at the end of the spline.
                        //add the last marker here.
                        if (markers && this.PrepareMarker(frame, frame.Buckets[frame.Buckets.Count - 1], collisionAvoider, Math.Min(i * bucketSize, this.FastItemsSource.Count - 1), markerCount, view))
                        {
                            ++markerCount;
                        }

                        break;
                    }

                    double x1 = xv(i);
                    double y1 = yv(i);
                    double x2 = xv(i + 1);
                    double y2 = yv(i + 1);
                    double h = x2 - x1;
                    double u1 = UColumn[i];
                    double u2 = UColumn[i + 1];

                    // [DN 3/17/2010] add the first point in the spline segment.  we already know it's position because it's a datapoint.                    
                    double firstPointX = xaxis.GetScaledValue(x1, xParams) + offset;
                    double firstPointY = yaxis.GetScaledValue(y1, yParams);
                    frame.Buckets.Add(new float[] { (float)firstPointX, (float)firstPointY, (float)firstPointY });

                    for (int j = 1; j < n; ++j)
                    {
                        double x = x1 + h * (double)j / (double)n;
                        double a = (x2 - x) / h;
                        double b = (x - x1) / h;
                        double y = a * y1 + b * y2 + ((a * a * a - a) * u1 + (b * b * b - b) * u2) * (h * h) / 6.0;

                        x = xaxis.GetScaledValue(x, xParams) + offset;
                        y = yaxis.GetScaledValue(y, yParams);

                        frame.Buckets.Add(new float[] { (float)x, (float)y, (float)y });
                    }

                    if (markers)
                    {
                        bucket = view.BucketCalculator.GetBucket(i);

                        bucket[0] = (float)(xaxis.GetScaledValue(bucket[0], xParams) + offset);
                        bucket[1] = (float)yaxis.GetScaledValue(bucket[1], yParams);
                        bucket[2] = (float)yaxis.GetScaledValue(bucket[2], yParams);
                    }
                }
                else
                {
                    bucket = view.BucketCalculator.GetBucket(i);

                    if (!float.IsNaN(bucket[0]))
                    {
                        bucket[0] = (float)(xaxis.GetScaledValue(bucket[0], xParams) + offset);
                        bucket[1] = (float)yaxis.GetScaledValue(bucket[1], yParams);
                        bucket[2] = (float)yaxis.GetScaledValue(bucket[2], yParams);

                        frame.Buckets.Add(bucket);
                    }
                }

                if (markers && this.PrepareMarker(frame, bucket, collisionAvoider, Math.Min(i * bucketSize, this.FastItemsSource.Count - 1), markerCount, view))
                {
                    ++markerCount;
                }
            }




            view.Markers.Count = markerCount;
            #endregion
        }

        internal double GetOffset(CategoryAxisBase axis, Rect windowRect, Rect viewportRect)
        {
            CategoryMode categoryMode = PreferredCategoryMode(axis);

            if (categoryMode == CategoryMode.Mode0 && axis.CategoryMode != CategoryMode.Mode0)
            {
                categoryMode = CategoryMode.Mode1;
            }

            double offset = 0.0;            // offset (pels) to the center of this categorySeries

            switch (categoryMode)
            {
                case CategoryMode.Mode0:    // use bucket.X as-is
                    offset = 0.0;
                    break;

                case CategoryMode.Mode1:    // offset x by half category width
                    offset = 0.5 * axis.GetCategorySize(windowRect, viewportRect);
                    break;

                case CategoryMode.Mode2:    // offset x by the appropriate amount for this categorySeries
                    offset = axis.GetGroupCenter(Index, windowRect, viewportRect);
                    break;
            }

            if (axis.IsInverted) offset = -offset;

            return offset;
        }

        private const string SplineTypePropertyName = "SplineType";
        /// <summary>
        /// Identifies the SplineType dependency property.
        /// </summary>
        public static readonly DependencyProperty SplineTypeProperty = DependencyProperty.Register(SplineTypePropertyName, typeof(SplineType), typeof(SplineSeriesBase), new PropertyMetadata(SplineType.Natural, (sender, e) =>
                {
                    (sender as SplineSeriesBase).RaisePropertyChanged(SplineTypePropertyName, e.OldValue, e.NewValue);
                }));
        /// <summary>
        /// Gets or sets the type of spline to be rendered.
        /// </summary>
        [WidgetDefaultString("natural")]
        public SplineType SplineType
        {
            get
            {
                return (SplineType)this.GetValue(SplineTypeProperty);
            }
            set
            {
                this.SetValue(SplineTypeProperty, value);
            }
        }

        /// <summary>
        /// Indicate that the spline fit must be recalculated.
        /// </summary>
        protected void SplineFitMustBeRecalculated()
        {
            UColumn = null;
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
            switch (propertyName)
            {
                case ValueColumnPropertyName:
                case FastItemsSourcePropertyName:
                    SplineFitMustBeRecalculated();
                    break;
            }

            base.PropertyUpdatedOverride(sender, propertyName, oldValue, newValue);

            switch (propertyName)
            {
                case SplineTypePropertyName:
                    SplineFitMustBeRecalculated();
                    RenderSeries(false);
                    this.NotifyThumbnailAppearanceChanged();
                    break;
            }
        }

        /// <summary>
        /// Overridden in derived classes when they want to respond to the bound data being updated.
        /// </summary>
        /// <param name="action">The type of action performed on the data source.</param>
        /// <param name="position">The position the change began at.</param>
        /// <param name="count">The number of items affected by the change.</param>
        /// <param name="propertyName">The name of the property changed, if applicable.</param>
        protected override void DataUpdatedOverride(FastItemsSourceEventAction action, int position, int count, string propertyName)
        {
            // [DN 4/29/2010:31528] we must reset UColumn before invoking DataUpdatedOverride.  otherwise, ValueColumn.Count and UColumn.Length can become desynced, resulting in an IndexOutOfRangeException during render.
            this.SplineFitMustBeRecalculated();
            base.DataUpdatedOverride(action, position, count, propertyName);
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