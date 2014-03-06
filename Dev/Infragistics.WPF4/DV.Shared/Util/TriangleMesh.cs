using System;
using System.Collections.Generic;
using System.Windows;
using System.Collections.ObjectModel;

namespace Infragistics.Silverlight
{
    /// <summary>
    /// A directed half-edge segment defined by two vertex codes.
    /// </summary>
    public class HalfEdge
    {
        /// <summary>
        /// HalfEdge constructor.
        /// </summary>
        public HalfEdge(int beg, int end)
        {
            this.Beg = beg;
            this.End = end;
        }
        /// <summary>
        /// B.
        /// </summary>
        public int Beg { get; set; }
        /// <summary>
        /// E.
        /// </summary>
        public int End { get; set; }
    }

    /// <summary>
    /// An EdgeSet represents an unordered set of half-edges.
    /// </summary>
    /// <remarks>
    /// The set may not contain two half-edges which form together form a full edge, and it is an
    /// error to attempt to insert such a pair.
    /// </remarks>
    public class HalfEdgeCollection : IEnumerable<HalfEdge>
    {
        /// <summary>
        /// Returns an enumerator that iterates through a collection. 
        /// </summary>
        /// <returns>An IEnumerator object that can be used to iterate through the collection.</returns>
        public IEnumerator<HalfEdge> GetEnumerator()
        {
            return edges.Keys.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() { return this.GetEnumerator(); }
        /// <summary>
        /// Adds a HalfEdge to the set.
        /// </summary>
        /// <param name="edge">The HalfEdge to add.</param>
        public void Add(HalfEdge edge)
        {
            edges.Add(edge, null);
        }
        /// <summary>
        /// Removes a HalfEdge from the set.
        /// </summary>
        /// <param name="edge">The HalfEdge to remove from the set.</param>
        public void Remove(HalfEdge edge)
        {
            edges.Remove(edge);
        }
        /// <summary>
        /// Clears the set.
        /// </summary>
        public void Clear()
        {
            edges.Clear();
        }
        /// <summary>
        /// Count of HalfEdges in the set.
        /// </summary>
        public int Count
        {
            get { return edges.Count; }
        }
        /// <summary>
        /// Determines whether or not the given HalfEdge exists in the set.
        /// </summary>
        /// <param name="edge">The HalfEdge under observation.</param>
        /// <returns>True if the set contains the HalfEdge, otherwise False.</returns>
        public bool Contains(HalfEdge edge)
        {
            return edges.ContainsKey(edge);
        }

        private class EdgeComparer : IEqualityComparer<HalfEdge>
        {
            public bool Equals(HalfEdge e1, HalfEdge e2)
            {
                return (e1.Beg == e2.Beg && e1.End == e2.End) || (e1.Beg == e2.End && e1.End == e2.Beg);
            }

            public int GetHashCode(HalfEdge e1)
            {
                return 65536 * Math.Max(e1.Beg, e1.End) + Math.Min(e1.Beg, e1.End);
            }
        }

        private Dictionary<HalfEdge, object> edges = new Dictionary<HalfEdge, object>(new EdgeComparer());
    }

    /// <summary>
    /// Triangle class.
    /// </summary>
    public class Triangle
    {
        /// <summary>
        /// v0.
        /// </summary>
        public int v0 { get; internal set; }
        /// <summary>
        /// v1.
        /// </summary>
        public int v1 { get; internal set; }
        /// <summary>
        /// v2.
        /// </summary>
        public int v2 { get; internal set; }
    }


