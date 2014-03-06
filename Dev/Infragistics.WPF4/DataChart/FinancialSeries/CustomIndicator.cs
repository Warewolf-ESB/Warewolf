using System;
using System.Collections.Generic;

namespace Infragistics.Controls.Charts
{
    /// <summary>
    /// Represents a XamDataChart Custom Indicator series.
    /// <para>
    /// The indicator value is calculated in the user specified Indicator event handler. 
    /// </para>
    /// </summary>
    public class CustomIndicator : StrategyBasedIndicator
    {
        /// <summary>
        /// Returns the calculation strategy to use for this indicator.
        /// </summary>
        protected override IndicatorCalculationStrategy CalculationStrategy
        {
            get
            {
                return new CustomIndicatorStrategy();
            }
        }

        /// <summary>
        /// Returns the default style key that should be used for this indicator.
        /// </summary>
        protected override Type StyleKeyType
        {
            get
            {
                return typeof(CustomIndicator);
            }
        }

        /// <summary>
        /// Exposes which columns this strategy uses in its calculation so that the
        /// consumers will know when they should ask the strategy to recalculate.
        /// </summary>
        /// <returns>The list of column names that this strategy depends on.</returns>
        protected override IList<string> BasedOn( int position, int count)
        {
            FinancialCalculationDataSource dataSource = ProvideDataSource(position, count);
            FinancialCalculationSupportingCalculations supportingCalculations = 
                ProvideSupportingCalculations(dataSource);

            List<string> list = new List<string>();

            if (_basedOnColumns != null)
            {
                FinancialEventArgs args = new FinancialEventArgs(dataSource.CalculateFrom, 
                    dataSource.CalculateCount, dataSource,
                    supportingCalculations);

                _basedOnColumns(this, args);

                if (args.BasedOn != null)
                {
                    foreach(string propertyName in args.BasedOn)
                    {
                        list.Add(propertyName);
                    }
                }
            }

            return list;
        }

        private event FinancialEventHandler _indicator;
        /// <summary>
        /// Handle this event in order to perform a custom indicator calculation.
        /// </summary>
        public event FinancialEventHandler Indicator
        {
            add
            {
                _indicator += value;
                FullIndicatorRefresh();
            }
            remove
            {
                _indicator -= value;
                FullIndicatorRefresh();
            }
        }

        private event FinancialEventHandler _basedOnColumns;
        /// <summary>
        /// Handle this event in order to indicate which columns this custom indicator
        /// is based on.
        /// </summary>
        public event FinancialEventHandler BasedOnColumns
        {
            add
            {
                _basedOnColumns += value;
                FullIndicatorRefresh();
            }
            remove
            {
                _basedOnColumns -= value;
                FullIndicatorRefresh();
            }
        }

        /// <summary>
        /// Performs the calculation for the indicator.
        /// </summary>
        /// <param name="position">The position to calculate from.</param>
        /// <param name="count">The number of positions to calculate for.</param>
        /// <returns>True if the calculation could be completed.</returns>
        protected override bool IndicatorOverride(int position, int count)
        {
            base.IndicatorOverride(position, count);

            if (_indicator != null)
            {
                FinancialCalculationDataSource dataSource = ProvideDataSource(position, count);

                if (count == 0)
                {
                    return false;
                }

                if (!ValidateBasedOn(BasedOn(position, count)))
                {
                    return false;
                }

                _indicator(this, new FinancialEventArgs(position, 
                    count, 
                    dataSource,
                    ProvideSupportingCalculations(dataSource)));

                if (UpdateRange(dataSource))
                {
                    YAxis.UpdateRange();
                }

                return true;
            }

            return false;
        }
    }

    /// <summary>
    /// Represents the strategy for calculating a custom indicator series.
    /// </summary>
    /// <remarks>
    /// For definition of indicator see: <see cref="CustomIndicator"/>
    /// </remarks>
    public class CustomIndicatorStrategy : IndicatorCalculationStrategy
    {     
        /// <summary>
        /// Performs the calculation for the indicator.
        /// </summary>
        /// <param name="dataSource">The data provided to perform the calculation.</param>
        /// <param name="supportingCalculations">The supporting calculation strategies provided to perform the calculation.</param>
        /// <returns>True if the calculation could be completed.</returns>
        public override bool CalculateIndicator(FinancialCalculationDataSource dataSource, 
            FinancialCalculationSupportingCalculations supportingCalculations)
        {
            return true;
        }

        /// <summary>
        /// Exposes which columns this strategy uses in its calculation so that the
        /// consumers will know when they should ask the strategy to recalculate.
        /// </summary>
        /// <param name="dataSource">The data source to be used in the calculation</param>
        /// <param name="supportingCalculations">The other calculations that this indicator may depend on.</param>
        /// <returns>The list of column names that this strategy depends on.</returns>
        public override IList<string> BasedOn(FinancialCalculationDataSource dataSource, FinancialCalculationSupportingCalculations supportingCalculations)
        {
            return new List<string>();
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