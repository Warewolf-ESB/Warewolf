using System;
using System.Windows;
using System.Windows.Media;

namespace Infragistics
{
    /// <summary>
    /// Utilty methods for PathFigures.
    /// </summary>
    public static class PathFigureUtil
    {
        /// <summary>
        /// Duplicates the specified path figure collection.
        /// </summary>
        /// <param name="pathFigureCollection">The path figure collection.</param>
        /// <returns></returns>
        public static PathFigureCollection Duplicate(this PathFigureCollection pathFigureCollection)
        {
            PathFigureCollection dup = new PathFigureCollection();

            foreach (PathFigure pathFigure in pathFigureCollection)
            {
                dup.Add(pathFigure.Duplicate());
            }

            return dup;
        }

        /// <summary>
        /// Creates a duplicate of the current PathFigure object.
        /// </summary>
        /// <param name="pathFigure">The current PathFigure object.</param>
        /// <returns>A new PathFigure</returns>
        public static PathFigure Duplicate(this PathFigure pathFigure)
        {
            if (pathFigure == null)
            {
                return null;
            }

            PathSegmentCollection segments = new PathSegmentCollection();

            foreach (PathSegment pathSegment in pathFigure.Segments)
            {


#region Infragistics Source Cleanup (Region)

























#endregion // Infragistics Source Cleanup (Region)

                ArcSegment arcSeg = pathSegment as ArcSegment;
                if (arcSeg != null)
                {
                    segments.Add(new ArcSegment() { IsLargeArc = arcSeg.IsLargeArc, Point = arcSeg.Point, RotationAngle = arcSeg.RotationAngle, Size = arcSeg.Size, SweepDirection = arcSeg.SweepDirection });
                    continue;
                }

                BezierSegment bezSeg = pathSegment as BezierSegment;
                if (bezSeg != null)
                {
                    segments.Add(new BezierSegment { Point1 = bezSeg.Point1, Point2 = bezSeg.Point2, Point3 = bezSeg.Point3 });
                    continue;
                }

                LineSegment lineSeg = pathSegment as LineSegment;
                if (lineSeg != null)
                {
                    segments.Add(new LineSegment() { Point = lineSeg.Point });
                    continue;
                }

                PolyBezierSegment polybezSeg = pathSegment as PolyBezierSegment;
                if (polybezSeg != null)
                {   
                    PointCollection points = new PointCollection();

                    foreach (Point point in polybezSeg.Points)
                    {
                        points.Add(point);
                    }

                    segments.Add(new PolyBezierSegment() { Points = points });
                    continue;
                }

                PolyLineSegment polyLineSeg = pathSegment as PolyLineSegment;
                if (polyLineSeg != null)
                {   
                    PointCollection points = new PointCollection();

                    foreach (Point point in polyLineSeg.Points)
                    {
                        points.Add(point);
                    }

                    segments.Add(new PolyLineSegment() { Points = points });
                    continue;
                }

                PolyQuadraticBezierSegment polyQuadBezSeg = pathSegment as PolyQuadraticBezierSegment;
                if (polyQuadBezSeg != null)
                {   
                    PointCollection points = new PointCollection();

                    foreach (Point point in polyQuadBezSeg.Points)
                    {
                        points.Add(point);
                    }

                    segments.Add(new PolyQuadraticBezierSegment() { Points = points });
                    continue;
                }

                QuadraticBezierSegment quadBezSeg = pathSegment as QuadraticBezierSegment;
                if (quadBezSeg != null)
                {
                    segments.Add(new QuadraticBezierSegment() { Point1 = quadBezSeg.Point1, Point2 = quadBezSeg.Point2 });
                    continue;
                }

                throw new NotImplementedException("PathFigure.Duplicate: " + pathSegment.ToString());

            }

            return new PathFigure() { IsClosed = pathFigure.IsClosed, IsFilled = pathFigure.IsFilled, StartPoint = pathFigure.StartPoint, Segments = segments };
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