    /// <summary>
    /// Delaunay triangulation of a list of points.
    /// </summary>
    public class TriangleMesh
    {
        /// <summary>
        /// Collection of Triangles.
        /// </summary>
        public Collection<Triangle> TriangleList
        { 
            get
            {
                if(triangleList!=null)
                {
                    return triangleList;
                }
                
                if (pointList==null || pointList.Count < 3)
                {
                    triangleList = new Collection<Triangle>();

                    return triangleList;
                }

                HalfEdgeCollection edgeBuffer = new HalfEdgeCollection();
                PointTester pointTester = new PointTester();

                int triangleCount = 0;
                Triangle[] triangleBuffer = new Triangle[4 * pointList.Count];
                bool[] complete = new bool[triangleBuffer.Length];

                List<int> sort = new List<int>() { Capacity = pointList.Count };

                for (int i = 0; i < pointList.Count; ++i)
                {
                    sort.Add(i);
                }

                sort.Sort(new Comparer(pointList));

                // add super triangle vertices

                double xmin = pointList[sort[0]].X;
                double xmax = pointList[sort[pointList.Count - 1]].X;

                double ymin = pointList[0].Y;
                double ymax = ymin;

                for (int i = 1; i < pointList.Count; i++)
                {
                    ymin = Math.Min(ymin, pointList[i].Y);
                    ymax = Math.Max(ymax, pointList[i].Y);
                }

                double dx = xmax - xmin;
                double dy = ymax - ymin;
                double dmax = Math.Max(dx, dy);
                double xmid = (xmax + xmin) / 2;
                double ymid = (ymax + ymin) / 2;

                s0 = new Point(xmid - 20 * dmax, ymid - dmax);
                s1 = new Point(xmid, ymid + 20 * dmax);
                s2 = new Point(xmid + 20 * dmax, ymid - dmax);

                // add super triangle and initial triangle buffer 

                triangleBuffer[0] = new Triangle() { v0 = pointList.Count, v1 = pointList.Count + 1, v2 = pointList.Count + 2 };
                complete[0] = false;

                for (int i = 1; i < triangleBuffer.Length; ++i)
                {
                    triangleBuffer[i] = new Triangle();
                    complete[i] = false;
                };

                ++triangleCount;

                for (int i = 0; i < pointList.Count; ++i)
                {
                    edgeBuffer.Clear();

                    for (int j = 0; j < triangleCount; ++j)
                    {
                        if (complete[j])
                        {
                            continue;
                        }

                        pointTester.Test(point(sort[i]), point(triangleBuffer[j].v0), point(triangleBuffer[j].v1), point(triangleBuffer[j].v2));

                        complete[j] = pointTester.Complete;

                        if (pointTester.Inside)
                        {
                            HalfEdge e;

                            e = new HalfEdge(triangleBuffer[j].v0, triangleBuffer[j].v1);
                            if (edgeBuffer.Contains(e)) { edgeBuffer.Remove(e); } else { edgeBuffer.Add(e); }

                            e = new HalfEdge(triangleBuffer[j].v1, triangleBuffer[j].v2);
                            if (edgeBuffer.Contains(e)) { edgeBuffer.Remove(e); } else { edgeBuffer.Add(e); }

                            e = new HalfEdge(triangleBuffer[j].v2, triangleBuffer[j].v0);
                            if (edgeBuffer.Contains(e)) { edgeBuffer.Remove(e); } else { edgeBuffer.Add(e); }

                            triangleBuffer[j].v0 = triangleBuffer[triangleCount - 1].v0;
                            triangleBuffer[j].v1 = triangleBuffer[triangleCount - 1].v1;
                            triangleBuffer[j].v2 = triangleBuffer[triangleCount - 1].v2;

                            complete[j] = complete[triangleCount - 1];

                            --triangleCount;
                            --j;
                        }
                    }
 
                    // (re)create triangles

                    foreach (HalfEdge edge in edgeBuffer)
                    {
                        triangleBuffer[triangleCount].v0 = edge.Beg;
                        triangleBuffer[triangleCount].v1 = edge.End;
                        triangleBuffer[triangleCount].v2 = sort[i];
                        complete[triangleCount] = false;

                        ++triangleCount;
                    }
                }

                // save permanent mesh

                triangleList = new Collection<Triangle>();

                for (int i = 0; i < triangleCount; ++i)
                {
                    if (triangleBuffer[i].v0 < pointList.Count && triangleBuffer[i].v1 < pointList.Count && triangleBuffer[i].v2 < pointList.Count)
                    {
                        triangleList.Add(triangleBuffer[i]);
                    }
                }

                return triangleList;
            }
        }
        private Collection<Triangle> triangleList;
        /// <summary>
        /// Set of HalfEdges.
        /// </summary>
        public HalfEdgeCollection EdgeCollection
        {
            get
            {
                if (_edgeCollection == null)
                {
                    _edgeCollection = new HalfEdgeCollection();
                    HalfEdge    e=null;

                    foreach (Triangle triangle in TriangleList)
                    {
                        e=new HalfEdge(triangle.v0, triangle.v1);
                        if (!_edgeCollection.Contains(e)) { _edgeCollection.Add(e); } else { _edgeCollection.Remove(e); }

                        e = new HalfEdge(triangle.v1, triangle.v2);
                        if (!_edgeCollection.Contains(e)) { _edgeCollection.Add(e); } else { _edgeCollection.Remove(e); }

                        e = new HalfEdge(triangle.v2, triangle.v0);
                        if (!_edgeCollection.Contains(e)) { _edgeCollection.Add(e); } else { _edgeCollection.Remove(e); }
                    }
                }

                return _edgeCollection;
            }
        }
        private HalfEdgeCollection _edgeCollection;
        /// <summary>
        /// List of Points.
        /// </summary>
        public IList<Point> Points
        {
            get { return pointList ?? (pointList = new List<Point>()); }
        }
        private Point point(int i)
        {
            if (i < pointList.Count) { return pointList[i]; }
            if (i == pointList.Count) { return s0; }
            if (i == pointList.Count + 1) { return s1; }

            return s2;
        }
        private IList<Point> pointList;

