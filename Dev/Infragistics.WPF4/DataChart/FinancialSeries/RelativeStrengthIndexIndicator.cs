using System.Windows;
using System;
using System.Collections.Generic;

namespace Infragistics.Controls.Charts
{
    /// <summary>
    /// Represents a XamDataChart Relative Strength Index indicator series.
    /// </summary>
    /// <remarks>
    /// Default required members: Close
    /// </remarks>
    public class RelativeStrengthIndexIndicator : StrategyBasedIndicator
    {
        /// <summary>
        /// Returns the calculation strategy to use for this indicator.
        /// </summary>
        protected override IndicatorCalculationStrategy CalculationStrategy
        {
            get
            {
                return new CalculationStrategies.RelativeStrengthIndexIndicatorStrategy();
            }
        }

        /// <summary>
        /// Returns the default style key that should be used for this indicator.
        /// </summary>
        protected override Type StyleKeyType
        {
            get
            {
                return typeof(RelativeStrengthIndexIndicator);
            }
        }

        #region Period Dependency Property
        /// <summary>
        /// Gets or sets the moving average period for the current RelativeStrengthIndexIndicator object.
        /// <para>
        /// This is a dependency property.
        /// </para>
        /// <remarks>
        /// The typical, and initial, value for RSI periods is 14.
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
            CreatePeriodProperty(14, typeof(RelativeStrengthIndexIndicator));

        /// <summary>
        /// Specifies the period to use for the calculation.
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
    /// Represents the strategy for calculating a Relative String Index indicator series.
    /// </summary>
    /// <remarks>
    /// For definition of indicator see: <see cref="RelativeStrengthIndexIndicator"/>
    /// </remarks>
    public class RelativeStrengthIndexIndicatorStrategy
        : StreamingIndicatorCalculationStrategy
    {
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
            list.Add(FinancialSeries.CloseColumnPropertyName);

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
            int i = 0;
            foreach (double value in ProvideStream(dataSource, supportingCalculations))
            {
                dataSource.IndicatorColumn[i] = value;
                i++;
            }

            return true;
        }

        
        /// <summary>
        /// Provides a stream of calculated indicator values and does not fill
        /// the IndicatorColumn in the datasource.
        /// </summary>
        /// <param name="dataSource">The data required by the contract between these 
        /// strategies and the financial series.</param>
        /// <param name="supportingCalculations">The calculation strategies required by the 
        /// contract between these strategies and the finiancial series.</param>
        /// <returns>True if the calculation could be completed.</returns>
        public override IEnumerable<double> ProvideStream(FinancialCalculationDataSource dataSource,
            FinancialCalculationSupportingCalculations supportingCalculations)
        {
            double period = (double)dataSource.Period;
            double alpha = 2.0 / (period + 1.0);
            double Uema = 0.0;
            double Dema = 0.0;
            IList<double> indicatorColumn = dataSource.IndicatorColumn;
            IList<double> closeColumn = dataSource.CloseColumn;


            if (indicatorColumn.Count > 0)
            {
                yield return 0.0; // period 1, neither ema is defined
            }

            for (int i = 1; i < Math.Min(dataSource.Period, indicatorColumn.Count); ++i)
            {
                double C = closeColumn[i] - closeColumn[i - 1];
                double U = C > 0 ? C : 0;
                double D = C > 0 ? 0 : -C;

                Uema += U / (period - 1.0);
                Dema += D / (period - 1.0);

                yield return 0.0;
            }

            for (int i = dataSource.Period; i < indicatorColumn.Count; ++i)
            {
                double C = closeColumn[i] - closeColumn[i - 1];
                double U = C > 0 ? C : 0;
                double D = C > 0 ? 0 : -C;

                Uema = (Uema * (period - 1.0) + U) / period;
                Dema = (Dema * (period - 1.0) + D) / period;

                yield return supportingCalculations.MakeSafe(
                    Uema != 0.0 ? 100.0 * Uema / (Uema + Dema) : 0.0);
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