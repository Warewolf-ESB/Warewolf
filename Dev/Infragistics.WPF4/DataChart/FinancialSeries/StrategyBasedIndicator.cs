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
using System.Collections;
using System.Collections.ObjectModel;

namespace Infragistics.Controls.Charts
{
    /// <summary>
    /// Represents a decoupled strategy for calculating a financial indicator from data
    /// provided by a financial series. 
    /// </summary>
    /// <remarks>
    /// The data which the strategy will use for its calculation is
    /// a simplified subset of the data that the series makes available and is defined by 
    /// the contract <see cref="FinancialCalculationDataSource"/>
    /// </remarks>
    public abstract class IndicatorCalculationStrategy
    {
        /// <summary>
        /// Performs the calculation associated with this calculation strategy.
        /// </summary>
        /// <param name="dataSource">The data required by the contract between these 
        /// strategies and the financial series.</param>
        /// <param name="supportingCalculations">The calculation strategies required by the 
        /// contract between these strategies and the finiancial series.</param>
        /// <returns>True if the calculation could be completed.</returns>
        public abstract bool CalculateIndicator(FinancialCalculationDataSource dataSource,
            FinancialCalculationSupportingCalculations supportingCalculations);

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
    /// Implementors of IStreamingIndicatorCalculationStrategy must be able to provide
    /// an enumeration of calculated values rather that filling the IndicatorColumn directly.
    /// </summary>
    public abstract class StreamingIndicatorCalculationStrategy
        : IndicatorCalculationStrategy
    {
        /// <summary>
        /// Provides a stream of calculated indicator values and does not fill
        /// the IndicatorColumn in the datasource.
        /// </summary>
        /// <param name="dataSource">The data required by the contract between these 
        /// strategies and the financial series.</param>
        /// <param name="supportingCalculations">The calculation strategies required by the 
        /// contract between these strategies and the finiancial series.</param>
        /// <returns>True if the calculation could be completed.</returns>
        public abstract IEnumerable<double> ProvideStream(FinancialCalculationDataSource dataSource,
            FinancialCalculationSupportingCalculations supportingCalculations);
    }

    /// <summary>
    /// A base class for indicator series with simple calculations that separates the calculation
    /// responsibility from the other resposibilities of the financial series, this enables
    /// easier unit testing and decoupling of individual calculation strategies. 
    /// </summary>
    /// <remarks>
    /// A contract is defined between the financial series and these simple indicator calculations 
    /// detailing the data which the series agrees to provide the simple indicator calculations, 
    /// this contract is defined by <see cref="FinancialCalculationDataSource"/>. If more 
    /// complex interactions are needed between the indicator calculation and the series, the 
    /// indicators should instead derive from <see cref="FinancialIndicator"/> directly, or some 
    /// derivative other than <see cref="StrategyBasedIndicator"/>
    /// </remarks>
    public abstract class StrategyBasedIndicator : FinancialIndicator
    {
        /// <summary>
        /// Constructs the SimpleFinancial indicator, registering the appropriate 
        /// default style key, and creating the appropriate calculation strategy.
        /// </summary>
        public StrategyBasedIndicator()
        {
            ActualCalculationStrategy = CalculationStrategy;
            DefaultStyleKey = StyleKeyType;
        }

        /// <summary>
        /// The effective calculation strategy to use for the calculation.
        /// </summary>
        protected IndicatorCalculationStrategy ActualCalculationStrategy { get; set; }

        /// <summary>
        /// Must be overridden in derived classes to specify which calculation strategy
        /// should be used for the calculation.
        /// </summary>
        protected abstract IndicatorCalculationStrategy CalculationStrategy { get; }

        /// <summary>
        /// Must be overridden in derived classes to specify which type to use as a key
        /// to find the default style for the indicator.
        /// </summary>
        protected abstract Type StyleKeyType { get; }

        internal static List<string> invalidatesSeries = new List<string>();

        #region "periodicity"
        /// <summary>
        /// Should be overridden in derived classes that are providing a period
        /// to the calculation strategy.
        /// </summary>
        /// <returns></returns>
        protected virtual int PeriodOverride()
        {
            return int.MinValue;
        }

        /// <summary>
        /// Should be overridden in derived classes that are providing a short period
        /// to the calculation strategy.
        /// </summary>
        /// <returns></returns>
        protected virtual int ShortPeriodOverride()
        {
            return int.MinValue;
        }

        /// <summary>
        /// Should be overridden in derived classes that are providing a long period
        /// to the calculation strategy.
        /// </summary>
        /// <returns></returns>
        protected virtual int LongPeriodOverride()
        {
            return int.MinValue;
        }

