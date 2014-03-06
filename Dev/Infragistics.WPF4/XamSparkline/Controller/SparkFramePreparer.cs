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
using System.Collections.Generic;
using System.Linq;
using Infragistics.Controls.Charts.Util;
namespace Infragistics.Controls.Charts
{
    internal class SparkFramePreparer
    {
        internal SparklineController Controller { get; set; }
        internal double Offset { get; set; }
        internal double Crossing { get; set; }
        internal double Resolution { get { return 1; } }

        internal void PrepareFrame(SparkFrame frame)
        {
            frame.Buckets.Clear();
            frame.TrendPoints.Clear();
            frame.Markers.Clear();
            frame.NegativeMarkers.Clear();
            frame.LowPoints.Clear();
            frame.HighPoints.Clear();

            IFastItemColumn<double> valueColumn = Controller.ValueColumn;

            int itemCount = valueColumn.Count;
            double width = Controller.ViewportWidth;
            double height = Controller.ViewportHeight;
            double rowsPerBucket = Math.Floor((itemCount + 1.0) * Resolution / width);
            int bucketSize = (int)Math.Max(1.0, rowsPerBucket);
            int firstBucket = 0;
            int lastBucket = (int)Math.Ceiling(1.0 * itemCount / bucketSize);
            double zero = 0.0;
            List<double> trendValues = new List<double>();
            TrendLineType trendLineType = Controller.Model.TrendLineType;
            SparklineDisplayType displayType = Controller.Model.DisplayType;
            int trendLinePeriod = Controller.Model.TrendLinePeriod;
            bool showTrendline = trendLineType != TrendLineType.None && trendLinePeriod >= 1;
            IEnumerable<double> average = null;
            double[] trendCoefficients = null;

            if (displayType == SparklineDisplayType.Area || displayType == SparklineDisplayType.Line)
            {
                Offset = 0;
            }
            else
            {
                if (lastBucket == 0)
                {
                    // [DN January 26 2012 : 100138] this is just so Offset doesn't get set to Infinity.
                    Offset = 0;
                }
                else
                {
                    Offset = Controller.ViewportWidth / lastBucket / 2;
                }
            }

            if (showTrendline)
            {
                Func<int, double> GetUnscaledX = (i) => i + 1;
                Func<int, double> GetUnscaledY = (i) => valueColumn[i];
                Func<double, double> GetScaledXValue = (x) => Controller.GetScaledXValue(x);
                Func<double, double> GetScaledYValue = (y) => Controller.GetScaledYValue(y);
                
                switch (Controller.Model.TrendLineType)
                {
                    case TrendLineType.SimpleAverage: average = TrendCalculators.SMA(valueColumn, trendLinePeriod); break;
                    case TrendLineType.ExponentialAverage: average = TrendCalculators.EMA(valueColumn, trendLinePeriod); break;
                    case TrendLineType.ModifiedAverage: average = TrendCalculators.MMA(valueColumn, trendLinePeriod); break;
                    case TrendLineType.CumulativeAverage: average = TrendCalculators.CMA(valueColumn); break;
                    case TrendLineType.WeightedAverage: average = TrendCalculators.WMA(valueColumn, trendLinePeriod); break;

                    case TrendLineType.LinearFit: trendCoefficients = LeastSquaresFit.LinearFit(itemCount, GetUnscaledX, GetUnscaledY); break;
                    case TrendLineType.QuadraticFit: trendCoefficients = LeastSquaresFit.QuadraticFit(itemCount, GetUnscaledX, GetUnscaledY); break;
                    case TrendLineType.CubicFit: trendCoefficients = LeastSquaresFit.CubicFit(itemCount, GetUnscaledX, GetUnscaledY); break;
                    case TrendLineType.QuarticFit: trendCoefficients = LeastSquaresFit.QuarticFit(itemCount, GetUnscaledX, GetUnscaledY); break;
                    case TrendLineType.QuinticFit: trendCoefficients = LeastSquaresFit.QuinticFit(itemCount, GetUnscaledX, GetUnscaledY); break;
                    case TrendLineType.ExponentialFit: trendCoefficients = LeastSquaresFit.ExponentialFit(itemCount, GetUnscaledX, GetUnscaledY); break;
                    case TrendLineType.LogarithmicFit: trendCoefficients = LeastSquaresFit.LogarithmicFit(itemCount, GetUnscaledX, GetUnscaledY); break;
                    case TrendLineType.PowerLawFit: trendCoefficients = LeastSquaresFit.PowerLawFit(itemCount, GetUnscaledX, GetUnscaledY); break;
                }

                if (average != null)
                {
                    foreach (var value in average)
                    {
                        trendValues.Add(value);
                    }
                }

                if (trendCoefficients != null)
                {
                    double xmin = firstBucket * bucketSize;
                    double xmax = lastBucket * bucketSize;

                    double xStart = 0.0 + this.Offset;
                    double xEnd = width - this.Offset;
                    for (double i = xStart; i <= xEnd; i += 2.0)
                    {
                        double p = i / (width - 1);
                        double xi = xmin + p * (xmax - xmin);
                        double yi = double.NaN;
                        
                        switch (trendLineType)
                        {
                            case TrendLineType.LinearFit: yi = LeastSquaresFit.LinearEvaluate(trendCoefficients, xi); break;
                            case TrendLineType.QuadraticFit: yi = LeastSquaresFit.QuadraticEvaluate(trendCoefficients, xi); break;
                            case TrendLineType.CubicFit: yi = LeastSquaresFit.CubicEvaluate(trendCoefficients, xi); break;
                            case TrendLineType.QuarticFit: yi = LeastSquaresFit.QuarticEvaluate(trendCoefficients, xi); break;
                            case TrendLineType.QuinticFit: yi = LeastSquaresFit.QuinticEvaluate(trendCoefficients, xi); break;
                            case TrendLineType.ExponentialFit: yi = LeastSquaresFit.ExponentialEvaluate(trendCoefficients, xi); break;
                            case TrendLineType.LogarithmicFit: yi = LeastSquaresFit.LogarithmicEvaluate(trendCoefficients, xi); break;
                            case TrendLineType.PowerLawFit: yi = LeastSquaresFit.PowerLawEvaluate(trendCoefficients, xi); break;
                        }

                        // convert xi and yi to pels
                        xi = GetScaledXValue(xi);
                        yi = GetScaledYValue(yi);

                        if (!double.IsNaN(yi) && !double.IsInfinity(yi))
                        {
                            frame.TrendPoints.Add(new Point(xi, yi));
                        }
                    }
                }
            }

            double minValue = Controller.ValueColumn.Minimum;
            double maxValue = Controller.ValueColumn.Maximum;

            double axisMinimum = Controller.Model.ActualMinimum;
            double axisMaximum = Controller.Model.ActualMaximum;

            Crossing = Controller.ViewportHeight - (zero - axisMinimum) / (axisMaximum - axisMinimum) * Controller.ViewportHeight;

            if (Crossing < 0) Crossing = 0;
            else if (Crossing > Controller.ViewportHeight) Crossing = Controller.ViewportHeight;

            for (int i = firstBucket; i < lastBucket; i++)
            {
                double[] bucket = GetBucket(i, bucketSize);
                ScaleBucket(bucket);
                
                frame.Buckets.Add(bucket);
                if (!double.IsNaN(bucket[0]) && !double.IsNaN(bucket[1]))
                {
                    // [DN January 24 2012 : 100116] don't bother adding a marker with no visible location
                    frame.Markers.Add(new Point(bucket[0], bucket[1]));
                }
                if (showTrendline && average != null)
                {
                    int index = i * bucketSize;
                    double scaledTrendValue = height - (((trendValues[index] - minValue) / (maxValue - minValue)) * height);
                    frame.TrendPoints.Add(new Point(bucket[0] + this.Offset, scaledTrendValue));
                }

                double value = Controller.ValueColumn[i];
                
                if (value < zero)
                {
                    frame.NegativeMarkers.Add(new Point(bucket[0], bucket[1]));
                }

                if (value == minValue)
                {
                    frame.LowPoints.Add(new Point(bucket[0], bucket[1]));
                }

                if (value == maxValue)
                {
                    frame.HighPoints.Add(new Point(bucket[0], bucket[1]));
                }
            }

            if (frame.Markers.Count == 0)
            {
                frame.FirstPoint = frame.LastPoint = new Point(double.NaN, double.NaN);
            }
            else
            {
                frame.FirstPoint = frame.Markers[0];
                frame.LastPoint = frame.Markers[frame.Markers.Count - 1];
            }
        }
  
