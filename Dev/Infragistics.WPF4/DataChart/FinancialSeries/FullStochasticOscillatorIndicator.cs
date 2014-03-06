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
    /// Represents a XamDataChart Full Stochastic Oscillator indicator series.
    /// </summary>
    /// <remarks>
    /// Default required members: High, Low, Close
    /// </remarks>
    public class FullStochasticOscillatorIndicator : StrategyBasedIndicator
    {
        /// <summary>
        /// Returns the calculation strategy to use for this indicator.
        /// </summary>
        protected override IndicatorCalculationStrategy CalculationStrategy
        {
            get
            {
                return new CalculationStrategies.FullStochasticOscillatorIndicatorStrategy();
            }
        }

        /// <summary>
        /// Returns the default style key that should be used for this indicator.
        /// </summary>
        protected override Type StyleKeyType
        {
            get
            {
                return typeof(FullStochasticOscillatorIndicator);
            }
        }

        #region Period Dependency Property
        /// <summary>
        /// Gets or sets the moving average period for the current FullStochasticOscillatorIndicator object.
        /// <para>
        /// This is a dependency property.
        /// </para>
        /// <remarks>
        /// The typical, and initial, value for FullStochasticOscillatorIndicator periods is 14.
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
            CreatePeriodProperty(14, typeof(FullStochasticOscillatorIndicator));

        /// <summary>
        /// Specifies the period value to be used for the calculation.
        /// </summary>
        /// <returns>The period to use.</returns>
        protected override int PeriodOverride()
        {
            return Period;
        }
        #endregion

        #region SmoothingPeriod Dependency Property
        /// <summary>
        /// Gets or sets the moving average SmoothingPeriod for the current FullStochasticOscillatorIndicator object.
        /// <para>
        /// This is a dependency property.
        /// </para>
        /// <remarks>
        /// The typical, and initial, value for FullStochasticOscillatorIndicator SmoothingPeriod is 3.
        /// </remarks>
        /// </summary>
        public int SmoothingPeriod
        {
            get
            {
                return (int)GetValue(SmoothingPeriodProperty);
            }
            set
            {
                SetValue(SmoothingPeriodProperty, value);
            }
        }

        /// <summary>
        /// Identifies the SmoothingPeriod dependency property.
        /// </summary>
        public static readonly DependencyProperty SmoothingPeriodProperty =
            CreatePeriodPropertyHelper(3, typeof(FullStochasticOscillatorIndicator), "SmoothingPeriod");

        /// <summary>
        /// Specifies the short period to use for the calculation.
        /// </summary>
        /// <returns>The period to use.</returns>
        protected override int ShortPeriodOverride()
        {
            return SmoothingPeriod;
        }
        #endregion

        #region TriggerPeriod Dependency Property
        /// <summary>
        /// Gets or sets the moving average TriggerPeriod for the current FullStochasticOscillatorIndicator object.
        /// <para>
        /// This is a dependency property.
        /// </para>
        /// <remarks>
        /// The typical, and initial, value for FullStochasticOscillatorIndicator TriggerPeriod is 3.
        /// </remarks>
        /// </summary>
        public int TriggerPeriod
        {
            get
            {
                return (int)GetValue(TriggerPeriodProperty);
            }
            set
            {
                SetValue(TriggerPeriodProperty, value);
            }
        }

        /// <summary>
        /// Identifies the TriggerPeriod dependency property.
        /// </summary>
        public static readonly DependencyProperty TriggerPeriodProperty =
            CreatePeriodPropertyHelper(3, typeof(FullStochasticOscillatorIndicator), "TriggerPeriod");

        /// <summary>
        /// Specifies the long period to use for the calculation.
        /// </summary>
        /// <returns>The period to use.</returns>
        protected override int LongPeriodOverride()
        {
            return TriggerPeriod;
        }

        /// <summary>
        /// Specifies the trend period to use for the trend line, overriding 
        /// the TrendLinePeriod for the series.
        /// </summary>
        /// <returns>The period to use.</returns>
        protected override int TrendPeriodOverride()
        {
            if (ReadLocalValue(TriggerPeriodProperty) == DependencyProperty.UnsetValue)
            {
                return -1;
            }
            return TriggerPeriod;
        }
        #endregion
    }
}

namespace Infragistics.Controls.Charts.CalculationStrategies
{
    /// <summary>
    /// Represents the strategy for calculating a FullStochasticOscillatorIndicator series.
    /// </summary>
    /// <remarks>
    /// For definition of indicator see: <see cref="FullStochasticOscillatorIndicator"/>
    /// </remarks>
    public class FullStochasticOscillatorIndicatorStrategy : IndicatorCalculationStrategy
    {
        internal StreamingIndicatorCalculationStrategy PercentKStrategy { get; set; }

        /// <summary>
        /// Constructs a FullStochasticOscillatorIndicatorStrategy
        /// </summary>
        public FullStochasticOscillatorIndicatorStrategy()
        {
            PercentKStrategy = new PercentKCalculationStrategy();
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
            list.AddRange(PercentKStrategy.BasedOn(dataSource, supportingCalculations));
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
            IEnumerable<double> percentK =
                PercentKStrategy.ProvideStream(dataSource, supportingCalculations);

            //short period will act as the smoothing period for %K, while 
            //long period will act as the further smoothing period for the trend line.
            IEnumerable<double> fullPercentK =
                supportingCalculations.EMA.Strategy(percentK, dataSource.ShortPeriod);

            int i = 0;
            foreach (double value in fullPercentK)
            {
                dataSource.IndicatorColumn[i] = value;
                i++;
            }

            return true;
        }
    }

    /// <summary>
    /// Represents the strategy for calculating a list of %K values.
    /// </summary>
    public class PercentKCalculationStrategy : StreamingIndicatorCalculationStrategy
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
            list.Add(FinancialSeries.CloseColumnPropertyName);

            return list;
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
        public override IEnumerable<double> ProvideStream(
            FinancialCalculationDataSource dataSource,
            FinancialCalculationSupportingCalculations supportingCalculations)
        {
            int period = dataSource.Period;
            IList<double> highColumn = dataSource.HighColumn;
            IList<double> lowColumn = dataSource.LowColumn;

            for (int i = 0; i < dataSource.Count; i++)
            {
                int ago = Math.Min(period, i);
                double highestHigh = double.MinValue;
                double lowestLow = double.MaxValue;
                for (int j = 0; j < ago; j++)
                {
                    if (!double.IsNaN(highColumn[i - j]))
                    {
                        highestHigh = Math.Max(highestHigh, highColumn[i - j]);
                    }
                    if (!double.IsNaN(lowColumn[i - j]))
                    {
                        lowestLow = Math.Min(lowestLow, lowColumn[i - j]);
                    }
                }

                yield return supportingCalculations.MakeSafe(
                    (dataSource.CloseColumn[i] - lowestLow)
                    / (highestHigh - lowestLow)
                    * 100);
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
            int i = 0;

            foreach (double value in ProvideStream(dataSource, supportingCalculations))
            {
                dataSource.IndicatorColumn[i] = value;
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