using System;
using System.Collections.Generic;
using System.Windows;

namespace Infragistics.Controls.Charts
{
    /// <summary>
    /// Represents a XamDataChart Force Index indicator series.
    /// </summary>
    /// <remarks>
    /// Default required members: Close, Volume
    /// </remarks>
    public class ForceIndexIndicator : StrategyBasedIndicator
    {
        /// <summary>
        /// Returns the calculation strategy to use for this indicator.
        /// </summary>
        protected override IndicatorCalculationStrategy CalculationStrategy
        {
            get
            {
                return new CalculationStrategies.ForceIndexIndicatorStrategy();
            }
        }

        /// <summary>
        /// Returns the default style key that should be used for this indicator.
        /// </summary>
        protected override Type StyleKeyType
        {
            get
            {
                return typeof(ForceIndexIndicator);
            }
        }

        #region Period Dependency Property
        /// <summary>
        /// Gets or sets the moving average period for the current AverageTrueRangeSeries object.
        /// <para>
        /// This is a dependency property.
        /// </para>
        /// <remarks>
        /// The typical, and initial, value for ForceIndiex periods is 0.
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
            CreatePeriodProperty(0, typeof(ForceIndexIndicator));

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
    /// Represents a strategy for calculating an Force Index indicator series.
    /// </summary>
    /// <remarks>
    /// For definition of indicator see: <see cref="ForceIndexIndicator"/>
    /// </remarks>
    public class ForceIndexIndicatorStrategy : IndicatorCalculationStrategy
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
            list.Add(FinancialSeries.VolumeColumnPropertyName);
            list.AddRange(supportingCalculations.EMA.BasedOn);
            
            return list;
        }

        /// <summary>
        /// Calculates the force index values for the target series.
        /// </summary>
        /// <param name="dataSource">The data to use for the calculation.</param>
        /// <returns>A list of force index values.</returns>
        protected IEnumerable<double> FI(FinancialCalculationDataSource dataSource)
        {
            int count = 0;
            IList<double> closeColumn = dataSource.CloseColumn;
            IList<double> volumeColumn = dataSource.VolumeColumn;
            IList<double> indicatorColumn = dataSource.IndicatorColumn;

            if (closeColumn != null && volumeColumn != null)
            {
                count = Math.Min(closeColumn.Count, volumeColumn.Count);
            }

            if (count > 0)
            {
                yield return 0;
            }

            for (int i = 1; i < count; ++i)
            {
                yield return volumeColumn[i] *
                    (closeColumn[i] - closeColumn[i - 1]);
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
            IEnumerable<double> fi = FI(dataSource);
            int period = dataSource.Period;
            IList<double> indicatorColumn = dataSource.IndicatorColumn;

            if (period != 0 &&
                !double.IsNaN(period) &&
                !double.IsInfinity(period))
            {
                fi = supportingCalculations.EMA.Strategy(fi, dataSource.Period);
            }

            int i = 0;

            foreach (double d in fi)
            {
                indicatorColumn[i] = d;
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