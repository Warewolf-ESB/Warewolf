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
using System.Runtime.CompilerServices;

namespace Infragistics.Controls.Charts
{
    internal class BarBucketCalculator : CategoryBucketCalculator
    {
        internal BarBucketCalculator(BarSeriesView view)
            : base(view)
        {
            this.BarView = view;
        }

        [Weak]
        protected BarSeriesView BarView { get; set; }

        /// <summary>
        /// Calculates the bucket values to use based on the desired resolution.
        /// </summary>
        /// <param name="resolution">The resolution desired.</param>
        protected internal override void CalculateBuckets(double resolution)
        {

            Rect windowRect = this.View.WindowRect;// this.View.CategoryModel.SeriesViewer != null ? this.View.CategoryModel.SeriesViewer.ActualWindowRect : Rect.Empty;
            Rect viewportRect = this.View.Viewport;

            FastItemsSource fastItemsSource = this.View.CategoryModel.FastItemsSource;
            if (windowRect.IsEmpty || viewportRect.IsEmpty || this.BarView.BarModel.YAxis == null || fastItemsSource == null || fastItemsSource.Count == 0)
            {
                BucketSize = 0;
                return;
            }

            BarSeries barSeries = this.View.CategoryModel as BarSeries;

            double y0 =
                Math.Floor(barSeries.YAxis.GetUnscaledValue(viewportRect.Top, windowRect, viewportRect, CategoryMode.Mode0));
            double y1 =
                Math.Ceiling(barSeries.YAxis.GetUnscaledValue(viewportRect.Bottom, windowRect, viewportRect, CategoryMode.Mode0));

            if (!barSeries.YAxis.IsInverted)
            {
                y1 = Math.Ceiling(barSeries.YAxis.GetUnscaledValue(viewportRect.Top, windowRect, viewportRect, CategoryMode.Mode0));
                y0 = Math.Floor(barSeries.YAxis.GetUnscaledValue(viewportRect.Bottom, windowRect, viewportRect, CategoryMode.Mode0));
            }

            double c = Math.Floor((y1 - y0 + 1.0) * resolution / viewportRect.Height); // the number of rows per bucket

            BucketSize = (int)Math.Max(1.0, c);
            FirstBucket = (int)Math.Max(0.0, Math.Floor(y0 / BucketSize) - 1.0); // last invisible bucket
            LastBucket = (int)Math.Ceiling(y1 / BucketSize); // first invisible bucket
        }

        internal override float[] GetBucket(int bucket)
        {




            var column = this.BarView.AnchoredModel.ValueColumn;
            var count = column.Count;

            int i0 = Math.Min(bucket * BucketSize, count - 1);
            int i1 = Math.Min(i0 + BucketSize - 1, count - 1);

            double min = double.NaN;
            double max = double.NaN;

            for (int i = i0; i <= i1; ++i)
            {
                double y = column[i];

                if (!double.IsNaN(min))
                {
                    if (!double.IsNaN(y))
                    {
                        min = Math.Min(min, y);
                        max = Math.Max(max, y);
                    }
                }
                else
                {
                    min = y;
                    max = y;
                }
            }

            if (!double.IsNaN(min))
            {
                return new float[] { (float)(0.5 * (i0 + i1)), (float)min, (float)max };
            }

            return new float[] { (float)(0.5 * (i0 + i1)), float.NaN, float.NaN };
        }


        public override float GetErrorBucket(int bucket, IFastItemColumn<double> column)
        {
            int count = column.Count;

            int i0 = Math.Min(bucket * BucketSize, count - 1);
            int i1 = Math.Min(i0 + BucketSize - 1, count - 1);

            double min = double.NaN;

            for (int i = i0; i <= i1; ++i)
            {
                double x = column[i];

                if (!double.IsNaN(min))
                {
                    if (!double.IsNaN(x))
                    {
                        min = Math.Min(min, x);
                    }
                }
                else
                {
                    min = x;
                }
            }

            if (!double.IsNaN(min))
            {
                return (float)min;
                //new float[] { (float)(0.5 * (i0 + i1)), (float)min, (float)max };
            }

            return float.NaN;
            //new float[] { (float)(0.5 * (i0 + i1)), float.NaN, float.NaN };
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