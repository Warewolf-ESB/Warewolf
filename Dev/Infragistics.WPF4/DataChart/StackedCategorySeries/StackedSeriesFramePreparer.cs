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

namespace Infragistics.Controls.Charts
{
    internal class StackedSeriesFramePreparer: CategoryFramePreparer
    {
        public StackedSeriesFramePreparer(IIsCategoryBased host) : base(host){}

        public StackedSeriesFramePreparer(IIsCategoryBased host, ISupportsMarkers markersHost, IProvidesViewport viewportHost, ISupportsErrorBars errorBarsHost, IBucketizer bucketizingHost)
            : base(host, markersHost, viewportHost, errorBarsHost, bucketizingHost){}

        protected override ValuesHolder GetValues(PreparationParams p)
        {
            SingleValuesHolder h = new SingleValuesHolder();

            if (CategoryBasedHost is StackedColumnSeries || CategoryBasedHost is StackedBarSeries)
            {
                //For the stacked bar/column series, we need to know how many values there are in order to calculate the date time positions.
                //The values themselves are of no concern at this point.
                StackedSeriesBase host = CategoryBasedHost as StackedSeriesBase;
                if (host.ActualSeries.Count > 0)
                h.Values = host.ActualSeries[0].ValueColumn;
                return h;
            }
            
            IList<double> values = ValuesProvider.ValueColumn;
            h.Values = values;

            return h;
        }

        protected override int PrepareData(PreparationParams p, ValuesHolder h, double offset, bool markers, bool errorBars)
        {
            int markerCount = 0;
            bool isCluster = false;
            float[] endBucket;
            bool isSortingScaler = p.SortingScaler != null;
            IDetectsCollisions collisionAvoider = CategoryBasedHost.ProvideCollisionDetector();

            ScalerParams sParams = new ScalerParams(p.WindowRect, p.ViewportRect, p.Scaler.IsInverted);
            ScalerParams yParams = new ScalerParams(p.WindowRect, p.ViewportRect, p.YScaler.IsInverted);

            float singlePixelSpan =
                Convert.ToSingle(
                    p.Scaler.GetUnscaledValue(2.0, sParams)
                    - p.Scaler.GetUnscaledValue(1.0, sParams));

            Rect windowRect = p.WindowRect;
            Rect viewportRect = p.ViewportRect;
            bool isLogarithmicYScaler = p.YScaler is NumericAxisBase && (p.YScaler as NumericAxisBase).IsReallyLogarithmic;

            for (int i = p.FirstBucket; i <= p.LastBucket; ++i)
            {
                float[] bucket;

                if (p.SortingScaler == null)
                {
                    // index based bucketing
                    bucket = BucketizingHost.GetBucket(i);
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

                    int itemIndex = i * p.BucketSize;
                    if (p.SortingScaler != null &&
                        p.SortingScaler.SortedIndices != null &&
                        itemIndex >= 0 && itemIndex < p.SortingScaler.SortedIndices.Count)
                    {
                        itemIndex = p.SortingScaler.SortedIndices[itemIndex];
                    }

                    if (markers && isValidBucket &&
                        this.PrepareMarker(
                            p.Frame, bucket,
                            collisionAvoider,
                            Math.Min(itemIndex, h.Count - 1),
                            markerCount))
                    {
                        ++markerCount;
                    }
                }
            }
            return markerCount;
        }

        protected override bool PrepareMarker(
            CategoryFrame frame,
            float[] bucket,
            IDetectsCollisions collisionAvoider,
            int itemIndex, int markerCount)
        {
            double x = bucket[0];
            double y = bucket[1];

            if (MarkersHost is IBarSeries)
            {
                y = bucket[0];
                x = bucket[1];
            }

            Rect markerRect = new Rect(x - 5, y - 5, 11, 11);


            if (!double.IsNaN(x) && !double.IsNaN(y) && !double.IsInfinity(x) && !double.IsInfinity(y) && collisionAvoider.TryAdd(markerRect))
            {
                frame.Markers.Add(new Point(x, y));
                MarkersHost.UpdateMarkerTemplate(markerCount, itemIndex);

                return true;
            }
            return false;
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