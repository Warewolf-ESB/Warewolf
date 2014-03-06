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
    /// Represents a XamDataChart StochRSI indicator series.
    /// </summary>
    /// <remarks>
    /// Default required members: Close
    /// </remarks>
    public class StochRSIIndicator : StrategyBasedIndicator
    {
        /// <summary>
        /// Returns the calculation strategy to use for this indicator.
        /// </summary>
        protected override IndicatorCalculationStrategy CalculationStrategy
        {
            get
            {
                return new CalculationStrategies.StochRSIIndicatorStrategy();
            }
        }

        /// <summary>
        /// Returns the default style key that should be used for this indicator.
        /// </summary>
        protected override Type StyleKeyType
        {
            get
            {
                return typeof(StochRSIIndicator);
            }
        }

        #region Period Dependency Property
        /// <summary>
        /// Gets or sets the moving average period for the current StochRSIIndicator object.
        /// <para>
        /// This is a dependency property.
        /// </para>
        /// <remarks>
        /// The typical, and initial, value for StochRSI periods is 14.
        /// </remarks>
        /// </summary>
        public int Period
        {
            get
            {
                return (int)GetValue(PeriodProperty);
            }
            set
            {
                SetValue(PeriodProperty, value);
            }
        }

        /// <summary>
        /// Identifies the Period dependency property.
        /// </summary>
        public static readonly DependencyProperty PeriodProperty =
            CreatePeriodProperty(14, typeof(StochRSIIndicator));

        /// <summary>
        /// Specifies the period value to be used for the calculation.
        /// </summary>
        /// <returns>The period to use.</returns>
        protected override int PeriodOverride()
        {
            return Period;
        }
        #endregion
    }
}

namespace Infragistics.Controls.Charts.CalculationStrategies
{
    /// <summary>
    /// Represents the strategy for calculating a StochRSI indicator series.
    /// </summary>
    /// <remarks>
    /// For definition of indicator see: <see cref="StochRSIIndicator"/>
    /// </remarks>
    public class StochRSIIndicatorStrategy : IndicatorCalculationStrategy
    {
        internal StreamingIndicatorCalculationStrategy RSIStrategy { get; set; }

        /// <summary>
        /// Constructs a StochRSI Indicator strategy.
        /// </summary>
        public StochRSIIndicatorStrategy()
        {
            RSIStrategy = new RelativeStrengthIndexIndicatorStrategy();
        }

        /// <summary>
        /// Exposes which columns this strategy uses in its calculation so that the
        /// consumers will know when they should ask the strategy to recalculate.
        /// </summary>
        /// <param name="dataSource">The data source to be used in the calculation</param>
        /// <param name="supportingCalculations">The other calculations that this indicator may depend on.</param>
        /// <returns>The list of column names that this strategy depends on.</returns>
        public override IList<string> BasedOn(FinancialCalculationDataSource dataSource,
            FinancialCalculationSupportingCalculations supportingCalculations)
        {
            List<string> list = new List<string>();
            list.AddRange(RSIStrategy.BasedOn(dataSource, supportingCalculations));

            return list;
        }

        /// <summary>
        /// Performs the calculation for the indicator.
        /// </summary>
        /// <param name="dataSource">The data provided to perform the calculation.</param>
        /// <param name="supportingCalculations">The supporting calculation strategies provided to perform the calculation.</param>
        /// <returns>True if the calculation could be completed.</returns>
        public override bool CalculateIndicator(FinancialCalculationDataSource dataSource,
            FinancialCalculationSupportingCalculations supportingCalculations)
        {
            IEnumerator<double> RSI =
                RSIStrategy.ProvideStream(dataSource, supportingCalculations).GetEnumerator();

            int period = dataSource.Period;
            int i = 0;
            double[] buffer = new double[period];

            while (RSI.MoveNext())
            {
                int ago = Math.Min(period, i);
                double highestHigh = double.MinValue;
                double lowestLow = double.MaxValue;
                int cursor = i % period;

                for (int j = 0; j < ago; j++)
                {
                    if (!double.IsNaN(buffer[ago - j - 1]))
                    {
                        highestHigh = Math.Max(highestHigh, buffer[ago - j - 1]);
                    }
                    if (!double.IsNaN(buffer[ago - j - 1]))
                    {
                        lowestLow = Math.Min(lowestLow, buffer[ago - j - 1]);
                    }
                }

                buffer[cursor] = RSI.Current;

                double stochRSI =
                    supportingCalculations.MakeSafe(
                    (RSI.Current - lowestLow)
                    / (highestHigh - lowestLow));

                dataSource.IndicatorColumn[i] = stochRSI;

                i++;
            }

            return true;
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