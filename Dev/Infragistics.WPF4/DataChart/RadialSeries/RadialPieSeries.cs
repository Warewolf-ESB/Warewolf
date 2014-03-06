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
    /// Represents a XamDataChart radial pie series.
    /// <remarks>Compare values across categories by using radial pie slices.</remarks>
    /// </summary>
    public sealed class RadialPieSeries : AnchoredRadialSeries
    {
        /// <summary>
        /// Called to create the view for the series.
        /// </summary>
        /// <returns>The created view.</returns>
        protected override SeriesView CreateView()
        {
            return new RadialPieSeriesView(this);
        }

        /// <summary>
        /// Called when the view for a series is created.
        /// </summary>
        /// <param name="view">The view that was created.</param>
        protected internal override void OnViewCreated(SeriesView view)
        {
            base.OnViewCreated(view);

            RadialPieView = (RadialPieSeriesView)view;
        }
        internal RadialPieSeriesView RadialPieView { get; set; }

        #region constructor and intialization
        /// <summary>
        /// Initializes a new instance of the ColumnSeries class. 
        /// </summary>
        public RadialPieSeries()
        {
            DefaultStyleKey = typeof(RadialPieSeries);
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
        public static readonly DependencyProperty RadiusXProperty = DependencyProperty.Register(RadiusXPropertyName, typeof(double), typeof(RadialPieSeries),
            new PropertyMetadata(2.0, (sender, e) =>
            {
                (sender as RadialPieSeries).RaisePropertyChanged(RadiusXPropertyName, e.OldValue, e.NewValue);
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
        public static readonly DependencyProperty RadiusYProperty = DependencyProperty.Register(RadiusYPropertyName, typeof(double), typeof(RadialPieSeries),
            new PropertyMetadata(2.0, (sender, e) =>
            {
                (sender as RadialPieSeries).RaisePropertyChanged(RadiusYPropertyName, e.OldValue, e.NewValue);
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

            var radialPieView = (RadialPieSeriesView)view;
            if (wipeClean && radialPieView.Slices != null)
            {
               radialPieView.Slices.Count = 0;
            }
        }

        internal override void RenderFrame(RadialFrame frame, RadialBaseView view)
        {
            base.RenderFrame(frame, view);

            var pieView = (RadialPieSeriesView)view;

            Rect windowRect = view.WindowRect;
            Rect viewportRect = view.Viewport;

            PolarAxisRenderingParameters p = 
                (PolarAxisRenderingParameters)
                ValueAxis.CreateRenderingParams(viewportRect, windowRect);

            // this is the brain-dead version. one slice per bucket

            List<float[]> buckets = frame.Buckets;

            NumericRadiusAxis rscale = ValueAxis;

            double refValue = Math.Max(0,
                .5 * rscale.ActualInnerRadiusExtentScale);

            double zero = refValue;
            zero = Math.Max(zero, p.MinLength);

            double groupWidthRadians = AngleAxis.GetGroupSize(windowRect, viewportRect);

            Point center = new Point(0.5, 0.5);
            double radiusX = RadiusX;
            double radiusY = RadiusY;
            bool doRounded = (radiusX > 0 && radiusY > 0);

            double radius = 0;
            for (int i = 0; i < buckets.Count; ++i)
            {
                Path slice = pieView.Slices[i];

                double angleRadians = buckets[i][0];
                double length = Math.Min(buckets[i][2], p.MaxLength);

                PathGeometry pg = null;
                if (doRounded)
                {
                    pg = DrawRoundedSlice(windowRect, viewportRect,
                      angleRadians - (groupWidthRadians * .5),
                      angleRadians + (groupWidthRadians * .5),
                      zero,
                      length, center,
                      radiusX,
                      radiusY);
                }
                else
                {
                    pg = DrawSlice(windowRect, viewportRect,
                      angleRadians - (groupWidthRadians * .5),
                      angleRadians + (groupWidthRadians * .5),
                      zero,
                      length, center);
                }
                slice.Data = pg;

                var sliceRadius = buckets[i][2];
                if (sliceRadius > radius)
                {
                    radius = sliceRadius;
                }
            }








            pieView.Slices.Count = buckets.Count;
            pieView.SlicesUpdated();
        }

        

        private PathGeometry DrawSlice(Rect windowRect, Rect viewportRect,
            double startAngle, double endAngle,
            double zero, double radius, Point center)
        {
            SliceCoords sc = SliceCoords.GetSliceCoords(
                windowRect, viewportRect, startAngle, endAngle,
                zero, radius, center);

            PathFigure pf = new PathFigure();

            pf.StartPoint = sc.A;
            pf.IsClosed = true;
            pf.Segments.Add(new LineSegment() { Point = sc.B });

            pf.Segments.Add(new ArcSegment()
                                {
                                    Point = sc.C,
                                    Size = sc.OuterSize,
                                    SweepDirection = SweepDirection.Clockwise,
                                    IsLargeArc = sc.IsLargeArc
                                });

            pf.Segments.Add(new LineSegment() { Point = sc.D });

            pf.Segments.Add(new ArcSegment()
                                {
                                    Point = sc.A,
                                    Size = sc.InnerSize,
                                    SweepDirection = SweepDirection.Counterclockwise,
                                    IsLargeArc = sc.IsLargeArc
                                });

            PathGeometry pg = new PathGeometry();
            pg.Figures.Add(pf);
            return pg;
        }

        private PathGeometry DrawRoundedSlice(Rect windowRect, Rect viewportRect,
            double startAngle, double endAngle,
            double zero, double radius, Point center,
            double radiusX, double radiusY)
        {
            SliceCoords sc = SliceCoords.GetRoundedSliceCoords(
                windowRect, viewportRect, startAngle, endAngle,
                zero, radius, center,
                radiusX, radiusY);
            if (sc == null)
            {
                return DrawSlice(windowRect, viewportRect,
                    startAngle, endAngle,
                    zero, radius, center);
            }

            PathFigure pf = new PathFigure();

            pf.StartPoint = sc.A;
            pf.IsClosed = true;

            double rotationAngle = ((startAngle + ((endAngle - startAngle) * .5)) * 180.0 / Math.PI) + 90;

            Size lowerCornerSize = 
                new Size(sc.CornerSize.Width * (zero / radius), 
                    sc.CornerSize.Height * (zero / radius));

            pf.Segments.Add(new ArcSegment()
            {
                Point = sc.A2,
                Size = lowerCornerSize,
                RotationAngle = rotationAngle,
                SweepDirection = SweepDirection.Clockwise,
                IsLargeArc = false
            });

            pf.Segments.Add(new LineSegment() { Point = sc.B });

            pf.Segments.Add(new ArcSegment()
            {
                Point = sc.B2,
                Size = sc.CornerSize,
                RotationAngle = rotationAngle,
                SweepDirection = SweepDirection.Clockwise,
                IsLargeArc = false
            });

            pf.Segments.Add(new ArcSegment()
            {
                Point = sc.C,
                Size = sc.OuterSize,
                SweepDirection = SweepDirection.Clockwise,
                IsLargeArc = sc.IsLargeArc
            });

            pf.Segments.Add(new ArcSegment()
            {
                Point = sc.C2,
                Size = sc.CornerSize,
                RotationAngle = rotationAngle,
                SweepDirection = SweepDirection.Clockwise,
                IsLargeArc = false
            });

            pf.Segments.Add(new LineSegment() { Point = sc.D });

            pf.Segments.Add(new ArcSegment()
            {
                Point = sc.D2,
                Size = lowerCornerSize,
                RotationAngle = rotationAngle,
                SweepDirection = SweepDirection.Clockwise,
                IsLargeArc = false
            });

            pf.Segments.Add(new ArcSegment()
            {
                Point = sc.A,
                Size = sc.InnerSize,
                SweepDirection = SweepDirection.Counterclockwise,
                IsLargeArc = sc.IsLargeArc
            });

            PathGeometry pg = new PathGeometry();
            pg.Figures.Add(pf);
            return pg;
        }

        /// <summary>
        /// When overridden in a derived class, gets called whenever a property value is updated
        /// on the series or owning chart. Gives the series a chance to respond to the various property updates.
        /// </summary>
        /// <param name="sender">The object being updated.</param>
        /// <param name="propertyName">The name of the property being updated.</param>
        /// <param name="oldValue">The old value of the property.</param>
        /// <param name="newValue">The new value of the property.</param>
        protected override void PropertyUpdatedOverride(object sender, string propertyName, object oldValue, object newValue)
        {
            base.PropertyUpdatedOverride(sender, propertyName, oldValue, newValue);

            switch (propertyName)
            {
                case RadiusXPropertyName:
                case RadiusYPropertyName:
                    RenderSeries(false);
                    break;
            }
        }
    }

    internal class SliceCoords
    {
        public Point A { get; set; }
        public Point B { get; set; }
        public Point C { get; set; }
        public Point D { get; set; }

        public Point A2 { get; set; }
        public Point B2 { get; set; }
        public Point C2 { get; set; }
        public Point D2 { get; set; }

        public bool IsLargeArc { get; set; }
        public Size OuterSize { get; set; }
        public Size InnerSize { get; set; }
        public Size CornerSize { get; set; }

        public static SliceCoords GetSliceCoords(
        Rect windowRect, Rect viewportRect,
        double startAngle, double endAngle,
        double zero, double radius, Point center)
        {
            double angleMin = Math.Min(startAngle, endAngle);
            double angleMax = Math.Max(startAngle, endAngle);

            double cosAngleMin = Math.Cos(angleMin);
            double sinAngleMin = Math.Sin(angleMin);

            double minLength = Math.Max(0, zero);
            double maxLength = radius;

            double startXmin = center.X + cosAngleMin * minLength;
            double startYmin = center.Y + sinAngleMin * minLength;
            double endXmin = center.X + cosAngleMin * maxLength;
            double endYmin = center.Y + sinAngleMin * maxLength;

            double cosAngleMax = Math.Cos(angleMax);
            double sinAngleMax = Math.Sin(angleMax);

            double startXmax = center.X + cosAngleMax * minLength;
            double startYmax = center.Y + sinAngleMax * minLength;
            double endXmax = center.X + cosAngleMax * maxLength;
            double endYmax = center.Y + sinAngleMax * maxLength;

            startXmin = ViewportUtils.TransformXToViewport(startXmin, windowRect, viewportRect);
            startYmin = ViewportUtils.TransformYToViewport(startYmin, windowRect, viewportRect);
            endXmin = ViewportUtils.TransformXToViewport(endXmin, windowRect, viewportRect);
            endYmin = ViewportUtils.TransformYToViewport(endYmin, windowRect, viewportRect);
            startXmax = ViewportUtils.TransformXToViewport(startXmax, windowRect, viewportRect);
            startYmax = ViewportUtils.TransformYToViewport(startYmax, windowRect, viewportRect);
            endXmax = ViewportUtils.TransformXToViewport(endXmax, windowRect, viewportRect);
            endYmax = ViewportUtils.TransformYToViewport(endYmax, windowRect, viewportRect);

            Point a = new Point(startXmin, startYmin);
            Point b = new Point(endXmin, endYmin);
            Point c = new Point(endXmax, endYmax);
            Point d = new Point(startXmax, startYmax);

            double maxLenX = ViewportUtils.TransformXToViewportLength(maxLength, windowRect, viewportRect);
            double maxLenY = ViewportUtils.TransformYToViewportLength(maxLength, windowRect, viewportRect);

            double minLenX = ViewportUtils.TransformXToViewportLength(minLength, windowRect, viewportRect);
            double minLenY = ViewportUtils.TransformYToViewportLength(minLength, windowRect, viewportRect);

            return new SliceCoords()
            {
                A = a,
                B = b,
                C = c,
                D = d,
                IsLargeArc = ((angleMax - angleMin) > Math.PI),
                OuterSize = new Size(maxLenX, maxLenY),
                InnerSize = new Size(minLenX, minLenY)
            };
        }

        internal static SliceCoords GetRoundedSliceCoords(
            Rect windowRect, Rect viewportRect, double startAngle,
            double endAngle, double zero, double radius, Point center,
            double radiusX, double radiusY)
        {
            double radDiff = ViewportUtils.TransformXFromViewportLength(
                radiusY, windowRect, viewportRect);
            double radLength = radius - zero;
            if (radLength < 0)
            {
                return null;
            }
            if (ViewportUtils.TransformXToViewportLength(
                radLength, windowRect, viewportRect) < 2.0)
            {
                return null;
            }

            if (radDiff * 2.0 > radLength)
            {
                radDiff = radLength / 2.0;
                radiusY =
                    ViewportUtils.TransformXToViewportLength(
                    radDiff,
                    windowRect,
                    viewportRect);
            }
            double xDiff = ViewportUtils.TransformXFromViewportLength(
                radiusX, windowRect, viewportRect);
            double oppOverAdj = xDiff / (radius - radDiff);

            double angleDiff = Math.Atan(oppOverAdj);
            if (angleDiff * 2.0 > Math.Abs(endAngle - startAngle))
            {
                angleDiff = Math.Abs(endAngle - startAngle) / 2.0;
                radiusX =
                    ViewportUtils.TransformXToViewportLength(
                    Math.Tan(angleDiff) * (radius - radDiff),
                    windowRect,
                    viewportRect);
            }

            SliceCoords sc = GetSliceCoords(
                windowRect, viewportRect, startAngle + angleDiff, endAngle - angleDiff,
                zero, radius, center);
            SliceCoords sc2 = GetSliceCoords(
                windowRect, viewportRect, startAngle, endAngle,
                zero + (radDiff * (zero / radius)), radius - radDiff, center);

            sc.B2 = sc.B;
            sc.D2 = sc.D;
            sc.A2 = sc2.A;
            sc.B = sc2.B;
            sc.C2 = sc2.C;
            sc.D = sc2.D;

            sc.CornerSize = new Size(radiusX, radiusY);

            return sc;
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