        private double[] GetBucket(int bucket, int bucketSize)
        {
            int itemCount = Controller.ValueColumn.Count;

            int i0 = Math.Min(bucket * bucketSize, itemCount - 1);
            int i1 = Math.Min(i0 + bucketSize - 1, itemCount - 1);

            bool first = true;
            double min = 0;
            double max = 0;

            for (int i = i0; i <= i1; ++i)
            {
                double y = Controller.ValueColumn[i];

                if (first)
                {
                    first = false;
                    min = y;
                    max = y;
                }
                else
                {
                    min = Math.Min(min, y);
                    max = Math.Max(max, y);
                }
            }

            double[] b = new[] { 0.0, 0.0, 0.0 };
            b[0] = (0.5 * (i0 + i1));
            b[1] = min;
            b[2] = max;

            return b;
        }

        private void ScaleBucket(double[] bucket)
        {
            int itemCount = Controller.ValueColumn.Count;
            double height = Controller.ViewportHeight;
            double width = Controller.ViewportWidth;
            double minValue = Controller.Model.ActualMinimum;
            double maxValue = Controller.Model.ActualMaximum;

            if (Controller.Model.DisplayType == SparklineDisplayType.Area 
                || Controller.Model.DisplayType == SparklineDisplayType.Line)
            {
                itemCount--;
            }

            if (itemCount < 0)
                itemCount = 0;

            double scaledValue =
                itemCount > 0 ? bucket[0] / itemCount
                : itemCount == 0 ? 0.5f
                : double.NaN;

            bucket[0] = scaledValue * width;

            if (Controller.Model.DisplayType == SparklineDisplayType.WinLoss)
            {
                if (bucket[1] > 0)
                {
                    bucket[1] = 0;
                    bucket[2] = 0;
                }
                else if (bucket[1] < 0)
                {
                    bucket[1] = height;
                    bucket[2] = height;
                }
                else
                {
                    bucket[1] = height - (((bucket[1] - minValue) / (maxValue - minValue)) * height);
                    bucket[2] = height - (((bucket[2] - minValue) / (maxValue - minValue)) * height);
                }
            }
            else
            {
                bucket[1] = height - (((bucket[1] - minValue) / (maxValue - minValue)) * height);
                bucket[2] = height - (((bucket[2] - minValue) / (maxValue - minValue)) * height);
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