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
    /// Represents a XamDataChart Ultimate Oscillator indicator series.
    /// </summary>
    /// <remarks>
    /// Default required members: High, Low, Close
    /// </remarks>
    public class UltimateOscillatorIndicator : StrategyBasedIndicator
    {
        /// <summary>
        /// Returns the calculation strategy to use for this indicator.
        /// </summary>
        protected override IndicatorCalculationStrategy CalculationStrategy
        {
            get
            {
                return new CalculationStrategies.UltimateOscillatorIndicatorCalculationStrategy();
            }
        }

        /// <summary>
        /// Returns the default style key that should be used for this indicator.
        /// </summary>
        /// <summary>
        /// Returns the default style key that should be used for this indicator.
        /// </summary>
        protected override Type StyleKeyType
        {
            get
            {
                return typeof(UltimateOscillatorIndicator);
            }
        }
    }
}

namespace Infragistics.Controls.Charts.CalculationStrategies
{
    /// <summary>
    /// Represents the strategy for calculating a Ultimate Oscillator indicator series.
    /// </summary>
    /// <remarks>
    /// For definition of indicator see: <see cref="UltimateOscillatorIndicator"/>
    /// </remarks>
    public class UltimateOscillatorIndicatorCalculationStrategy : IndicatorCalculationStrategy
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
            list.AddRange(dataSource.TrueLow.BasedOn);
            list.Add(FinancialSeries.CloseColumnPropertyName);
            list.AddRange(dataSource.TrueRange.BasedOn);
            list.AddRange(supportingCalculations.MovingSum.BasedOn);

            return list;
        }
        
        /// <summary>
        /// Calculates a sequence representing the buying pressure of the provided data.
        /// </summary>
        /// <param name="dataSource">The data provided to perform the calculation.</param>
        /// <returns>A sequence representing each buying pressure value.</returns>
        protected IEnumerable<double> BuyingPressure(FinancialCalculationDataSource dataSource)
        {
            int i = 0;
            IEnumerator<double> tl = dataSource.TrueLow.GetEnumerator();
            while (tl.MoveNext())
            {
                yield return dataSource.CloseColumn[i] - tl.Current;
                i++;
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
            IEnumerable<double> buyingPressure = BuyingPressure(dataSource);
            IEnumerable<double> trueRange = dataSource.TrueRange;

            IEnumerator<double> bpShort =
                supportingCalculations.MovingSum.Strategy(buyingPressure, 7).GetEnumerator();
            IEnumerator<double> trShort =
                supportingCalculations.MovingSum.Strategy(trueRange, 7).GetEnumerator();

            IEnumerator<double> bpMedium =
                supportingCalculations.MovingSum.Strategy(buyingPressure, 14).GetEnumerator();
            IEnumerator<double> trMedium =
                supportingCalculations.MovingSum.Strategy(trueRange, 14).GetEnumerator();

            IEnumerator<double> bpLong =
                supportingCalculations.MovingSum.Strategy(buyingPressure, 28).GetEnumerator();
            IEnumerator<double> trLong =
                supportingCalculations.MovingSum.Strategy(trueRange, 28).GetEnumerator();

            int i = 0;

            while (bpShort.MoveNext() && trShort.MoveNext() &&
                bpMedium.MoveNext() && trMedium.MoveNext() &&
                bpLong.MoveNext() && trLong.MoveNext())
            {
                double rawValue = supportingCalculations.MakeSafe(
                    4 * (bpShort.Current / trShort.Current)
                    + 2 * (bpMedium.Current / trMedium.Current)
                    + (bpLong.Current / trLong.Current));

                double uo = (rawValue / (4.0 + 2.0 + 1.0)) * 100.0;

                dataSource.IndicatorColumn[i] = uo;
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