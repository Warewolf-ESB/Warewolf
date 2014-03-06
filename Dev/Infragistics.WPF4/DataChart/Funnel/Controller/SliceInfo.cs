using System;
using System.Collections;
using System.ComponentModel;


using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;


namespace Infragistics.Controls.Charts
{
    /// <summary>
    /// Represents information about a funnel slice.
    /// </summary>
    internal class SliceInfo
    {
        public SliceInfo()
        {
            Slice = new SliceAppearance();
            OuterLabelPosition = new Point(0, 0);
        }

        private object _outerLabel;
        /// <summary>
        /// Sets or Gets the outer label of a funnel slice
        /// </summary>
        public object OuterLabel
        {
            get { return _outerLabel; }
            set { _outerLabel = value; }
        }

        private Point _outerLabelPosition;

        public Point OuterLabelPosition
        {
            get { return _outerLabelPosition; }
            set { _outerLabelPosition = value; }
        }

        private int _index;
        public int Index
        {
            get { return _index; }
            set { _index = value; }
        }

        private bool _hasSlice;
        public bool HasSlice
        {
            get { return _hasSlice; }
            set { _hasSlice = value; }
        }

        private SliceAppearance _slice;
        public SliceAppearance Slice
        {
            get { return _slice; }
            set { _slice = value; }
        }

        private bool _hasOuterLabel;
        public bool HasOuterLabel
        {
            get { return _hasOuterLabel; }
            set { _hasOuterLabel = value; }
        }

        internal static SliceInfo Interpolate(
            SliceInfo interpolated,
            SliceInfo min,
            SliceInfo max,
            double p,
            double q)
        {
            if (interpolated == null)
            {
                interpolated = new SliceInfo();
            }

            interpolated.HasOuterLabel = max.HasOuterLabel;
            interpolated.HasSlice = max.HasSlice;
            interpolated.Index = max.Index;
            interpolated.OuterLabel = max.OuterLabel;
            interpolated.OuterLabelPosition =
                new Point(
                    (min.OuterLabelPosition.X * q) + (max.OuterLabelPosition.X * p),
                    (min.OuterLabelPosition.Y * q) + (max.OuterLabelPosition.Y * p));
            interpolated.Slice = SliceAppearance.Interpolate(
                interpolated.Slice, min.Slice, max.Slice, p, q);

            return interpolated;
        }
    }

    /// <summary>
    /// Stores information about how a funnel slice should be rendered.
    /// </summary>
    public class SliceAppearance
    {
        private Point _offset;
        /// <summary>
        /// Gets or sets the offset to render the slice at.
        /// </summary>
        public Point Offset
        {
            get { return _offset; }
            set
            {
                _offset = value;
            }
        }

        private Point _upperLeft;
        /// <summary>
        /// Gets or sets the position of the upper left point.
        /// </summary>
        public Point UpperLeft
        {
            get { return _upperLeft; }
            set
            {
                _upperLeft = value;
                UpdatePoints();
            }
        }

        private PointList _bezierPoints;
        /// <summary>
        /// Gets or sets the left hand side rasterization of the bezier curve.
        /// </summary>
        public PointList BezierPoints
        {
            get { return _bezierPoints; }
            set
            {
                _bezierPoints = value;
                UpdatePoints();
            }
        }

        private PointList _rightBezierPoints;

        /// <summary>
        /// Gets or sets the right hand side rasterization of the bezier curve.
        /// </summary>
        public PointList RightBezierPoints
        {
            get { return _rightBezierPoints; }
            set
            {
                _rightBezierPoints = value;
                UpdatePoints();
            }
        }

        private PointCollection _points;
        /// <summary>
        /// Gets or sets the aggregated points of the slice.
        /// </summary>
        public PointCollection Points
        {
            get { return _points; }
            set
            {
                _points = value;
            }
        }

        private void UpdatePoints()
        {
            PointCollection points = new PointCollection();
            if (RightBezierPoints != null)
            {
                foreach (Point point in RightBezierPoints)
                {
                    points.Add(point);
                }
            }
            else
            {
                points.Add(UpperLeft);
                points.Add(UpperRight);
            }
            
            if (BezierPoints != null)
            {
                foreach (Point point in BezierPoints)
                {
                    points.Add(point);
                }
                
                if (RightBezierPoints != null && RightBezierPoints.Count > 0)
                {
                    points.Add(RightBezierPoints[0]);
                }
            }
            else
            {
                points.Add(LowerRight);
                points.Add(LowerLeft);
                points.Add(UpperLeft);
            }

            Points = points;
        }

