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
    internal class LineFragmentBucketCalculator : AnchoredCategoryBucketCalculator
    {
        public LineFragmentBucketCalculator(AnchoredCategorySeriesView view) : base(view) { }
        internal override float[] GetBucket(int bucket)
        {
            
            int i0 = Math.Min(bucket * BucketSize, this.AnchoredView.AnchoredModel.ValueColumn.Count - 1);
            int i1 = Math.Min(i0 + BucketSize - 1, this.AnchoredView.AnchoredModel.ValueColumn.Count - 1);

            double min = double.NaN;
            double max = double.NaN;

            FragmentBase fragment = this.AnchoredView.AnchoredModel as FragmentBase;
            StackedSeriesBase parentSeries = fragment.ParentSeries;

            for (int i = i0; i <= i1; ++i)
            {
                double y = this.AnchoredView.AnchoredModel.ValueColumn[i];
                double total = Math.Abs(parentSeries.Lows[i]) + parentSeries.Highs[i];

                if (double.IsNaN(y) || double.IsInfinity(y))
                {
                    y = 0;
                }

                if (parentSeries is IStacked100Series)
                {
                    if (total == 0)
                    {
                        y = 0;
                    }
                    else if (y < 0)
                    {
                        y = (fragment.LogicalSeriesLink.LowValues[i] + y) / total * 100;
                    }
                    else
                    {
                        y = (fragment.LogicalSeriesLink.HighValues[i] + y) / total * 100;
                    }
                }
                else
                {
                    y = y < 0 ? fragment.LogicalSeriesLink.LowValues[i] + y : fragment.LogicalSeriesLink.HighValues[i] + y;
                }

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