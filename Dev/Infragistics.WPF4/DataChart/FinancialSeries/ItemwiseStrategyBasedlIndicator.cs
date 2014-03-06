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
    /// Represents a decoupled strategy for calculating a financial indicator from data
    /// provided by a financial series. Will be called seperately for each item.
    /// </summary>
    /// <remarks>
    /// The data which the strategy will use for its calculation is
    /// a simplified subset of the data that the series makes available and is defined by 
    /// the contract <see cref="FinancialCalculationDataSource"/>
    /// </remarks>
    public abstract class ItemwiseIndicatorCalculationStrategy
    {
        /// <summary>
        /// Performs the calculation associated with this calculation strategy for one item.
        /// </summary>
        /// <param name="dataSource">The data required by the contract between these 
        /// strategies and the financial series.</param>
        /// <param name="supportingCalculations">The calculation strategies required by the 
        /// contract between these strategies and the finiancial series.</param>
        /// <param name="currentIndex">The index in the indicator collection to calculate.</param>
        /// <returns>True if the calculation could be completed.</returns>
        public abstract bool CalculateIndicatorItem(FinancialCalculationDataSource dataSource,
            FinancialCalculationSupportingCalculations supportingCalculations, 
            int currentIndex);

        /// <summary>
        /// The calculation strategy is required to express which columns its values are based on
        /// so that the series knows when to ask for recalculation.
        /// </summary>
        /// <param name="dataSource">The data source we will be calculating for.</param>
        /// <param name="supportingCalculations">The supporting calculations to be used
        /// in the calculation.</param>
        /// <returns>The list of column names that this calculation is based on.</returns>
        public abstract IList<string> BasedOn(FinancialCalculationDataSource dataSource,
            FinancialCalculationSupportingCalculations supportingCalculations);
    }

    /// <summary>
    /// Financial indicator base class for doing calculating an itemwize indicator.
    /// </summary>
    /// <remarks>
    /// An itemwise indicator's individual values don't depend on any other values in the 
    /// collection so can always be independantly recalculated. Also, because of this, the 
    /// base class handles all the looping logic and inheritors only need to provide the 
    /// actual calculation.
    /// </remarks>
    public abstract class ItemwiseStrategyBasedIndicator : StrategyBasedIndicator
    {
        internal ItemwiseIndicatorCalculationStrategy ActualItemwiseStrategy { get; set; }
        abstract internal ItemwiseIndicatorCalculationStrategy ItemwiseStrategy { get; }

        /// <summary>
        /// Specifies the strategy to use for the calculation.
        /// </summary>
        protected override IndicatorCalculationStrategy CalculationStrategy
        {
            get { return new ItemwiseStrategyCalculationStrategy(); }
        }

        /// <summary>
        /// Constructs the ItemwiseStrategyBasedIndicator.
        /// </summary>
        public ItemwiseStrategyBasedIndicator()
        {
            ActualItemwiseStrategy = ItemwiseStrategy;
            ((ItemwiseStrategyCalculationStrategy)ActualCalculationStrategy).ItemwiseStrategy = ActualItemwiseStrategy;
        }
    }

    /// <summary>
    /// Indicator calculation strategy that handles applying an itemwise strategy
    /// to the indicator values that are requested to be calculated.
    /// </summary>
    public class ItemwiseStrategyCalculationStrategy : IndicatorCalculationStrategy
    {
        /// <summary>
        /// The strategy that will be applied itemwize across the calculation.
        /// </summary>
        public ItemwiseIndicatorCalculationStrategy ItemwiseStrategy { get; set; }

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
            return ItemwiseStrategy.BasedOn(dataSource, supportingCalculations);
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
            bool shouldContinue = true;
            for (int i = dataSource.CalculateFrom; i < dataSource.CalculateFrom + dataSource.CalculateCount; i++)
            {
                shouldContinue =
                    ItemwiseStrategy.CalculateIndicatorItem(dataSource, supportingCalculations, i);
                if (!shouldContinue)
                {
                    return false;
                }
            }
            return shouldContinue;
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