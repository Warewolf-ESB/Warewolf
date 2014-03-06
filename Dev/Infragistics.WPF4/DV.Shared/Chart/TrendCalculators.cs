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

namespace Infragistics.Controls.Charts
{
    /// <summary>
    /// Utility class for calculating trendline values.
    /// </summary>
    public static class TrendCalculators
    {
        /// <summary>
        /// Calculates the weighted moving average.
        /// </summary>
        /// <remarks>
        /// The first period values are calculated by accumulation and may be considered invalid.
        /// </remarks>
        /// <param name="sequence">Sequence to average.</param>
        /// <param name="period">Average period.</param>
        /// <returns>The weighted moving average.</returns>
        public static IEnumerable<double> WMA(IEnumerable<double> sequence, int period)
        {
            double[] buffer = new double[period]; // buffer of values within period
            double total = double.NaN; // sum of values within period
            double numerator = double.NaN; // numerator of wma expression
            double weightsum = double.NaN; // sum of weights
            double wma = double.NaN;

            int i = 0;
            foreach (double value in sequence)
            {
                if (!double.IsNaN(value))
                {
                    int cursor = i % period;

                    if (i == 0)
                    {
                        weightsum = 1.0;
                        wma = numerator = total = value;
                    }
                    else if (i < period)
                    {
                        weightsum += (i + 1);
                        total += value;
                        numerator += (i + 1) * value;
                        wma = numerator / weightsum;
                    }
                    else // if (i >= period)
                    {
                        numerator = numerator + (period * value) - total;
                        wma = numerator / weightsum;
                        total = total + value - buffer[cursor];
                    }

                    buffer[cursor] = value;
                    ++i;
                }

                yield return wma;
            }
        }

        /// <summary>
        /// Calculates the exponential moving average.
        /// </summary>
        /// <remarks>
        /// The first period values are calculated by accumulation and may be considered invalid.
        /// </remarks>
        /// <param name="sequence">Sequence to average.</param>
        /// <param name="period">Average period.</param>
        /// <returns>The exponential moving average.</returns>
        public static IEnumerable<double> EMA(IEnumerable<double> sequence, int period)
        {
            int i = 0;
            double ema = double.NaN;
            double alpha = 2.0 / (1.0 + period);

            foreach (double value in sequence)
            {
                if (!double.IsNaN(value))
                {
                    if (i < period)
                    {
                        ema = double.IsNaN(ema) ? value : (ema * i + value) / (i + 1);
                    }
                    else
                    {
                        ema = (value - ema) * alpha + ema;
                    }

                    ++i;
                }

                yield return ema;
            }
        }

        /// <summary>
        /// Calculates the modified moving average.
        /// </summary>
        /// <remarks>
        /// The first period values are calculated by accumulation and may be considered invalid.
        /// </remarks>
        /// <param name="sequence">Sequence to average.</param>
        /// <param name="period">Average period.</param>
        /// <returns>The modified moving average.</returns>
        public static IEnumerable<double> MMA(IEnumerable<double> sequence, int period)
        {
            int i = 0;
            double mma = double.NaN;
            double alpha = 1.0 / period;

            foreach (double value in sequence)
            {
                if (!double.IsNaN(value))
                {
                    if (i < period)
                    {
                        mma = double.IsNaN(mma) ? value : (mma * i + value) / (i + 1);
                    }
                    else
                    {
                        mma = (value - mma) * alpha + mma;
                    }

                    ++i;
                }

                yield return mma;
            }
        }

        /// <summary>
        /// Calculates the cumulative moving average.
        /// </summary>
        /// <param name="sequence">Sequence to average.</param>
        /// <returns>The cumulative moving average.</returns>
        public static IEnumerable<double> CMA(IEnumerable<double> sequence)
        {
            double cma = double.NaN;
            int i = 0;

            foreach (double value in sequence)
            {
                if (!double.IsNaN(value))
                {
                    cma = double.IsNaN(cma) ? value : (cma * i + value) / (i + 1);
                    ++i;
                }

                yield return cma;
            }
        }

        /// <summary>
        /// Calculates the simple moving average.
        /// </summary>
        /// <remarks>
        /// The first period values are calculated by accumulation and may be considered invalid.
        /// </remarks>
        /// <param name="sequence">Sequence to average.</param>
        /// <param name="period">Average period.</param>
        /// <returns>The simple moving average.</returns>
        public static IEnumerable<double> SMA(IEnumerable<double> sequence, int period)
        {
            double[] buffer = new double[period];
            int i = 0;

            double sma = double.NaN;

            foreach (double value in sequence)
            {
                if (!double.IsNaN(value))
                {
                    double next = value / period;
                    int cursor = i % period;

                    if (i < period)
                    {
                        sma = double.IsNaN(sma) ? value : (sma * i + value) / (i + 1.0);
                    }
                    else
                    {
                        sma = sma + next - buffer[cursor];
                    }

                    buffer[cursor] = next;
                    ++i;
                }

                yield return sma;
            }
        }

        /// <summary>
        /// Calculates a moving sum over a sequence with a given period.
        /// </summary>
        /// <param name="sequence">The sequence for which to calculate the moving sum.</param>
        /// <param name="period">The period to use for the calculation.</param>
        /// <returns>The moving sum values.</returns>
        public static IEnumerable<double> MovingSum(IEnumerable<double> sequence, int period)
        {
            double[] buffer = new double[period];
            int i = 0;

            double ms = double.NaN;

            foreach (double value in sequence)
            {
                if (!double.IsNaN(value))
                {
                    double next = value;
                    int cursor = i % period;

                    if (i < period)
                    {
                        ms = double.IsNaN(ms) ? next : ms + next;
                    }
                    else
                    {
                        ms = ms + next - buffer[cursor];
                    }

                    buffer[cursor] = next;
                    ++i;
                }

                yield return ms;
            }
        }

        /// <summary>
        /// Calculates the standard deviation of a sequence with a given period.
        /// </summary>
        /// <param name="sequence">The sequence for which to calculate the standard deviation values.</param>
        /// <param name="period">The period to use for the calculation.</param>
        /// <returns>The sequence of calculated standard deviaton values.</returns>
        public static IEnumerable<double> STDEV(IEnumerable<double> sequence, int period)
        {
            IEnumerator<double> sma = SMA(sequence, period).GetEnumerator();
            IEnumerator<double> price = sequence.GetEnumerator();

            double[] buffer = new double[period];
            int i = 0;

            while (price.MoveNext() && sma.MoveNext())
            {
                buffer[(i++) % period] = (double)price.Current;

                double S = 0.0;

                if (i < period)
                {
                    int effectivePeriod = 0;
                    for (int j = 0; j < i; j++)
                    {
                        double t = (sma.Current - buffer[j]);

                        S += t * t;
                        effectivePeriod++;
                    }

                    yield return Math.Sqrt(S / effectivePeriod);
                }
                else
                {
                    for (int j = 0; j < period; ++j)
                    {
                        double t = (sma.Current - buffer[j]);

                        S += t * t;
                    }

                    yield return Math.Sqrt(S / period);
                }
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