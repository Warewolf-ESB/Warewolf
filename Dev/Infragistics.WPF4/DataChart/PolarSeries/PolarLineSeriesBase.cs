using System;



using System.Linq;

using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Collections.Generic;
using Infragistics.Controls.Charts.Util;

namespace Infragistics.Controls.Charts
{
    /// <summary>
    /// Represents the base class from which all XamDataChart polar line series are derived.
    /// </summary>
    public abstract class PolarLineSeriesBase
        : PolarBase
    {

        /// <summary>
        /// Called to create the view for the series
        /// </summary>
        /// <returns>The created view.</returns>
        protected override SeriesView CreateView()
        {
            return new PolarLineSeriesBaseView(this);
        }

        /// <summary>
        /// Called when the view for the series is created.
        /// </summary>
        /// <param name="view">The created view.</param>
        protected internal override void OnViewCreated(SeriesView view)
        {
            base.OnViewCreated(view);

            PolarLineBaseView = (PolarLineSeriesBaseView)view;
        }
        internal PolarLineSeriesBaseView PolarLineBaseView { get; set; }

        /// <summary>
        /// Instantiates a polar line series base series.
        /// </summary>
        public PolarLineSeriesBase()
            : base()
        {
            CartesianRenderer = new PathRenderer(new DefaultFlattener());
            PolarRenderer = new PathRenderer();
        }

        internal PathRenderer CartesianRenderer { get; set; }
        internal PathRenderer PolarRenderer { get; set; }

        internal virtual UnknownValuePlotting GetUnknownValuePlotting()
        {
            return UnknownValuePlotting.DontPlot;
        }

        /// <summary>
        /// Gets whether the clipper is disabled.
        /// </summary>
        protected virtual bool ClippingDisabled
        {
            get { return false; }
        }

        /// <summary>
        /// Gets whether the shape of the series is a closed polygon.
        /// </summary>
        protected virtual bool IsClosed
        {
            get { return false; }
        }

        internal override void PrepareFrame(PolarFrame frame, PolarBaseView view)
        {
            base.PrepareFrame(frame, view);

            frame.Points.Clear();

            Rect windowRect = view.WindowRect;
            Rect viewportRect = view.Viewport;

            //GetViewInfo(out viewportRect, out windowRect);

            int angleCount = AngleColumn != null ? AngleColumn.Count : 0;
            int radiusCount = RadiusColumn != null ? RadiusColumn.Count : 0;

            int count = Math.Min(angleCount, radiusCount);

            PolarLinePlanner planner = new PolarLinePlanner()
            {
                AngleProvider = (i) => 
                {
                    return AxisInfoCache.AngleAxis.GetScaledAngle(
                        AngleColumn[i], 
                        AxisInfoCache.AngleAxisIsLogarithmic,
                        AxisInfoCache.AngleAxisIsInverted);
                },
                Count = count,
                RadiusProvider = (i) =>
                {
                    return RadiusAxis.GetScaledValue(
                        RadiusColumn[i], 
                        AxisInfoCache.RadiusAxisIsLogarithmic,
                        AxisInfoCache.RadiusAxisIsInverted,
                        AxisInfoCache.RadiusExtentScale,
                        AxisInfoCache.InnerRadiusExtentScale);
                },
                Resolution = Resolution,
                UseCartesianInterpolation = UseCartesianInterpolation,
                UnknownValuePlotting = GetUnknownValuePlotting(),
                TransformedXProvider = (i) => GetTransformedX(frame, i),
                TransformedYProvider = (i) => GetTransformedY(frame, i),
                Viewport = viewportRect,
                Window = windowRect,
                IsClosed = IsClosed,
                ClippingDisabled = ClippingDisabled
            };

            IEnumerable<int> viableIndices = 
                from i in Enumerable.Range(0, count)
                where IndexViable(i)
                select i;

            if (IsClosed)
            {
                var first = new List<int>(
                    Enumerable.Range(0, count)
                    .Where(IndexViable).Take(1));
                viableIndices = viableIndices.Concat<int>(first);
            }

            planner.PrepareLine(
                frame.Points,
                viableIndices
                );

        }

        private double GetTransformedX(PolarFrame frame, int i)
        {
            return frame.Transformed[i].X;
        }

        private double GetTransformedY(PolarFrame frame, int i)
        {
            return frame.Transformed[i].Y;
        }

        private bool CenterVisible()
        {
            return !CenterNotVisible();
        }

        private bool CenterNotVisible()
        {
            var window = SeriesViewer.ActualWindowRect;
            return !window.Contains(new Point(0.5, 0.5));
        }

        private bool IndexViable(int i)
        {
            return (AngleColumn[i] >= AxisInfoCache.AngleAxis.ActualMinimumValue &&
                AngleColumn[i] <= AxisInfoCache.AngleAxis.ActualMaximumValue &&
                RadiusColumn[i] >= AxisInfoCache.RadiusAxis.ActualMinimumValue &&
                RadiusColumn[i] <= AxisInfoCache.RadiusAxis.ActualMaximumValue) ||
                (double.IsNaN(AngleColumn[i]) || double.IsNaN(RadiusColumn[i]));
        }

        /// <summary>
        /// Overridden in derived classes to clear the series.
        /// </summary>
        protected abstract void ClearPoints(SeriesView view);

        /// <summary>
        /// Overridden in derived classes to render the series.
        /// </summary>
        /// <param name="frame">The frame to render.</param>
        /// <param name="view">The PolarBaseView in context.</param>
        internal abstract void RenderPoints(PolarFrame frame, PolarBaseView view);

        /// <summary>
        /// Clears the rendering for the series.
        /// </summary>
        /// <param name="wipeClean">True if the cached visuals should also be cleared.</param>
        /// <param name="view">The SeriesView in context.</param>
        protected internal override void ClearRendering(bool wipeClean, SeriesView view)
        {
            base.ClearRendering(wipeClean, view);

            ClearPoints(view);
        }

        internal PathRenderer CurrentRenderer { get; set; }

        internal override void RenderFrame(PolarFrame frame, PolarBaseView view)
        {
            base.RenderFrame(frame, view);

            if (UseCartesianInterpolation)
            {
                CurrentRenderer = CartesianRenderer;
            }
            else
            {
                CurrentRenderer = PolarRenderer;
            }
            CurrentRenderer.UnknownValuePlotting = GetUnknownValuePlotting();

            ClearPoints(view);
            RenderPoints(frame, view);
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