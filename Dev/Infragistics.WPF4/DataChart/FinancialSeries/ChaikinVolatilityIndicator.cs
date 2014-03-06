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
    /// Represents a XamDataChart Chaikin Volatility indicator series.
    /// </summary>
    /// <remarks>
    /// The ChaikinVolatility indicator attempts to show volatility by displaying the spread between
    /// the high and low values.
    /// <para>
    /// Default required members: High, Low
    /// </para>
    /// </remarks>
    public class ChaikinVolatilityIndicator : StrategyBasedIndicator
    {
        /// <summary>
        /// Returns the calculation strategy to use for this indicator.
        /// </summary>
        protected override IndicatorCalculationStrategy CalculationStrategy
        {
            get
            {
                return new CalculationStrategies.ChaikinVolatilityIndicatorStrategy();
            }
        }

        /// <summary>
        /// Returns the default style key that should be used for this indicator.
        /// </summary>
        protected override Type StyleKeyType
        {
            get
            {
                return typeof(ChaikinVolatilityIndicator);
            }
        }

        #region Period Dependency Property
        /// <summary>
        /// Gets or sets the moving average period for the current ChaikinVolatilityIndicator object.
        /// <para>
        /// This is a dependency property.
        /// </para>
        /// <remarks>
        /// The typical, and initial, value for ChaikinVolatilityIndicator periods is 10.
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
            CreatePeriodProperty(10, typeof(ChaikinVolatilityIndicator));

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
    /// Represents the strategy for calculating a Chaikin Volatility indicator series.
    /// </summary>
    /// <remarks>
    /// For definition of indicator see: <see cref="ChaikinVolatilityIndicator"/>
    /// </remarks>
    public class ChaikinVolatilityIndicatorStrategy : IndicatorCalculationStrategy
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
            list.Add(FinancialSeries.HighColumnPropertyName);
            list.Add(FinancialSeries.LowColumnPropertyName);
            list.AddRange(supportingCalculations.EMA.BasedOn);

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
            IEnumerable<double> highMinusLow =
                supportingCalculations.ToEnumerable(
                (index) => dataSource.HighColumn[index] - dataSource.LowColumn[index],
                dataSource.Count);

            IEnumerator<double> emaHighLow =
                supportingCalculations.EMA.Strategy(highMinusLow, dataSource.Period)
                .GetEnumerator();

            IList<double> indicatorColumn = dataSource.IndicatorColumn;

            double[] buffer = new double[dataSource.Period];

            int i = 0;

            while (emaHighLow.MoveNext())
            {
                int cursor = i % dataSource.Period;

                double chaikinVolatility =
                    supportingCalculations.MakeSafe(
                    (emaHighLow.Current - buffer[cursor]) / (buffer[cursor] * 100.00));

                if (i < dataSource.Period)
                {
                    indicatorColumn[i] = 0.0;
                }
                else
                {
                    indicatorColumn[i] = chaikinVolatility;
                }

                buffer[cursor] = emaHighLow.Current;
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