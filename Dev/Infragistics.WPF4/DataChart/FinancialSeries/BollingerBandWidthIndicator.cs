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
    /// Represents a XamDataChart Bollinger Bandwidth indicator series.
    /// </summary>
    /// <remarks>
    /// Represents the normalized width of the bollinger bands for each provided value.
    /// For more info see: <see cref="BollingerBandsOverlay"/>
    /// <para>
    /// Default required members: High, Low, Close
    /// </para>
    /// </remarks>
    public class BollingerBandWidthIndicator : StrategyBasedIndicator
    {
        /// <summary>
        /// Returns the calculation strategy to use for this indicator.
        /// </summary>
        protected override IndicatorCalculationStrategy CalculationStrategy
        {
            get
            {
                return new CalculationStrategies.BollingerBandWidthIndicatorStrategy();
            }
        }

        /// <summary>
        /// Returns the default style key that should be used for this indicator.
        /// </summary>
        protected override Type StyleKeyType
        {
            get
            {
                return typeof(BollingerBandWidthIndicator);
            }
        }

        #region Period Dependency Property
        /// <summary>
        /// Gets or sets the moving average period for the Bollinger Band Width Indicator.
        /// <para>
        /// This is a dependency property.
        /// </para>
        /// <remarks>
        /// The typical, and initial, value for CCI periods is 20.
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
            CreatePeriodProperty(20, typeof(BollingerBandWidthIndicator));

        /// <summary>
        /// Specifies the period value to be used for the calculation.
        /// </summary>
        /// <returns>The period to use.</returns>
        protected override int PeriodOverride()
        {
            return Period;
        }
        #endregion

        #region ScalingFactor Dependency Property
        /// <summary>
        /// Gets or sets the multiplier for the Bollinger Band width.
        /// <para>
        /// This is a dependency property.
        /// </para>
        /// <remarks>
        /// The typical, and initial, value for Bollinger Band width multiplier is 2.
        /// </remarks>
        /// </summary>
        public double Multiplier
        {
            get
            {
                return (double)GetValue(MultiplierProperty);
            }
            set
            {
                SetValue(MultiplierProperty, value);
            }
        }

        /// <summary>
        /// Identifies the ScalingFactor dependency property.
        /// </summary>
        public static readonly DependencyProperty MultiplierProperty =
            CreateMultiplierProperty(2.0, typeof(BollingerBandWidthIndicator));

        /// <summary>
        /// Specifies the scaling factor to be used for the calculation.
        /// </summary>
        /// <returns>The scaling factor to use.</returns>
        protected override double MultiplierOverride()
        {
            return Multiplier;
        }
        #endregion



    }
}

namespace Infragistics.Controls.Charts.CalculationStrategies
{
    /// <summary>
    /// Represents the strategy for calculating a Bollinger Band Width indicator series.
    /// </summary>
    /// <remarks>
    /// For definition of indicator see: <see cref="BollingerBandWidthIndicator"/>
    /// </remarks>
    public class BollingerBandWidthIndicatorStrategy : IndicatorCalculationStrategy
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
            list.AddRange(supportingCalculations.SMA.BasedOn);
            list.AddRange(supportingCalculations.STDEV.BasedOn);

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
            
            //there are low amounts of data in the chart and the calculation is in the first period.
            //how are these edge cases to be compensated for? Or will the chart always contain enough
            //data that these edge cases won't be visible?
            IEnumerator<double> sma = supportingCalculations.SMA.Strategy(dataSource.TypicalColumn,
                dataSource.Period).GetEnumerator();
            IEnumerator<double> stdev = supportingCalculations.STDEV.Strategy(dataSource.TypicalColumn,
                dataSource.Period).GetEnumerator();
            double multiplier = dataSource.Multiplier;
            IList<double> indicatorColumn = dataSource.IndicatorColumn;

            int i = 0;

            while (sma.MoveNext() && stdev.MoveNext())
            {
                double offset = stdev.Current * multiplier;
                double upperBand = sma.Current + offset;
                double lowerBand = sma.Current - offset;
                double middleBand = sma.Current;

                double bandWidth = supportingCalculations.MakeSafe((upperBand - lowerBand) / middleBand);
                indicatorColumn[i] = bandWidth;
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