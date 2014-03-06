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



using System.Linq;
using System.ComponentModel;


namespace Infragistics.Controls.Charts
{
    /// <summary>
    /// Represents one part in a StackedSplineSeries or StackedSplineAreaSeries.
    /// </summary>
    [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
    public abstract class SplineFragmentBase : SplineSeriesBase
    {
        internal StackedFragmentSeries LogicalSeriesLink { get; set; }
        internal StackedSeriesBase ParentSeries { get; set; }

        internal override CategoryMode PreferredCategoryMode(CategoryAxisBase axis)
        {
            return CategoryMode.Mode0;
        }

        internal override bool PrepareMarker(CategoryFrame frame, float[] bucket, CollisionAvoider collisionAvoider, int itemIndex, int markerCount, CategorySeriesView view)
        {
            double x = bucket[0];
            double y = bucket[1];

            Rect markerRect = new Rect(x - 5, y - 5, 11, 11);

            if (!double.IsNaN(x) && !double.IsNaN(y) && !double.IsInfinity(x) && !double.IsInfinity(y) && collisionAvoider.TryAdd(markerRect))
            {
                frame.Markers.Add(new Point(x, y));
                //FramePreparer.MarkersHost.UpdateMarkerTemplate(markerCount, itemIndex);
                Marker marker = view.Markers[markerCount];
                (marker.Content as DataContext).Item = this.FastItemsSource[itemIndex];
                return true;
            }

            return false;
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
            CategoryAxisBase xAxis = ParentSeries.GetXAxis() as CategoryAxisBase;
            Axis yAxis = ParentSeries.GetYAxis();
            if (ParentSeries == null
                || xAxis == null
                || xAxis.ItemsSource == null
                || yAxis == null
                || ParentSeries.FastItemsSource == null
                || xAxis.SeriesViewer == null
                || yAxis.SeriesViewer == null
                )
            {
                isValid = false;
            }

            if (ValueColumn == null)
            {
                return false;
            }

            if (double.IsInfinity(ValueColumn.Minimum) &&
                double.IsInfinity(ValueColumn.Maximum))
            {
                isValid = false;
            }

            if (double.IsNaN(ValueColumn.Minimum) &&
                double.IsNaN(ValueColumn.Maximum))
            {
                isValid = false;
            }

            return isValid;
        }
        /// <summary>
        /// Returns the range requirement of this series on the given axis.
        /// </summary>
        /// <param name="axis">The axis for which to provide the range requirement.</param>
        /// <returns>The axis range needed for this series to fully display, or null, if there is no range requirement.</returns>
        protected internal override AxisRange GetRange(Axis axis)
        {
            return null;
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


            float singlePixelSpan = ConvertToSingle(xaxis.GetUnscaledValue(2.0, xParams) - xaxis.GetUnscaledValue(1.0, xParams));

            bool markers = this.ShouldDisplayMarkers();
            int markerCount = 0;

            double offset = GetOffset(xaxis, windowRect, viewportRect);

            Func<int, double> total = (i) => Math.Abs(ParentSeries.Lows[i]) + ParentSeries.Highs[i];
            Func<int, double> xv = (i) => i;
            Func<int, double> yv = (i) =>
                {
                    int index = sortingXAxis.SortedIndices[i];
                    if (ParentSeries is IStacked100Series)
                    {
                        return ValueColumn[index] < 0 ? (LogicalSeriesLink.LowValues[index] + ValueColumn[index]) / total(index) * 100 : (LogicalSeriesLink.HighValues[index] + ValueColumn[index]) / total(index) * 100;
                    }
                    else
                    {
                        return ValueColumn[index] < 0 ? LogicalSeriesLink.LowValues[index] + ValueColumn[index] : LogicalSeriesLink.HighValues[index] + ValueColumn[index];
                    }
                };

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

            for (int i = firstBucket; i < lastBucket + 1; ++i)
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
                frame.Buckets.Add(new float[] { (float)firstPointX, (float)firstPointY, (float)firstPointY });

                for (int j = 1; j < n; ++j)
                {
                    double pp = ((double)j) / ((double)n);

                    double x = x1 + h * pp;
                    double a = (x2 - x) / h;
                    double b = (x - x1) / h;
                    double y = a * y1 + b * y2 + ((a * a * a - a) * u1 + (b * b * b - b) * u2) * (h * h) / 6.0;

                    //calculate the intermediate x values
                    double unscaledValueFirst = sortingXAxis.GetUnscaledValueAt(sortingXAxis.SortedIndices[i]);
                    double unscaledValueNext = sortingXAxis.GetUnscaledValueAt(sortingXAxis.SortedIndices[i + 1]);

                    if (unscaledValueFirst == unscaledValueNext && y1 == y2)
                    {
                        //Avoid extrapolating between two identical buckets.
                        break;
                    }

                    double currentUnscaledValue = unscaledValueFirst + (unscaledValueNext - unscaledValueFirst) * pp;
                    x = xaxis.GetScaledValue(currentUnscaledValue, xParams) + offset;
                    y = yaxis.GetScaledValue(y, yParams);

                    frame.Buckets.Add(new float[] { (float)x, (float)y, (float)y });
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

        internal override void PrepareFrame(CategoryFrame frame, CategorySeriesView view)
        {
          
            if (ValueColumn == null || ParentSeries == null || LogicalSeriesLink == null) 
                return;

            if (LogicalSeriesLink.LowValues.Count == 0 || LogicalSeriesLink.HighValues.Count == 0)
                return;

            if (CategoryView.BucketCalculator.BucketSize == 0)
                return;

            base.PrepareFrame(frame, view);

            //first, let's examine the frame created by the frame preparer.
            //handle cases where frame.buckets is empty or has one bucket
            //We don't need to bother with custom spline logic if there are fewer than 2 buckets.
            if (frame.Buckets.Count <= 1)
            {
                return;
            }

            // bucketize using UColumn instead of ValueColumn
            Rect windowRect = view.WindowRect;
            Rect viewportRect = view.Viewport;
            
            CategoryAxisBase xaxis = ParentSeries.GetXAxis() as CategoryAxisBase;
            NumericYAxis yaxis = ParentSeries.GetYAxis() as NumericYAxis;
            
            ScalerParams xParams = new ScalerParams(windowRect, viewportRect, xaxis.IsInverted);
            ScalerParams yParams = new ScalerParams(windowRect, viewportRect, yaxis.IsInverted);

            frame.Buckets.Clear();
            frame.Markers.Clear();

            bool markers = this.ShouldDisplayMarkers();
            int markerCount = 0;
            CategoryFrame parentFrame;
            CategorySeriesView parentView;
            if (view == this.ThumbnailView)
            {
                parentFrame = this.ParentSeries.ThumbnailFrame;
                parentView = this.ParentSeries.ThumbnailView as CategorySeriesView;
            }
            else
            {
                parentFrame = this.ParentSeries.CurrentFrame;
                parentView = this.ParentSeries.CategoryView;
            }
            int parentBucketSize = parentView.BucketCalculator.BucketSize;

            ISortingAxis sortingXAxis = xaxis as ISortingAxis;
            if (sortingXAxis != null && sortingXAxis.SortedIndices.Count != FastItemsSource.Count)
            {
                //mismatch in series and axis data sources.
                return;
            }

            CategoryMode categoryMode = PreferredCategoryMode(xaxis);

            if (categoryMode == CategoryMode.Mode0 && xaxis.CategoryMode != CategoryMode.Mode0)
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
                    offset = 0.5 * xaxis.GetCategorySize(windowRect, viewportRect);
                    break;

                case CategoryMode.Mode2:    // offset x by the appropriate amount for this categorySeries
                    int index = Index;
                    offset = xaxis.GetGroupCenter(Index, windowRect, viewportRect);
                    break;
            }

            if (xaxis.IsInverted) offset = -offset;

            Func<int,double> total = (i) => Math.Abs(ParentSeries.Lows[i]) + ParentSeries.Highs[i];
            Func<int, double> xv = (i) => i;
            Func<int, double> yv = (i) =>
                {
                    double value = ValueColumn[i];

                    if (double.IsNaN(value) || double.IsInfinity(value))
                    {
                        value = 0;
                    }

                    if (ParentSeries is IStacked100Series)
                    {
                        if (total(i) == 0)
                            return 0;

                        return value < 0 ? (LogicalSeriesLink.LowValues[i] + value) / total(i) * 100 : (LogicalSeriesLink.HighValues[i] + value) / total(i) * 100;
                    }

                    return value < 0 ? LogicalSeriesLink.LowValues[i] + value : LogicalSeriesLink.HighValues[i] + value;
                };

            int bucketSize = view.BucketCalculator.BucketSize;

            double endPointsFirstDerivative = this.SplineType == SplineType.Natural ? double.NaN : 0.0;
            if (xaxis != null && xaxis is ISortingAxis && (xaxis as ISortingAxis).SortedIndices != null)
            {
                SafeSortedReadOnlyDoubleCollection sorted =
                new SafeSortedReadOnlyDoubleCollection(ValueColumn, (xaxis as ISortingAxis).SortedIndices);
                yv = (i) => sorted[i];
            }
            UColumn = Numeric.SafeCubicSplineFit(ValueColumn.Count, xv, yv, endPointsFirstDerivative, endPointsFirstDerivative);

            int firstBucket = parentView.BucketCalculator.FirstBucket;
            int lastBucket = parentView.BucketCalculator.LastBucket;
            int n = (int)Math.Ceiling(viewportRect.Width / (lastBucket - firstBucket));
            CollisionAvoider collisionAvoider = new CollisionAvoider();

            #region bucketize data and markers
            if (sortingXAxis != null)
            {
                PrepareDateTimeFrame(frame, windowRect, viewportRect, xaxis, yaxis, view);
                return;
            }
            for (int i = firstBucket; i < lastBucket + 1; ++i)
            {
                if (i >= ValueColumn.Count) break;

                float[] bucket = null;

                if (bucketSize == 1)
                {
                    // if an axis of larger range is being used our bucketsize == 1
                    // spline should stop short when it runs out of values.
                    if (i >= (ValueColumn.Count - 1))
                    {
                        //the bucket for the last value does not get added to the frame, therefore there is no marker at the end of the spline.
                        //add the last marker here.
                        if (markers 
                            && frame.Buckets.Count > 0
                            && this.PrepareMarker(frame, frame.Buckets[frame.Buckets.Count - 1], collisionAvoider, Math.Min(i * bucketSize, this.FastItemsSource.Count - 1), markerCount, view))
                        {
                            ++markerCount;
                        }
                        break;
                    }

                    double x1 = xv(i);
                    double x2 = xv(i + 1);

                    double y1 = yv(i);
                    double y2 = yv(i + 1);

                    double h = x2 - x1;
                    double u1 = UColumn[i];
                    double u2 = UColumn[i + 1];

                    // [DN 3/17/2010] add the first point in the spline segment.  we already know its position because it's a datapoint.                    
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
                        bucket = new float[] { (float)firstPointX, (float)firstPointY, (float)firstPointY };
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