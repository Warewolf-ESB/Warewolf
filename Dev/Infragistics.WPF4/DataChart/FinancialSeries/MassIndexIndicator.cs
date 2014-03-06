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
    /// Represents a XamDataChart Mass Index indicator series.
    /// </summary>
    /// <remarks>
    /// Default required members: High, Low
    /// </remarks>
    public class MassIndexIndicator : StrategyBasedIndicator
    {
        /// <summary>
        /// Returns the calculation strategy to use for this indicator.
        /// </summary>
        protected override IndicatorCalculationStrategy CalculationStrategy
        {
            get
            {
                return new CalculationStrategies.MassIndexIndicatorStrategy();
            }
        }

        /// <summary>
        /// Returns the default style key that should be used for this indicator.
        /// </summary>
        protected override Type StyleKeyType
        {
            get
            {
                return typeof(MassIndexIndicator);
            }
        }
    }
}

namespace Infragistics.Controls.Charts.CalculationStrategies
{
    /// <summary>
    /// Represents the strategy for calculating a Mass Index indicator series.
    /// </summary>
    /// <remarks>
    /// For definition of indicator see: <see cref="MassIndexIndicator"/>
    /// </remarks>
    public class MassIndexIndicatorStrategy : IndicatorCalculationStrategy
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
        /// Returns a list of range values between the high values and the low values.
        /// </summary>
        /// <param name="highColumn">The column from which to take the high values.</param>
        /// <param name="lowColumn">The column from which to take the low values.</param>
        /// <returns>The list of range values.</returns>
        protected IEnumerable<double> HighLowRange(IList<double> highColumn, IList<double> lowColumn)
        {
            for (int i = 0; i < (Math.Min(highColumn.Count, lowColumn.Count)); i++)
            {
                yield return highColumn[i] - lowColumn[i];
            }
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
            //descriptions of this indicator seem to lock the period at 9
            
            int period = 9;

            IList<double> highColumn = dataSource.HighColumn;
            IList<double> lowColumn = dataSource.LowColumn;
            IList<double> indicatorColumn = dataSource.IndicatorColumn;

            IEnumerator<double> ema9 =
                supportingCalculations.EMA.Strategy(HighLowRange(highColumn, lowColumn), period)
                .GetEnumerator();
            IEnumerator<double> ema9ema9 =
                supportingCalculations.EMA.Strategy(
                    supportingCalculations.EMA.Strategy(HighLowRange(highColumn, lowColumn), period), period)
                    .GetEnumerator();

            double[] buffer = new double[period];

            double mass = 0.0;

            for (int i = 0; i < indicatorColumn.Count; i++)
            {
                int cursor = i % period;
                mass -= buffer[cursor];

                ema9.MoveNext();
                ema9ema9.MoveNext();
                double newMassValue = supportingCalculations.MakeSafe(
                    ema9.Current / ema9ema9.Current);
                mass += newMassValue;
                indicatorColumn[i] = mass;
                buffer[cursor] = newMassValue;
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