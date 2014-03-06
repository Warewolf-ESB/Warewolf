using System;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Shapes;
using Infragistics.Controls.Charts.Util;
using System.Collections.Generic;
using System.Windows.Controls.Primitives;
using System.Windows.Input;


namespace Infragistics.Controls.Charts
{
    /// <summary>
    /// Represents a XamDataChart scatter line series
    /// </summary>
    public sealed class ScatterLineSeries : ScatterBase
    {
        #region constructor and initialisation
        /// <summary>
        /// Initializes a new instance of the ScatterLineSeries class. 
        /// </summary>
        public ScatterLineSeries()
        {
            DefaultStyleKey = typeof(ScatterLineSeries);

            PreviousFrame = new ScatterFrame();
            TransitionFrame = new ScatterFrame();
            CurrentFrame = new ScatterFrame();

            Func<OwnedPoint, ScatterFrame, ScatterFrame, OwnedPoint> getMinValue = (maxPoint, minFrame, maxFrame) =>
            {
                int index = FastItemsSource[maxPoint.OwnerItem];
                if (index == -1 || index == 0)
                {
                    return maxPoint;
                }
                object prev = FastItemsSource[index - 1];
                OwnedPoint prevPoint;
                if (!minFrame.CachedPoints.TryGetValue(prev, out prevPoint))
                {
                    return maxPoint;
                }
                return prevPoint;
            };

            PreviousFrame.GetNewMinValue = getMinValue;
            TransitionFrame.GetNewMinValue = getMinValue;
            CurrentFrame.GetNewMinValue = getMinValue;
        }


        /// <summary>
        /// When overridden in a derived class, is invoked whenever application code or internal processes
        /// call ApplyTemplate.
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
         
            // RenderSeries(false);
        }

        #endregion

        /// <summary>
        /// Called to create the view for the series.
        /// </summary>
        /// <returns>The created view.</returns>
        protected override SeriesView CreateView()
        {
            return new ScatterLineSeriesView(this);
        }

        internal override void PrepareFrame(ScatterFrame frame, ScatterBaseView view)
        {
            base.PrepareFrame(frame, view);

            frame.Points.Clear();
            frame.LinePoints.Clear();

            Rect windowRect = view.WindowRect; 
            Rect viewportRect = view.Viewport;

            double measure = Resolution * Resolution;
            int count = 0;
            if (XColumn != null)
            {
                count = XColumn.Count;
            }
            if (YColumn != null)
            {
                count = Math.Min(YColumn.Count, count);
            }
            //ColumnUtil.GetSafeCount(XColumn, YColumn);

            ScalerParams px = new ScalerParams(windowRect, viewportRect, AxisInfoCache.XAxisIsInverted);

            ScalerParams py = new ScalerParams(windowRect, viewportRect, AxisInfoCache.YAxisIsInverted);

            Func<int, double> X = (i) => 
                AxisInfoCache.XAxis.GetScaledValue(
                XColumn[i],
                px);
            Func<int, double> Y = (i) => 
                AxisInfoCache.YAxis.GetScaledValue(
                YColumn[i],
                py);

            double top = viewportRect.Top - 10;
            double bottom = viewportRect.Bottom + 10;
            double left = viewportRect.Left - 10;
            double right = viewportRect.Right + 10;

            Clipper clipper = new Clipper(left, bottom, right, top, false) { Target = frame.Points };

            for (int i = 0; i < count; )
            {
                int j = i;
                ++i;

                if (count > MaximumMarkers)
                {
                    while (i < count && Measure(X, Y, j, i) < measure)
                    {
                        ++i;
                    }
                    clipper.Add(Centroid(X, Y, j, i - 1));
                }
                else
                {
                    OwnedPoint newPoint = new OwnedPoint();
                    newPoint.Point = new Point(X(j), Y(j));
                    newPoint.OwnerItem = FastItemsSource[j];
                    if (!frame.LinePoints.ContainsKey(newPoint.OwnerItem))
                    {
                        frame.LinePoints.Add(newPoint.OwnerItem, newPoint);
                    }
                }
            }

            if (count > MaximumMarkers)
            {
                clipper.Target = null;
            }
        }
        private Point Centroid(Func<int, double> X, Func<int, double> Y, int a, int b)
        {
            if (a == b)
            {
                return new Point(X(a), Y(a));
            }

            double cx = 0.0;
            double cy = 0.0;
            double weight = (double)(b - a + 1);

            for (int i = a; i <= b; ++i)
            {
                cx += X(i);
                cy += Y(i);
            }

            return new Point(cx / weight, cy / weight);
        }
        private double Measure(Func<int, double> X, Func<int, double> Y, int a, int b)
        {
            double x = X(b) - X(a);
            double y = Y(b) - Y(a);

            return x * x + y * y;
        }

