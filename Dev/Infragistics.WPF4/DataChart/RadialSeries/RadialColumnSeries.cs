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
using Infragistics.Controls.Charts.Util;
using System.Windows.Data;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Infragistics.Controls.Charts
{
    /// <summary>
    /// Represents a XamDataChart radial column series.
    /// <remarks>Compare values across categories by using radial rectangles.</remarks>
    /// </summary>
    public sealed class RadialColumnSeries : AnchoredRadialSeries
    {
        /// <summary>
        /// Called to create the view for the series.
        /// </summary>
        /// <returns>The created view.</returns>
        protected override SeriesView CreateView()
        {
            return new RadialColumnSeriesView(this);
        }

        /// <summary>
        /// Called when the view for the series is created.
        /// </summary>
        /// <param name="view">The created view.</param>
        protected internal override void OnViewCreated(SeriesView view)
        {
            base.OnViewCreated(view);

            RadialColumnView = (RadialColumnSeriesView)view;
        }
        internal RadialColumnSeriesView RadialColumnView { get; set; }

        #region constructor and intialisation
        /// <summary>
        /// Initializes a new instance of the RadialColumnSeries class. 
        /// </summary>
        public RadialColumnSeries()
        {   
            DefaultStyleKey = typeof(RadialColumnSeries);
        }


        /// <summary>
        /// When overridden in a derived class, is invoked whenever application code or internal processes
        /// call ApplyTemplate.
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            this.RenderSeries(false);
        }

        #endregion

        #region RadiusX Dependency Property
        /// <summary>
        /// Gets or sets the x-radius of the ellipse that is used to round the corners of the column.
        /// <para>
        /// This is a dependency property.
        /// </para>
        /// </summary>
        [WidgetDefaultNumber(2.0)]
        public double RadiusX
        {
            get
            {
                return (double)GetValue(RadiusXProperty);
            }
            set
            {
                SetValue(RadiusXProperty, value);
            }
        }

        internal const string RadiusXPropertyName = "RadiusX";

        /// <summary>
        /// Identifies the RadiusX dependency property.
        /// </summary>
        public static readonly DependencyProperty RadiusXProperty = DependencyProperty.Register(RadiusXPropertyName, typeof(double), typeof(RadialColumnSeries),
            new PropertyMetadata(2.0, (sender, e) =>
            {
                (sender as RadialColumnSeries).RaisePropertyChanged(RadiusXPropertyName, e.OldValue, e.NewValue);
            }));
        #endregion

        #region RadiusY Dependency Property
        /// <summary>
        /// Gets or sets the y-radius of the ellipse that is used to round the corners of the column.
        /// <para>
        /// This is a dependency property.
        /// </para>
        /// </summary>
        [WidgetDefaultNumber(2.0)]
        public double RadiusY
        {
            get
            {
                return (double)GetValue(RadiusYProperty);
            }
            set
            {
                SetValue(RadiusYProperty, value);
            }
        }

        internal const string RadiusYPropertyName = "RadiusY";

        /// <summary>
        /// Identifies the RadiusY dependency property.
        /// </summary>
        public static readonly DependencyProperty RadiusYProperty = DependencyProperty.Register(RadiusYPropertyName, typeof(double), typeof(RadialColumnSeries),
            new PropertyMetadata(2.0, (sender, e) =>
            {
                (sender as RadialColumnSeries).RaisePropertyChanged(RadiusYPropertyName, e.OldValue, e.NewValue);
            }));
        #endregion

        internal override CategoryMode PreferredCategoryMode(CategoryAxisBase axis)
        {
            return axis != null && axis == AngleAxis ? CategoryMode.Mode2 : CategoryMode.Mode0;
        }

        /// <summary>
        /// Clears the rendering for the series.
        /// </summary>
        /// <param name="wipeClean">True if the cached visuals should also be cleared.</param>
        /// <param name="view">The SeriesView in context.</param>
        protected internal override void ClearRendering(bool wipeClean, SeriesView view)
        {
            base.ClearRendering(wipeClean, view);

            var radialColumnView = (RadialColumnSeriesView)view;
            if (wipeClean)
            {
                radialColumnView.Columns.Count = 0;
            }
        }

        internal override void RenderFrame(RadialFrame frame, RadialBaseView view)
        {
            base.RenderFrame(frame, view);

            // this is the brain-dead version. one column per bucket

            List<float[]> buckets = frame.Buckets;

            Rect windowRect = view.WindowRect;
            Rect viewportRect = view.Viewport;

            PolarAxisRenderingParameters p =
               (PolarAxisRenderingParameters)
               ValueAxis.CreateRenderingParams(viewportRect, windowRect);

            var columnView = (RadialColumnSeriesView)view;

            NumericRadiusAxis rscale = ValueAxis;

            double refValue = Math.Max(0,
                .5 * rscale.ActualInnerRadiusExtentScale);

            refValue = Math.Max(refValue, p.MinLength * .9);

            double zero = ViewportUtils.TransformXToViewportLength(refValue, windowRect, viewportRect);
            double groupWidthRadians = AngleAxis.GetGroupSize(windowRect, viewportRect);
            //get the column width based on the group size at 10% the radius extent. Find something else to do here?
            double widthAtRadius = Math.Max(
                .1 * ValueAxis.ActualRadiusExtentScale,
                refValue);
            double x0 = _axes.GetXValue(0, widthAtRadius, windowRect, viewportRect, Math.Cos);
            double y0 = _axes.GetYValue(0, widthAtRadius, windowRect, viewportRect, Math.Sin);
            double x1 = _axes.GetXValue(groupWidthRadians, widthAtRadius, windowRect, viewportRect, Math.Cos);
            double y1 = _axes.GetYValue(groupWidthRadians, widthAtRadius, windowRect, viewportRect, Math.Sin);

            double groupWidth = Math.Sqrt((x0 - x1) * (x0 - x1) +
                   (y0 - y1) * (y0 - y1));


            if (groupWidth < 1.0)
            {
                groupWidth = 1.0;
            }


            Point center = new Point(ViewportUtils.TransformXToViewport(0.5, windowRect, viewportRect),
                ViewportUtils.TransformYToViewport(0.5, windowRect, viewportRect));

            for (int i = 0; i < buckets.Count; ++i)
            {
                double length = Math.Min(frame.Buckets[i][2], p.MaxLength * 1.1);

                double x = _axes.GetXValue(frame.Buckets[i][0],
                        length, windowRect, viewportRect, Math.Cos);
                double y = _axes.GetYValue(frame.Buckets[i][0],
                        length, windowRect, viewportRect, Math.Sin);
                double radius = Math.Sqrt((x - center.X) * (x - center.X) +
                    (y - center.Y) * (y - center.Y));

                double top = radius;
                double bottom = zero;



#region Infragistics Source Cleanup (Region)




































#endregion // Infragistics Source Cleanup (Region)

                Rectangle column = columnView.Columns[i];
                column.Width = groupWidth;
                column.Height = Math.Abs(top - bottom);

                //#if TINYCLR
                //                double angle = frame.Buckets[i][0] - (Math.PI / 2.0);
                //#else
                double angle = (frame.Buckets[i][0] * 180.0 / Math.PI) - 90.0;
                //#endif
                double xpos = center.X - (groupWidth * 0.5);
                double ypos = center.Y + (Math.Min(bottom, top));

                columnView.PositionRectangle(column, xpos, ypos, angle, center.X, center.Y);

            }




            columnView.Columns.Count = buckets.Count;
            RadialColumnView.FinalizeColumns();
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