        internal const string PeriodPropertyName = "Period";
        internal const string LongPeriodPropertyName = "LongPeriod";
        internal const string ShortPeriodPropertyName = "ShortPeriod";

        internal static DependencyProperty CreatePeriodPropertyHelper(int defaultValue, Type ownerType, string propertyName)
        {
            DependencyProperty prop = DependencyProperty.Register(propertyName, typeof(int), ownerType,
            new PropertyMetadata(defaultValue, (sender, e) =>
            {
                (sender as StrategyBasedIndicator)
                    .RaisePropertyChanged(propertyName, e.OldValue, e.NewValue);
            }));

            invalidatesSeries.Add(propertyName);

            return prop;
        }



        /// <summary>
        /// Helper method for defining the various period dependency properties that
        /// derivatives of this class need.
        /// </summary>
        /// <param name="defaultValue">The default value for the period property to create.</param>
        /// <param name="ownerType">The type that will own the period property.</param>
        /// <returns>The dependency property identifier.</returns>
        protected static DependencyProperty CreatePeriodProperty(int defaultValue, Type ownerType)
        {
            return CreatePeriodPropertyHelper(defaultValue, ownerType, PeriodPropertyName);
        }

        /// <summary>
        /// Helper method for defining the various long period dependency properties that
        /// derivatives of this class need.
        /// </summary>
        /// <param name="defaultValue">The default value for the period property to create.</param>
        /// <param name="ownerType">The type that will own the period property.</param>
        /// <returns>The dependency property identifier.</returns>
        protected static DependencyProperty CreateLongPeriodProperty(int defaultValue, Type ownerType)
        {
            return CreatePeriodPropertyHelper(defaultValue, ownerType, LongPeriodPropertyName);
        }

        /// <summary>
        /// Helper method for defining the various short period dependency properties that
        /// derivatives of this class need.
        /// </summary>
        /// <param name="defaultValue">The default value for the period property to create.</param>
        /// <param name="ownerType">The type that will own the period property.</param>
        /// <returns>The dependency property identifier.</returns>
        protected static DependencyProperty CreateShortPeriodProperty(int defaultValue, Type ownerType)
        {
            return CreatePeriodPropertyHelper(defaultValue, ownerType, ShortPeriodPropertyName);
        }


        #endregion

        /// <summary>
        /// Returns the list of property names whose changing should result in the 
        /// indicator being recalculated.
        /// </summary>
        /// <param name="position">The position that will be recalculated from.</param>
        /// <param name="count">The number of positions that will be recalculated.</param>
        /// <returns>The list of property names.</returns>
        protected override IList<string> BasedOn(int position, int count)
        {
            FinancialCalculationDataSource dataSource = ProvideDataSource(position, count);
            FinancialCalculationSupportingCalculations supportingCalculations =
                ProvideSupportingCalculations(dataSource);
            return ActualCalculationStrategy.BasedOn(dataSource, supportingCalculations);
        }

        /// <summary>
        /// Provides the indicator calculation logic for the Simple financial indicators
        /// </summary>
        /// <param name="position"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        protected override bool IndicatorOverride(int position, int count)
        {
            FinancialCalculationDataSource dataSource =
                ProvideDataSource(position, count);

            if (count == 0)
            {
                return false;
            }

            if (!ValidateBasedOn(BasedOn(position, count)))
            {
                return false;
            }

            FinancialCalculationSupportingCalculations supportingCalculations =
                ProvideSupportingCalculations(dataSource);

            if (IndicatorRange != null)
            {
                dataSource.MinimumValue = IndicatorRange.Minimum;
                dataSource.MaximumValue = IndicatorRange.Maximum;
            }

            bool retVal = ActualCalculationStrategy.CalculateIndicator(dataSource, supportingCalculations);

            // hide some indicator values, if IgnoreFirst is set > 0
            for (int i = 0; i < this.IgnoreFirst && i < dataSource.IndicatorColumn.Count; i++)
            {
                dataSource.IndicatorColumn[i] = double.NaN;
            }

            if (YAxis != null && UpdateRange(dataSource))
            {
                YAxis.UpdateRange();
            }

            return retVal;
        }

