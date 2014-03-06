using System;
using System.ComponentModel;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Infragistics.Controls.Charts
{
    
    /// <summary>
    /// Represents a content control that holds a single chart slice.
    /// </summary>

    [DesignTimeVisible(false)]


    public class Slice : ContentControl
    {
        internal virtual SliceView CreateView()
        {
            return new SliceView(this);
        }
        internal virtual void OnViewCreated(SliceView view)
        {
            View = view;
        }
        internal SliceView View { get; set; }

        #region C'tor

        /// <summary>
        /// Creates an instance of the Slice.
        /// </summary>
        public Slice()
        {
            SliceView view = CreateView();
            OnViewCreated(view);
            view.OnInit();
        }

        #endregion C'tor

        #region Properties

        #region StartAngle

        internal const string StartAnglePropertyName = "StartAngle";

        /// <summary>
        /// Identifies the StartAngle dependency property.
        /// </summary>
        public static readonly DependencyProperty StartAngleProperty = DependencyProperty.Register(StartAnglePropertyName, typeof(double), typeof(Slice),
            new PropertyMetadata(0.0, (o, e) => (o as Slice).CreateShape()));

        /// <summary>
        /// Gets the shape's start angle.
        /// </summary>
        public double StartAngle
        {
            get { return (double)GetValue(StartAngleProperty); }
            internal set { SetValue(StartAngleProperty, value); }
        }

        #endregion StartAngle

        #region EndAngle

        internal const string EndAnglePropertyName = "EndAngle";

        /// <summary>
        /// Identifies the EndAngle dependency property.
        /// </summary>
        public static readonly DependencyProperty EndAngleProperty = DependencyProperty.Register(EndAnglePropertyName, typeof(double), typeof(Slice),
            new PropertyMetadata(0.0, (o, e) => (o as Slice).CreateShape()));

        /// <summary>
        /// Gets the shape's end angle.
        /// </summary>
        public double EndAngle
        {
            get { return (double)GetValue(EndAngleProperty); }
            internal set { SetValue(EndAngleProperty, value); }
        }

        #endregion EndAngle

        #region InnerExtentStart

        internal const string InnerExtentStartPropertyName = "InnerExtentStart";

        /// <summary>
        /// Identifies the InnerExtentStart dependency property.
        /// </summary>
        public static readonly DependencyProperty InnerExtentStartProperty = DependencyProperty.Register(InnerExtentStartPropertyName, typeof(double), typeof(Slice),
            new PropertyMetadata(0.0, (o, e) => (o as Slice).CreateShape()));

        /// <summary>
        /// Gets the shape's start inner extent.
        /// </summary>
        public double InnerExtentStart
        {
            get { return (double)GetValue(InnerExtentStartProperty); }
            internal set { SetValue(InnerExtentStartProperty, value); }
        }

        #endregion InnerExtentStart

        #region InnerExtentEnd

        internal const string InnerExtentEndPropertyName = "InnerExtentEnd";

        /// <summary>
        /// Identifies the InnerExtentEnd dependency property.
        /// </summary>
        public static readonly DependencyProperty InnerExtentEndProperty = DependencyProperty.Register(InnerExtentEndPropertyName, typeof(double), typeof(Slice),
            new PropertyMetadata(0.0, (o, e) => (o as Slice).CreateShape()));

        /// <summary>
        /// Gets the shape's end inner extent.
        /// </summary>
        public double InnerExtentEnd
        {
            get { return (double)GetValue(InnerExtentEndProperty); }
            internal set { SetValue(InnerExtentEndProperty, value); }
        }

        #endregion InnerExtentEnd

        #region IsSelected

        internal const string IsSelectedPropertyName = "IsSelected";

        /// <summary>
        /// Identifies the IsSelected dependency property.
        /// </summary>
        public static readonly DependencyProperty IsSelectedProperty = DependencyProperty.Register(IsSelectedPropertyName, typeof(bool), typeof(Slice),
            new PropertyMetadata(false, (o, e) =>
            {
                Slice slice = o as Slice;
                bool shouldSelect = (bool)e.NewValue;
                slice.Owner.SelectSlice(slice, shouldSelect);
            }));

        /// <summary>
        /// Gets or sets whether the slice is selected.
        /// </summary>
        [DontObfuscate]
        public bool IsSelected
        {
            get { return (bool)GetValue(IsSelectedProperty); }
            set { SetValue(IsSelectedProperty, value); }
        }

        #endregion IsSelected

        #region IsExploded

        internal const string IsExplodedPropertyName = "IsExploded";

        /// <summary>
        /// Identifies the IsExploded dependency property.
        /// </summary>
        public static readonly DependencyProperty IsExplodedProperty = DependencyProperty.Register(IsExplodedPropertyName, typeof(bool), typeof(Slice),
            new PropertyMetadata(false, (o, e) =>
            {
                Slice slice = o as Slice;
                bool explode = (bool)e.NewValue;
                slice.Owner.ExplodeSlice(slice, explode);
            }));

        /// <summary>
        /// Gets or sets whether the slice is exploded.
        /// </summary>
        [DontObfuscate]
        public bool IsExploded
        {
            get { return (bool)GetValue(IsExplodedProperty); }
            set { SetValue(IsExplodedProperty, value); }
        }

        #endregion IsExploded

        #region IsOtherSlice



#region Infragistics Source Cleanup (Region)













#endregion // Infragistics Source Cleanup (Region)

        internal const string IsOtherSlicePropertyName = "IsOtherSlice";

        /// <summary>
        /// Identifies the IsOtherSlice dependency property.
        /// </summary>
        public static readonly DependencyProperty IsOtherSliceProperty = DependencyProperty.Register(IsOtherSlicePropertyName, typeof(bool), typeof(Slice),
            new PropertyMetadata(false, (o, e) => (o as Slice).CreateShape()));

        /// <summary>
        /// Gets whether the slice represents the Others slice.
        /// </summary>
        public bool IsOthersSlice
        {
            get { return (bool)GetValue(IsOtherSliceProperty); }
            internal set { SetValue(IsOtherSliceProperty, value); }
        }


        #endregion IsOtherSlice

        #region Origin

        internal const string OriginPropertyName = "Origin";

        /// <summary>
        /// Identifies the Origin dependency property.
        /// </summary>
        public static readonly DependencyProperty OriginProperty = DependencyProperty.Register(OriginPropertyName, typeof(Point), typeof(Slice),
            new PropertyMetadata(new Point(), (o, e) => (o as Slice).CreateShape()));

        /// <summary>
        /// Gets the origin of the slice.
        /// </summary>
        public Point Origin
        {
            get { return (Point)GetValue(OriginProperty); }
            internal set { SetValue(OriginProperty, value); }
        }

        #endregion Origin

        #region ExplodedOrigin

        internal const string ExplodedOriginPropertyName = "ExplodedOrigin";

        /// <summary>
        /// Identifies the ExplodedOrigin dependency property.
        /// </summary>
        public static readonly DependencyProperty ExplodedOriginProperty = DependencyProperty.Register(ExplodedOriginPropertyName, typeof(Point), typeof(Slice),
            new PropertyMetadata(new Point(), (o, e) => (o as Slice).CreateShape()));

        /// <summary>
        /// Gets the origin of the exploded slice.
        /// </summary>
        public Point ExplodedOrigin
        {
            get { return (Point)GetValue(ExplodedOriginProperty); }
            internal set { SetValue(ExplodedOriginProperty, value); }
        }

        #endregion ExplodedOrigin

        #region Radius

        internal const string RadiusPropertyName = "Radius";

        /// <summary>
        /// Identifies the Radius dependency property.
        /// </summary>
        public static readonly DependencyProperty RadiusProperty = DependencyProperty.Register(RadiusPropertyName, typeof(double), typeof(Slice),
            new PropertyMetadata(0.0, (o, e) => (o as Slice).CreateShape()));

        /// <summary>
        /// Gets the slice's radius.
        /// </summary>
        public double Radius
        {
            get { return (double)GetValue(RadiusProperty); }
            internal set { SetValue(RadiusProperty, value); }
        }

        #endregion Radius

        #region ExplodedRadius

        internal const string ExplodedRadiusPropertyName = "ExplodedRadius";

        /// <summary>
        /// Identifies the ExplodedRadius dependency property.
        /// </summary>
        public static readonly DependencyProperty ExplodedRadiusProperty = DependencyProperty.Register(ExplodedRadiusPropertyName, typeof(double), typeof(Slice),
            new PropertyMetadata(0.0, (o, e) => (o as Slice).CreateShape()));

        /// <summary>
        /// Gets the radius of the exploded slice.
        /// </summary>
        public double ExplodedRadius
        {
            get { return (double)GetValue(ExplodedRadiusProperty); }
            internal set { SetValue(ExplodedRadiusProperty, value); }
        }

        #endregion ExplodedRadius

        #region Index

        internal const string IndexPropertyName = "Index";

        /// <summary>
        /// Identifies the Index dependency property.
        /// </summary>
        public static readonly DependencyProperty IndexProperty = DependencyProperty.Register(IndexPropertyName, typeof(int), typeof(Slice),
            new PropertyMetadata(-1, (o, e) => (o as Slice).CreateShape()));

        /// <summary>
        /// Gets the index of the slice.
        /// </summary>
        public int Index
        {
            get { return (int)GetValue(IndexProperty); }
            internal set { SetValue(IndexProperty, value); }
        }

        #endregion Index

        #region LeaderLineVisibility

        #endregion LeaderLineVisibility

        #region Label

        #endregion Label

        #region StrokeThickness

        internal const string StrokeThicknessPropertyName = "StrokeThickness";

        /// <summary>
        /// Identifies the StrokeThickness dependency property.
        /// </summary>
        public static DependencyProperty StrokeThicknessProperty = DependencyProperty.Register(StrokeThicknessPropertyName, typeof(double), typeof(Slice),
            new PropertyMetadata(1.0, (o, e) => (o as Slice).CreateShape()));

        /// <summary>
        /// Gets or sets the stroke thickness of the slice.
        /// </summary>
        public double StrokeThickness
        {
            get { return (double)GetValue(StrokeThicknessProperty); }
            set { SetValue(StrokeThicknessProperty, value); }
        }

        #endregion StrokeThickness

        #endregion Properties

        #region Internal Properties

        internal Rect Bounds { get; set; }

        internal PieChartBase Owner { get; set; }

        internal PieLabel Label { get; set; }

        internal Rect CorrectedExplodedBounds { get; set; }
        internal Point CorrectedExplodedOrigin { get; set; }
        internal bool HasCorrecttedBounds { get; set; }

        #endregion Internal Properties

        /// <summary>
        /// When overridden in a derived class, is invoked whenever application code or internal processes (such as a rebuilding layout pass) call <see cref="M:System.Windows.Controls.Control.ApplyTemplate"/>.
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            View.OnTemplateProvided();
        }

        #region Methods

        /// <summary>
        /// Gets the square bounds of the slice with the origin at the center.
        /// </summary>
        /// <remarks>This rect is different from SlicePath.Data.Bounds</remarks>
        private Rect GetSliceBounds()
        {
            bool allowExploded = (Owner != null && Owner.AllowSliceExplosion) ? true : false;
            if (IsExploded && allowExploded)
            {
                Rect bounds = new Rect(ExplodedOrigin.X - Radius, ExplodedOrigin.Y - Radius, Radius * 2, Radius * 2);
                return bounds;
            }

            return new Rect(Origin.X - Radius, Origin.Y - Radius, Radius * 2, Radius * 2);
        }

        /// <summary>
        /// Gets the origin of the slice.
        /// </summary>
        /// <returns></returns>
        internal Point GetSliceOrigin()
        {
            if (IsExploded && Owner != null && Owner.AllowSliceExplosion)
            {
                if (HasCorrecttedBounds)
                {
                    return CorrectedExplodedOrigin;
                }

                return ExplodedOrigin;
            }

            return Origin;
        }

        /// <summary>
        /// Returns a silverlight shape that represents the current arc.
        /// </summary>
        /// <returns>Silverlight shape that represents the current arc.</returns>
        internal void CreateShape()
        {
            Path slicePath = View.GetSlicePath();
            Rect viewport = Owner.Viewport;

            if (double.IsNaN(StartAngle)
                || double.IsNaN(EndAngle)
                || double.IsNaN(Radius)
                || double.IsNaN(ExplodedRadius)
                || Index < 0
                || Owner == null
                || viewport.Width == 0
                || viewport.Height == 0
                || (Origin.X == 0 && Origin.Y == 0)
                || (ExplodedOrigin.X == 0 && ExplodedOrigin.Y == 0))
            {
                return;
            }

            Bounds = GetSliceBounds();
            double ecc = GeometryUtil.Eccentricity(Bounds);
            double halfHeight = this.Bounds.Height / 2.0;

            //Center, Start and End points for the outer and inner arcs.
            Point center = Bounds.GetCenter();
            Point startPointOuter = EllipsePointAlternate(MathUtil.Radians(StartAngle), ecc, center, halfHeight, 100.0);
            Point endPointOuter = EllipsePointAlternate(MathUtil.Radians(EndAngle), ecc, center, halfHeight, 100.0);
            Point endPointInner = EllipsePointAlternate(MathUtil.Radians(EndAngle), ecc, center, halfHeight, InnerExtentEnd);
            Point startPointInner = EllipsePointAlternate(MathUtil.Radians(StartAngle), ecc, center, halfHeight, InnerExtentStart);

            bool circle = Math.Abs(PieChartBase.RoundAngle(EndAngle - StartAngle)) == 360;
            if (circle)
            {
                EllipseGeometry circleGeometry = new EllipseGeometry
                                                     {
                                                         Center = center,
                                                         RadiusX = Radius,
                                                         RadiusY = Radius
                                                     };
                slicePath.Data = circleGeometry;
                return;
            }

            PathGeometry geometry = new PathGeometry();
            slicePath.Data = geometry;

            PathFigure figure = new PathFigure { IsClosed = true };

            geometry.Figures = new PathFigureCollection();
            geometry.Figures.Add(figure);

            figure.StartPoint = startPointOuter;

            bool largeArc = Math.Abs(EndAngle - StartAngle) > 180.0;

            ArcSegment arcOuter = new ArcSegment();
            arcOuter.Point = endPointOuter;
            arcOuter.Size = new Size(Bounds.Width / 2.0, Bounds.Height / 2.0);
            arcOuter.IsLargeArc = largeArc;
            if (EndAngle > StartAngle)
            {
                arcOuter.SweepDirection = SweepDirection.Clockwise;
            }
            else
            {
                arcOuter.SweepDirection = SweepDirection.Counterclockwise;
            }

            figure.Segments = new PathSegmentCollection();
            figure.Segments.Add(arcOuter);

            LineSegment connectEnd = new LineSegment();
            connectEnd.Point = endPointInner;
            figure.Segments.Add(connectEnd);

            ArcSegment arcInner = new ArcSegment();
            arcInner.Point = startPointInner;
            arcInner.Size = arcOuter.Size;
            arcInner.IsLargeArc = arcOuter.IsLargeArc;
            arcInner.SweepDirection = arcOuter.SweepDirection;
            figure.Segments.Add(arcInner);

            Rect bounds = GetBounds(slicePath, startPointOuter, endPointOuter, StartAngle, EndAngle, center, Radius);

            //restrict the slice to be within chart's bounds.
            if (Bounds.Height > 0 && Bounds.Width > 0
                && !viewport.Contains(bounds))
            {
                Rect sliceBounds = bounds;
                Rect chartBounds = new Rect(0,0, viewport.Width, viewport.Height);
                chartBounds.Intersect(sliceBounds);

                double midAngle = GeometryUtil.SimplifyAngle((StartAngle + EndAngle) / 2);
                double midAngleRad = midAngle / 180 * Math.PI;
                double dx = Math.Abs((sliceBounds.Height - chartBounds.Height) / Math.Sin(midAngleRad));
                double dy = Math.Abs((sliceBounds.Width - chartBounds.Width) / Math.Cos(midAngleRad));

                if (double.IsNaN(dx) || double.IsInfinity(dx)) dx = 0;
                if (double.IsNaN(dy) || double.IsInfinity(dy)) dy = 0;

                double distance = Math.Max(dx, dy);

                Point explodedCenterNew = GeometryUtil.FindCenter(Owner.Viewport.Width, Owner.Viewport.Height, true, midAngle, Radius * Owner.ActualExplodedRadius - distance);

                View.PositionSlice(explodedCenterNew.X - ExplodedOrigin.X, explodedCenterNew.Y - ExplodedOrigin.Y);

                HasCorrecttedBounds = true;
                CorrectedExplodedOrigin = explodedCenterNew;
                CorrectedExplodedBounds = new Rect(
                    sliceBounds.X - (explodedCenterNew.X - ExplodedOrigin.X),
                    sliceBounds.Y - (explodedCenterNew.Y - ExplodedOrigin.Y),
                    sliceBounds.Width,
                    sliceBounds.Height);
            }
            else
            {
                HasCorrecttedBounds = false;
                View.ResetSlicePosition();
            }
        }

        /// <summary>
        /// Returns whether the slice contains the specified point.
        /// </summary>
        /// <param name="p">The point to check.</param>
        /// <returns>True if the slice contains the point.</returns>
        public bool ContainsPoint(Point p)
        {
            Rect viewport = Owner.Viewport;

            if (Visibility == System.Windows.Visibility.Collapsed)
            {
                return false;
            }

            if (double.IsNaN(StartAngle)
               || double.IsNaN(EndAngle)
               || double.IsNaN(Radius)
               || double.IsNaN(ExplodedRadius)
               || Index < 0
               || Owner == null
               || viewport.Width == 0
               || viewport.Height == 0
               || (Origin.X == 0 && Origin.Y == 0)
               || (ExplodedOrigin.X == 0 && ExplodedOrigin.Y == 0))
            {
                return false;
            }

            double startRadius = (InnerExtentEnd * Radius);
            double endRadius = Radius;
            Point center = Origin;

            if (IsExploded)
            {
                center = ExplodedOrigin;

                if (HasCorrecttedBounds)
                {
                    center = CorrectedExplodedOrigin;
                }
            }

            double startRadiusSquared = startRadius * startRadius;
            double endRadiusSquared = endRadius * endRadius;
            double dist = Math.Pow(p.X - center.X, 2) + Math.Pow(p.Y - center.Y, 2);

            if (dist < startRadiusSquared || dist > endRadiusSquared)
            {
                return false;
            }

            double angle = Math.Atan2(p.Y - center.Y, p.X - center.X);
            angle = angle * 180.0 / Math.PI;
            angle = GeometryUtil.SimplifyAngle(angle);

            double midAngle = GeometryUtil.SimplifyAngle((StartAngle + EndAngle) / 2);
            double angleDist = Math.Abs(EndAngle - StartAngle) / 2.0;

            if (angle > (midAngle + angleDist) ||
                angle < (midAngle - angleDist))
            {
                return false;
            }

            return true;
        }

        private Rect GetBounds(Path slicePath, Point outerStart, Point outerEnd, double startAngle, double endAngle, Point center, double radius)
        {


#region Infragistics Source Cleanup (Region)



































#endregion // Infragistics Source Cleanup (Region)

            if (Bounds.Width > 0 && Bounds.Height > 0)
                return slicePath.Data.Bounds;

            return new Rect(0, 0, 0, 0);

        }

        #endregion Methods

        private static Point EllipsePointAlternate(double theta, double eccentricity, Point center, double halfHeight, double extent)
        {
            double cos = Math.Cos(theta);
            double sin = Math.Sin(theta);
            double r = Math.Sqrt(halfHeight * halfHeight / (1 - (eccentricity * Math.Pow(cos, 2))));
            r *= (extent / 100.0);
            return new Point(r * cos + center.X, r * sin + center.Y);
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