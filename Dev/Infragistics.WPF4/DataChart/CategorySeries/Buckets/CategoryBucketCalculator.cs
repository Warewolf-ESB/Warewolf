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
    internal class CategoryBucketCalculator : IBucketizer
    {
        [Weak]
        protected CategorySeriesView View { get; private set; }

        internal CategoryBucketCalculator(CategorySeriesView view)
            : base()
        {
            if (view == null)
            {
                throw new ArgumentNullException("view");
            }
            this.View = view;

            FirstBucket = -1;
            LastBucket = LastBucket;
            BucketSize = 0;   
        }

        /// <summary>
        /// Gets or sets the first visible bucket of the series.
        /// </summary>
        protected internal int FirstBucket { get; set; }
        /// <summary>
        /// Gets or sets the last visible bucket of the series.
        /// </summary>
        protected internal int LastBucket { get; set; }
        /// <summary>
        /// Gets or sets the bucket size of the series.
        /// </summary>
        protected internal int BucketSize { get; set; }

        internal virtual float[] GetBucket(int bucket)
        {
            return null;
        }

        public virtual float GetErrorBucket(int bucket, IFastItemColumn<double> column)
        {
            return float.NaN;
        }

        /// <summary>
        /// Calculates the bucket values to use based on the desired resolution.
        /// </summary>
        /// <param name="resolution">The resolution desired.</param>
        protected internal virtual void CalculateBuckets(double resolution)
        {
            Rect windowRect = this.View.WindowRect;
            Rect viewportRect = this.View.Viewport;

            CategoryAxisBase xAxis = this.View.CategoryModel.GetXAxis() as CategoryAxisBase;

            FastItemsSource fastItemsSource = this.View.CategoryModel.FastItemsSource;
            if (windowRect.IsEmpty
                || viewportRect.IsEmpty
                || xAxis == null
                || fastItemsSource == null
                || fastItemsSource.Count == 0)
            {
                BucketSize = 0;
                return;
            }

            ISortingAxis sortingXAxis = xAxis as ISortingAxis;
            if (sortingXAxis == null || sortingXAxis.SortedIndices == null)
            {
                // index-based bucketing
                
                double x0 = Math.Floor(xAxis.GetUnscaledValue(viewportRect.Left, windowRect, viewportRect, CategoryMode.Mode0));
                double x1 = Math.Ceiling(xAxis.GetUnscaledValue(viewportRect.Right, windowRect, viewportRect, CategoryMode.Mode0));

                if (xAxis.IsInverted)
                {
                    x1 = Math.Ceiling(xAxis.GetUnscaledValue(viewportRect.Left, windowRect, viewportRect, CategoryMode.Mode0));
                    x0 = Math.Floor(xAxis.GetUnscaledValue(viewportRect.Right, windowRect, viewportRect, CategoryMode.Mode0));
                }

                double c = Math.Floor((x1 - x0 + 1.0) * resolution / viewportRect.Width);     // the number of rows per bucket

                BucketSize = (int)Math.Max(1.0, c);
                FirstBucket = (int)Math.Max(0.0, Math.Floor(x0 / BucketSize) - 1.0);            // last invisible bucket
                LastBucket = (int)Math.Ceiling(x1 / BucketSize);                                // first invisible bucket
            }
            else
            {
                // SortedAxis based bucketing (for CategoryDateTimeXAxis)

                FirstBucket = sortingXAxis.GetFirstVisibleIndex(windowRect, viewportRect);
                LastBucket = sortingXAxis.GetLastVisibleIndex(windowRect, viewportRect);
                BucketSize = 1;
            }
        }


        #region IBucketizer Members

        float[] IBucketizer.GetBucket(int index)
        {
            return this.GetBucket(index);
        }

        void IBucketizer.GetBucketInfo(out int firstBucket, out int lastBucket, out int bucketSize, out double resolution)
        {
            firstBucket = this.FirstBucket;
            lastBucket = this.LastBucket;
            bucketSize = this.BucketSize;
            resolution = this.View.CategoryModel.Resolution;
        }
        #endregion



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