        /// <summary>
        /// Implements lexical comparison for points
        /// </summary>
        private class Comparer : IComparer<int>
        {
            public Comparer(IList<Point> points)
            {
                this.points = points;
            }

            public int Compare(int i0, int i1)
            {
                if (points[i0].X < points[i1].X)
                {
                    return -1;
                }

                if (points[i0].X == points[i1].X)
                {
                    if(points[i0].Y < points[i1].Y) {
                        return -1;
                    }

                    if(points[i0].Y == points[i1].Y) {
                        return 0;
                    }
                }

                return 1;
            }

            private IList<Point> points;
        }
        private class PointTester
        {
            public bool Test(Point P, Point P1, Point P2, Point P3)
            {
                double fabsy1y2 = Math.Abs(P1.Y - P2.Y);
                double fabsy2y3 = Math.Abs(P2.Y - P3.Y);
                double xc = 0;
                double yc = 0;

                if (fabsy1y2 == 0.0 && fabsy2y3 == 0.0)
                {
                    return false;   // test failed
                }

                if (fabsy1y2 == 0 && fabsy2y3 != 0)
                {
                    xc = (P2.X + P1.X) / 2.0;
                    yc = (-(P3.X - P2.X) / (P3.Y - P2.Y)) * (xc - ((P2.X + P3.X) / 2.0)) + ((P2.Y + P3.Y) / 2.0);
                }

                if (fabsy1y2 != 0 && fabsy2y3 == 0.0)
                {
                    xc = (P3.X + P2.X) / 2.0;
                    yc = (-(P2.X - P1.X) / (P2.Y - P1.Y)) * (xc - ((P1.X + P2.X) / 2.0)) + ((P1.Y + P2.Y) / 2.0);
                }

                if (fabsy1y2 != 0 && fabsy2y3 != 0.0)
                {
                    double m1 = -(P2.X - P1.X) / (P2.Y - P1.Y);
                    double m2 = -(P3.X - P2.X) / (P3.Y - P2.Y);
                    double mx1 = (P1.X + P2.X) / 2.0;
                    double mx2 = (P2.X + P3.X) / 2.0;
                    double my1 = (P1.Y + P2.Y) / 2.0;
                    double my2 = (P2.Y + P3.Y) / 2.0;

                    xc = (m1 * mx1 - m2 * mx2 + my2 - my1) / (m1 - m2);
                    yc = fabsy1y2 > fabsy2y3 ? m1 * (xc - mx1) + my1 : m2 * (xc - mx2) + my2;
                }

                double dx = P2.X - xc;
                double dy = P2.Y - yc;
                double rsqr = dx * dx + dy * dy;

                dx = P.X - xc;
                dy = P.Y - yc;
                double drsqr = dx * dx + dy * dy;

                Inside = drsqr <= rsqr;
                Complete = xc < P.X && ((P.X - xc) * (P.X - xc)) > rsqr;

                return true;
            }

            public bool Complete { get; private set; }
            public bool Inside { get; set; }
        }

        private Point s0;
        private Point s1;
        private Point s2;
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