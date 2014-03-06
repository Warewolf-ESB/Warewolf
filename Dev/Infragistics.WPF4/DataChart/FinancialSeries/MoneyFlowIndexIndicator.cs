using System;
using System.Windows;
using System.Collections.Generic;

namespace Infragistics.Controls.Charts
{
    /// <summary>
    /// Represents a XamDataChart Money Flow Index indicator series.
    /// </summary>
    /// <remarks> 
    /// <para>
    /// Default required members: Close, Low, High, Volume
    /// </para>
    /// </remarks>
    public class MoneyFlowIndexIndicator : StrategyBasedIndicator
    {
        /// <summary>
        /// Returns the calculation strategy to use for this indicator.
        /// </summary>
        protected override IndicatorCalculationStrategy CalculationStrategy
        {
            get
            {
                return new CalculationStrategies.MoneyFlowIndexIndicatorStrategy();
            }
        }

        /// <summary>
        /// Returns the default style key that should be used for this indicator.
        /// </summary>
        protected override Type StyleKeyType
        {
            get
            {
                return typeof(MoneyFlowIndexIndicator);
            }
        }

        #region Period Dependency Property
        /// <summary>
        /// Gets or sets the moving average period for the current MoneyFlowIndexIndicator object.
        /// <para>
        /// This is a dependency property.
        /// </para>
        /// <remarks>
        /// The typical, and initial, value for MoneyFlowIndexIndicator periods is 14.
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
            CreatePeriodProperty(14, typeof(MoneyFlowIndexIndicator));

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
    /// Represents the strategy for calculating a Money Flow Index indicator series.
    /// </summary>
    /// <remarks>
    /// For definition of indicator see: <see cref="MoneyFlowIndexIndicator"/>
    /// </remarks>
    public class MoneyFlowIndexIndicatorStrategy : IndicatorCalculationStrategy
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
            list.AddRange(dataSource.TypicalColumn.BasedOn);
            list.Add(FinancialSeries.VolumeColumnPropertyName);

            return list;
        }
        
        /// <summary>
        /// Performs the calculation for the indicator.
        /// </summary>
        /// <param name="dataSource">The data provided to perform the calculation.</param>
        /// <param name="supportingCalculations">The supporting calculation strategies provided to perform the calculation.</param>
        /// <returns>True if the calculation could be completed.</returns>
        public override bool CalculateIndicator(FinancialCalculationDataSource dataSource, FinancialCalculationSupportingCalculations supportingCalculations)
        {
            IEnumerator<double> typical = dataSource.TypicalColumn.GetEnumerator();
            IEnumerator<double> volume = dataSource.VolumeColumn.GetEnumerator();
            int period = dataSource.Period;

            double[] positiveBuffer = new double[period];
            double positiveFlow = 0.0;

            double[] negativeBuffer = new double[period];
            double negativeFlow = 0.0;

            double flowYesterday = 0.0;
            int i = 0;

            while (typical.MoveNext() && volume.MoveNext())
            {
                int cursor = i % period;
                double flowToday = typical.Current * volume.Current;

                positiveFlow -= positiveBuffer[cursor];
                negativeFlow -= negativeBuffer[cursor];

                switch (Math.Sign(flowToday - flowYesterday))
                {
                    case -1:
                        positiveBuffer[cursor] = 0.0;
                        negativeBuffer[cursor] = flowToday;
                        break;

                    case 0:
                        positiveBuffer[cursor] = 0.0;
                        negativeBuffer[cursor] = 0.0;
                        break;

                    case 1:
                        positiveBuffer[cursor] = flowToday;
                        negativeBuffer[cursor] = 0.0;
                        break;
                }

                positiveFlow += positiveBuffer[cursor];
                negativeFlow += negativeBuffer[cursor];

                double mfi = supportingCalculations.MakeSafe(
                    100.0 * positiveFlow / (positiveFlow + negativeFlow));

                dataSource.IndicatorColumn[i] = mfi;

                flowYesterday = flowToday;
                ++i;
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