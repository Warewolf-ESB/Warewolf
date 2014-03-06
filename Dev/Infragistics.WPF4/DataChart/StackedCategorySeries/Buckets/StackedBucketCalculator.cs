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
    internal class StackedBucketCalculator : CategoryBucketCalculator
    {
        internal StackedBucketCalculator(CategorySeriesView view) : base(view) { } 
        internal override float[] GetBucket(int index)
        {
            StackedSeriesBase series = this.View.CategoryModel as StackedSeriesBase;

            int count = Math.Min(series.Lows != null ? series.Lows.Length : 0, series.Highs != null ? series.Highs.Length : 0);
            int i0 = Math.Min(index * BucketSize, count - 1);
            int i1 = Math.Min(i0 + BucketSize - 1, count - 1);

            double min = double.NaN;
            double max = double.NaN;

            for (int i = i0; i <= i1; ++i)
            {
                double low = Math.Min(series.Lows[i], series.Highs[i]);
                double high = Math.Max(series.Lows[i], series.Highs[i]);

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

            if (!double.IsNaN(min) && !double.IsNaN(max))
            {
                return new float[] { (float)(0.5 * (i0 + i1)), (float)min, (float)max };
            }

            return new float[] { float.NaN, float.NaN, float.NaN };
        }
        
        internal virtual float[] GetBucket(AnchoredCategorySeries series, int index, int sortingIndex, Rect windowRect, Rect viewportRect, CategoryFrame currentFrame)
        {
            return null;
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