        /// <summary>
        /// Updates the range for the Axes based on the calculated indicator values.
        /// </summary>
        /// <param name="dataSource">The data used for calculation and the calculated values.</param>
        protected bool UpdateRange(FinancialCalculationDataSource dataSource)
        {
            //check if the caculcation has specified the range.
            if (!double.IsNaN(dataSource.MinimumValue) && !double.IsNaN(dataSource.MaximumValue)
                && dataSource.SpecifiesRange)
            {
                AxisRange pRange = IndicatorRange;
                IndicatorRange = new AxisRange(dataSource.MinimumValue, dataSource.MaximumValue);

                return RangesDiffer(pRange, IndicatorRange);
            }

            //calculation did not specify the range so find the min and max of
            //the indicator column.
            double minimum = double.MaxValue;
            double maximum = double.MinValue;
            foreach (double value in dataSource.IndicatorColumn)
            {
                if (!double.IsNaN(value))
                {
                    minimum = Math.Min(minimum, value);
                    maximum = Math.Max(maximum, value);
                }
            }

            AxisRange prevRange = IndicatorRange;
            IndicatorRange = new AxisRange(minimum, maximum);
            return RangesDiffer(prevRange, IndicatorRange);
        }

        private bool RangesDiffer(AxisRange prevRange, AxisRange indicatorRange)
        {
            if (prevRange == null || indicatorRange == null)
            {
                return true;
            }

            if (prevRange.Minimum != indicatorRange.Minimum)
            {
                return true;
            }

            if (prevRange.Maximum != indicatorRange.Maximum)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Makes sure the preriod has a valid value.
        /// </summary>
        /// <param name="periodValue">Input period value.</param>
        /// <returns>Sanitized period value.</returns>
        protected int SanitizePeriod(int periodValue)
        {
            if (periodValue == int.MinValue)
            {
                return 0;
            }

            if (periodValue > IndicatorColumn.Count && IndicatorColumn.Count > 0)
            {
                periodValue = IndicatorColumn.Count - 1;
            }

            if (periodValue < 1)
            {
                return 1;
            }

            return periodValue;
        }

        /// <summary>
        /// Overridden in derived classed to provide the FinancialCalculationDataSource to perform
        /// financial calculations on behalf of the series.
        /// </summary>
        /// <param name="position">The position the calculation must begin at.</param>
        /// <param name="count">The number of items to be calculated.</param>
        /// <returns>The data source for the calculation.</returns>
        protected override FinancialCalculationDataSource ProvideDataSource(int position, int count)
        {
            FinancialCalculationDataSource dataSource = base.ProvideDataSource(position, count);

            //The indicator that is being calculated by the calculation
            //strategy needs to be writable in order to accept the calculated values.
            dataSource.IndicatorColumn = this.IndicatorColumn;

            dataSource.Period = SanitizePeriod(this.PeriodOverride());
            dataSource.ShortPeriod = SanitizePeriod(this.ShortPeriodOverride());
            dataSource.LongPeriod = SanitizePeriod(this.LongPeriodOverride());
            dataSource.Multiplier = MultiplierOverride();

            return dataSource;
        }

        /// <summary>
        /// Should be overridden in derived classes if the indicator supports some sort
        /// of scaling factor to be passed to the calculation strategy.
        /// </summary>
        /// <returns></returns>
        protected virtual double MultiplierOverride()
        {
            return 1.0;
        }

        private const string MultiplerPropertyName = "Multiplier";


        /// <summary>
        /// Creates a dependency property for storing the scaling factor.
        /// </summary>
        /// <param name="defaultValue">The default value for the scaling factor.</param>
        /// <param name="ownerType">The owner of the dependency property</param>
        /// <returns>The created dependency property identifier.</returns>
        protected static DependencyProperty CreateMultiplierProperty(double defaultValue, Type ownerType)
        {
            DependencyProperty prop = DependencyProperty.Register(MultiplerPropertyName, typeof(double), ownerType,
            new PropertyMetadata(defaultValue, (sender, e) =>
            {
                (sender as StrategyBasedIndicator)
                    .RaisePropertyChanged(MultiplerPropertyName, e.OldValue, e.NewValue);
            }));

            invalidatesSeries.Add(MultiplerPropertyName);

            return prop;
        }


        /// <summary>
        /// When overridden in a derived class, gets called whenever a property value is updated
        /// on the series or owning chart. Gives the series a chance to respond to the various property updates.
        /// </summary>
        /// <param name="sender">The object being updated.</param>
        /// <param name="propertyName">The name of the property being updated.</param>
        /// <param name="oldValue">The old value of the property.</param>
        /// <param name="newValue">The new value of the property.</param>
        protected override void PropertyUpdatedOverride(object sender, string propertyName, object oldValue, object newValue)
        {
            base.PropertyUpdatedOverride(sender, propertyName, oldValue, newValue);

            if (invalidatesSeries.Contains(propertyName))
            {
                if (YAxis != null && !YAxis.UpdateRange())
                {
                    this.FinancialView.BucketCalculator.CalculateBuckets(Resolution);
                    IndicatorOverride(0, IndicatorColumn.Count);
                    this.IndicatorView.TrendLineManager.Reset();
                    RenderSeries(false);
                }
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