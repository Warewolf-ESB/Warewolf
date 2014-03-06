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
    internal abstract class FinancialSeriesView
        : SeriesView
    {
        protected internal FinancialSeries FinancialModel { get; set; }
        protected internal FinancialBucketCalculator BucketCalculator { get; set; }
        public FinancialSeriesView(FinancialSeries model)
            : base(model)
        {
            FinancialModel = model;
            this.BucketCalculator = this.CreateBucketCalculator();
        }
        
        public override void AttachUI(Canvas rootCanvas)
        {
            base.AttachUI(rootCanvas);

            BucketCalculator.CalculateBuckets(Model.Resolution);
        }

        protected abstract FinancialBucketCalculator CreateBucketCalculator();
        ///// <summary>
        ///// Calculates the bucket values to use based on the desired resolution.
        ///// </summary>
        ///// <param name="resolution">The resolution desired.</param>
        //protected internal void CalculateBuckets(double resolution)
        //{
        //    //Rect windowRect = SeriesViewer != null ? SeriesViewer.ActualWindowRect : Rect.Empty;
        //    //Rect viewportRect = View.Viewport;

        //    Rect windowRect = this.WindowRect;
        //    Rect viewportRect = this.Viewport;

        //    if (windowRect.IsEmpty || viewportRect.IsEmpty || this.FinancialModel.XAxis == null)
        //    {
        //        BucketSize = 0;
        //        return;
        //    }

        //    var xIsInverted = (this.FinancialModel.XAxis != null) ? this.FinancialModel.XAxis.IsInverted : false;
        //    var yIsInverted = (this.FinancialModel.YAxis != null) ? this.FinancialModel.YAxis.IsInverted : false;

        //    ScalerParams xParams = new ScalerParams(windowRect, viewportRect, xIsInverted);
        //    ScalerParams yParams = new ScalerParams(windowRect, viewportRect, yIsInverted);

        //    ISortingAxis sortingXAxis = this.FinancialModel.XAxis as ISortingAxis;
        //    if (sortingXAxis == null || sortingXAxis.SortedIndices == null)
        //    {
        //        // index-based bucketing
        //        double x0 = Math.Floor(this.FinancialModel.XAxis.GetUnscaledValue(viewportRect.Left, xParams));
        //        double x1 = Math.Ceiling(this.FinancialModel.XAxis.GetUnscaledValue(viewportRect.Right, xParams));

        //        if (this.FinancialModel.XAxis.IsInverted)
        //        {
        //            x1 = Math.Ceiling(this.FinancialModel.XAxis.GetUnscaledValue(viewportRect.Left, xParams));
        //            x0 = Math.Floor(this.FinancialModel.XAxis.GetUnscaledValue(viewportRect.Right, xParams));
        //        }

        //        double c = Math.Floor((x1 - x0 + 1.0) * resolution / viewportRect.Width); // the number of rows per bucket

        //        BucketSize = (int)Math.Max(1.0, c);
        //        FirstBucket = (int)Math.Floor(x0 / BucketSize); // first visibile bucket
        //        LastBucket = (int)Math.Ceiling(x1 / BucketSize); // first invisible bucket
        //    }
        //    else
        //    {
        //        // SortedAxis based bucketing (for CategoryDateTimeXAxis)
        //        this.FirstBucket = sortingXAxis.GetFirstVisibleIndex(windowRect, viewportRect);
        //        this.LastBucket = sortingXAxis.GetLastVisibleIndex(windowRect, viewportRect);
        //        this.BucketSize = 1;
        //    }
        //}
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