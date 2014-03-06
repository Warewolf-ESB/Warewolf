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
using System.Windows.Data;
using System.Collections.Generic;
using Infragistics.Controls.Charts.Util;

namespace Infragistics.Controls.Charts
{
    /// <summary>
    /// Represents a XamDataChart range area series.
    /// </summary>
    public sealed class RangeAreaSeries : HorizontalRangeCategorySeries
    {
        /// <summary>
        /// Called to create the view for the series.
        /// </summary>
        /// <returns>The created view.</returns>
        protected override SeriesView CreateView()
        {
            return new RangeAreaSeriesView(this);
        }

        /// <summary>
        /// Called when the view for the series is created.
        /// </summary>
        /// <param name="view">The created view.</param>
        protected internal override void OnViewCreated(SeriesView view)
        {
            base.OnViewCreated(view);

            RangeAreaView = (RangeAreaSeriesView)view;
        }
        internal RangeAreaSeriesView RangeAreaView { get; set; }

        #region Constructor and Initalisation
        /// <summary>
        /// Initializes a new instance of the RangeAreaSeries class. 
        /// </summary>
        public RangeAreaSeries()
        {
            DefaultStyleKey = typeof(RangeAreaSeries);
        }
        #endregion

        #region RangeCategorySeries Implementation
        internal override CategoryMode PreferredCategoryMode(CategoryAxisBase axis)
        {
            return CategoryMode.Mode0;
        }

        /// <summary>
        /// Clears the rendering for the series.
        /// </summary>
        /// <param name="wipeClean">True if the cached visuals should also be cleared.</param>
        /// <param name="view">The SeriesView in context.</param>
        protected internal override void ClearRendering(bool wipeClean, SeriesView view)
        {
            base.ClearRendering(wipeClean, view);

            var rangeAreaView = (RangeAreaSeriesView)view;
            rangeAreaView.ClearRangeArea();
        }

        internal override void RenderFrame(CategoryFrame frame, CategorySeriesView view)
        {
            base.RenderFrame(frame, view);



#region Infragistics Source Cleanup (Region)



















#endregion // Infragistics Source Cleanup (Region)

            Func<int, double> x0 = delegate(int i) { return frame.Buckets[i][0]; };
            Func<int, double> y0 = delegate(int i) { return frame.Buckets[i][1]; };
            Func<int, double> x1 = delegate(int i) { return frame.Buckets[frame.Buckets.Count - 1 - i][0]; };
            Func<int, double> y1 = delegate(int i) { return frame.Buckets[frame.Buckets.Count - 1 - i][2]; };

            RangeAreaSeriesView rangeAreaView = view as RangeAreaSeriesView;
            rangeAreaView.RasterizeRangeArea(frame.Buckets.Count, x0, y0, x1, y1);

        }
        #endregion

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