        internal override void RenderFrame(ScatterFrame frame, ScatterBaseView view)
        {
            view.ClearRendering(false);
            
            base.RenderFrame(frame, view);

            Rect clipRect = new Rect(
                view.Viewport.Left,
                view.Viewport.Top,
                view.Viewport.Width,
                view.Viewport.Height);

            clipRect.Inflate(this.Thickness, this.Thickness);

            PrepLinePoints(frame, new Clipper(clipRect, false));

            #region render the polyline

            ScatterLineSeriesView scatterLineView = view as ScatterLineSeriesView;

            this.PolylineSegments(scatterLineView.Polyline, frame.Points.Count,
                (j) => frame.Points[j].X, (j) => frame.Points[j].Y, 
                this.UnknownValuePlotting, this.Resolution);

            #endregion
        }
        private void PolylineSegments(Path polylines, int count, Func<int, double> x0, Func<int, double> y0, UnknownValuePlotting unknownValuePlotting, double resolution)
        {
            PathGeometry polylineData = new PathGeometry();

            polylines.Data = polylineData;

            polylineData.Figures = new PathFigureCollection();

            List<PolyLineSegment> polylineSegments = new List<PolyLineSegment>();


            int currentLineStartIndex = 0;
            for (int i = 0; i < count; i++)
            {
                if (double.IsNaN(x0(i)) || double.IsNaN(y0(i))) // point i is nanners.
                {
                    int pointsInCurrentLine = i - currentLineStartIndex;
                    if (pointsInCurrentLine > 0) // [DN May 16 2012 : 101933] reducing condition from 1 to 0
                    {
                        if (unknownValuePlotting == UnknownValuePlotting.DontPlot || polylineSegments.Count == 0)
                        {
                            // start a new segment if this is the first segment, or if we're in DontPlot mode
                            PolyLineSegment currentPolylineSegment = new PolyLineSegment();

                            polylineSegments.Add(currentPolylineSegment);

                        }
                        this.PolylineSegments(polylineSegments[polylineSegments.Count - 1].Points, currentLineStartIndex, i - 1, x0, y0, resolution);
                    }
                    currentLineStartIndex = i + 1;
                }
            }
            if (unknownValuePlotting == UnknownValuePlotting.DontPlot || polylineSegments.Count == 0)
            {
                // start a new segment if this is the first segment, or if we're in DontPlot mode
                PolyLineSegment lastPolylineSegment = new PolyLineSegment();

                polylineSegments.Add(lastPolylineSegment);
            }
            this.PolylineSegments(polylineSegments[polylineSegments.Count - 1].Points, currentLineStartIndex, count - 1, x0, y0, resolution);

            for (int current = 0; current < polylineSegments.Count; current++)
            {
                PolyLineSegment polylineSegment = polylineSegments[current];

                if (polylineSegment.Points.Count > 0)
                {
                    PathFigure polylineFigure = new PathFigure() { StartPoint = polylineSegment.Points[0] };
                    polylineFigure.Segments.Add(polylineSegment);
                    polylineData.Figures.Add(polylineFigure);
                }
            }

        }

        private const int FLATTENER_CHUNKING = 512;

        private IList<int> FlattenHelper(IList<int> result, Func<int, double> X, Func<int, double> Y, int b, int e, double E)
        {
            List<int> indices = new List<int>();
            
            int start = b;
            int end = e;
            int toFlatten = end - start + 1;

            while (toFlatten > 0)
            {
                if (toFlatten <= FLATTENER_CHUNKING)
                {
                    Flattener.Flatten(indices, X, Y, start, end, E);
                    start = end + 1;
                }
                else
                {
                    int currentEnd = start + FLATTENER_CHUNKING - 1;
                    Flattener.Flatten(indices, X, Y, start, currentEnd, E);
                    start = currentEnd + 1;

                }
                toFlatten = end - start + 1;
            }

            return indices;
        }

        private void PolylineSegments(PointCollection polylinePoints, int startIndex, int endIndex, Func<int, double> x0, Func<int, double> y0, double resolution)
        {
            if (endIndex > -1)
            {



                double res = ReadLocalValue(ResolutionProperty) == DependencyProperty.UnsetValue ? 1 : resolution;

                IList<int> indices = FlattenHelper(new List<int>(), x0, y0, startIndex, endIndex, res);
                int index;
                for (int i = 0; i < indices.Count; i++)
                {
                    index = indices[i];
                    polylinePoints.Add(new Point(x0(index), y0(index)));
                }
            }
        }

        private const string UnknownValuePlottingPropertyName = "UnknownValuePlotting";
        /// <summary>
        /// Identifies the UnknownValuePlotting dependency property.
        /// </summary>
        public static readonly DependencyProperty UnknownValuePlottingProperty = DependencyProperty.Register(UnknownValuePlottingPropertyName, typeof(UnknownValuePlotting), typeof(ScatterLineSeries), new PropertyMetadata(UnknownValuePlotting.DontPlot, (sender, e) =>
        {
            (sender as ScatterLineSeries).RaisePropertyChanged(UnknownValuePlottingPropertyName, e.OldValue, e.NewValue);
        }));
        /// <summary>
        /// Determines how unknown values will be plotted on the chart.
        /// </summary>
        /// <remarks>
        /// Null and Double.NaN are two examples of unknown values.
        /// </remarks>
        public UnknownValuePlotting UnknownValuePlotting
        {
            get
            {
                return (UnknownValuePlotting)this.GetValue(UnknownValuePlottingProperty);
            }
            set
            {
                this.SetValue(UnknownValuePlottingProperty, value);
            }
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
                case UnknownValuePlottingPropertyName:
                    this.RenderSeries(false);
                    this.NotifyThumbnailAppearanceChanged();
                    break;
            }
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