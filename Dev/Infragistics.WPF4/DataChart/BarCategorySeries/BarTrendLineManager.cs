using System;
using System.Collections.Generic;
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

namespace Infragistics.Controls.Charts
{
    internal class BarTrendFitCalculator
    {
        public static double[] CalculateFit(List<Point> trend, TrendLineType trendLineType, TrendResolutionParams trendResolutionParams,
            double[] trendCoefficients, int count,
            Func<int, double> GetUnscaledX, Func<int, double> GetUnscaledY,
            Func<double, double> GetScaledXValue, Func<double, double> GetScaledYValue,
            double ymin, double ymax)
        {
            if (trendCoefficients == null)
            {
                //(i) => i + 1;
                switch (trendLineType)
                {
                    case TrendLineType.LinearFit: trendCoefficients = LeastSquaresFit.LinearFit(count, GetUnscaledY, GetUnscaledX); break;
                    case TrendLineType.QuadraticFit: trendCoefficients = LeastSquaresFit.QuadraticFit(count, GetUnscaledY, GetUnscaledX); break;
                    case TrendLineType.CubicFit: trendCoefficients = LeastSquaresFit.CubicFit(count, GetUnscaledY, GetUnscaledX); break;
                    case TrendLineType.QuarticFit: trendCoefficients = LeastSquaresFit.QuarticFit(count, GetUnscaledY, GetUnscaledX); break;
                    case TrendLineType.QuinticFit: trendCoefficients = LeastSquaresFit.QuinticFit(count, GetUnscaledY, GetUnscaledX); break;
                    case TrendLineType.ExponentialFit: trendCoefficients = LeastSquaresFit.ExponentialFit(count, GetUnscaledY, GetUnscaledX); break;
                    case TrendLineType.LogarithmicFit: trendCoefficients = LeastSquaresFit.LogarithmicFit(count, GetUnscaledY, GetUnscaledX); break;
                    case TrendLineType.PowerLawFit: trendCoefficients = LeastSquaresFit.PowerLawFit(count, GetUnscaledY, GetUnscaledX); break;
                    default:
                        throw new NotImplementedException();
                }
            }

            if (trendCoefficients == null)
            {
                return null;
            }

            for (int i = 0; i < trendResolutionParams.Viewport.Height; i += 2)
            {
                double p = i / (trendResolutionParams.Viewport.Height - 1);
                double yi = ymin + p * (ymax - ymin);
                double xi = double.NaN;

                switch (trendLineType)
                {
                    case TrendLineType.LinearFit: xi = LeastSquaresFit.LinearEvaluate(trendCoefficients, yi); break;
                    case TrendLineType.QuadraticFit: xi = LeastSquaresFit.QuadraticEvaluate(trendCoefficients, yi); break;
                    case TrendLineType.CubicFit: xi = LeastSquaresFit.CubicEvaluate(trendCoefficients, yi); break;
                    case TrendLineType.QuarticFit: xi = LeastSquaresFit.QuarticEvaluate(trendCoefficients, yi); break;
                    case TrendLineType.QuinticFit: xi = LeastSquaresFit.QuinticEvaluate(trendCoefficients, yi); break;
                    case TrendLineType.ExponentialFit: xi = LeastSquaresFit.ExponentialEvaluate(trendCoefficients, yi); break;
                    case TrendLineType.LogarithmicFit: xi = LeastSquaresFit.LogarithmicEvaluate(trendCoefficients, yi); break;
                    case TrendLineType.PowerLawFit: xi = LeastSquaresFit.PowerLawEvaluate(trendCoefficients, yi); break;
                    default:
                        throw new NotImplementedException();
                }

                // convert xi and yi to pels

                xi = GetScaledXValue(xi);
                yi = GetScaledYValue(yi);

                if (!double.IsNaN(xi) && !double.IsInfinity(xi))
                {
                    trend.Add(new Point(xi, yi + trendResolutionParams.Offset));
                }
            }

            return trendCoefficients;
        }
    }

    internal class BarTrendLineManager:CategoryTrendLineManager
    {
        public override void PrepareLine(List<Point> flattenedPoints, 
            TrendLineType trendLineType, 
            IList<double> valueColumn, 
            int period, 
            Func<double, double> GetScaledXValue, 
            Func<double, double> GetScaledYValue, 
            TrendResolutionParams trendResolutionParams)
        {
            double ymin = trendResolutionParams.FirstBucket * trendResolutionParams.BucketSize;
            double ymax = trendResolutionParams.LastBucket * trendResolutionParams.BucketSize;
            List<Point> trend = new List<Point>();

            if (trendLineType == TrendLineType.None)
            {
                TrendCoefficients = null;
                TrendColumn.Clear();
                return;
            }

            if (IsFit(trendLineType))
            {
                TrendColumn.Clear();

                TrendCoefficients =
                    BarTrendFitCalculator.CalculateFit(trend, trendLineType, trendResolutionParams,
                    TrendCoefficients, valueColumn.Count, (i) => valueColumn[i], (i) => i + 1, 
                    GetScaledXValue, GetScaledYValue, ymin, ymax);
            }

            if (IsAverage(trendLineType))
            {
                TrendCoefficients = null;

                TrendAverageCalculator.CalculateSingleValueAverage(trendLineType, TrendColumn, valueColumn, period);

                for (int i = trendResolutionParams.FirstBucket; i <= trendResolutionParams.LastBucket; i += 1)
                {
                    int itemIndex = i * trendResolutionParams.BucketSize;

                    if (itemIndex >= 0 && itemIndex < TrendColumn.Count)
                    {
                        double xi = GetScaledXValue(TrendColumn[itemIndex]);
                        double yi = GetScaledYValue(itemIndex);

                        trend.Add(new Point(xi, yi + trendResolutionParams.Offset));
                    }
                }
            }

            FlattenTrendLine(trend, trendResolutionParams, flattenedPoints);
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