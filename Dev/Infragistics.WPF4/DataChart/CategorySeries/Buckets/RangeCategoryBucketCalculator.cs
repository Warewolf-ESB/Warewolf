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
    internal class RangeCategoryBucketCalculator : CategoryBucketCalculator
    {
        [Weak]
        internal RangeCategorySeriesView RangeView { get; set; }

        internal RangeCategoryBucketCalculator(RangeCategorySeriesView view) 
            : base(view) 
        {
            RangeView = view;
        }
        // work out a bucket contents from a bucket index
        internal override float[] GetBucket(int bucket)
        {            






            RangeCategorySeries series = this.View.CategoryModel as RangeCategorySeries;
            var lowColumn = series.LowColumn;
            var highColumn = series.HighColumn;
            var lowCount = series.LowColumn != null ? series.LowColumn.Count : 0;
            var highCount = series.HighColumn != null ? series.HighColumn.Count : 0;

            int count = Math.Min(lowCount, highCount);
            int i0 = bucket * BucketSize;
            int i1 = Math.Min(i0 + BucketSize - 1, count - 1);

            double min = double.NaN;
            double max = double.NaN;
            bool first = true;
            double lowVal;
            double highVal;
            double low;
            double high;

            for (int i = i0; i <= i1; ++i)
            {
                lowVal = lowColumn[i];
                highVal = highColumn[i];
                if (lowVal < highVal)
                {
                    low = lowVal;
                    high = highVal;
                }
                else
                {
                    high = lowVal;
                    low = highVal;
                }

                if (!first)
                {
                    if (!double.IsNaN(low))
                    {
                        min = min < low ? min : low;
                        max = max > low ? max : low;
                    }

                    if (!double.IsNaN(high))
                    {
                        min = min < high ? min : high;
                        max = max > high ? max : high;
                    }
                }
                else
                {
                    if (!double.IsNaN(low))
                    {
                        if (double.IsNaN(min))
                        {
                            min = low;
                        }
                        else
                        {
                            min = Math.Min(min, low);
                        }
                        
                        if (!double.IsNaN(max))
                        {
                            max = Math.Max(max, low);
                        }
                    }
                    if (!double.IsNaN(high))
                    {
                        if (double.IsNaN(max))
                        {
                            max = high;
                        }
                        else
                        {
                            max = Math.Max(max, high);
                        }

                        if (!double.IsNaN(min))
                        {
                            min = Math.Min(min, high);
                        }
                    }

                    if (!double.IsNaN(min) && !double.IsNaN(max))
                    {
                        first = false;
                    }
                }
            }

            if (!first)
            {


#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

                return new float[] { (float)(0.5 * (i0 + i1)), (float)min, (float)max };

            }



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

            return new float[] { float.NaN, float.NaN, float.NaN };

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