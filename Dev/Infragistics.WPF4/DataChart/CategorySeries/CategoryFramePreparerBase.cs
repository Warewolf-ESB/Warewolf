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
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Infragistics.Controls.Charts
{
    [WidgetIgnoreDepends("CategorySeries")]
    [WidgetIgnoreDepends("AnchoredCategorySeries")]
    [WidgetIgnoreDepends("RangeCategorySeries")]
    [WidgetIgnoreDepends("RangeAreaSeries")]
    [WidgetIgnoreDepends("LineSeries")]
    [WidgetIgnoreDepends("SplineSeriesBase")]
    [WidgetIgnoreDepends("AreaSeries")]
    [WidgetIgnoreDepends("StepLineSeries")]
    [WidgetIgnoreDepends("StepAreaSeries")]
    [WidgetIgnoreDepends("CategoryDateTimeXAxis")]
    [WidgetIgnoreDepends("CategoryYAxis")]
    internal abstract class CategoryFramePreparerBase
        : FramePreparer
    {
        public CategoryFramePreparerBase(IIsCategoryBased host)
            : this(host, host as ISupportsMarkers, host as IProvidesViewport, host as ISupportsErrorBars, host as IBucketizer)
        {

        }
        public CategoryFramePreparerBase(IIsCategoryBased host, ISupportsMarkers markersHost, IProvidesViewport viewportHost, ISupportsErrorBars errorBarsHost, IBucketizer bucketizingHost)
            : base(markersHost, viewportHost, errorBarsHost)
        {
            CategoryBasedHost = host;
            BucketizingHost = bucketizingHost;
        }

        [Weak]
        protected internal IBucketizer BucketizingHost { get; set; }

        [Weak]
        protected internal IIsCategoryBased CategoryBasedHost { get; set; }

        protected abstract bool PrepareMarker(
            CategoryFrame frame,
            float[] bucket,
            IDetectsCollisions collisionAvoider,
            int itemIndex, int markerCount);

        private PreparationParams GetParams(Frame inputFrame)
        {
            PreparationParams p = new PreparationParams();

            p.Scaler = CategoryBasedHost.Scaler;
            p.YScaler = CategoryBasedHost.YScaler;
            p.SortingScaler = p.Scaler as ISortingAxis;
            p.Frame = inputFrame as CategoryFrame;
            if (p.Frame == null ||
                p.Scaler == null ||
                p.YScaler == null)
            {
                return null;
            }

            int firstBucket;
            int lastBucket;
            int bucketSize;
            double resolution;
            BucketizingHost.GetBucketInfo(out firstBucket, out lastBucket, out bucketSize, out resolution);
            p.FirstBucket = firstBucket;
            p.LastBucket = lastBucket;
            p.BucketSize = bucketSize;
            p.Resolution = resolution;
            if (p.LastBucket < p.FirstBucket)
            {
                return null;
            }

            Rect windowRect;
            Rect viewportRect;
            ViewportHost.GetViewInfo(out viewportRect, out windowRect);
            p.WindowRect = windowRect;
            p.ViewportRect = viewportRect;
            if (p.WindowRect == Rect.Empty ||
                p.ViewportRect == Rect.Empty)
            {
                return null;
            }


            if (CategoryBasedHost != null && CategoryBasedHost is FragmentBase && BucketizingHost != null)
            {
                p.IsFragment = true;
            }


            return p;
        }

        public double GetOffset(ICategoryScaler scaler, Rect windowRect, Rect viewportRect)
        {
            CategoryMode categoryMode = CategoryBasedHost.CurrentCategoryMode;
            double offset = 0.0;

            if (categoryMode == CategoryMode.Mode0 && scaler.CategoryMode != CategoryMode.Mode0)
            {
                categoryMode = CategoryMode.Mode1;
            }

            switch (categoryMode)
            {
                case CategoryMode.Mode0:    // use bucket.X as-is
                    offset = 0.0;
                    break;

                case CategoryMode.Mode1:    // offset x by half category width
                    offset = 0.5 * scaler.GetCategorySize(windowRect, viewportRect);
                    break;

                case CategoryMode.Mode2:    // offset x by the appropriate amount for this categorySeries
                    offset = scaler.GetGroupCenter(CategoryBasedHost.CurrentMode2Index, windowRect, viewportRect);
                    break;
            }

            if (scaler is CategoryYAxis)
            {
                if (!scaler.IsInverted) offset = -offset;
            }
            else if (scaler.IsInverted)
            {
                offset = -offset;
            }

            return offset;
        }

        private double GetOffset(PreparationParams p)
        {
            return GetOffset(p.Scaler, p.WindowRect, p.ViewportRect);
        }

        public override void PrepareFrame(Frame inputFrame, SeriesView view)
        {
            PreparationParams p = GetParams(inputFrame);
            if (p == null || BucketizingHost == null)
            {
                return;
            }

            p.Frame.Buckets.Clear();
            p.Frame.ErrorBuckets.Clear();
            p.Frame.Markers.Clear();
            p.Frame.Trend.Clear();
            p.Frame.ErrorBars.Clear();
            p.Frame.ErrorBarSizes.Clear();

            bool markers = MarkersHost.ShouldDisplayMarkers;



            bool errorBars = ErrorBarsHost.ShouldDisplayErrorBars();

            double offset = GetOffset(p);

            ValuesHolder h = GetValues(p);

            if (p.SortingScaler != null && p.SortingScaler.SortedIndices.Count != h.Count)
            { 
                //mismatch in series and axis data sources.
                return;
            }

            // [DN:Feb-7-2011:63421] surgical fix.
            if (p.SortingScaler != null && p.SortingScaler is CategoryDateTimeXAxis)
            {
                ((CategoryDateTimeXAxis)p.SortingScaler).InitializeActualMinimumAndMaximum();
            }

            PrepareTrendline(p, h, offset);
            int markerCount = PrepareData(p, h, offset, markers, errorBars);

            MarkersHost.UpdateMarkerCount(markerCount);
            //Markers.Count = markerCount;
            
            PrepareErrorBars(inputFrame as CategoryFrame, view);
            return;
        }

        internal float ConvertToBucket(double value)
        {



            return Convert.ToSingle(value);

        }

        protected void PrepareErrorBars(CategoryFrame frame, SeriesView view)
        {

            frame.ErrorBars.Clear();
            frame.ErrorBarSizes.Clear();
            if (frame == null ||
                !ErrorBarsHost.ShouldDisplayErrorBars())
            {
                return;
            }
            CategoryErrorBarSettings settings = ErrorBarsHost.ErrorBarSettings as CategoryErrorBarSettings;
            NumericAxisBase axis;
            if (this.ErrorBarsHost is IBarSeries)
            {
                axis = ((BarSeries)ErrorBarsHost).XAxis as NumericAxisBase;
            }
            else
            {
                axis = ErrorBarsHost.YAxis as NumericAxisBase;
            }
            if (settings == null || axis == null)
            {
                return;
            }

            IDetectsCollisions collisionAvoider = CategoryBasedHost.ProvideCollisionDetector();
            ErrorBarsHelper errorBarsHelper = new ErrorBarsHelper(ErrorBarsHost, view);

            double errorBarSize = 0.0; // error bar size
            double errorBarPosition = 0.0; // error bar position
            
            // compute error bar size independently, if appropriate
            if (errorBarsHelper.IsCalculatorIndependent(settings.Calculator))
            {
                errorBarsHelper.CalculateIndependentErrorBarPosition(settings.Calculator, ref errorBarPosition);
                errorBarsHelper.CalculateIndependentErrorBarSize(settings.Calculator, axis, ref errorBarSize);
            }

            int stop;
            if (ErrorBarsHost.ShouldSyncErrorBarsWithMarkers())
            {
                stop = frame.Markers.Count;
            }
            else
            {
                stop = frame.Buckets.Count;
            }
            for (int i = 0; i < stop; ++i)
            {
                // compute error bar position
                double x, y, dataVal;
                if (ErrorBarsHost.ShouldSyncErrorBarsWithMarkers())
                {
                    x = Math.Round(frame.Markers[i].X);
                    y = Math.Round(frame.Markers[i].Y);
                    dataVal = y;
                }
                else if (this.ErrorBarsHost is IBarSeries)
                {
                    x = Math.Round(frame.Buckets[i][1]);
                    y = Math.Round(frame.Buckets[i][0]);
                    dataVal = x;
                }
                else
                {
                    x = Math.Round(frame.Buckets[i][0]);
                    y = Math.Round(frame.Buckets[i][1]);
                    dataVal = y;
                }

                // compute error bar size dependently, if appropriate
                if (settings.Calculator.GetCalculatorType() == ErrorBarCalculatorType.Percentage)
                {
                    errorBarsHelper.CalculateDependentErrorBarSize(dataVal, settings.Calculator, axis, ref errorBarSize);
                }
                else if (settings.Calculator.GetCalculatorType() == ErrorBarCalculatorType.Data)
                {
                    if (i < frame.ErrorBuckets.Count)
                    {
                        errorBarSize = frame.ErrorBuckets[i];
                        //double unscaledValue = frame.ErrorBuckets[i];
                        //errorBarsHelper.CalculateErrorBarSize(unscaledValue, axis, ref errorBarSize);
                    }
                }

                Point position;
                if (this.ErrorBarsHost is IBarSeries)
                {
                    position = errorBarsHelper.CalculateErrorBarCenterHorizontal(settings.Calculator,
                                                                                 axis,
                                                                                 new Point() { X = x, Y = y },
                                                                                 errorBarPosition);
                }
                else
                {
                    position = errorBarsHelper.CalculateErrorBarCenterVertical(settings.Calculator,
                                                                                 axis,
                                                                                 new Point() { X = x, Y = y },
                                                                                 errorBarPosition);
                }
                

                if (ErrorBarsHost.ShouldSyncErrorBarsWithMarkers())
                {
                    frame.ErrorBars.Add(new Point() { X = position.X, Y = position.Y });
                    frame.ErrorBarSizes.Add(errorBarSize);
                }
                else
                {
                    // if not synchronizing with markers, do some collision avoidance.
                    Rect errorBarRect = new Rect(position.X - 3, position.Y - 3, 7, 7);
                    if (collisionAvoider.TryAdd(errorBarRect))
                    {
                        frame.ErrorBars.Add(new Point() { X = position.X, Y = position.Y });
                        frame.ErrorBarSizes.Add(errorBarSize);
                    }
                }
            }          

        }

        protected virtual int PrepareData(PreparationParams p, ValuesHolder h, double offset, bool markers, bool errorBars)
        {
            int markerCount = 0;
            bool isCluster = false;
            float[] endBucket = null;
            bool isAreaOrLineBasedSeries = false;
            bool isSortingScaler = p.SortingScaler != null;
            Rect windowRect = p.WindowRect;
            Rect viewportRect = p.ViewportRect;
            bool isLogarithmicYScaler = p.YScaler is NumericAxisBase && (p.YScaler as NumericAxisBase).IsReallyLogarithmic;

            IDetectsCollisions collisionAvoider = CategoryBasedHost.ProvideCollisionDetector();

            float singlePixelSpan = (float)0.0;

            ScalerParams sParams = new ScalerParams(windowRect,viewportRect, p.Scaler.IsInverted);
            ScalerParams yParams = new ScalerParams(windowRect, viewportRect, p.YScaler.IsInverted);

            if (isSortingScaler)
            {
                singlePixelSpan = ConvertToBucket(
                    p.Scaler.GetUnscaledValue(2.0, sParams)
                    - p.Scaler.GetUnscaledValue(1.0, sParams));

                isAreaOrLineBasedSeries = IsAreaOrLine();
            }





            IFastItemColumn<double> errorColumn = null;
            if (ErrorBarsHost != null)
            {
                CategoryErrorBarSettings settings = ErrorBarsHost.ErrorBarSettings as CategoryErrorBarSettings;
                if (settings != null && settings.Calculator != null)
                {
                    errorColumn = settings.Calculator.GetItemColumn();
                }
            }

            for (int i = p.FirstBucket; i <= p.LastBucket; ++i)
            {
                float[] bucket;

                float errorBucket = float.NaN;


                if (!isSortingScaler)
                {
                    // index based bucketing
                    bucket = BucketizingHost.GetBucket(i);

                    if (errorBars && errorColumn != null)
                    {
                        errorBucket = BucketizingHost.GetErrorBucket(i, errorColumn);
                    }

                }
                else
                {
                    // SortedAxis based bucketing (for CategoryDateTimeXAxis)
                    bucket = SortingBucketize(p, ref i, h, singlePixelSpan, out isCluster, out endBucket, offset);
                }

                bool isValidBucket = !isLogarithmicYScaler || (isLogarithmicYScaler && bucket[1] > 0);

                if (!float.IsNaN(bucket[0]))
                {
                    ScaleBucketValues(p, bucket, offset, isSortingScaler, sParams, yParams);
                    p.Frame.Buckets.Add(bucket);

                    if (isCluster && isAreaOrLineBasedSeries)
                    {
                        //add end bucket
                        if (endBucket != null)
                        {
                            ScaleBucketValues(p, endBucket, offset, isSortingScaler, sParams, yParams);
                            p.Frame.Buckets.Add(endBucket);
                        }
                    }

                    if (markers && isValidBucket)
                    {
                        int itemIndex = i * p.BucketSize;
                        if (isSortingScaler &&
                            p.SortingScaler.SortedIndices != null &&
                            itemIndex >= 0 && itemIndex < p.SortingScaler.SortedIndices.Count)
                        {
                            itemIndex = p.SortingScaler.SortedIndices[itemIndex];
                        }

                        if (this.PrepareMarker(
                            p.Frame, bucket,
                            collisionAvoider,
                            Math.Min(itemIndex, h.Count - 1),
                            markerCount))
                        {
                            ++markerCount;
                        }

                        if (errorBars &&
                            !float.IsNaN(errorBucket) &&
                            p.YScaler is NumericAxisBase)
                        {
                            double zero = p.YScaler.GetScaledValue(((NumericAxisBase)p.YScaler).ReferenceValue, yParams);
                            errorBucket = (float)Math.Abs(zero - p.YScaler.GetScaledValue(errorBucket, yParams));
                            p.Frame.ErrorBuckets.Add(errorBucket);
                        }

                    }

                    else if (errorBars &&
                            !float.IsNaN(errorBucket) &&
                            p.YScaler is NumericAxisBase)
                    {
                        double zero = p.YScaler.GetScaledValue(((NumericAxisBase)p.YScaler).ReferenceValue, yParams);
                        errorBucket = (float)Math.Abs(zero - p.YScaler.GetScaledValue(errorBucket, yParams));
                        p.Frame.ErrorBuckets.Add(errorBucket);
                    }

                }
            }





            return markerCount;
        }

        protected bool IsAreaOrLine()
        {
            bool isAreaOrLineBasedSeries =
                   CategoryBasedHost is LineSeries
                || CategoryBasedHost is SplineSeriesBase
                || CategoryBasedHost is AreaSeries
                || CategoryBasedHost is StepLineSeries
                || CategoryBasedHost is StepAreaSeries
                || CategoryBasedHost is RangeAreaSeries

                || CategoryBasedHost is LineFragment
                || CategoryBasedHost is AreaFragment

                ;
            return isAreaOrLineBasedSeries;
        }

        protected abstract void StoreYValues(ValuesHolder h, int index, bool useTemp, bool isFragment);
        protected abstract void MinMaxYValues(ValuesHolder h, int index, bool isFragment);
        protected abstract float[] GetBucketSorting(double xVAl, ValuesHolder h);

        protected virtual float[] SortingBucketize(PreparationParams p, ref int currentIndex, ValuesHolder h, float singlePixelSpan, out bool isCluster, out float[] endBucket, double offset)
        {
            float[] bucket;
            endBucket = null;
            isCluster = false;

            CategorySeries series = CategoryBasedHost as CategorySeries;
            RangeCategorySeries rangeSeries = CategoryBasedHost as RangeCategorySeries;
            AnchoredCategorySeries anchoredSeries = CategoryBasedHost as AnchoredCategorySeries;

            Rect viewportRect = p.ViewportRect;
            Rect windowRect = p.WindowRect;
            ScalerParams sParams = new ScalerParams(windowRect, viewportRect, p.Scaler.IsInverted);
            ScalerParams yParams = new ScalerParams(windowRect, viewportRect, p.YScaler.IsInverted);
            bool isFragment = p.IsFragment;

            if (p.SortingScaler == null || p.SortingScaler.SortedIndices == null || p.SortingScaler.SortedIndices.Count == 0)
            {
                return new[] { float.NaN, float.NaN, float.NaN };
            }

            if (series != null && series.FastItemsSource != null
                && series.FastItemsSource.Count < p.SortingScaler.SortedIndices.Count)
            {
                return new[] {float.NaN, float.NaN, float.NaN};
            }

            int index = p.SortingScaler.SortedIndices[currentIndex];
            double bucketX = p.SortingScaler.GetUnscaledValueAt(index);
            double currentX = bucketX;
            
            StoreYValues(h, index, false, isFragment);

            //var prev = GetValues(p);
            
            while (currentIndex < p.LastBucket)
            {
                index = p.SortingScaler.SortedIndices[currentIndex + 1];
                currentX = p.SortingScaler.GetUnscaledValueAt(index);

                StoreYValues(h, index, true, isFragment);

                if (currentX - bucketX >= singlePixelSpan || double.IsNaN(h.TempY0) || double.IsNaN(h.TempY1))
                {
                    // next item does not belong in this bucket
                    if (isCluster)
                    {
                        int previousIndex = p.SortingScaler.SortedIndices[currentIndex];
                        //end of a cluster - add cluster end bucket.

                        StoreYValues(h, previousIndex, true, isFragment);

                        endBucket = new[]
                                {
                                    (float) p.Scaler.GetScaledValue(bucketX + singlePixelSpan, sParams),
                                    (float) h.TempY0,
                                    (float) h.TempY1
                                };
                    }
                    break;
                }

                if (!isCluster &&
                    IsAreaOrLine())
                {
                    //start of a cluster - add cluster start bucket.
                    int previousIndex = p.SortingScaler.SortedIndices[currentIndex];

                    StoreYValues(h, previousIndex, true, isFragment);

                    float[] startBucket = new[]
                                      {
                                          (float) p.Scaler.GetScaledValue(bucketX, sParams),
                                          (float) h.TempY0,
                                          (float) h.TempY1
                                      };

                    if (!double.IsNaN(startBucket[0]))
                    {
                        if (!double.IsNaN(startBucket[1]) &&
                            !double.IsNaN(startBucket[2]))
                        {

                            ScaleBucketValues(p, startBucket, offset, p.SortingScaler != null, sParams, yParams);
                            p.Frame.Buckets.Add(startBucket);

                            isCluster = true;
                        }
                        else
                        {
                            break;
                        }
                        
                    }
                }

                // add next item to this bucket
                currentIndex++;
                MinMaxYValues(h, index, isFragment);
                //float y = Convert.ToSingle(values[index]);
                //bucketY0 = Math.Min(bucketY0, y);
                //bucketY1 = Math.Max(bucketY1, y);

                
            }

            double xVal = double.NaN;
            if (!double.IsNaN(bucketX))
            {
                xVal = p.Scaler.GetScaledValue(bucketX, sParams);
            }

            bucket = GetBucketSorting(xVal, h);
            //bucket = new float[] { Convert.ToSingle(xVal), bucketY0, bucketY1 };
            return bucket;
        }

        protected abstract void ScaleBucketValues(PreparationParams pp, float[] bucket, double offset, bool isSortingScaler, ScalerParams xParams, ScalerParams yParams);
        protected virtual void PrepareTrendline(PreparationParams pp, ValuesHolder h, double offset)
        {

        }
        protected abstract ValuesHolder GetValues(PreparationParams p);
    }

    internal abstract class ValuesHolder
    {
        public abstract int Count { get; }

        public float BucketY0 { get; set; }
        public float BucketY1 { get; set; }
        public float TempY0 { get; set; }
        public float TempY1 { get; set; }
    }

    internal class PreparationParams
    {
        public int FirstBucket { get; set; }
        public int LastBucket { get; set; }
        public int BucketSize { get; set; }
        public double Resolution { get; set; }
        public Rect WindowRect { get; set; }
        public Rect ViewportRect { get; set; }
        public ICategoryScaler Scaler { get; set; }
        public ISortingAxis SortingScaler { get; set; }
        public IScaler YScaler { get; set; }
        public CategoryFrame Frame { get; set; }
        public bool IsFragment { get; set; }
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