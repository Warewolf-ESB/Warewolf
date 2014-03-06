using System.Collections.Generic;
using System.Windows;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Infragistics
{
    /// <summary>
    /// Modified Sutherland-Hodge clipping
    /// </summary>

    [EditorBrowsable(EditorBrowsableState.Never)]

    public class Clipper
    {
        /// <summary>
        /// Destination target for clipped points.
        /// </summary>
        public IList<Point> Target
        {
            get
            {
                return target;
            }
            set
            {
                if (_firstClipper != null)
                {
                    _firstClipper.Clear();
                }
                _firstClipper = null;

                target = value;

                IList<Point> head = target;

                if (leftClipper != null) 
                { 
                    leftClipper.Dst = head; 
                    head = leftClipper;
                    _firstClipper = leftClipper; 
                }
                if (bottomClipper != null) 
                { 
                    bottomClipper.Dst = head; 
                    head = bottomClipper;
                    bottomClipper.NextClipper = _firstClipper;
                    _firstClipper = bottomClipper; 
                }
                if (rightClipper != null) 
                { 
                    rightClipper.Dst = head; 
                    head = rightClipper;
                    rightClipper.NextClipper = _firstClipper;
                    _firstClipper = rightClipper; 
                }
                if (topClipper != null) 
                { 
                    topClipper.Dst = head; 
                    head = topClipper;
                    topClipper.NextClipper = _firstClipper;
                    _firstClipper = topClipper; 
                }

                Head = head;
            }
        }

        private IList<Point> Head;
        private EdgeClipper _firstClipper;

        private IList<Point> target;
        private LeftClipper leftClipper;
        private BottomClipper bottomClipper;
        private RightClipper rightClipper;
        private TopClipper topClipper;

        /// <summary>
        /// Initializes a new instance of the Clipper class.
        /// </summary>
        /// <param name="clip">Clip rectangle</param>
        /// <param name="isClosed">True to clip as polygon, false to clip as polyline</param>
        public Clipper(Rect clip, bool isClosed)
        {
            leftClipper = new LeftClipper() { Edge = clip.Left, IsClosed = isClosed };
            bottomClipper = new BottomClipper() { Edge = clip.Bottom, IsClosed = isClosed };
            rightClipper = new RightClipper() { Edge = clip.Right, IsClosed = isClosed };
            topClipper = new TopClipper() { Edge = clip.Top, IsClosed = isClosed };
        }

        /// <summary>
        /// Initializes a new instance of the Clipper class.
        /// </summary>
        /// <param name="left">Left edge of clip rectangle or NaN.</param>
        /// <param name="bottom">Bottom edge of clip rectangle or NaN.</param>
        /// <param name="right">Right edge of clip rectangle or NaN.</param>
        /// <param name="top">Top edge of clip rectangle or NaN.</param>
        /// <param name="isClosed">True to clip as polygon, false to clip as polyline</param>
        public Clipper(double left, double bottom, double right, double top, bool isClosed)
        {
            leftClipper = !double.IsNaN(left) ? new LeftClipper() { Edge = left, IsClosed = isClosed } : null;
            bottomClipper = !double.IsNaN(bottom) ? new BottomClipper() { Edge = bottom, IsClosed = isClosed } : null;
            rightClipper = !double.IsNaN(right) ? new RightClipper() { Edge = right, IsClosed = isClosed } : null;
            topClipper = !double.IsNaN(top) ? new TopClipper() { Edge = top, IsClosed = isClosed } : null;
        }

        /// <summary>
        /// Adds a Point to the clipper.
        /// </summary>
        /// <param name="point">The Point to be considered in the clipping operation.</param>
        public void Add(Point point)
        {
            Head.Add(point);
        }

        /// <summary>
        /// Boolean value. True if the clipper is configured to perform closed-shape clipping, else false.
        /// </summary>
        public bool IsClosed
        {
            get
            {
                return (leftClipper == null || leftClipper.IsClosed) &&
                       (bottomClipper == null || bottomClipper.IsClosed) &&
                       (rightClipper == null || rightClipper.IsClosed) &&
                       (topClipper == null || topClipper.IsClosed);
            }
            set
            {
                if (leftClipper != null) leftClipper.IsClosed = value;
                if (bottomClipper != null) bottomClipper.IsClosed = value;
                if (rightClipper != null) rightClipper.IsClosed = value;
                if (topClipper != null) topClipper.IsClosed = value;
            }
        }
    }

    #region EdgeClipper
    /// <summary>
    /// Represents a clipping stage in the Sutherland-Hodge clipper.
    /// </summary>
    /// <remarks>
    /// EdgeClipper implements IList so that it can be transparently
    /// pipe to either another edge clipper or a "real" IList implementation.
    /// </remarks>
    internal abstract class EdgeClipper : IList<Point>
    {
        /// <summary>
        /// Sets or gets the destination for the current edge clipper object.
        /// </summary>
        /// <remarks>
        /// Setting an edge clipper's destination resets the stage.
        /// </remarks>
        public IList<Point> Dst
        {
            get { return dst; }
            set
            {
                if (dst != value)
                {
                    init = true;
                    dst = value;
                }
            }
        }
        public IList<Point> dst;

        [DontObfuscate]
        private EdgeClipper _nextClipper;



        public EdgeClipper NextClipper 
        {
            get
            {
                return _nextClipper;
            }
            set
            {
                _nextClipper = value;
            }
        }

        private bool init = true;
        private Point First;

        private Point Prev;
        private bool PrevInside;

        public bool IsClosed;
        private bool IsOutput = false;

        /// <summary>
        /// Adds a point to the current edge clipper, resulting in zero, one or two
        /// points being piped to the desitnation IList.
        /// </summary>
        /// <param name="cur">Point to add to the clipping stage.</param>
        public void Add(Point cur)
        {
            bool CurInside = IsInside(cur);

            if (init)
            {
                init = false;
                First = cur;
            }
            else
            {
                if (true)// Prev.X != cur.X || Prev.Y != cur.Y)
                {
                    if (CurInside)
                    {
                        if (!PrevInside)
                        {
                            Dst.Add(Intersection(Prev, cur));
                        }
                        else
                        {
                            if (!IsClosed && !IsOutput)
                            {
                                Dst.Add(Prev);
                                IsOutput = true;
                            }
                        }

                        Dst.Add(cur);
                    }
                    else
                    {
                        if (PrevInside)
                        {
                            if (!IsClosed && !IsOutput)
                            {
                                Dst.Add(Prev);
                                IsOutput = true;
                            }

                            Dst.Add(Intersection(Prev, cur));
                        }
                    }
                }
            }

            Prev = cur;
            PrevInside = CurInside;
        }

        /// <summary>
        /// Flushes the edge clipping stage.
        /// </summary>
        public void Clear()
        {
            if (IsClosed && !init)
            {
                Add(First);
            }

            if (_nextClipper != null)
            {
                _nextClipper.Clear();
            }

            init = true;
            IsOutput = false;
        }

        /// <summary>
        /// Gets the status of the point with respect to the current clipping stage's edge.
        /// </summary>
        /// <param name="pt">Point to test</param>
        /// <returns>True if the point is inside or on the edge, false otherwise</returns>
        protected abstract bool IsInside(Point pt);

        /// <summary>
        /// Gets the intersection of an edge with the current clipping stage's edge.
        /// </summary>
        /// <param name="b">Start of edge</param>
        /// <param name="e">End of edge</param>
        /// <returns>Intersection of edge with the current clipping stage's edge</returns>
        protected abstract Point Intersection(Point b, Point e);

        #region place-holder implementations for the other members of IList
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return null;
        }
        public System.Collections.Generic.IEnumerator<Point> GetEnumerator() { return null; }
        public bool IsReadOnly { get { return false; } }
        public int Count { get { return 0; } }
        public bool Remove(Point pt) { return false; }
        public void RemoveAt(int n) { }
        public void CopyTo(Point[] pt, int n) { }
        public bool Contains(Point pt) { return false; }
        public Point this[int n] { get { return new Point(0, 0); } set { } }
        public void Insert(int n, Point pt) { }
        public int IndexOf(Point pt) { return -1; }
        #endregion
    }
    #endregion

    #region EdgeClipper for left edges
    /// <summary>
    /// Represents a specialized clipping stage for a clip window's left edge.
    /// </summary>
    internal class LeftClipper : EdgeClipper
    {
        public double Edge;

        protected override bool IsInside(Point pt)
        {
            return pt.X >= Edge;
        }

        protected override Point Intersection(Point b, Point e)
        {
            return new Point(Edge, b.Y + (e.Y - b.Y) * (Edge - b.X) / (e.X - b.X));
        }
    }
    #endregion

    #region EdgeClipper for bottom edges
    /// <summary>
    /// Represents a specialized clipping stage for a clip window's bottom edge.
    /// </summary>
    internal class BottomClipper : EdgeClipper
    {
        public double Edge;

        protected override bool IsInside(Point pt)
        {
            return pt.Y <= Edge;
        }

        protected override Point Intersection(Point b, Point e)
        {
            return new Point(b.X + (e.X - b.X) * (Edge - b.Y) / (e.Y - b.Y), Edge);
        }
    }
    #endregion

    #region EdgeClipper for right edges
    /// <summary>
    /// Represents a specialized clipping stage for a clip window's right edge.
    /// </summary>
    internal class RightClipper : EdgeClipper
    {
        public double Edge;

        protected override bool IsInside(Point pt)
        {
            return pt.X <= Edge;
        }

        protected override Point Intersection(Point b, Point e)
        {
            return new Point(Edge, b.Y + (e.Y - b.Y) * (Edge - b.X) / (e.X - b.X));
        }
    }
    #endregion

    #region EdgeClipper for top edges
    /// <summary>
    /// Represents a specialized clipping stage for a clip window's top edge.
    /// </summary>
    internal class TopClipper : EdgeClipper
    {
        public double Edge;

        protected override bool IsInside(Point pt)
        {
            return pt.Y >= Edge;
        }

        protected override Point Intersection(Point b, Point e)
        {
            return new Point(b.X + (e.X - b.X) * (Edge - b.Y) / (e.Y - b.Y), Edge);
        }
    }
    #endregion
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