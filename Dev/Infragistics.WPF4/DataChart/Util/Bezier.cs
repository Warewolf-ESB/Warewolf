using System;
using System.Collections;
using System.Collections.Generic;

using System.Windows;
using System.Windows.Media;


namespace Infragistics.Controls.Charts.Util
{
    internal class Bezier
    {
        private double _resolution;
        public double Resolution
        {
            get { return _resolution; }
        }

        private Point _p0;
        public Point P0
        {
            get { return _p0; }
        }

        private Point _p1;
        public Point P1
        {
            get { return _p1; }
        }

        private Point _p2;
        public Point P2
        {
            get { return _p2; }
        }

        private Point _p3;
        public Point P3
        {
            get { return _p3; }
        }

        private double _threshold = double.MaxValue;

        public Bezier(
            Point p0, Point p1, 
            Point p2, Point p3, 
            double resolution,
            double threshold)
        {
            _p0 = p0;
            _p1 = p1;
            _p2 = p2;
            _p3 = p3;

            _resolution = resolution;
            _threshold = threshold;

            Valid = Raseterize();
        }

        public bool Valid { get; set; }



#region Infragistics Source Cleanup (Region)










#endregion // Infragistics Source Cleanup (Region)

        public List<BezierPoint> Points { get; set; }
        private List<BezierPoint> SortedPoints { get; set; }


        private BezierPoint Evaluate(double t)
        {
            double oneMinusT = 1.0 - t;
            double oneMinusT2 = oneMinusT * oneMinusT;
            double oneMinusT3 = oneMinusT2 * oneMinusT;
            double t2 = t * t;
            double t3 = t2 * t;

            Point point = 
                new Point(
                oneMinusT3 * _p0.X +
                3 * oneMinusT2 * t * _p1.X +
                3 * oneMinusT * t2 * _p2.X +
                t3 * _p3.X,
                oneMinusT3 * _p0.Y +
                3 * oneMinusT2 * t * _p1.Y +
                3 * oneMinusT * t2 * _p2.Y +
                t3 * _p3.Y);

            BezierPoint bp = new BezierPoint();
            bp.Point = point;
            bp.TValue = t;
            return bp;
        }




        Stack<BezierOp> _opStack = new Stack<BezierOp>();


        private bool Raseterize()
        {




            Points = new List<BezierPoint>();
            SortedPoints = new List<BezierPoint>();


            BezierOp op = new BezierOp();
            op.Start = 0; 
            op.End = 1;

            _opStack.Clear();
            _opStack.Push(op);

            int index = 0;

            while (_opStack.Count > 0)
            {
                BezierOp curr = (BezierOp)_opStack.Pop();
                BezierPoint p0 = Evaluate(curr.Start);
                BezierPoint p1 = Evaluate(curr.End);
                double distSquared = (p1.Point.X - p0.Point.X) * (p1.Point.X - p0.Point.X) + (p1.Point.Y - p0.Point.Y) * (p1.Point.Y - p0.Point.Y);
                if (distSquared < _resolution)
                {
                    p0.Index = index++;
                    p1.Index = index++;
                    Points.Add(p0);
                    Points.Add(p1);
                    if (p0.Point.X > _threshold ||
                        p1.Point.X > _threshold)
                    {
                        return false;
                    }
                }
                else
                {
                    double mid = (curr.Start + curr.End) / 2.0;
                    BezierOp op1 = new BezierOp();
                    op1.Start = curr.Start;
                    op1.End = mid;
                    BezierOp op2 = new BezierOp();
                    op2.Start = mid;
                    op2.End = curr.End;
                    _opStack.Push(op2);
                    _opStack.Push(op1);
                }
            }

            foreach (BezierPoint point in Points)
            {
                SortedPoints.Add(point);
            }

            SortPoints();

            return true;
        }

        private void SortPoints()
        {


#region Infragistics Source Cleanup (Region)














#endregion // Infragistics Source Cleanup (Region)

            SortedPoints.Sort((p1, p2) => p1.Point.Y.CompareTo(p2.Point.Y));

        }

        public BezierPoint GetPointAt(double y)
        {
            int index = BinarySearch(delegate(BezierPoint p)
            {
                if (y < p.Point.Y)
                {
                    return -1;
                }

                if (y > p.Point.Y)
                {
                    return 1;
                }

                return 0;
            });

            if (index < 0)
            {
                index = ~index;
            }

            if (index < 0)
            {
                index = 0;
            }
            if (index > SortedPoints.Count - 1)
            {
                index = SortedPoints.Count - 1;
            }

            double dist1 = 100000001;
            double dist2 = 100000000;
            double dist3 = 100000002;

            dist2 = Math.Abs(((BezierPoint)SortedPoints[index]).Point.Y - y);
            if (index - 1 >= 0)
            {
                dist1 = Math.Abs(((BezierPoint)SortedPoints[index - 1]).Point.Y - y);
            }
            if (index + 1 < SortedPoints.Count)
            {
                dist3 = Math.Abs(((BezierPoint)SortedPoints[index + 1]).Point.Y - y);
            }

            if (dist2 <= dist1 && dist2 <= dist3)
            {
                return ((BezierPoint)SortedPoints[index]);
            }
            if (dist1 <= dist2 && dist1 <= dist3 && index - 1 > 0)
            {
                return ((BezierPoint)SortedPoints[index - 1]);
            }
            if (dist3 <= dist1 && dist3 <= dist2 && index + 1 < SortedPoints.Count)
            {
                return ((BezierPoint)SortedPoints[index + 1]);
            }

            return ((BezierPoint)SortedPoints[index]);
        }

        private int BinarySearch(BezierPointComparison comparisonFunction)
        {
            int currMin = 0;
            int currMax = SortedPoints.Count - 1;

            while (currMin <= currMax)
            {
                int currMid = (currMin + ((currMax - currMin) >> 1));
                int compResult = comparisonFunction((BezierPoint)SortedPoints[currMid]);
                if (compResult < 0)
                {
                    currMax = currMid - 1;
                }
                else if (compResult > 0)
                {
                    currMin = currMid + 1;
                }
                else
                {
                    return currMid;
                }
            }

            return ~currMin;
        }
    }

    internal delegate int BezierPointComparison(BezierPoint p);

    internal class BezierOp
    {
        public double Start;
        public double End;
    }

    /// <summary>
    /// Represents a point on a segmented bezier curve.
    /// </summary>
    internal class BezierPoint
    {
        /// <summary>
        /// The point on the curve.
        /// </summary>
        public Point Point;
        /// <summary>
        /// The T value of the point.
        /// </summary>
        public double TValue;
        /// <summary>
        /// The index of the point.
        /// </summary>
        public int Index;
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