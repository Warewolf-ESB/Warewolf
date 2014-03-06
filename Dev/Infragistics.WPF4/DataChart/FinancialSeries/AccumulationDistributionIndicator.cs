using System;
using System.Collections.Generic;
using Infragistics.Controls.Charts.Util;
namespace Infragistics.Controls.Charts
{
    /// <summary>
    /// Represents a XamDataChart Accumulation/Distribution indicator series.
    /// </summary>
    /// <remarks>
    /// Default required members: Close, Low, High, Volume
    /// </remarks>
    public sealed class AccumulationDistributionIndicator : StrategyBasedIndicator
    {
        /// <summary>
        /// Returns the calculation strategy to use for this indicator.
        /// </summary>
        protected override IndicatorCalculationStrategy CalculationStrategy
        {
            get
            {
                return new CalculationStrategies.AccumulationDistributionIndicatorStrategy();
            }
        }

        /// <summary>
        /// Returns the default style key that should be used for this indicator.
        /// </summary>
        protected override Type StyleKeyType
        {
            get
            {
                return typeof(AccumulationDistributionIndicator);
            }
        }
    }
}

namespace Infragistics.Controls.Charts.CalculationStrategies
{
    /// <summary>
    /// Represents a strategy for calculating an Accumulation/Distribution indicator series.
    /// </summary>
    /// <remarks>
    /// For definition of indicator see: <see cref="AccumulationDistributionIndicator"/>
    /// </remarks>
    public class AccumulationDistributionIndicatorStrategy : StreamingIndicatorCalculationStrategy
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
            list.Add(FinancialSeries.LowColumnPropertyName);
            list.Add(FinancialSeries.HighColumnPropertyName);
            list.Add(FinancialSeries.VolumeColumnPropertyName);

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
            double ad = 0.0;
            double min = double.PositiveInfinity;
            double max = double.NegativeInfinity;

            int indicatorCount = dataSource.IndicatorColumn != null ? dataSource.IndicatorColumn.Count : 0;
            int closeCount = dataSource.CloseColumn != null ? dataSource.CloseColumn.Count : 0;
            int highCount = dataSource.HighColumn != null ? dataSource.HighColumn.Count : 0;
            int volumeCount = dataSource.VolumeColumn != null ? dataSource.VolumeColumn.Count : 0;

            int count = Math.Min(indicatorCount, Math.Min(closeCount, Math.Min(highCount, volumeCount)));

            for (int i = 0; i < count; ++i)
            {
                double C = dataSource.CloseColumn[i];
                double L = dataSource.LowColumn[i];
                double H = dataSource.HighColumn[i];
                double V = dataSource.VolumeColumn[i];

                double CLV = ((C - L) - (H - C)) / (H - L);

                ad += supportingCalculations.MakeSafe(CLV * V);
                min = Math.Min(min, ad);
                max = Math.Max(max, ad);

                yield return ad;
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