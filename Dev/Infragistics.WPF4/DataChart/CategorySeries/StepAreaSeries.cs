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
    /// Represents a XamDataChart step area series.
    /// </summary>
    public sealed class StepAreaSeries : HorizontalAnchoredCategorySeries, IIsCategoryBased
    {
        /// <summary>
        /// Called to create the view for the series.
        /// </summary>
        /// <returns>The created view.</returns>
        protected override SeriesView CreateView()
        {            
            return new StepAreaSeriesView(this);
        }

        /// <summary>
        /// Called when the view for the series is created.
        /// </summary>
        /// <param name="view">The created view.</param>
        protected internal override void OnViewCreated(SeriesView view)
        {
            base.OnViewCreated(view);

            StepAreaView = (StepAreaSeriesView)view;
        }
        internal StepAreaSeriesView StepAreaView { get; set; }

        #region constructor and initialisation
        /// <summary>
        /// Initializes a new instance of the StepAreaSeries class. 
        /// </summary>
        public StepAreaSeries()
        {
            DefaultStyleKey = typeof(StepAreaSeries);
        }

        #endregion

        internal override CategoryMode PreferredCategoryMode(CategoryAxisBase axis)
        {
            return CategoryMode.Mode1;
        }

        /// <summary>
        /// Clears the rendering for the series.
        /// </summary>
        /// <param name="wipeClean">True if the cached visuals should also be cleared.</param>
        /// <param name="view">The SeriesView in context.</param>
        protected internal override void ClearRendering(bool wipeClean, SeriesView view)
        {
            base.ClearRendering(wipeClean, view);

            var stepAreaView = (StepAreaSeriesView)view;
            stepAreaView.ClearStepArea();
        }



#region Infragistics Source Cleanup (Region)


























#endregion // Infragistics Source Cleanup (Region)

        private Func<int, double> DefineXAccessor(bool fromEnd, CategoryFrame frame, double width)
        {
            if (fromEnd)
            {
                if (XAxis is ISortingAxis)
                {
                    return (i) =>
                    {
                        if ((i & 1) == 0 || ((i / 2) + 1) >= frame.Buckets.Count)
                        {
                            return frame.Buckets[(i / 2)][0];
                        }

                        return frame.Buckets[((i / 2)) + 1][0];
                    };
                }

                return (i) =>
                {
                    return frame.Buckets[(i / 2)][0] + ((i & 1) == 1 ? -width : width);
                };

            }

            if (XAxis is ISortingAxis)
            {
                return (i) =>
                {
                    if ((i & 1) == 0 || ((i / 2) + 1) >= frame.Buckets.Count)
                    {
                        return frame.Buckets[(i / 2)][0];
                    }

                    return frame.Buckets[((i / 2)) + 1][0];
                };
            }

            return (i) => { return frame.Buckets[(i / 2)][0] + ((i & 1) == 0 ? -width : width); };
        }


        internal override void RenderFrame(CategoryFrame frame, CategorySeriesView view)
        {
            base.RenderFrame(frame, view);

            // GetViewInfo(out viewportRect, out windowRect);
            Rect windowRect = view.WindowRect;
            Rect viewportRect = view.Viewport;
            
            double width = XAxis != null ? 0.5 * XAxis.GetCategorySize(windowRect, viewportRect) : 0.0;

            if (XAxis != null && XAxis.IsInverted)
            {
                width = -width;
            }



#region Infragistics Source Cleanup (Region)

















#endregion // Infragistics Source Cleanup (Region)

            Func<int, double> x0 = DefineXAccessor(false, frame, width);
            Func<int, double> y0 = (i) => { return frame.Buckets[(i / 2)][1]; };
            Func<int, double> x1 = DefineXAccessor(true, frame, width);
            Func<int, double> y1 = (i) => { return frame.Buckets[(i / 2)][2]; };

            int bucketSize = this.CategoryView.BucketCalculator.BucketSize;

            StepAreaSeriesView stepAreaView = view as StepAreaSeriesView;
            stepAreaView.RasterizeStepArea(2 * frame.Buckets.Count, x0, y0, x1, y1, bucketSize, Resolution, (p0, l0, p01, l1, f) => TerminatePolygon(p0, 2 * frame.Buckets.Count, view), UnknownValuePlotting.DontPlot);

        }

        CategoryMode IIsCategoryBased.CurrentCategoryMode
        {
            get
            {
                if (XAxis != null && XAxis is ISortingAxis)
                {
                    XAxis.CategoryMode = CategoryMode.Mode0;
                    return CategoryMode.Mode0;
                }
                return CategoryMode.Mode1;
            }
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