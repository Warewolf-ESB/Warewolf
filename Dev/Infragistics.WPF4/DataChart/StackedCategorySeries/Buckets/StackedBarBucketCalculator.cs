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

namespace Infragistics.Controls.Charts
{
    internal class StackedBarBucketCalculator : StackedBucketCalculator
    {
        internal StackedBarBucketCalculator(CategorySeriesView view) : base(view) { }
        /// <summary>
        /// Calculates the bucket values to use based on the desired resolution.
        /// </summary>
        /// <param name="resolution">The resolution desired.</param>
        protected internal override void CalculateBuckets(double resolution)
        {

            Rect windowRect = this.View.WindowRect;
            Rect viewportRect = this.View.Viewport;
            StackedBarSeries series = this.View.CategoryModel as StackedBarSeries;
            FastItemsSource fastItemsSource = this.View.CategoryModel.FastItemsSource;

            if (windowRect.IsEmpty || viewportRect.IsEmpty || series.YAxis == null || fastItemsSource == null || fastItemsSource.Count == 0)
            {
                BucketSize = 0;
                return;
            }

            double y0 =
                Math.Floor(series.YAxis.GetUnscaledValue(viewportRect.Top, windowRect, viewportRect, CategoryMode.Mode0));
            double y1 =
                Math.Ceiling(series.YAxis.GetUnscaledValue(viewportRect.Bottom, windowRect, viewportRect, CategoryMode.Mode0));

            if (!series.YAxis.IsInverted)
            {
                y1 = Math.Ceiling(series.YAxis.GetUnscaledValue(viewportRect.Top, windowRect, viewportRect, CategoryMode.Mode0));
                y0 = Math.Floor(series.YAxis.GetUnscaledValue(viewportRect.Bottom, windowRect, viewportRect, CategoryMode.Mode0));
            }

            double c = Math.Floor((y1 - y0 + 1.0) * resolution / viewportRect.Height); // the number of rows per bucket

            BucketSize = (int)Math.Max(1.0, c);
            FirstBucket = (int)Math.Max(0.0, Math.Floor(y0 / BucketSize) - 1.0); // last invisible bucket
            LastBucket = (int)Math.Ceiling(y1 / BucketSize); // first invisible bucket
        }

        internal override float[] GetBucket(AnchoredCategorySeries series, int index, int sortingIndex, Rect windowRect, Rect viewportRect, CategoryFrame currentFrame)
        {
            float[] bucket = new float[] { float.NaN, float.NaN, float.NaN };
            BarFragment fragment = series as BarFragment;
            if (fragment == null || fragment.LogicalSeriesLink == null) return bucket;

            StackedBarSeries barSeries = this.View.CategoryModel as StackedBarSeries;
            double value = series.ValueColumn[sortingIndex];
            double zero = 0.0;
            double min = double.NaN;
            double max = double.NaN;
            double high = double.NegativeInfinity;
            double low = double.PositiveInfinity;

            int count = Math.Min(barSeries.Lows != null ? barSeries.Lows.Length : 0, barSeries.Highs != null ? barSeries.Highs.Length : 0);
            int i0 = sortingIndex * BucketSize;
            int i1 = Math.Min(i0 + BucketSize - 1, count - 1);

            for (int i = i0; i <= i1; ++i)
            {
                value = series.ValueColumn[i];
                
                if (value < zero)
                {
                    low = Math.Min(low, fragment.LogicalSeriesLink.LowValues[i] + value);
                    high = Math.Max(high, fragment.LogicalSeriesLink.LowValues[i]);
                }
                else
                {
                    low = Math.Min(low, fragment.LogicalSeriesLink.HighValues[i]);
                    high = Math.Max(high, fragment.LogicalSeriesLink.HighValues[i] + value);
                }

                if (!double.IsNaN(min))
                {
                    if (!double.IsNaN(low))
                    {
                        min = Math.Min(min, low);
                        max = Math.Max(max, low);
                    }

                    if (!double.IsNaN(high))
                    {
                        min = Math.Min(min, high);
                        max = Math.Max(max, high);
                    }
                }
                else
                {
                    min = low;
                    max = high;
                }
            }

            ScalerParams xParams = new ScalerParams(windowRect, viewportRect, barSeries.XAxis.IsInverted);

            bucket = new[]
                {
                    currentFrame.Buckets[index - FirstBucket][0],
                    (float)barSeries.XAxis.GetScaledValue(max, xParams),
                    (float)barSeries.XAxis.GetScaledValue(min, xParams)
                };

            return bucket;
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