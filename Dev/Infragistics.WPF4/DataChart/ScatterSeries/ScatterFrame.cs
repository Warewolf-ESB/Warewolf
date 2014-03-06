using System.ComponentModel;
using System.Collections.Generic;
using System.Windows;
using System;



using System.Linq;


namespace Infragistics.Controls.Charts
{
    /// <summary>
    /// Represents the base class for all XamDataChart scatter series
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal class ScatterFrameBase<T> : Frame
        where T : ScatterFrameBase<T>
    {
        public ScatterFrameBase()
        {
            Points = new List<Point>();
            CachedPoints = new Dictionary<object, OwnedPoint>();
            Markers = new Dictionary<object, OwnedPoint>();
            TrendLine = new List<Point>();
            HorizontalErrorBars = new Dictionary<object, OwnedPoint>();
            VerticalErrorBars = new Dictionary<object, OwnedPoint>();
            HorizontalErrorBarWidths = new Dictionary<object, double>();
            VerticalErrorBarHeights = new Dictionary<object, double>();

            GetNewMinValue = (maxPoint, minFrame, maxFrame) => maxPoint;

            OwnedPointDictInterpolator =
                new DictInterpolator<object, OwnedPoint, T>(
                    this.InterpolatePoint,
                    (p) => p.OwnerItem,
                    (p) => !double.IsNaN(p.Point.X) && !double.IsNaN(p.Point.Y),
                    () => new OwnedPoint());
        }

        internal DictInterpolator<object, OwnedPoint, T> OwnedPointDictInterpolator { get; set; }
        
        #region IFrame Members
        public override void Interpolate(float p, Frame minFrame, Frame maxFrame)
        {
            var min = (T)minFrame;
            var max = (T)maxFrame;

            if (min == null ||
                max == null)
            {
                return;
            }

            OwnedPointDictInterpolator.Interpolate(
                CachedPoints, p,
                min.CachedPoints, max.CachedPoints,
                min, max);
            Interpolate(Points, p, min.Points, max.Points);
           

            OwnedPointDictInterpolator.Interpolate(
                Markers, p,
                min.Markers, max.Markers,
                min, max);

            OwnedPointDictInterpolator.Interpolate(
                HorizontalErrorBars, p,
                min.HorizontalErrorBars, max.HorizontalErrorBars,
                min, max);

            OwnedPointDictInterpolator.Interpolate(
                VerticalErrorBars, p,
                min.VerticalErrorBars, max.VerticalErrorBars,
                min, max);

            AddPointsThatSweepThroughTheView(Markers, p, min, max);

            Interpolate(TrendLine, p, min.TrendLine, max.TrendLine);

            //Interpolate(HorizontalErrorBarWidths, p, min.HorizontalErrorBarWidths, max.HorizontalErrorBarWidths);
            //Interpolate(VerticalErrorBarHeights, p, min.VerticalErrorBarHeights, max.VerticalErrorBarHeights);
            HorizontalErrorBarWidths = max.HorizontalErrorBarWidths;
            VerticalErrorBarHeights = max.VerticalErrorBarHeights;

            InterpolateOverride(p, min, max);
        }

        protected virtual void InterpolateOverride(float p, Frame minFrame, Frame maxFrame)
        {
        }

        #endregion

        private void AddPointsThatSweepThroughTheView(Dictionary<object, OwnedPoint> markers,
            float p,
            T minFrame, T maxFrame)
        {
            foreach (var changedPoint in
                minFrame.CachedPoints.Values.Where(
                changedPoint => !markers.ContainsKey(changedPoint.OwnerItem)))
            {
                OwnedPoint maxPoint;
                if (!maxFrame.CachedPoints.TryGetValue(changedPoint.OwnerItem, out maxPoint) ||
                    (maxPoint.ColumnValues.X == changedPoint.ColumnValues.X &&
                    maxPoint.ColumnValues.Y == changedPoint.ColumnValues.Y))
                {
                    continue;
                }
                var newPoint = new OwnedPoint();

                InterpolatePoint(newPoint, p, changedPoint, maxPoint, minFrame, maxFrame);
                if (double.IsNaN(newPoint.Point.X) ||
                    double.IsNaN(newPoint.Point.Y))
                {
                    continue;
                }
                markers.Add(newPoint.OwnerItem, newPoint);
            }
        }

        public Func<OwnedPoint, T, T, OwnedPoint> GetNewMinValue { get; set; }

        protected virtual void InterpolateColumnValues(OwnedPoint point, float p, OwnedPoint minPoint, OwnedPoint maxPoint)
        {
            if (minPoint != null)
            {
                point.ColumnValues = new Point(minPoint.ColumnValues.X, minPoint.ColumnValues.Y);
            }
            else if (maxPoint != null)
            {
                point.ColumnValues = new Point(maxPoint.ColumnValues.X, maxPoint.ColumnValues.Y);
            }
        }

        private void InterpolatePoint(OwnedPoint point, float p, OwnedPoint minPoint, OwnedPoint maxPoint,
            T minFrame,
            T maxFrame)
        {
            OwnedPoint min;
            OwnedPoint max;

            if (minPoint == null)
            {
                if (maxPoint != null)
                {
                    OwnedPoint minValue;

                    if (minFrame.CachedPoints.TryGetValue(
                        maxPoint.OwnerItem, out minValue))
                    {
                        min = minValue;
                    }
                    else
                    {
                        min = GetNewMinValue(maxPoint, minFrame, maxFrame);
                    }
                }
                else
                {
                    point.Point = new Point(double.NaN, double.NaN);
                    return;
                }
            }
            else
            {
                min = minPoint;
                if (point.OwnerItem == null)
                {
                    point.OwnerItem = minPoint.OwnerItem;
                }
            }

            if (maxPoint == null)
            {
                if (minPoint != null)
                {
                    OwnedPoint maxValue;

                    if (maxFrame.CachedPoints.TryGetValue(
                        minPoint.OwnerItem, out maxValue))
                    {
                        max = maxValue;
                    }
                    else
                    {
                        //max = minPoint.Point;
                        //point is missing from target just remove.
                        point.Point = new Point(double.NaN, double.NaN);
                        return;
                    }
                }
                else
                {
                    point.Point = new Point(double.NaN, double.NaN);
                    return;
                }
            }
            else
            {
                max = maxPoint;
                if (point.OwnerItem == null)
                {
                    point.OwnerItem = maxPoint.OwnerItem;
                }
            }

            InterpolateColumnValues(point, p, min, max);
            if (double.IsNaN(min.Point.X) || double.IsNaN(min.Point.Y))
            {
                min = max;
            }
            InterpolatePointOverride(point, p, min, max);
        }

        protected virtual void InterpolatePointOverride(OwnedPoint point, double p, OwnedPoint min, OwnedPoint max)
        {
            double q = 1.0 - p;
            point.Point = new Point(min.Point.X * q + max.Point.X * p, min.Point.Y * q + max.Point.Y * p);
        }

        public Dictionary<object, OwnedPoint> Markers { get; private set; }
        public Dictionary<object, OwnedPoint> CachedPoints { get; internal set; }
        public List<Point> Points { get; private set; }
        public List<Point> TrendLine { get; private set; }
        public Dictionary<object, OwnedPoint> HorizontalErrorBars { get; private set; }
        public Dictionary<object, OwnedPoint> VerticalErrorBars { get; private set; }
        public Dictionary<object, double> HorizontalErrorBarWidths { get; private set; }
        public Dictionary<object, double> VerticalErrorBarHeights { get; private set; }
    }

