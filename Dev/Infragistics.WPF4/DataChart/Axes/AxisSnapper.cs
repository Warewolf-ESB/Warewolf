using System;

namespace Infragistics.Controls.Charts
{
    // grid line calculation for decimal numeric axes

    internal class Snapper
    {
        protected const double Resolution = 7.0;

        protected static double expt(double a, int n)
        {
            double x = 1.0;

            if (n > 0)
            {
                for (; n > 0; --n)
                {
                    x *= a;
                }
            }
            else
            {
                for (; n < 0; ++n)
                {
                    x /= a;
                }
            }

            return x;
        }
        protected static double nicenum(double x, bool round)
        {
            int exp = (int)Math.Floor(Math.Log10(x));
            double f = x / Math.Pow(10.0, exp);

            if (round)
            {
                double nf = f < 1.5 ? 1.0 : f < 3.0 ? 2.0 : f < 7.0 ? 5.0 : 10.0;

                return nf * Math.Pow(10.0, exp);
            }
            else
            {
                double nf = f <= 1.0 ? 1.0 : f <= 2.0 ? 2.0 : f <= 5.0 ? 5.0 : 10.0;

                return nf * Math.Pow(10.0, exp);
            }
        }

        protected static TimeSpan nicenum(TimeSpan span, bool round)
        {
            TimeSpan niceSpan = TimeSpan.Zero;

            if (span.TotalDays > 1)
            {
                niceSpan = TimeSpan.FromDays(Math.Ceiling(span.TotalDays));
            }
            else if (span.TotalHours > 1)
            {
                niceSpan = TimeSpan.FromHours(Math.Ceiling(span.TotalHours));
            }
            else if (span.TotalMinutes > 1)
            {
                niceSpan = TimeSpan.FromMinutes(Math.Ceiling(span.TotalMinutes));
            }
            else if (span.TotalSeconds > 1)
            {
                niceSpan = TimeSpan.FromSeconds(Math.Ceiling(span.TotalSeconds));
            }
            else if (span.TotalMilliseconds > 1)
            {
                niceSpan = TimeSpan.FromMilliseconds(Math.Ceiling(span.TotalMilliseconds));
            }

            return niceSpan;
        }
    }
 
    class LinearNumericSnapper : Snapper
    {
        public LinearNumericSnapper(double visibleMinimum, double visibleMaximum, double pixels)
        {
            Initialize(visibleMinimum, visibleMaximum, pixels, 10);
        }

        public LinearNumericSnapper(double visibleMinimum, double visibleMaximum, double pixels, int minTicks)
        {
            Initialize(visibleMinimum, visibleMaximum, pixels, minTicks);
        }

        private void Initialize(double visibleMinimum, double visibleMaximum, double pixels, int minTicks)
        {
            Interval = double.NaN;
            Precision = 0;
            MinorCount = 0;

            int ticks = Math.Min(minTicks, (int)(pixels / Resolution));

            if (ticks > 0)
            {
                double range = nicenum(visibleMaximum - visibleMinimum, false);

                Interval = nicenum(range / (ticks - 1), true);

                double graphmin = Math.Floor(visibleMinimum / Interval) * Interval;
                double graphmax = Math.Ceiling(visibleMaximum / Interval) * Interval;

                ticks = (int)Math.Round((graphmax - graphmin) / Interval); // recalc ticks to find out how many we really got

                // minor count

                if (pixels / ticks > Resolution * 10.0)
                {
                    MinorCount = 10;
                }
                else
                {
                    if (pixels / ticks > Resolution * 5.0)
                    {
                        MinorCount = 5;
                    }
                    else
                    {
                        if (pixels / ticks > Resolution * 2.0)
                        {
                            MinorCount = 2;
                        }
                    }
                }

                Precision = Math.Max(-(int)Math.Floor(Math.Log10(Interval)), 0); // number of decimal places for major grid lines
            }
        }

        /// <summary>
        /// Gets the major line interval.
        /// </summary>
        public double Interval { get; private set; }

        /// <summary>
        /// Gets the number of decimal places required to label major lines.
        /// </summary>
        public int Precision { get; private set; }

        /// <summary>
        /// Gets the number of minor lines per major line.
        /// </summary>
        public int MinorCount { get; private set; }
    }

    // grid line calculation for logarithmic numeric axes

    internal class LogarithmicNumericSnapper : Snapper
    {
        public LogarithmicNumericSnapper(double visibleMinimum, double visibleMaximum, int logarithmBase, double pixels)
        {
            Interval = 1;
            MinorCount = logarithmBase;
        }

        public double Interval { get; private set; }
        public double MinorCount { get; private set; }
    }

    class LinearCategorySnapper : Snapper
    {

        public LinearCategorySnapper(double visibleMinimum, double visibleMaximum, double pixels) : this(visibleMinimum, visibleMaximum, pixels, double.NaN, CategoryMode.Mode0)
        {
        
        }
        public LinearCategorySnapper(double visibleMinimum, double visibleMaximum, double pixels, double interval, CategoryMode categoryMode)
        {
            Interval = interval;
            MinorCount = 0;

            int ticks = Math.Min(10, (int)(pixels / Resolution));

            if (ticks > 0)
            {
                double range = nicenum(visibleMaximum - visibleMinimum, false);

                if (double.IsNaN(Interval))
                {
                    Interval = nicenum(range / (ticks - 1), true);  // major tick mark spacing
                }

                //interval should be at least 1, as the smallest interval on a category axis is one category.
                //this avoid meaningless major gridlines between data points.
                if (Interval < 1)
                {
                    Interval = 1.0;
                }

                double graphmin = Math.Floor(visibleMinimum / Interval) * Interval;
                double graphmax = Math.Ceiling(visibleMaximum / Interval) * Interval;

                ticks = (int)Math.Round((graphmax - graphmin) / Interval); // recalc ticks to find out how many we really got

                // minor count for continuous category axis (Mode0)

                if (pixels / ticks > Resolution * 10.0)
                {
                    MinorCount = 10;
                }
                else
                {
                    if (pixels / ticks > Resolution * 5.0)
                    {
                        MinorCount = 5;
                    }
                    else
                    {
                        if (pixels / ticks > Resolution * 2.0)
                        {
                            MinorCount = 2;
                        }
                    }
                }
            }
        }

        public double Interval { get; private set; }
        public double MinorCount { get; private set; }
    }

    
#region Infragistics Source Cleanup (Region)













































#endregion // Infragistics Source Cleanup (Region)

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