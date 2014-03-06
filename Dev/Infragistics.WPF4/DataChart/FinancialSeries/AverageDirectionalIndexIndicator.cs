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
    /// Represents a XamDataChart Average Directional indicator series.
    /// </summary>
    /// <remarks>
    /// Default required members: High, Low, Close
    /// </remarks>
    public class AverageDirectionalIndexIndicator
        : StrategyBasedIndicator
    {
        /// <summary>
        /// Returns the calculation strategy to use for this indicator.
        /// </summary>
        protected override IndicatorCalculationStrategy CalculationStrategy
        {
            get
            {
                return new CalculationStrategies.AverageDirectionalIndexIndicatorStrategy();
            }
        }

        /// <summary>
        /// Returns the default style key that should be used for this indicator.
        /// </summary>
        protected override Type StyleKeyType
        {
            get
            {
                return typeof(AverageDirectionalIndexIndicator);
            }
        }

        #region Period Dependency Property
        /// <summary>
        /// Gets or sets the moving average period for the current AverageDirectionalIndexIndicator object.
        /// <para>
        /// This is a dependency property.
        /// </para>
        /// <remarks>
        /// The typical, and initial, value for AverageDirectionalIndexIndicator periods is 14.
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
            CreatePeriodProperty(14, typeof(AverageDirectionalIndexIndicator));


#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)


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
    /// Represents the strategy for calculating a AverageDirectionalIndexIndicator series.
    /// </summary>
    /// <remarks>
    /// For definition of indicator see: <see cref="AverageDirectionalIndexIndicator"/>
    /// </remarks>
    public class AverageDirectionalIndexIndicatorStrategy
        : IndicatorCalculationStrategy
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
            list.Add(FinancialSeries.LowColumnPropertyName);
            list.Add(FinancialSeries.HighColumnPropertyName);
            list.AddRange(dataSource.TrueRange.BasedOn);
            list.AddRange(supportingCalculations.EMA.BasedOn);

            return list;
        }

        private double UpMove(int index, IList<double> highColumn, IList<double> lowColumn)
        {
            return highColumn[index] - highColumn[index - 1];
        }

        private double DownMove(int index, IList<double> highColumn, IList<double> lowColumn)
        {
            return lowColumn[index - 1] - lowColumn[index];
        }

        private IEnumerable<double> PlusDM(IList<double> highColumn, IList<double> lowColumn)
        {
            yield return 0.0;
            double upMove = 0.0;
            double downMove = 0.0;
            for (int i = 1; i < highColumn.Count; i++)
            {
                upMove = UpMove(i, highColumn, lowColumn);
                downMove = DownMove(i, highColumn, lowColumn);

                if (upMove > downMove && upMove > 0.0)
                {
                    yield return upMove;
                }
                else
                {
                    yield return 0.0;
                }
            }
        }

        private IEnumerable<double> MinusDM(IList<double> highColumn, IList<double> lowColumn)
        {
            yield return 0.0;
            double upMove = 0.0;
            double downMove = 0.0;
            for (int i = 1; i < highColumn.Count; i++)
            {
                upMove = UpMove(i, highColumn, lowColumn);
                downMove = DownMove(i, highColumn, lowColumn);

                if (downMove > upMove && downMove > 0.0)
                {
                    yield return downMove;
                }
                else
                {
                    yield return 0.0;
                }
            }
        }

        private IEnumerable<double> PlusDI(FinancialCalculationDataSource dataSource,
            FinancialCalculationSupportingCalculations supportingCalculations)
        {
            IEnumerator<double> emaPlusDM =
                supportingCalculations.EMA.Strategy(PlusDM(dataSource.HighColumn, dataSource.LowColumn),
                dataSource.Period).GetEnumerator();
            IEnumerator<double> averageTrueRange =
                supportingCalculations.EMA.Strategy(dataSource.TrueRange, dataSource.Period).GetEnumerator();

            while (emaPlusDM.MoveNext() && averageTrueRange.MoveNext())
            {
                yield return supportingCalculations.MakeSafe(emaPlusDM.Current / averageTrueRange.Current);
            }
        }

        private IEnumerable<double> MinusDI(FinancialCalculationDataSource dataSource,
           FinancialCalculationSupportingCalculations supportingCalculations)
        {
            IEnumerator<double> emaMinusDM =
                supportingCalculations.EMA.Strategy(MinusDM(dataSource.HighColumn, dataSource.LowColumn),
                dataSource.Period).GetEnumerator();
            IEnumerator<double> averageTrueRange =
                supportingCalculations.EMA.Strategy(dataSource.TrueRange, dataSource.Period).GetEnumerator();

            while (emaMinusDM.MoveNext() && averageTrueRange.MoveNext())
            {
                yield return supportingCalculations.MakeSafe(emaMinusDM.Current / averageTrueRange.Current);
            }
        }

        private IEnumerable<double> ADXHelper(FinancialCalculationDataSource dataSource,
            FinancialCalculationSupportingCalculations supportingCalculations)
        {
            IEnumerator<double> plusDI = PlusDI(dataSource, supportingCalculations).GetEnumerator();
            IEnumerator<double> minusDI = MinusDI(dataSource, supportingCalculations).GetEnumerator();

            //discard values for index 1
            plusDI.MoveNext();
            minusDI.MoveNext();

            while (plusDI.MoveNext() && minusDI.MoveNext())
            {
                yield return Math.Abs(
                    supportingCalculations.MakeSafe((plusDI.Current - minusDI.Current) /
                    (plusDI.Current + minusDI.Current)));
            }
        }

        private IEnumerable<double> ADX(FinancialCalculationDataSource dataSource,
            FinancialCalculationSupportingCalculations supportingCalculations)
        {
            IEnumerator<double> adxHelperEMA =
                supportingCalculations.EMA.Strategy(
                    ADXHelper(dataSource, supportingCalculations), dataSource.Period)
                    .GetEnumerator();

            yield return 0.0;

            while (adxHelperEMA.MoveNext())
            {
                yield return adxHelperEMA.Current * 100.0;
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
            IEnumerator<double> adx = ADX(dataSource, supportingCalculations).GetEnumerator();
            IList<double> indicatorColumn = dataSource.IndicatorColumn;

            for (int i = 0; i < indicatorColumn.Count; i++)
            {
                if (adx.MoveNext())
                {
                    indicatorColumn[i] = adx.Current;
                }
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