    internal class DictInterpolator<TKey, TValue, TFrame> where TValue : new()
    {
        public DictInterpolator(



        Action<TValue, float, TValue, TValue, TFrame, TFrame> interpolatePointStrat,

        Func<TValue, TKey> getKeyStrat,
        Func<TValue, bool> validPointStrat,
        Func<TValue> createValueStrat
        )
        {
            InterpolatePointStrat = interpolatePointStrat;
            GetKeyStrat = getKeyStrat;
            ValidPointStrat = validPointStrat;
            CreateValueStrat = createValueStrat;
        }




        private Action<TValue, float, TValue, TValue, TFrame, TFrame> InterpolatePointStrat { get; set; }

        private Func<TValue, TKey> GetKeyStrat { get; set; }
        private Func<TValue, bool> ValidPointStrat { get; set; }
        private Func<TValue> CreateValueStrat { get; set; }

        public void Interpolate(
            IDictionary<TKey, TValue> target,
            float p,
            IDictionary<TKey, TValue> min,
            IDictionary<TKey, TValue> max,
            TFrame minFrame,
            TFrame maxFrame
            )
        {
            List<TKey> removeFromTarget = new List<TKey>();
            foreach (var key in target.Keys)
            {
                TValue minValue;
                TValue maxValue;
                TValue targetValue = target[key];
                bool minContains = min.TryGetValue(key, out minValue);
                bool maxContains = max.TryGetValue(key, out maxValue);

                if (!minContains &&
                    !maxContains)
                {
                    removeFromTarget.Add(key);
                }
                else
                {
                    InterpolatePointStrat(targetValue, p, minValue, maxValue, minFrame, maxFrame);
                    if (!ValidPointStrat(targetValue))
                    {
                        removeFromTarget.Add(key);
                    }
                }
            }
            foreach (var key in removeFromTarget)
            {
                target.Remove(key);
            }

            foreach (var key in min.Keys)
            {
                TValue minValue = min[key];
                TValue maxValue;
                TValue targetValue;
                max.TryGetValue(key, out maxValue);
                bool targetContains = target.TryGetValue(key, out targetValue);

                if (!targetContains)
                {
                    targetValue = CreateValueStrat();
                    InterpolatePointStrat(targetValue, p, minValue, maxValue, minFrame, maxFrame);
                    if (!ValidPointStrat(targetValue))
                    {
                        continue;
                    }
                    target.Add(GetKeyStrat(targetValue), targetValue);
                }
            }

            foreach (var key in max.Keys)
            {
                TValue maxValue = max[key];
                TValue minValue;
                TValue targetValue;
                bool minContains = min.TryGetValue(key, out minValue);
                bool targetContains = target.TryGetValue(key, out targetValue);

                if (!targetContains && !minContains)
                {
                    targetValue = CreateValueStrat();
                    InterpolatePointStrat(targetValue, p, minValue, maxValue, minFrame, maxFrame);
                    if (!ValidPointStrat(targetValue))
                    {
                        continue;
                    }
                    target.Add(GetKeyStrat(targetValue), targetValue);
                }
            }
        }
    }
    /// <summary>
    /// Class representing a Point and its owning item and data values.
    /// </summary>
    [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
    public class OwnedPoint
    {
        public OwnedPoint()
        {
            ColumnValues = new Point(0, 0);
        }

        /// <summary>
        /// The Point.
        /// </summary>
        public Point Point { get; set; }
        /// <summary>
        /// The owning data item.
        /// </summary>
        public object OwnerItem { get; set; }
        /// <summary>
        /// The data values, expressed as a point.
        /// </summary>
        public Point ColumnValues { get; set; }
    }

    internal class ScatterFrame
        : ScatterFrameBase<ScatterFrame>
    {
        public ScatterFrame()
        {
            LinePoints = new Dictionary<object, OwnedPoint>();
            
        }

        public Dictionary<object, OwnedPoint> LinePoints { get; internal set; }

        protected override void InterpolateOverride(float p, Frame minFrame, Frame maxFrame)
        {
            base.InterpolateOverride(p, minFrame, maxFrame);

            ScatterFrame min = minFrame as ScatterFrame;
            ScatterFrame max = maxFrame as ScatterFrame;
            if (min == null || max == null)
            {
                return;
            }

            OwnedPointDictInterpolator.Interpolate(
               LinePoints, p,
               min.LinePoints, max.LinePoints,
               min, max);
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