        private Point _upperRight;
        /// <summary>
        /// Gets or sets the upper right point of the slice.
        /// </summary>
        public Point UpperRight
        {
            get { return _upperRight; }
            set
            {
                _upperRight = value;
                UpdatePoints();
            }
        }

        private Point _lowerRight;
        /// <summary>
        /// Gets or sets the lower right point of the slice.
        /// </summary>
        public Point LowerRight
        {
            get { return _lowerRight; }
            set
            {
                _lowerRight = value;
                UpdatePoints();
            }
        }

        private Point _lowerLeft;
        /// <summary>
        /// Gets or sets the lower left point of the slice.
        /// </summary>
        public Point LowerLeft
        {
            get { return _lowerLeft; }
            set
            {
                _lowerLeft = value;
                UpdatePoints();
            }
        }

        private Style _style;
        /// <summary>
        /// Gets or sets the style of the slice.
        /// </summary>
        public Style Style
        {
            get { return _style; }
            set
            {
                _style = value;
            }
        }

        private Brush _fill;
        /// <summary>
        /// Gets or sets the fill of the slice.
        /// </summary>
        public Brush Fill
        {
            get { return _fill; }
            set
            {
                _fill = value;
            }
        }

        private Brush _outline;
        /// <summary>
        /// Gets or sets the outline of the slice.
        /// </summary>
        public Brush Outline
        {
            get { return _outline; }
            set
            {
                _outline = value;
            }
        }

        private object _innerLabel;
        /// <summary>
        /// Gets or sets the inner label of the slice.
        /// </summary>
        public object InnerLabel
        {
            get { return _innerLabel; }
            set
            {
                _innerLabel = value;
            }
        }

        private Point _innerLabelPosition;
        /// <summary>
        /// Gets or sets the position of the inner label of the slice.
        /// </summary>
        public Point InnerLabelPosition
        {
            get { return _innerLabelPosition; }
            set
            {
                _innerLabelPosition = value;
            }
        }

        private bool _hasInnerLabel;
        /// <summary>
        /// Gets or sets whether the slice has an inner label.
        /// </summary>
        public bool HasInnerLabel
        {
            get { return _hasInnerLabel; }
            set
            {
                _hasInnerLabel = value;
            }
        }

        private DataTemplate _innerLabelTemplate;
        /// <summary>
        /// Gets or sets the template to use for the inner label.
        /// </summary>
        public DataTemplate InnerLabelTemplate
        {
            get { return _innerLabelTemplate; }
            set { _innerLabelTemplate = value; }
        }

        private object _item;
        /// <summary>
        /// Gets or sets the item that is associated with the slice.
        /// </summary>
        public object Item
        {
            get { return _item; }
            set { _item = value; }
        }

        private int _index;
        /// <summary>
        /// Gets or sets the index that is associated with the slice.
        /// </summary>
        public int Index
        {
            get { return _index; }
            set { _index = value; }
        }

        internal static SliceAppearance Interpolate(
            SliceAppearance interpolated,
            SliceAppearance min,
            SliceAppearance max,
            double p, double q)
        {
            if (interpolated == null)
            {
                interpolated = new SliceAppearance();
            }

            interpolated.Fill = max.Fill;
            interpolated.HasInnerLabel = max.HasInnerLabel;
            interpolated.InnerLabel = max.InnerLabel;
            interpolated.InnerLabelPosition = new Point(
                (min.InnerLabelPosition.X * q) + (max.InnerLabelPosition.X * p),
                (min.InnerLabelPosition.Y * q) + (max.InnerLabelPosition.Y * p));
            interpolated.InnerLabelTemplate = max.InnerLabelTemplate;
            interpolated.LowerLeft = new Point(
                (min.LowerLeft.X * q) + (max.LowerLeft.X * p),
                (min.LowerLeft.Y * q) + (max.LowerLeft.Y * p));
            interpolated.LowerRight = new Point(
                (min.LowerRight.X * q) + (max.LowerRight.X * p),
                (min.LowerRight.Y * q) + (max.LowerRight.Y * p));
            interpolated.UpperLeft = new Point(
                (min.UpperLeft.X * q) + (max.UpperLeft.X * p),
                (min.UpperLeft.Y * q) + (max.UpperLeft.Y * p));
            interpolated.UpperRight = new Point(
                (min.UpperRight.X * q) + (max.UpperRight.X * p),
                (min.UpperRight.Y * q) + (max.UpperRight.Y * p));
            interpolated.Offset = new Point(
                (min.Offset.X * q) + (max.Offset.X * p),
                (min.Offset.Y * q) + (max.Offset.Y * p));
            if (max.BezierPoints != null)
            {
                interpolated.BezierPoints =
                    InterpolatePoints(
                    interpolated.BezierPoints,
                    min.BezierPoints,
                    max.BezierPoints,
                    p, q);
            }
            else
            {
                interpolated.BezierPoints = null;
            }
            if (max.RightBezierPoints != null)
            {
                interpolated.RightBezierPoints =
                    InterpolatePoints(
                    interpolated.RightBezierPoints,
                    min.RightBezierPoints,
                    max.RightBezierPoints,
                    p, q);
            }
            else
            {
                interpolated.RightBezierPoints = null;
            }
            interpolated.Outline = max.Outline;
            interpolated.Style = max.Style;
            interpolated.Item = max.Item;
            interpolated.Index = max.Index;

            return interpolated;
        }


        private static PointList InterpolatePoints(
            PointList interpolatedPoints, 
            PointList minPoints, 
            PointList maxPoints, 
            double p, 
            double q)
        {
            if (interpolatedPoints == null)
            {
                interpolatedPoints = new PointList();
            }
            if (minPoints == null)
            {
                minPoints = new PointList();
            }

            int minCount = minPoints.Count;
            int maxCount = maxPoints.Count;
            int count = Math.Max(minCount, maxCount);

            if (interpolatedPoints.Count < count)
            {
                interpolatedPoints.InsertRange(interpolatedPoints.Count, new Point[count - interpolatedPoints.Count]);
            }

            if (interpolatedPoints.Count > count)
            {
                interpolatedPoints.RemoveRange(count, interpolatedPoints.Count - count);
            }

            for (int i = 0; i < Math.Min(minCount, maxCount); ++i)
            {
                interpolatedPoints[i] =
                    new Point(
                        (minPoints[i].X * q) + (maxPoints[i].X * p),
                        (minPoints[i].Y * q) + (maxPoints[i].Y * p)
                        );
            }

            if (minCount < maxCount)
            {
                Point mn = minCount > 0 ? minPoints[minCount - 1] : new Point(0.0,0.0);

                for (int i = minCount; i < maxCount; ++i)
                {
                    interpolatedPoints[i] =
                    new Point(
                        (mn.X * q) + (maxPoints[i].X * p),
                        (mn.Y * q) + (maxPoints[i].Y * p));
                }
            }

            if (minCount > maxCount)
            {
                Point mx = maxCount > 0 ? maxPoints[maxCount - 1] : new Point(0.0,0.0);

                for (int i = maxCount; i < minCount; ++i)
                {
                    interpolatedPoints[i] =
                     interpolatedPoints[i] =
                    new Point(
                        (minPoints[i].X * q) + (mx.X * p),
                        (minPoints[i].Y * q) + (mx.Y * p)
                        );
                }
            }

            return interpolatedPoints;
        }
    }

    internal class SliceInfoList

        : List<SliceInfo>

    {


#region Infragistics Source Cleanup (Region)









































































































#endregion // Infragistics Source Cleanup (Region)


        public SliceInfoList()
        {

        }

        public void IndexSort()
        {
            Sort(delegate(SliceInfo s1, SliceInfo s2)
                           {
                               if (s1.Index < s2.Index)
                               {
                                   return -1;
                               }
                               if (s1.Index > s2.Index)
                               {
                                   return 1;
                               }
                               return 0;
                           });
        }

        public void SortByY()
        {

            Sort(delegate(SliceInfo s1, SliceInfo s2)
                           {
                               if (s1.Slice.Offset.Y < s2.Slice.Offset.Y)
                               {
                                   return -1;
                               }
                               if (s1.Slice.Offset.Y > s2.Slice.Offset.Y)
                               {
                                   return 1;
                               }
                               return 0;
                           });
        }

        public int GetByYValue(double yValue)
        {
            SortByY();
            int index = BinarySearch(delegate(SliceInfo si)
            {
                if (yValue < (si.Slice.UpperLeft.Y + si.Slice.Offset.Y))
                {
                    return -1;
                }
                if (yValue > (si.Slice.LowerLeft.Y + si.Slice.Offset.Y))
                {
                    return 1;
                }
                else
                {
                    return 0;
                }
            });

            if (index >= 0)
            {
                index = (this[index]).Index;
            }
            else
            {
                index = -1;
            }

            IndexSort();

            if (index >= 0)
            {
                return index;
            }

            return -1;
        }

        private int BinarySearch(SliceInfoUnaryComparison comparisonFunction)
        {
            int currMin = 0;
            int currMax = Count - 1;

            while (currMin <= currMax)
            {
                int currMid = (currMin + ((currMax - currMin) >> 1));
                int compResult = comparisonFunction(this[currMid]);
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

    internal delegate int SliceInfoComparison(SliceInfo s1, SliceInfo s2);

    internal delegate int SliceInfoUnaryComparison(SliceInfo s1);
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